using System.Threading.Tasks;
using LinkUp254.Features.Organizers.DTOs;

namespace LinkUp254.Features.Organizers.Services;

public interface IOrganizerService
{
    Task<OrganizerDto> GetOrganizerAsync(int organizerId);
    Task<bool> FollowOrganizerAsync(int followerId, int organizerId);
    Task<bool> UnfollowOrganizerAsync(int followerId, int organizerId);
    Task<bool> RateOrganizerAsync(int userId, int organizerId, int rating, string? comment);
    Task<bool> ContactOrganizerAsync(int userId, int organizerId, string message);
}