using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.Events.DTOs;
using LinkUp254.Features.Events.models;
using LinkUp254.Features.Groups.Models;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUp254.Features.Events.Models;

namespace LinkUp254.Features.Events.Services;

public class EventServices
{
    private readonly LinkUpContext _context;
    private readonly ILogger<EventServices> _logger;

    public string? CoverImage { get; private set; }

    public EventServices(LinkUpContext context, ILogger<EventServices> logger)
    {
        _context = context;
        _logger = logger;
    }

    private IQueryable<Event> ApplyVisibilityFilter(IQueryable<Event> query, int? userId)
    {
        var publicEvents = query.Where(e => e.Visibility == 0);

        if (!userId.HasValue)
            return publicEvents;

        var userGroupIds = _context.GroupMembers
            .Where(gm => gm.UserId == userId.Value && gm.IsActive)
            .Select(gm => gm.GroupId);

        var groupEvents = query
            .Where(e => e.Visibility == 1
                     && e.GroupEvents.Any(ge => userGroupIds.Contains(ge.GroupId)));

        var privateEvents = query
            .Where(e => e.Visibility == 2 && e.OrganizerId == userId.Value);

        return publicEvents
            .Concat(groupEvents)
            .Concat(privateEvents)
            .Distinct();
    }











    public async Task<PagedResult<Event>> GetNearbyEventsAsync(
    double userLatitude,
    double userLongitude,
    double radiusKm,
    EventFilterDto filters,
    int? userId = null)
    {
        try
        {
           
            var query = _context.Events
                .Where(e => e.IsActive
                         && e.IsPublished
                         && e.Latitude.HasValue
                         && e.Longitude.HasValue
                         && e.StartTime >= DateTime.UtcNow.Date)
                .AsNoTracking()
                .AsQueryable();

           
            query = ApplyVisibilityFilter(query, userId);

           
            if (!string.IsNullOrEmpty(filters.City))
                query = query.Where(e => EF.Functions.Like(e.City, $"%{filters.City}%"));
            if (!string.IsNullOrEmpty(filters.Country))
                query = query.Where(e => EF.Functions.Like(e.Country, $"%{filters.Country}%"));
            if (filters.StartDate.HasValue)
                query = query.Where(e => e.StartTime >= filters.StartDate.Value);
            if (filters.EndDate.HasValue)
                query = query.Where(e => e.StartTime <= filters.EndDate.Value);
            if (filters.IsFreeOnly == true)
                query = query.Where(e => e.IsFree);

           
            var allEvents = await query
                .Include(e => e.EventInterests).ThenInclude(ei => ei.Interest)
                .Include(e => e.Organizer)
                .AsSplitQuery()
                .ToListAsync();

            var nearbyEvents = allEvents.Where(e =>
            {
                if (!e.Latitude.HasValue || !e.Longitude.HasValue) return false;
                var distance = CalculateHaversineDistance(
                    userLatitude, userLongitude,
                    e.Latitude.Value, e.Longitude.Value);
                return distance <= radiusKm;
            }).Select(e => new {
                Event = e,
                Distance = CalculateHaversineDistance(
                    userLatitude, userLongitude,
                    e.Latitude!.Value, e.Longitude!.Value)
            })
              .OrderBy(x => x.Distance)
              .ToList();

            // Apply pagination
            var total = nearbyEvents.Count;
            var pagedEvents = nearbyEvents
                .Skip(filters.Offset)
                .Take(filters.Limit)
                .Select(x => x.Event)
                .ToList();

            return new PagedResult<Event>
            {
                Items = pagedEvents,
                Total = total,
                Limit = filters.Limit,
                Offset = filters.Offset
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetNearbyEventsAsync failed");
            return new PagedResult<Event> { Items = new List<Event>(), Total = 0 };
        }
    }

    // Haversine formula helper
    private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees) => degrees * (Math.PI / 180);








    public async Task<PagedResult<Event>> GetEventsAsync(EventFilterDto filters, int? userId = null)
    {
        try
        {
            var query = _context.Events
                .Where(e => e.IsActive && e.IsPublished)
                .AsNoTracking()
                .AsQueryable();

            query = ApplyVisibilityFilter(query, userId);

            //if (!string.IsNullOrEmpty(filters.Search))
            //{
            //    var searchTerm = $"%{filters.Search}%";
            //    query = query.Where(e =>
            //        EF.Functions.Like(e.Title, searchTerm) ||
            //        EF.Functions.Like(e.Description, searchTerm) ||
            //        EF.Functions.Like(e.Location, searchTerm) ||
            //        EF.Functions.Like(e.City, searchTerm) ||
            //        EF.Functions.Like(e.Country, searchTerm)
            //    );
            //}

            if (!string.IsNullOrEmpty(filters.Search))
            {
                var searchTerm = $"%{filters.Search}%";
                query = query.Where(e =>
                    EF.Functions.Like(e.Title, searchTerm) ||
                    EF.Functions.Like(e.Description, searchTerm) ||
                    EF.Functions.Like(e.Location, searchTerm) ||
                    EF.Functions.Like(e.City, searchTerm) ||
                    EF.Functions.Like(e.Country, searchTerm) ||
                    EF.Functions.Like(e.VenueName, searchTerm) ||
                    EF.Functions.Like(e.StreetAddress, searchTerm)
                );
            }



            if (!string.IsNullOrEmpty(filters.City))
                query = query.Where(e => EF.Functions.Like(e.City, $"%{filters.City}%"));
            if (!string.IsNullOrEmpty(filters.Country))
                query = query.Where(e => EF.Functions.Like(e.Country, $"%{filters.Country}%"));
            if (!string.IsNullOrEmpty(filters.Location))
                query = query.Where(e => EF.Functions.Like(e.Location, $"%{filters.Location}%"));

            if (filters.StartDate.HasValue)
                query = query.Where(e => e.StartTime >= filters.StartDate.Value);
            if (filters.EndDate.HasValue)
                query = query.Where(e => e.StartTime <= filters.EndDate.Value);

            if (filters.IsFreeOnly == true)
                query = query.Where(e => e.IsFree);
            if (filters.MinPrice.HasValue)
                query = query.Where(e => e.Price >= filters.MinPrice);
            if (filters.MaxPrice.HasValue)
                query = query.Where(e => e.Price <= filters.MaxPrice);

            query = filters.SortBy switch
            {
                "date_asc" => query.OrderBy(e => e.StartTime),
                "date_desc" => query.OrderByDescending(e => e.StartTime),
                "popularity" => query.OrderByDescending(e => e.AttendeeCount),
                "price_asc" => query.OrderBy(e => e.Price).ThenBy(e => e.StartTime),
                "price_desc" => query.OrderByDescending(e => e.Price).ThenBy(e => e.StartTime),
                _ => query.OrderByDescending(e => e.StartTime)
            };

            var total = await query.CountAsync();
            var events = await query
                .Skip(filters.Offset)
                .Take(filters.Limit)
                .Include(e => e.EventInterests)
                    .ThenInclude(ei => ei.Interest)
                .Include(e => e.Organizer)
                .AsSplitQuery()
                .ToListAsync();

            return new PagedResult<Event>
            {
                Items = events,
                Total = total,
                Limit = filters.Limit,
                Offset = filters.Offset
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEventsAsync failed");
            return new PagedResult<Event> { Items = new List<Event>(), Total = 0 };
        }
    }

    public async Task<PagedResult<Event>> GetPersonalizedEventsAsync(string userId, EventFilterDto filters)
    {
        try
        {
            if (!int.TryParse(userId, out var userIdInt))
                return new PagedResult<Event> { Items = new List<Event>(), Total = 0 };

            
            var userInterestIds = await _context.UserInterests
                .AsNoTracking()
                .Where(ui => ui.UserId == userIdInt && ui.IsActive)
                .Select(ui => ui.InterestId)
                .ToListAsync();

           
            var userGroupIds = await _context.GroupMembers
                .AsNoTracking()
                .Where(gm => gm.UserId == userIdInt && gm.IsActive)
                .Select(gm => gm.GroupId)
                .ToListAsync();

         
            var baseQuery = _context.Events
                .AsNoTracking()
                .Where(e => e.IsActive && e.IsPublished && e.StartTime >= DateTime.UtcNow.Date);

            
            var visibleQuery = baseQuery.Where(e =>
                e.Visibility == 0 ||
                e.Visibility == 1 && e.GroupEvents.Any(ge => userGroupIds.Contains(ge.GroupId)) ||
                e.Visibility == 2 && e.OrganizerId == userIdInt
            );

           
            if (!string.IsNullOrEmpty(filters.City))
                visibleQuery = visibleQuery.Where(e => EF.Functions.Like(e.City, $"%{filters.City}%"));
            if (!string.IsNullOrEmpty(filters.Country))
                visibleQuery = visibleQuery.Where(e => EF.Functions.Like(e.Country, $"%{filters.Country}%"));
            if (filters.StartDate.HasValue)
                visibleQuery = visibleQuery.Where(e => e.StartTime >= filters.StartDate.Value);
            if (filters.EndDate.HasValue)
                visibleQuery = visibleQuery.Where(e => e.StartTime <= filters.EndDate.Value);
            if (filters.IsFreeOnly == true)
                visibleQuery = visibleQuery.Where(e => e.IsFree);

          
            var eventIds = await visibleQuery
                .Select(e => e.Id)
                .ToListAsync();

            if (!eventIds.Any())
                return new PagedResult<Event> { Items = new List<Event>(), Total = 0 };

           
            var personalizedIds = await _context.EventInterests
                .AsNoTracking()
                .Where(ei => eventIds.Contains(ei.EventId) && userInterestIds.Contains(ei.InterestId))
                .Select(ei => ei.EventId)
                .Distinct()
                .ToListAsync();

            if (!personalizedIds.Any() && userInterestIds.Any())
                return new PagedResult<Event> { Items = new List<Event>(), Total = 0 };

        
            var finalEventIds = userInterestIds.Any() ? personalizedIds : eventIds;

         
            var events = await _context.Events
                .AsNoTracking()
                .Include(e => e.EventInterests).ThenInclude(ei => ei.Interest)
                .Include(e => e.Organizer)
                .Where(e => finalEventIds.Contains(e.Id))
                .AsSplitQuery()
                .ToListAsync();

           
            if (userInterestIds.Any())
            {
                events = events
                    .OrderByDescending(e => e.EventInterests.Count(ei => userInterestIds.Contains(ei.InterestId)))
                    .ThenByDescending(e => e.AttendeeCount)
                    .ThenByDescending(e => e.StartTime)
                    .ToList();
            }
            else
            {
                events = events
                    .OrderByDescending(e => e.AttendeeCount)
                    .ThenByDescending(e => e.StartTime)
                    .ToList();
            }

            
            var userProfile = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userIdInt);
            if (!string.IsNullOrEmpty(userProfile?.City))
            {
                events = events
                    .OrderByDescending(e => e.City == userProfile.City)
                    .ThenByDescending(e => e.Country == userProfile.Country)
                    .ThenBy(e => e.Id) 
                    .ToList();
            }

          
            var total = events.Count;
            var pagedEvents = events
                .Skip(filters.Offset)
                .Take(filters.Limit)
                .ToList();

            return new PagedResult<Event>
            {
                Items = pagedEvents,
                Total = total,
                Limit = filters.Limit,
                Offset = filters.Offset
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPersonalizedEventsAsync failed for user {UserId}", userId);
            return new PagedResult<Event> { Items = new List<Event>(), Total = 0 };
        }
    }

    public async Task<PagedResult<Event>> GetTrendingEventsAsync(EventFilterDto filters, int? userId = null)
    {
        try
        {
            var query = _context.Events
                .Where(e => e.IsActive && e.IsPublished && e.StartTime >= DateTime.UtcNow.Date)
                .AsNoTracking()
                .AsQueryable();

            query = ApplyVisibilityFilter(query, userId);

            query = query
                .OrderByDescending(e => e.AttendeeCount)
                .ThenByDescending(e => e.LikeCount)
                .ThenByDescending(e => e.ViewCount);

            if (!string.IsNullOrEmpty(filters.City))
                query = query.Where(e => EF.Functions.Like(e.City, $"%{filters.City}%"));
            if (!string.IsNullOrEmpty(filters.Country))
                query = query.Where(e => EF.Functions.Like(e.Country, $"%{filters.Country}%"));
            if (filters.StartDate.HasValue)
                query = query.Where(e => e.StartTime >= filters.StartDate.Value);

            var total = await query.CountAsync();
            var events = await query
                .Skip(filters.Offset)
                .Take(filters.Limit)
                .Include(e => e.EventInterests)
                    .ThenInclude(ei => ei.Interest)
                .Include(e => e.Organizer)
                .AsSplitQuery()
                .ToListAsync();

            return new PagedResult<Event>
            {
                Items = events,
                Total = total,
                Limit = filters.Limit,
                Offset = filters.Offset
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTrendingEventsAsync failed");
            return new PagedResult<Event> { Items = new List<Event>(), Total = 0 };
        }
    }

    public async Task<Event?> GetEventByIdAsync(int eventId, int? userId = null)
    {
        try
        {
            var baseQuery = _context.Events
                .Include(e => e.EventInterests).ThenInclude(ei => ei.Interest)
                .Include(e => e.Organizer)
                .Include(e => e.Tickets)
                .Where(e => e.Id == eventId && e.IsActive && e.IsPublished)
                .AsNoTracking()
                .AsQueryable();

            var filtered = ApplyVisibilityFilter(baseQuery, userId);
            return await filtered.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEventByIdAsync failed for id {EventId}", eventId);
            return null;
        }
    }

    public async Task<List<Event>> GetEventsByOrganizerAsync(int organizerId)
    {
        try
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.OrganizerId == organizerId)
                .Include(e => e.EventInterests)
                .OrderByDescending(e => e.StartTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEventsByOrganizerAsync failed for organizer {OrganizerId}", organizerId);
            return new List<Event>();
        }
    }

    public async Task<AuthResult> CreateEventAsync(CreateEventDto dto, int organizerId)
    {
        try
        {
            var organizer = await _context.Users.FirstOrDefaultAsync(u => u.Id == organizerId);
            if (organizer == null)
                return AuthResult.Failure("Organizer not found.");

            //var newEvent = new Event(
            //    title: dto.Title,
            //    description: dto.Description ?? "",
            //    city: dto.City ?? "",
            //    country: dto.Country ?? "",
            //    location: dto.Location ?? "",
            //    startTime: dto.StartDate,
            //    endTime: dto.EndDate ?? dto.StartDate.AddHours(3),
            //    organizerId: organizerId
            //)
            //{
            //    Price = dto.Price,
            //    CoverImage = dto.ImageUrl,
            //    MaxAttendees = dto.MaxAttendees,
            //    IsPublished = dto.IsPublished ?? true,
            //    Visibility = dto.Visibility ?? 0
            //};

            var newEvent = new Event(
    title: dto.Title,
    description: dto.Description ?? "",
    city: dto.City ?? "",
    country: dto.Country ?? "",
    location: dto.Location ?? "",
    startTime: dto.StartDate,
    endTime: dto.EndDate ?? dto.StartDate.AddHours(3),
    organizerId: organizerId
)
            {
                Price = dto.Price,
                CoverImage = dto.ImageUrl,
                MaxAttendees = dto.MaxAttendees,
                IsPublished = dto.IsPublished ?? true,
                Visibility = dto.Visibility ?? 0,
                // NEW: Precise venue fields
                VenueName = dto.VenueName?.Trim(),
                StreetAddress = dto.StreetAddress?.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                MapProviderPlaceId = dto.MapProviderPlaceId?.Trim(),
                LocationVisibility = dto.LocationVisibility ?? 0
            };



            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            if (dto.GroupId.HasValue)
            {
                int groupId = dto.GroupId.Value;

                bool hasPermission = await _context.GroupMembers
                    .AnyAsync(gm => gm.GroupId == groupId
                                 && gm.UserId == organizerId
                                 && gm.IsActive
                                 && (gm.Role == "admin" || gm.Role == "moderator"));

                if (!hasPermission)
                {
                    hasPermission = await _context.Groups
                        .AnyAsync(g => g.Id == groupId && g.OrganizerId == organizerId);
                }

                if (!hasPermission)
                {
                    _context.Events.Remove(newEvent);
                    await _context.SaveChangesAsync();
                    return AuthResult.Failure("You do not have permission to create events in this group.");
                }

                var groupEvent = new GroupEvent
                {
                    GroupId = groupId,
                    EventId = newEvent.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.GroupEvents.Add(groupEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Event {EventId} linked to group {GroupId}", newEvent.Id, groupId);
            }

            if (dto.InterestIds?.Any() == true)
            {
                foreach (var interestId in dto.InterestIds)
                {
                    _context.EventInterests.Add(new EventInterest
                    {
                        EventId = newEvent.Id,
                        InterestId = interestId
                    });
                }
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("Event created successfully: {Title} (ID: {Id}) by user {UserId}",
                dto.Title, newEvent.Id, organizerId);

            return AuthResult.Success("Event created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateEventAsync failed for user {OrganizerId}", organizerId);
            return AuthResult.Failure("Failed to create event. Please try again.");
        }
    }

    public async Task<AuthResult> UpdateEventAsync(int eventId, UpdateEventDto dto, int organizerId)
    {
        try
        {
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId && e.OrganizerId == organizerId);

            if (existingEvent == null)
                return AuthResult.Failure("Event not found or you don't have permission to edit it.");

            //if (!string.IsNullOrEmpty(dto.Title)) existingEvent.Title = dto.Title;
            //if (dto.Description != null) existingEvent.Description = dto.Description;
            //if (!string.IsNullOrEmpty(dto.City)) existingEvent.City = dto.City;
            //if (!string.IsNullOrEmpty(dto.Country)) existingEvent.Country = dto.Country;
            //if (!string.IsNullOrEmpty(dto.Location)) existingEvent.Location = dto.Location;
            //if (dto.StartDate.HasValue) existingEvent.StartTime = dto.StartDate.Value;
            //if (dto.EndDate.HasValue) existingEvent.EndTime = dto.EndDate.Value;
            //if (dto.Price.HasValue) existingEvent.Price = dto.Price;
            //if (dto.ImageUrl != null) existingEvent.CoverImage = dto.ImageUrl;
            //if (dto.MaxAttendees.HasValue) existingEvent.MaxAttendees = dto.MaxAttendees;
            //if (dto.IsPublished.HasValue) existingEvent.IsPublished = dto.IsPublished.Value;

            //existingEvent.UpdatedAt = DateTime.UtcNow;


            if (!string.IsNullOrEmpty(dto.Title)) existingEvent.Title = dto.Title;
            if (dto.Description != null) existingEvent.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.City)) existingEvent.City = dto.City;
            if (!string.IsNullOrEmpty(dto.Country)) existingEvent.Country = dto.Country;
            if (!string.IsNullOrEmpty(dto.Location)) existingEvent.Location = dto.Location;
            if (dto.StartDate.HasValue) existingEvent.StartTime = dto.StartDate.Value;
            if (dto.EndDate.HasValue) existingEvent.EndTime = dto.EndDate.Value;
            if (dto.Price.HasValue) existingEvent.Price = dto.Price;
            if (dto.ImageUrl != null) existingEvent.CoverImage = dto.ImageUrl;
            if (dto.MaxAttendees.HasValue) existingEvent.MaxAttendees = dto.MaxAttendees;
            if (dto.IsPublished.HasValue) existingEvent.IsPublished = dto.IsPublished.Value;
            // NEW: Venue field updates
            if (dto.VenueName != null) existingEvent.VenueName = dto.VenueName.Trim();
            if (dto.StreetAddress != null) existingEvent.StreetAddress = dto.StreetAddress.Trim();
            if (dto.Latitude.HasValue) existingEvent.Latitude = dto.Latitude.Value;
            if (dto.Longitude.HasValue) existingEvent.Longitude = dto.Longitude.Value;
            if (dto.MapProviderPlaceId != null) existingEvent.MapProviderPlaceId = dto.MapProviderPlaceId.Trim();
            if (dto.LocationVisibility.HasValue) existingEvent.LocationVisibility = dto.LocationVisibility.Value;

            existingEvent.UpdatedAt = DateTime.UtcNow;


            if (dto.InterestIds != null)
            {
                var existingAssociations = _context.EventInterests
                    .Where(ei => ei.EventId == eventId);
                _context.EventInterests.RemoveRange(existingAssociations);

                foreach (var interestId in dto.InterestIds)
                {
                    _context.EventInterests.Add(new EventInterest
                    {
                        EventId = eventId,
                        InterestId = interestId
                    });
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Event updated: {EventId}", eventId);
            return AuthResult.Success("Event updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateEventAsync failed for event {EventId}", eventId);
            return AuthResult.Failure("Failed to update event.");
        }
    }

    public async Task<AuthResult> DeleteEventAsync(int eventId, int requestingUserId)
    {
        try
        {
            var existingEvent = await _context.Events
                .Include(e => e.GroupEvents)
                .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive);

            if (existingEvent == null)
                return AuthResult.Failure("Event not found.");

            bool hasPermission = existingEvent.OrganizerId == requestingUserId;

            if (!hasPermission && existingEvent.GroupEvents?.Any() == true)
            {
                foreach (var groupEvent in existingEvent.GroupEvents)
                {
                    bool isGroupAdmin = await _context.GroupMembers
                        .AnyAsync(gm =>
                            gm.GroupId == groupEvent.GroupId &&
                            gm.UserId == requestingUserId &&
                            gm.IsActive &&
                            (gm.Role == "admin" || gm.Role == "moderator"));

                    if (isGroupAdmin)
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }

            if (!hasPermission)
                return AuthResult.Failure("You do not have permission to delete this event.");

            existingEvent.IsActive = false;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Event {EventId} soft-deleted by user {UserId}", eventId, requestingUserId);
            return AuthResult.Success("Event deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteEventAsync failed for event {EventId}", eventId);
            return AuthResult.Failure("Failed to delete event.");
        }
    }



  
    public string GetSafeDisplayLocation(Event eventEntity, int? viewingUserId = null)
    {
        
        if (viewingUserId.HasValue && viewingUserId.Value == eventEntity.OrganizerId)
        {
            return !string.IsNullOrEmpty(eventEntity.VenueName)
                ? $"{eventEntity.VenueName}, {eventEntity.City}"
                : $"{eventEntity.City}, {eventEntity.Country}";
        }

        
        return eventEntity.LocationVisibility switch
        {
            0 => !string.IsNullOrEmpty(eventEntity.VenueName)
                ? $"{eventEntity.VenueName}, {eventEntity.City}"
                : $"{eventEntity.City}, {eventEntity.Country}",
            1 => eventEntity.StartTime <= DateTime.UtcNow.AddHours(2)
                ? !string.IsNullOrEmpty(eventEntity.VenueName)
                    ? $"{eventEntity.VenueName}, {eventEntity.City}"
                    : $"{eventEntity.City}, {eventEntity.Country}"
                : $"{eventEntity.City}, {eventEntity.Country}",
            _ => "Location shared upon RSVP"
        };
    }













    
    public async Task<EventWithTicketsDto> GetEventWithTicketsAsync(int eventId, int? userId = null)
    {
        var eventEntity = await _context.Events
            .Include(e => e.TicketTiers)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive && e.IsPublished);

        if (eventEntity == null) return null;

        var now = DateTime.UtcNow;
        var availableTiers = eventEntity.TicketTiers
            .Where(t => t.IsActive
                     && t.SoldCount < t.Capacity
                     && (!t.SaleStartsAt.HasValue || now >= t.SaleStartsAt.Value)
                     && (!t.SaleEndsAt.HasValue || now <= t.SaleEndsAt.Value))
            .Select(t => new TicketTierDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Price = t.Price,
                RemainingCapacity = t.Capacity - t.SoldCount,
                MinPerOrder = t.MinPerOrder,
                MaxPerOrder = t.MaxPerOrder,
                RequirePhoneNumber = t.RequirePhoneNumber,
                RequireStudentId = t.RequireStudentId,
                IsTransferable = t.IsTransferable,
                IsRefundable = t.IsRefundable
            })
            .OrderBy(t => t.Price)
            .ToList();

        return new EventWithTicketsDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            StartTime = eventEntity.StartTime,
            EndTime = eventEntity.EndTime,
            City = eventEntity.City,
            Country = eventEntity.Country,
            Location = eventEntity.Location,
            VenueName = eventEntity.VenueName,
            StreetAddress = eventEntity.StreetAddress,
            Latitude = eventEntity.Latitude,
            Longitude = eventEntity.Longitude,
            LocationVisibility = eventEntity.LocationVisibility,
            DisplayLocation = GetSafeDisplayLocation(eventEntity, userId),
            CoverImage = eventEntity.CoverImage,
            Price = eventEntity.Price,
            IsFree = eventEntity.IsFree,
            Organizer = new UserDto
            {
                Id = eventEntity.Organizer.Id,
                FirstName = eventEntity.Organizer.FirstName,
                LastName = eventEntity.Organizer.LastName,
                ProfilePicture = eventEntity.Organizer.ProfilePicture
            },
            AvailableTicketTiers = availableTiers,
            IsOrganizer = userId.HasValue && userId.Value == eventEntity.OrganizerId
        };
    }

    public class EventWithTicketsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? VenueName { get; set; }
        public string? StreetAddress { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int LocationVisibility { get; set; }
        public string DisplayLocation { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public decimal? Price { get; set; }
        public bool IsFree { get; set; }
        public UserDto? Organizer { get; set; }
        public List<TicketTierDto> AvailableTicketTiers { get; set; } = new();
        public bool IsOrganizer { get; set; }
    }

}