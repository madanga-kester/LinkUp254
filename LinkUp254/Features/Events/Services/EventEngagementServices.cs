// src/Features/Events/Services/EventEngagementServices.cs
using LinkUp254.Database;
using LinkUp254.Features.Events.DTOs;
using LinkUp254.Features.Events.Models;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp254.Features.Events.Services;

/// <summary>
/// Handles user engagement actions: likes, RSVPs, social context, view tracking.
/// Separated from EventServices for maintainability and single responsibility.
/// </summary>
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

    #region 🔹 Likes

    /// <summary>
    /// Toggle like status for an event by a user.
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <param name="userId">The user ID</param>
    /// <param name="like">True to like, false to unlike</param>
    /// <returns>True if operation succeeded</returns>
    public async Task<bool> ToggleLikeAsync(int eventId, int userId, bool like)
    {
        try
        {
            // Verify event exists and is active
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
                // Add like if not already present
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

                    // Denormalize: increment like count for faster reads
                    await _context.Events
                        .Where(e => e.Id == eventId)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(e => e.LikeCount, e => e.LikeCount + 1));
                }
            }
            else
            {
                // Remove like if present
                var likeEntity = await _context.EventLikes
                    .FirstOrDefaultAsync(el => el.EventId == eventId && el.UserId == userId);

                if (likeEntity != null)
                {
                    _context.EventLikes.Remove(likeEntity);

                    // Denormalize: decrement like count (ensure non-negative)
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

    /// <summary>
    /// Check if a user has liked an event.
    /// </summary>
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

    /// <summary>
    /// Get total like count for an event (uses denormalized count for performance).
    /// </summary>
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

    #region 🔹 RSVPs

    /// <summary>
    /// Create or update an RSVP for an event.
    /// </summary>
    public async Task<EventRsvpDto> UpsertRsvpAsync(int eventId, int userId, RsvpRequest request)
    {
        try
        {
            // Verify event exists, is active, and not full
            var eventEntity = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId && e.IsActive && e.IsPublished);

            if (eventEntity == null)
            {
                _logger.LogWarning("UpsertRsvpAsync: Event {EventId} not found or not published", eventId);
                throw new InvalidOperationException("Event not found or not available for RSVP");
            }

            // Check capacity if event has max attendees
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

            // Find existing RSVP or create new
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
                // Update existing
                if (rsvp.Status != request.Status)
                {
                    // If changing from "going" to something else, decrement attendee count
                    if (rsvp.Status == "going" && request.Status != "going")
                    {
                        await DecrementAttendeeCountAsync(eventId);
                    }
                    // If changing to "going" from something else, increment
                    else if (rsvp.Status != "going" && request.Status == "going")
                    {
                        await IncrementAttendeeCountAsync(eventId);
                    }
                }

                rsvp.Status = request.Status;
                rsvp.GuestCount = request.GuestCount ?? rsvp.GuestCount;
                rsvp.TicketTierId = request.TicketTierId;
                rsvp.RsvpedAt = DateTime.UtcNow; // Update timestamp
            }

            // If status is "going" and it's a new RSVP, increment attendee count
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
            throw; // Re-throw to let controller handle error response
        }
    }

    /// <summary>
    /// Cancel an RSVP (set status to "none" or delete).
    /// </summary>
    public async Task<bool> CancelRsvpAsync(int eventId, int userId)
    {
        try
        {
            var rsvp = await _context.EventRsvps
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (rsvp == null)
            {
                _logger.LogWarning("CancelRsvpAsync: No RSVP found for user {UserId} on event {EventId}", userId, eventId);
                return true; // Already cancelled, consider success
            }

            // If was "going", decrement attendee count
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

    /// <summary>
    /// Get a user's RSVP status for an event.
    /// </summary>
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

    /// <summary>
    /// Get RSVP count for an event (optionally filtered by status).
    /// </summary>
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

    // Helper: Increment attendee count (denormalized)
    private async Task IncrementAttendeeCountAsync(int eventId)
    {
        await _context.Events
            .Where(e => e.Id == eventId)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(e => e.AttendeeCount, e => e.AttendeeCount + 1));
    }

    // Helper: Decrement attendee count (denormalized)
    private async Task DecrementAttendeeCountAsync(int eventId)
    {
        await _context.Events
            .Where(e => e.Id == eventId && e.AttendeeCount > 0)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(e => e.AttendeeCount, e => e.AttendeeCount - 1));
    }

    #endregion

    #region 🔹 Social Context (Mocked — No Friendship Data Yet)

    /// <summary>
    /// Get social context for an event (friends going, network interest).
    /// NOTE: Currently mocked since friendship data not yet implemented.
    /// </summary>
    public async Task<SocialContextDto> GetSocialContextAsync(int eventId, int userId)
    {
        try
        {
            // 🔹 MOCKED: Since no friendship/connection model exists yet
            // In production, this would query:
            // 1. User's friends/connections who RSVP'd "going"
            // 2. Network interest based on friend activity + event popularity
            // 3. Real-time viewers (if using WebSockets/Redis)

            // For now, return sensible defaults:
            var rsvpCount = await GetRsvpCountAsync(eventId, "going");

            // Mock network interest based on total RSVPs (simple heuristic)
            string networkInterest = rsvpCount switch
            {
                >= 100 => "high",
                >= 25 => "medium",
                _ => "low"
            };

            // Mock live viewers (could be cached count from last 5 min)
            int? liveViewers = rsvpCount > 50 ? Random.Shared.Next(5, 25) : null;

            return new SocialContextDto
            {
                FriendsGoing = 0, // 🔹 Mocked: Would be count of mutual friends RSVP'd
                NetworkInterest = networkInterest,
                LiveViewers = liveViewers,
                MutualFriends = new List<UserSummaryDto>() // 🔹 Mocked: Would be list of friend profiles
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetSocialContextAsync failed for user {UserId} on event {EventId}", userId, eventId);
            // Return safe defaults on error
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

    #region 🔹 View Tracking

    /// <summary>
    /// Increment view count for an event (called when event detail page is loaded).
    /// Uses ExecuteUpdate for performance (no entity tracking overhead).
    /// </summary>
    public async Task IncrementViewCountAsync(int eventId)
    {
        try
        {
            // Simple increment with concurrency safety (SQL Server handles atomic update)
            await _context.Events
                .Where(e => e.Id == eventId && e.IsActive)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(e => e.ViewCount, e => e.ViewCount + 1));
        }
        catch (Exception ex)
        {
            // Log but don't throw — view tracking is non-critical
            _logger.LogError(ex, "IncrementViewCountAsync failed for event {EventId}", eventId);
        }
    }

    #endregion
}