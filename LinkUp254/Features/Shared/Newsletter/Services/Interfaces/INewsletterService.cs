using LinkUp254.Features.Shared.Newsletter.Models.DTOs;

namespace LinkUp254.Features.Shared.Newsletter.Services.Interfaces;

public interface INewsletterService
{
    Task<bool> IsUserSubscribedAsync(int userId);
    Task<SubscribeResponse> SubscribeAsync(int userId, string email);
    Task<bool> IsEmailSubscribedAsync(string email);
    Task<SubscribeResponse> SubscribeByEmailAsync(string email);
}