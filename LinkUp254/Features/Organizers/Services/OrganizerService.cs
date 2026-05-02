using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkUp254.Database;
using LinkUp254.Features.Organizers.DTOs;
using LinkUp254.Features.Organizers.Models;
using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Organizers.Services;

public class OrganizerService : IOrganizerService
{
    private readonly LinkUpContext _context;
    private readonly ILogger<OrganizerService> _logger;

    public OrganizerService(LinkUpContext context, ILogger<OrganizerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OrganizerDto> GetOrganizerAsync(int organizerId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == organizerId)
            ?? throw new KeyNotFoundException("Organizer not found.");

        var followerCount = await _context.Follows.CountAsync(f => f.OrganizerId == organizerId);

        var eventCount = await _context.Events.CountAsync(e => e.OrganizerId == organizerId);

        var avgRating = await _context.OrganizerRatings
            .Where(r => r.OrganizerId == organizerId)
            .AverageAsync(r => (double?)r.Rating) ?? 0.0;

        return new OrganizerDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfilePicture = user.ProfilePicture,
            JoinedYear = user.CreatedAt.Year,
            FollowerCount = followerCount,
            EventCount = eventCount,
            AverageRating = Math.Round(avgRating, 1),
            IsVerified = user.Role == "Organizer" || user.Role == "Admin"
        };
    }

    public async Task<bool> FollowOrganizerAsync(int followerId, int organizerId)
    {
        if (followerId == organizerId)
            throw new InvalidOperationException("Cannot follow yourself.");

        var exists = await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.OrganizerId == organizerId);
        if (exists) return false;

        await _context.Follows.AddAsync(new LinkUp254.Features.Organizers.Models.Follow { FollowerId = followerId, OrganizerId = organizerId });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnfollowOrganizerAsync(int followerId, int organizerId)
    {
        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.OrganizerId == organizerId);

        if (follow == null) return false;

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RateOrganizerAsync(int userId, int organizerId, int rating, string? comment)
    {
        var existing = await _context.OrganizerRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.OrganizerId == organizerId);

        if (existing != null)
        {
            existing.Rating = rating;
            existing.Comment = comment;
            _context.OrganizerRatings.Update(existing);
        }
        else
        {
            await _context.OrganizerRatings.AddAsync(new OrganizerRating
            {
                UserId = userId,
                OrganizerId = organizerId,
                Rating = rating,
                Comment = comment
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ContactOrganizerAsync(int userId, int organizerId, string message)
    {
        _logger.LogInformation("User {UserId} contacted organizer {OrganizerId}. Message: {Message}", userId, organizerId, message);
        await Task.CompletedTask;
        return true;
    }
}