using LinkUp254.Features.Auth;
using LinkUp254.Features.GroupCoverImage.DTOs;
using LinkUp254.Features.GroupCoverImage.Models;
using System.Threading.Tasks;

namespace LinkUp254.Features.GroupCoverImage.Services;

public interface IGroupCoverImageServices
{
    Task<GroupCoverImageModel?> GetCoverImageAsync(int groupId);
    Task<AuthResult> UpdateCoverImageAsync(int groupId, int organizerId, string? imageUrl);
    Task<AuthResult> DeleteCoverImageAsync(int groupId, int organizerId);
    Task<GroupCoverImageDto?> GetCoverImageDtoAsync(int groupId);
}
