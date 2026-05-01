using LinkUp254.Database;
using LinkUp254.Features.Shared.Newsletter.Models;
using LinkUp254.Features.Shared.Newsletter.Models.DTOs;
using LinkUp254.Features.Shared.Newsletter.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LinkUp254.Features.Shared.Newsletter.Services;

public class NewsletterService : INewsletterService
{
    private readonly LinkUpContext _db;

    public NewsletterService(LinkUpContext db)
    {
        _db = db;
    }

    public async Task<bool> IsUserSubscribedAsync(int userId)
    {
        return await _db.NewsletterSubscriptions
            .AnyAsync(s => s.UserId == userId && s.IsConfirmed);
    }

    public async Task<SubscribeResponse> SubscribeAsync(int userId, string email)
    {
        var existing = await _db.NewsletterSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (existing != null)
        {
            return new SubscribeResponse
            {
                Success = true,
                Message = existing.IsConfirmed ? "Already subscribed" : "Pending confirmation"
            };
        }

        _db.NewsletterSubscriptions.Add(new NewsletterSubscription
        {
            UserId = userId,
            Email = email,
            IsConfirmed = true,
            SubscribedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return new SubscribeResponse { Success = true, Message = "Subscribed successfully" };
    }

    public async Task<bool> IsEmailSubscribedAsync(string email)
    {
        return await _db.NewsletterSubscriptions
            .AnyAsync(s => s.Email == email && s.IsConfirmed);
    }

    public async Task<SubscribeResponse> SubscribeByEmailAsync(string email)
    {
        var existing = await _db.NewsletterSubscriptions
            .FirstOrDefaultAsync(s => s.Email == email);

        if (existing != null)
        {
            return new SubscribeResponse
            {
                Success = true,
                Message = existing.IsConfirmed ? "Already subscribed" : "Pending confirmation"
            };
        }

        _db.NewsletterSubscriptions.Add(new NewsletterSubscription
        {
            UserId = null,
            Email = email,
            IsConfirmed = true,
            SubscribedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return new SubscribeResponse { Success = true, Message = "Subscribed successfully" };
    }
}