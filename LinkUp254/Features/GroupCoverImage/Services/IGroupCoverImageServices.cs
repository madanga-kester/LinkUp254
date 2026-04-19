using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.GroupCoverImage.DTOs;
using LinkUp254.Features.GroupCoverImage.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkUp254.Features.GroupCoverImage.Services;

public class GroupCoverImageServices : IGroupCoverImageServices
{
    private readonly LinkUpContext _context;
    private readonly ILogger<GroupCoverImageServices> _logger;
    private const int MaxBase64ImageSize = 5_000_000;

    public GroupCoverImageServices(LinkUpContext context, ILogger<GroupCoverImageServices> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GroupCoverImageModel?> GetCoverImageAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await _context.GroupCoverImages
            .AsNoTracking()
            .FirstOrDefaultAsync(gci => gci.GroupId == groupId && gci.IsActive, cancellationToken);
    }

    public async Task<GroupCoverImageDto?> GetCoverImageDtoAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var record = await GetCoverImageAsync(groupId, cancellationToken);
        if (record == null) return null;

        return new GroupCoverImageDto
        {
            Id = record.Id,
            GroupId = record.GroupId,
            ImageUrl = record.ImageUrl,
            ThumbnailUrl = record.ThumbnailUrl,
            UploadedBy = record.UploadedBy,
            UploadedAt = record.UploadedAt,
            IsActive = record.IsActive
        };
    }

    public async Task<AuthResult> UpdateCoverImageAsync(int groupId, int organizerId, string? imageUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

            if (group == null)
                return AuthResult.Failure("Group not found");

            if (group.OrganizerId != organizerId)
                return AuthResult.Failure("Only the organizer can update the cover image");

            if (string.IsNullOrEmpty(imageUrl))
                return AuthResult.Failure("Cover image URL cannot be empty");

            if (imageUrl.StartsWith("data:image", StringComparison.OrdinalIgnoreCase) && imageUrl.Length > MaxBase64ImageSize)
                return AuthResult.Failure("Image too large. Please use an external URL or compress the image.");

            var existing = await GetCoverImageAsync(groupId, cancellationToken);
            var now = DateTime.UtcNow;

            if (existing != null)
            {
                existing.ImageUrl = imageUrl;
                existing.UpdatedAt = now;
                existing.UploadedBy = organizerId;
                _context.GroupCoverImages.Update(existing);
            }
            else
            {
                var newRecord = new GroupCoverImageModel
                {
                    GroupId = groupId,
                    ImageUrl = imageUrl,
                    UploadedBy = organizerId,
                    UploadedAt = now,
                    IsActive = true
                };
                await _context.GroupCoverImages.AddAsync(newRecord, cancellationToken);
            }

            group.CoverImage = imageUrl;
            group.UpdatedAt = now;
            _context.Groups.Update(group);

            await _context.SaveChangesAsync(cancellationToken);
            return AuthResult.Success("Cover image updated successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return AuthResult.Failure("The record was modified by another user. Please refresh and try again.");
        }
        catch (DbUpdateException ex)
        {
            return AuthResult.Failure($"Database error: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            return AuthResult.Failure($"Unexpected error: {ex.Message}");
        }
    }

    public async Task<AuthResult> DeleteCoverImageAsync(int groupId, int organizerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

            if (group == null)
                return AuthResult.Failure("Group not found");

            if (group.OrganizerId != organizerId)
                return AuthResult.Failure("Only the organizer can delete the cover image");

            var record = await GetCoverImageAsync(groupId, cancellationToken);
            var now = DateTime.UtcNow;

            if (record != null)
            {
                record.IsActive = false;
                record.UpdatedAt = now;
                _context.GroupCoverImages.Update(record);
            }

            group.CoverImage = null;
            group.UpdatedAt = now;
            _context.Groups.Update(group);

            await _context.SaveChangesAsync(cancellationToken);
            return AuthResult.Success("Cover image removed successfully");
        }
        catch (DbUpdateConcurrencyException)
        {
            return AuthResult.Failure("The record was modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            return AuthResult.Failure($"Unexpected error: {ex.Message}");
        }
    }
}