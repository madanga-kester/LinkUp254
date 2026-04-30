using LinkUp254.Database;
using LinkUp254.Features.Events.DTOs;
using LinkUp254.Features.Events.EventDtos;
using LinkUp254.Features.Events.Models;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp254.Features.Events.Services;

public class EventEngagementServices
{
    private readonly LinkUpContext _context;
    private readonly ILogger<EventEngagementServices> _logger;

    public EventEngagementServices(
        LinkUpContext context,
        ILogger<EventEngagementServices> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Likes

    public async Task<bool> ToggleLikeAsync(int eventId, int userId, bool like)
    {
        try
        {
            var eventExists = await _context.Events
                .AsNoTracking()
                .AnyAsync(e => e.Id == eventId && e.IsActive && e.IsPublished);

            if (!eventExists)
            {
                _logger.LogWarning("ToggleLikeAsync: Event {EventId} not found or not published", eventId);
                return false;
            }

            if (like)
            {
                var existing = await _context.EventLikes
                    .AnyAsync(el => el.EventId == eventId && el.UserId == userId);

                if (!existing)
                {
                    _context.EventLikes.Add(new EventLike
                    {
                        EventId = eventId,
                        UserId = userId,
                        LikedAt = DateTime.UtcNow
                    });

                    await _context.Events
                        .Where(e => e.Id == eventId)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(e => e.LikeCount, e => e.LikeCount + 1));
                }
            }
            else
            {
                var likeEntity = await _context.EventLikes
                    .FirstOrDefaultAsync(el => el.EventId == eventId && el.UserId == userId);

                if (likeEntity != null)
                {
                    _context.EventLikes.Remove(likeEntity);

                    await _context.Events
                        .Where(e => e.Id == eventId && e.LikeCount > 0)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(e => e.LikeCount, e => e.LikeCount - 1));
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("ToggleLikeAsync: User {UserId} {Action} event {EventId}",
                userId, like ? "liked" : "unliked", eventId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ToggleLikeAsync failed for user {UserId} on event {EventId}", userId, eventId);
            return false;
        }
    }

    public async Task<bool> IsLikedByUserAsync(int eventId, int userId)
    {
        try
        {
            return await _context.EventLikes
                .AsNoTracking()
                .AnyAsync(el => el.EventId == eventId && el.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IsLikedByUserAsync failed for user {UserId} on event {EventId}", userId, eventId);
            return false;
        }
    }

    public async Task<int> GetLikeCountAsync(int eventId)
    {
        try
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e => e.Id == eventId)
                .Select(e => e.LikeCount)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetLikeCountAsync failed for event {EventId}", eventId);
            return 0;
        }
    }

    #endregion

    #region RSVPs

    public async Task<EventRsvpDto> UpsertRsvpAsync(int eventId, int userId, RsvpRequest request)
    {
        try
        {
            var eventEntity = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive && e.IsPublished);

            if (eventEntity == null)
            {
                _logger.LogWarning("UpsertRsvpAsync: Event {EventId} not found or not published", eventId);
                throw new InvalidOperationException("Event not found or not available for RSVP");
            }

            if (eventEntity.MaxAttendees.HasValue)
            {
                var currentRsvps = await _context.EventRsvps
                    .AsNoTracking()
                    .CountAsync(r => r.EventId == eventId && r.Status == "going");

                if (currentRsvps >= eventEntity.MaxAttendees.Value && request.Status == "going")
                {
                    throw new InvalidOperationException("Event is full");
                }
            }

            var rsvp = await _context.EventRsvps
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            bool isNew = rsvp == null;
            if (isNew)
            {
                rsvp = new EventRsvp
                {
                    EventId = eventId,
                    UserId = userId,
                    Status = request.Status,
                    GuestCount = request.GuestCount ?? 1,
                    TicketTierId = request.TicketTierId,
                    RsvpedAt = DateTime.UtcNow
                };
                _context.EventRsvps.Add(rsvp);
            }
            else
            {
                if (rsvp.Status != request.Status)
                {
                    if (rsvp.Status == "going" && request.Status != "going")
                    {
                        await DecrementAttendeeCountAsync(eventId);
                    }
                    else if (rsvp.Status != "going" && request.Status == "going")
                    {
                        await IncrementAttendeeCountAsync(eventId);
                    }
                }

                rsvp.Status = request.Status;
                rsvp.GuestCount = request.GuestCount ?? rsvp.GuestCount;
                rsvp.TicketTierId = request.TicketTierId;
                rsvp.RsvpedAt = DateTime.UtcNow;
            }

            if (isNew && request.Status == "going")
            {
                await IncrementAttendeeCountAsync(eventId);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("UpsertRsvpAsync: User {UserId} {Action} RSVP for event {EventId} (status: {Status})",
                userId, isNew ? "created" : "updated", eventId, request.Status);

            return new EventRsvpDto
            {
                EventId = eventId,
                Status = rsvp.Status,
                RsvpedAt = rsvp.RsvpedAt,
                GuestCount = rsvp.GuestCount,
                TicketTierId = rsvp.TicketTierId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpsertRsvpAsync failed for user {UserId} on event {EventId}", userId, eventId);
            throw;
        }
    }

    public async Task<bool> CancelRsvpAsync(int eventId, int userId)
    {
        try
        {
            var rsvp = await _context.EventRsvps
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (rsvp == null)
            {
                _logger.LogWarning("CancelRsvpAsync: No RSVP found for user {UserId} on event {EventId}", userId, eventId);
                return true;
            }

            if (rsvp.Status == "going")
            {
                await DecrementAttendeeCountAsync(eventId);
            }

            _context.EventRsvps.Remove(rsvp);
            await _context.SaveChangesAsync();

            _logger.LogInformation("CancelRsvpAsync: User {UserId} cancelled RSVP for event {EventId}", userId, eventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CancelRsvpAsync failed for user {UserId} on event {EventId}", userId, eventId);
            return false;
        }
    }

    public async Task<EventRsvpDto?> GetUserRsvpAsync(int eventId, int userId)
    {
        try
        {
            var rsvp = await _context.EventRsvps
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (rsvp == null) return null;

            return new EventRsvpDto
            {
                EventId = rsvp.EventId,
                Status = rsvp.Status,
                RsvpedAt = rsvp.RsvpedAt,
                GuestCount = rsvp.GuestCount,
                TicketTierId = rsvp.TicketTierId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserRsvpAsync failed for user {UserId} on event {EventId}", userId, eventId);
            return null;
        }
    }

    public async Task<int> GetRsvpCountAsync(int eventId, string status = "going")
    {
        try
        {
            return await _context.EventRsvps
                .AsNoTracking()
                .CountAsync(r => r.EventId == eventId && r.Status == status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRsvpCountAsync failed for event {EventId}", eventId);
            return 0;
        }
    }

    private async Task IncrementAttendeeCountAsync(int eventId)
    {
        await _context.Events
            .Where(e => e.Id == eventId)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(e => e.AttendeeCount, e => e.AttendeeCount + 1));
    }

    private async Task DecrementAttendeeCountAsync(int eventId)
    {
        await _context.Events
            .Where(e => e.Id == eventId && e.AttendeeCount > 0)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(e => e.AttendeeCount, e => e.AttendeeCount - 1));
    }

    #endregion

    #region Social Context

    public async Task<SocialContextDto> GetSocialContextAsync(int eventId, int userId)
    {
        try
        {
            var rsvpCount = await GetRsvpCountAsync(eventId, "going");

            string networkInterest = rsvpCount switch
            {
                >= 100 => "high",
                >= 25 => "medium",
                _ => "low"
            };

            int? liveViewers = rsvpCount > 50 ? Random.Shared.Next(5, 25) : null;

            return new SocialContextDto
            {
                FriendsGoing = 0,
                NetworkInterest = networkInterest,
                LiveViewers = liveViewers,
                MutualFriends = new List<UserSummaryDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSocialContextAsync failed for user {UserId} on event {EventId}", userId, eventId);
            return new SocialContextDto
            {
                FriendsGoing = 0,
                NetworkInterest = "low",
                LiveViewers = null,
                MutualFriends = new List<UserSummaryDto>()
            };
        }
    }

    #endregion

    #region View Tracking

    public async Task IncrementViewCountAsync(int eventId)
    {
        try
        {
            await _context.Events
                .Where(e => e.Id == eventId && e.IsActive)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(e => e.ViewCount, e => e.ViewCount + 1));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IncrementViewCountAsync failed for event {EventId}", eventId);
        }
    }

    #endregion






 
    public async Task<AttendeeAvatarsResponse> GetAttendeeAvatarsAsync(List<int> eventIds)
    {
        try
        {
            if (!eventIds.Any())
                return new AttendeeAvatarsResponse();

            // Fetch RSVPs for these events + join with Users to get avatars
            var avatarData = await _context.EventRsvps
                .AsNoTracking()
                .Where(r => eventIds.Contains(r.EventId)
                         && r.Status == "going"
                         && !string.IsNullOrEmpty(r.User.ProfilePicture))
                .Select(r => new
                {
                    r.EventId,
                    AvatarUrl = r.User.ProfilePicture!
                })
                .ToListAsync();

            // Group by EventId, limit to 4 unique avatars per event
            var result = new AttendeeAvatarsResponse();

            foreach (var eventId in eventIds)
            {
                var avatars = avatarData
                    .Where(a => a.EventId == eventId)
                    .Select(a => a.AvatarUrl)
                    .Distinct()
                    .Take(4)
                    .ToList();

                if (avatars.Any())
                {
                    result.Avatars[eventId] = avatars;
                }
            }

            
            var counts = await _context.EventRsvps
                .AsNoTracking()
                .Where(r => eventIds.Contains(r.EventId) && r.Status == "going")
                .GroupBy(r => r.EventId)
                .Select(g => new { EventId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var c in counts)
            {
                result.AttendeeCounts[c.EventId] = c.Count;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAttendeeAvatarsAsync failed for events {EventIds}", string.Join(",", eventIds));
            return new AttendeeAvatarsResponse(); 
        }
    }
}