using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.GroupCoverImage.DTOs;
using LinkUp254.Features.GroupCoverImage.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace LinkUp254.Features.GroupCoverImage.Services;

public class GroupCoverImageServices : IGroupCoverImageServices
{
    private readonly LinkUpContext _context;

    public GroupCoverImageServices(LinkUpContext context)
    {
        _context = context;
    }

    public async Task<GroupCoverImageModel?> GetCoverImageAsync(int groupId)
    {
        return await _context.GroupCoverImages
            .FirstOrDefaultAsync(gci => gci.GroupId == groupId && gci.IsActive);
    }

    public async Task<GroupCoverImageDto?> GetCoverImageDtoAsync(int groupId)
    {
        var record = await GetCoverImageAsync(groupId);
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

    public async Task<AuthResult> UpdateCoverImageAsync(int groupId, int organizerId, string? imageUrl)
    {
        try
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
                return AuthResult.Failure("Group not found");

            if (group.OrganizerId != organizerId)
                return AuthResult.Failure("Only the organizer can update the cover image");

            if (string.IsNullOrEmpty(imageUrl))
                return AuthResult.Failure("Cover image URL cannot be empty");

           
            if (imageUrl.StartsWith("data:image") && imageUrl.Length > 5_000_000)
                return AuthResult.Failure("Image too large. Please use an external URL or compress the image.");

            var existing = await GetCoverImageAsync(groupId);

            if (existing != null)
            {
                existing.ImageUrl = imageUrl;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UploadedBy = organizerId;
            }
            else
            {
                var newRecord = new GroupCoverImageModel
                {
                    GroupId = groupId,
                    ImageUrl = imageUrl,
                    UploadedBy = organizerId,
                    UploadedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.GroupCoverImages.Add(newRecord);
            }

           
            group.CoverImage = imageUrl;
            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return AuthResult.Success("Cover image updated successfully");
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

    public async Task<AuthResult> DeleteCoverImageAsync(int groupId, int organizerId)
    {
        var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);
        if (group == null)
            return AuthResult.Failure("Group not found");

        if (group.OrganizerId != organizerId)
            return AuthResult.Failure("Only the organizer can delete the cover image");

        var record = await GetCoverImageAsync(groupId);
        if (record != null)
        {
            record.IsActive = false;
            record.UpdatedAt = DateTime.UtcNow;
        }

       
        group.CoverImage = null;
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return AuthResult.Success("Cover image removed successfully");
    }
}
