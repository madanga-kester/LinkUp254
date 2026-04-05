using LinkUp254.Database;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using GroupModel = LinkUp254.Features.Groups.Models.Group;
using GroupMemberModel = LinkUp254.Features.Groups.Models.GroupMember;
using GroupEventModel = LinkUp254.Features.Groups.Models.GroupEvent;

namespace LinkUp254.Features.Groups;

public class GroupServices
{
    private readonly LinkUpContext _context;

    public GroupServices(LinkUpContext context)
    {
        _context = context;
    }

    // Get all active groups
    public async Task<List<GroupModel>> GetAllGroupsAsync(string? city = null, string? country = null)
    {
        var query = _context.Groups
            .Where(g => g.IsActive)
            .Include(g => g.Organizer)
            .AsQueryable();

        if (!string.IsNullOrEmpty(city))
            query = query.Where(g => g.City == city);
        if (!string.IsNullOrEmpty(country))
            query = query.Where(g => g.Country == country);

        return await query.OrderByDescending(g => g.MemberCount).ToListAsync();
    }

    // Get group by ID
    public async Task<GroupModel?> GetGroupByIdAsync(int id)
    {
        return await _context.Groups
            .Include(g => g.Organizer)
            .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
            .Include(g => g.GroupEvents)
                .ThenInclude(ge => ge.Event)
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);
    }

    // Create new group
    public async Task<GroupModel> CreateGroupAsync(CreateGroupDto dto, int organizerId)
    {
        var group = new GroupModel
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CoverImage = dto.CoverImage,
            OrganizerId = organizerId,
            City = dto.City?.Trim(),
            Country = dto.Country?.Trim(),
            MemberCount = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        var member = new GroupMemberModel
        {
            GroupId = group.Id,
            UserId = organizerId,
            Role = "admin",
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.GroupMembers.Add(member);
        await _context.SaveChangesAsync();

        return group;
    }

    // Join group
    public async Task<bool> JoinGroupAsync(int groupId, int userId)
    {
        var existing = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (existing != null)
        {
            existing.IsActive = true;
            await _context.SaveChangesAsync();
            return false;
        }

        var member = new GroupMemberModel
        {
            GroupId = groupId,
            UserId = userId,
            Role = "member",
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.GroupMembers.Add(member);

        var group = await _context.Groups.FindAsync(groupId);
        if (group != null)
        {
            group.MemberCount++;
            group.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // Leave group
    public async Task<bool> LeaveGroupAsync(int groupId, int userId)
    {
        var member = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

        if (member == null)
            return false;

        var group = await _context.Groups.FindAsync(groupId);
        if (group != null && group.OrganizerId == userId)
            return false;

        member.IsActive = false;
        group!.MemberCount = Math.Max(0, group.MemberCount - 1);
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // Get user's groups
    public async Task<List<GroupModel>> GetUserGroupsAsync(int userId)
    {
        return await _context.GroupMembers
            .Where(gm => gm.UserId == userId && gm.IsActive)
            .Include(gm => gm.Group)
                .ThenInclude(g => g.Organizer)
            .Select(gm => gm.Group)
            .Where(g => g.IsActive)
            .OrderByDescending(g => g.MemberCount)
            .ToListAsync();
    }

    // Delete group (organizer only)
    public async Task<bool> DeleteGroupAsync(int groupId, int userId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != userId)
            return false;

        group.IsActive = false;
        group.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}

// DTOs
public class CreateGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImage { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CoverImage { get; set; }
    public int OrganizerId { get; set; }
    public string? OrganizerName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int MemberCount { get; set; }
    public bool IsMember { get; set; }
    public bool IsOrganizer { get; set; }
    public DateTime CreatedAt { get; set; }
}