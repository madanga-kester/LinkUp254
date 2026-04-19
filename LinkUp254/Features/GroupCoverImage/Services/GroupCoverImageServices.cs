using System.Threading;
using System.Threading.Tasks;
using LinkUp254.Features.Auth;
using LinkUp254.Features.GroupCoverImage.DTOs;
using LinkUp254.Features.GroupCoverImage.Models;

namespace LinkUp254.Features.GroupCoverImage.Services;

public interface IGroupCoverImageServices
{
    Task<GroupCoverImageModel?> GetCoverImageAsync(int groupId, CancellationToken cancellationToken = default);
    Task<AuthResult> UpdateCoverImageAsync(int groupId, int organizerId, string? imageUrl, CancellationToken cancellationToken = default);
    Task<AuthResult> DeleteCoverImageAsync(int groupId, int organizerId, CancellationToken cancellationToken = default);
    Task<GroupCoverImageDto?> GetCoverImageDtoAsync(int groupId, CancellationToken cancellationToken = default);
}