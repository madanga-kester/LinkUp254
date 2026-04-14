using LinkUp254.Database;
using LinkUp254.Features.Events.DTOs;
using LinkUp254.Features.Events.models;
using LinkUp254.Features.Shared;
using LinkUp254.Features.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUp254.Features.Groups.Models; 
namespace LinkUp254.Features.Events;

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







    // GET: All events with filters + visibility awareness
    public async Task<PagedResult<Event>> GetEventsAsync(EventFilterDto filters, int? userId = null)
    {
        try
        {
            var query = _context.Events
                .Where(e => e.IsActive && e.IsPublished)
                .AsQueryable();

            query = ApplyVisibilityFilter(query, userId);

            // Search by title, description, location, city, country
            if (!string.IsNullOrEmpty(filters.Search))
            {
                var searchTerm = $"%{filters.Search}%";
                query = query.Where(e =>
                    EF.Functions.Like(e.Title, searchTerm) ||
                    EF.Functions.Like(e.Description, searchTerm) ||
                    EF.Functions.Like(e.Location, searchTerm) ||
                    EF.Functions.Like(e.City, searchTerm) ||
                    EF.Functions.Like(e.Country, searchTerm)
                );
            }

            // Filter by location
            if (!string.IsNullOrEmpty(filters.City))
                query = query.Where(e => EF.Functions.Like(e.City, $"%{filters.City}%"));
            if (!string.IsNullOrEmpty(filters.Country))
                query = query.Where(e => EF.Functions.Like(e.Country, $"%{filters.Country}%"));
            if (!string.IsNullOrEmpty(filters.Location))
                query = query.Where(e => EF.Functions.Like(e.Location, $"%{filters.Location}%"));

            // Filter by date range
            if (filters.StartDate.HasValue)
                query = query.Where(e => e.StartTime >= filters.StartDate.Value);
            if (filters.EndDate.HasValue)
                query = query.Where(e => e.StartTime <= filters.EndDate.Value);

            // Filter by price
            if (filters.IsFreeOnly == true)
                query = query.Where(e => e.IsFree);
            if (filters.MinPrice.HasValue)
                query = query.Where(e => e.Price >= filters.MinPrice);
            if (filters.MaxPrice.HasValue)
                query = query.Where(e => e.Price <= filters.MaxPrice);

            // Sorting
            query = filters.SortBy switch
            {
                "date_asc" => query.OrderBy(e => e.StartTime),
                "date_desc" => query.OrderByDescending(e => e.StartTime),
                "popularity" => query.OrderByDescending(e => e.AttendeeCount),
                "price_asc" => query.OrderBy(e => e.Price).ThenBy(e => e.StartTime),
                "price_desc" => query.OrderByDescending(e => e.Price).ThenBy(e => e.StartTime),
                _ => query.OrderByDescending(e => e.StartTime)
            };

            // Pagination
            var total = await query.CountAsync();
            var events = await query
                .Skip(filters.Offset)
                .Take(filters.Limit)
                .Include(e => e.EventInterests)
                    .ThenInclude(ei => ei.Interest)
                .Include(e => e.Organizer)
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











    // GET: Personalized events for a user 
    public async Task<PagedResult<Event>> GetPersonalizedEventsAsync(string userId, EventFilterDto filters)
    {
        try
        {
            int userIdInt = int.Parse(userId);

            var query = _context.Events
                .Where(e => e.IsActive && e.IsPublished && e.StartTime >= DateTime.UtcNow.Date)
                .AsQueryable();

            
            query = ApplyVisibilityFilter(query, userIdInt);

            var userInterestIds = await _context.UserInterests
                .Where(ui => ui.UserId == userIdInt && ui.IsActive)
                .Select(ui => ui.InterestId)
                .ToListAsync();

            if (userInterestIds.Any())
            {
                query = query.Where(e =>
                    e.EventInterests.Any(ei => userInterestIds.Contains(ei.InterestId))
                );

            
                query = query.OrderByDescending(e =>
                    e.EventInterests.Count(ei => userInterestIds.Contains(ei.InterestId))
                );
            }

            var userProfile = await _context.Users.FirstOrDefaultAsync(u => u.Id == userIdInt);
            if (!string.IsNullOrEmpty(userProfile?.City))
            {
                query = query.OrderByDescending(e => e.City == userProfile.City)
                            .ThenByDescending(e => e.Country == userProfile.Country);
            }

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

            // Sort by relevance (interest match + recency + popularity)
            query = query
                .OrderByDescending(e => e.EventInterests.Count(ei => userInterestIds.Contains(ei.InterestId)))
                .ThenByDescending(e => e.AttendeeCount)
                .ThenByDescending(e => e.StartTime);

            // Pagination
            var total = await query.CountAsync();
            var events = await query
                .Skip(filters.Offset)
                .Take(filters.Limit)
                .Include(e => e.EventInterests)
                    .ThenInclude(ei => ei.Interest)
                .Include(e => e.Organizer)
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
            _logger.LogError(ex, "GetPersonalizedEventsAsync failed for user {UserId}", userId);
            return new PagedResult<Event> { Items = new List<Event>(), Total = 0 };
        }
    }





    // GET: Trending events (by engagement metrics) + visibility awareness
    public async Task<PagedResult<Event>> GetTrendingEventsAsync(EventFilterDto filters, int? userId = null)
    {
        try
        {
            var query = _context.Events
                .Where(e => e.IsActive && e.IsPublished && e.StartTime >= DateTime.UtcNow.Date)
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









    // GET: Single event by ID + visibility check
    public async Task<Event?> GetEventByIdAsync(int eventId, int? userId = null)
    {
        try
        {
            var baseQuery = _context.Events
                .Include(e => e.EventInterests).ThenInclude(ei => ei.Interest)
                .Include(e => e.Organizer)
                .Include(e => e.Tickets)
                .Where(e => e.Id == eventId && e.IsActive && e.IsPublished)
                .AsQueryable();

            //  Apply visibility filtering
            var filtered = ApplyVisibilityFilter(baseQuery, userId);

            return await filtered.FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEventByIdAsync failed for id {EventId}", eventId);
            return null;
        }
    }

    // GET: Events by organizer
    public async Task<List<Event>> GetEventsByOrganizerAsync(int organizerId)
    {
        try
        {
            return await _context.Events
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

            // 1. Create the event
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
                Visibility = dto.Visibility ?? 0   // 0=Public, 1=GroupOnly, 2=Private
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

                // Fallback for main organizer
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

            // 3. Associate interests if provided
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























    // PUT: Update event
    public async Task<AuthResult> UpdateEventAsync(int eventId, UpdateEventDto dto, int organizerId)
    {
        try
        {
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId && e.OrganizerId == organizerId);

            if (existingEvent == null)
                return AuthResult.Failure("Event not found or you don't have permission to edit it.");

            // Update fields
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

            existingEvent.UpdatedAt = DateTime.UtcNow;

            // Update interests if provided
            if (dto.InterestIds != null)
            {
                // Remove existing associations
                var existingAssociations = _context.EventInterests
                    .Where(ei => ei.EventId == eventId);
                _context.EventInterests.RemoveRange(existingAssociations);

                // Add new associations
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

    // DELETE: Soft deleting an event
    public async Task<AuthResult> DeleteEventAsync(int eventId, int organizerId)
    {
        try
        {
            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId && e.OrganizerId == organizerId);

            if (existingEvent == null)
                return AuthResult.Failure("Event not found or you don't have permission to delete it.");

            // Soft delete: just marking  as inactive
            existingEvent.IsActive = false;
            existingEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Event soft-deleted: {EventId}", eventId);
            return AuthResult.Success("Event deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteEventAsync failed for event {EventId}", eventId);
            return AuthResult.Failure("Failed to delete event.");
        }
    }
}