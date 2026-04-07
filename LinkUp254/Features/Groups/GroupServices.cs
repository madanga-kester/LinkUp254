using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.Groups.Models;
using LinkUp254.Features.Shared;
using LinkUp254.Features.Groups.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroupEventModel = LinkUp254.Features.Groups.Models.GroupEvent;
using GroupMemberModel = LinkUp254.Features.Groups.Models.GroupMember;
using GroupModel = LinkUp254.Features.Groups.Models.Group;

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












    // Send a message in group chat
    public async Task<AuthResult> SendMessageAsync(int groupId, int userId, string content)
    {
        var group = await _context.Groups
            .Include(g => g.Settings)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

        if (group == null)
            return AuthResult.Failure("Group not found.");

        // Check membership
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

        if (!isMember)
            return AuthResult.Failure("You must be a member to send messages.");

        // Check if member posting is allowed
        if (!group.Settings?.AllowMemberPosts ?? false)
        {
            var isOrganizer = group.OrganizerId == userId;
            if (!isOrganizer)
                return AuthResult.Failure("Posting is disabled for members in this group.");
        }

        // Get or create group chat
        var chat = await _context.GroupChats
            .FirstOrDefaultAsync(c => c.GroupId == groupId && c.IsActive);

        if (chat == null)
        {
            chat = new GroupChat { GroupId = groupId };
            _context.GroupChats.Add(chat);
            await _context.SaveChangesAsync();
        }

        // Create message
        var message = new GroupMessage
        {
            GroupChatId = chat.Id,
            SenderId = userId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow
        };

        _context.GroupMessages.Add(message);
        await _context.SaveChangesAsync();

        return AuthResult.Success("Message sent.");
    }

    // Get recent messages for a group
    public async Task<List<GroupMessage>> GetGroupMessagesAsync(int groupId, int userId, int limit = 50)
    {
        // Verify membership
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

        if (!isMember)
            return new List<GroupMessage>();

        return await _context.GroupMessages
            .Where(m => m.GroupChat.GroupId == groupId && !m.IsDeleted)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.SentAt)
            .Take(limit)
            .ToListAsync();
    }



    // GET: Group settings by ID
    public async Task<GroupSettings?> GetSettingsAsync(int groupId)
    {
        return await _context.GroupSettings.FindAsync(groupId);
    }


    // GROUP SETTINGS 

    // Update group settings (organizer only)
    public async Task<AuthResult> UpdateSettingsAsync(int groupId, int organizerId, UpdateGroupSettingsDto dto)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != organizerId)
            return AuthResult.Failure("Group not found or permission denied.");

        var settings = await _context.GroupSettings.FindAsync(groupId);
        if (settings == null)
        {
            settings = new GroupSettings { GroupId = groupId };
            _context.GroupSettings.Add(settings);
        }

        // Update settings
        if (dto.IsPrivate.HasValue) settings.IsPrivate = dto.IsPrivate.Value;
        if (dto.AllowMemberInvites.HasValue) settings.AllowMemberInvites = dto.AllowMemberInvites.Value;
        if (dto.AllowMemberPosts.HasValue) settings.AllowMemberPosts = dto.AllowMemberPosts.Value;
        if (dto.ModerateMessages.HasValue) settings.ModerateMessages = dto.ModerateMessages.Value;
        if (dto.AllowLinks.HasValue) settings.AllowLinks = dto.AllowLinks.Value;
        if (dto.AllowMedia.HasValue) settings.AllowMedia = dto.AllowMedia.Value;
        if (dto.NotifyOnNewEvent.HasValue) settings.NotifyOnNewEvent = dto.NotifyOnNewEvent.Value;
        if (dto.NotifyOnNewMember.HasValue) settings.NotifyOnNewMember = dto.NotifyOnNewMember.Value;

        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return AuthResult.Success("Settings updated.");
    }

    //  GROUP RULES 

    // Adding a new rule
    public async Task<AuthResult> AddRuleAsync(int groupId, int organizerId, CreateGroupRuleDto dto)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != organizerId)
            return AuthResult.Failure("Group not found or permission denied.");

        // Get max order for this group
        var maxOrder = await _context.GroupRules
            .Where(gr => gr.GroupId == groupId)
            .MaxAsync(gr => (int?)gr.Order) ?? 0;

        var rule = new GroupRule
        {
            GroupId = groupId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Order = dto.Order ?? (maxOrder + 1),
            IsActive = true
        };

        _context.GroupRules.Add(rule);
        await _context.SaveChangesAsync();

        return AuthResult.Success("Rule added.");
    }

    // Get all active rules for a group
    public async Task<List<GroupRule>> GetGroupRulesAsync(int groupId)
    {
        return await _context.GroupRules
            .Where(gr => gr.GroupId == groupId && gr.IsActive)
            .OrderBy(gr => gr.Order)
            .ToListAsync();
    }

    //  MEMBER REQUESTS

    // Request to join a private group
    public async Task<AuthResult> RequestJoinAsync(int groupId, int userId, string? message)
    {
        var group = await _context.Groups
            .Include(g => g.Settings)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

        if (group == null)
            return AuthResult.Failure("Group not found.");

        // Check if already a member
        var existingMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

        if (existingMember)
            return AuthResult.Failure("You are already a member of this group.");

        // Check if already has a pending request
        var existingRequest = await _context.GroupJoinRequests
            .AnyAsync(r => r.GroupId == groupId && r.UserId == userId && r.Status == "pending");

        if (existingRequest)
            return AuthResult.Failure("You already have a pending request to join this group.");

        // If group is public, auto-approve
        if (!(group.Settings?.IsPrivate ?? false))
        {
            // Auto-add as member
            var member = new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                Role = "member",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.GroupMembers.Add(member);

            group.MemberCount++;
            group.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return AuthResult.Success("Joined group successfully.");
        }

        // Create join request for private group
        var request = new GroupJoinRequest
        {
            GroupId = groupId,
            UserId = userId,
            Message = message?.Trim(),
            Status = "pending",
            RequestedAt = DateTime.UtcNow
        };

        _context.GroupJoinRequests.Add(request);
        await _context.SaveChangesAsync();

        return AuthResult.Success("Join request submitted. Waiting for organizer approval.");
    }

    // Get pending join requests (organizers only)
    public async Task<List<GroupJoinRequest>> GetPendingJoinRequestsAsync(int groupId, int organizerId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != organizerId)
            return new List<GroupJoinRequest>();

        return await _context.GroupJoinRequests
            .Where(r => r.GroupId == groupId && r.Status == "pending")
            .Include(r => r.User)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();
    }

    // Review a join request (approve/reject)
    public async Task<AuthResult> ReviewJoinRequestAsync(int requestId, int organizerId, bool approve, string? notes)
    {
        var request = await _context.GroupJoinRequests
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null || request.Group.OrganizerId != organizerId)
            return AuthResult.Failure("Request not found or permission denied.");

        if (request.Status != "pending")
            return AuthResult.Failure("Request has already been reviewed.");

        request.Status = approve ? "approved" : "rejected";
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = organizerId;
        request.ReviewNotes = notes?.Trim();

        if (approve)
        {
            // Add as member
            var member = new GroupMember
            {
                GroupId = request.GroupId,
                UserId = request.UserId,
                Role = "member",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.GroupMembers.Add(member);

            request.Group.MemberCount++;
            request.Group.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return AuthResult.Success(approve ? "Member approved and added." : "Request rejected.");
    }

    // MEMBER MANAGEMENT 

    // Remove a member from group (organizer only)
    public async Task<AuthResult> RemoveMemberAsync(int groupId, int organizerId, int targetUserId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != organizerId)
            return AuthResult.Failure("Group not found or permission denied.");

        // Cannot remove organizer
        if (targetUserId == organizerId)
            return AuthResult.Failure("Cannot remove the group organizer.");

        var member = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == targetUserId && gm.IsActive);

        if (member == null)
            return AuthResult.Failure("User is not a member of this group.");

        member.IsActive = false;
        group.MemberCount = Math.Max(0, group.MemberCount - 1);
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return AuthResult.Success("Member removed from group.");
    }

    // Promote/demote member role
    public async Task<AuthResult> UpdateMemberRoleAsync(int groupId, int organizerId, int targetUserId, string newRole)
    {
        var validRoles = new[] { "member", "moderator", "admin" };
        if (!validRoles.Contains(newRole.ToLower()))
            return AuthResult.Failure("Invalid role specified.");

        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != organizerId)
            return AuthResult.Failure("Group not found or permission denied.");

        var member = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == targetUserId && gm.IsActive);

        if (member == null)
            return AuthResult.Failure("User is not a member of this group.");

        member.Role = newRole.ToLower();
        await _context.SaveChangesAsync();

        return AuthResult.Success($"Member role updated to {newRole}.");
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