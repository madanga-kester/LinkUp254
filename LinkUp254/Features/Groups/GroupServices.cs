

using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.Groups.DTOs;
using LinkUp254.Features.Groups.Models;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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






    public async Task<GroupModel?> GetGroupByIdAsync(int id, int? currentUserId = null)
    {
        // Fetch group with proper eager loading
        var group = await _context.Groups
            .Include(g => g.Organizer)
            .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.User)
            .Include(g => g.GroupEvents)
                .ThenInclude(ge => ge.Event!)      
            .Include(g => g.Settings)
            .AsSplitQuery()                        
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

        if (group == null)
            return null;





        //  DEBUG 
        var loadedCount = group.GroupEvents?.Count ?? 0;
        var withEventData = group.GroupEvents?.Count(ge => ge.Event != null) ?? 0;
        Console.WriteLine($" Group {id}: Loaded {loadedCount} GroupEvents, {withEventData} have Event data loaded");

        
        if (currentUserId == null)
        {
            group.GroupEvents = group.GroupEvents?
                .Where(ge => ge.Event?.Visibility == 0)
                .ToList();
            return group;
        }

        // Check user permissions
        var isMember = group.GroupMembers?.Any(gm => gm.UserId == currentUserId && gm.IsActive) == true;
        var isOrganizer = group.OrganizerId == currentUserId;


        

        // Filter events according to visibility rules (with safe null handling)
        group.GroupEvents = group.GroupEvents?
            .Where(ge =>
            {
                
                if (ge.Event == null || !ge.Event.IsActive)  
                {
                    return false;
                }

                return ge.Event.Visibility == 0 ||                           // Public
                       (ge.Event.Visibility == 1 && (isMember || isOrganizer)) || // Group Only
                       (ge.Event.Visibility == 2 && isOrganizer);            // Private
            })
            .OrderByDescending(ge => ge.CreatedAt)
            .ToList();



        return group;

        
    }













    public async Task<GroupModel> CreateGroupAsync(CreateGroupDto dto, int organizerId)
    {
        
        var group = new GroupModel
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CoverImage = dto.CoverImage?.Trim() ?? dto.CoverImageBase64?.Trim(),
            OrganizerId = organizerId,
            City = dto.City?.Trim(),
            Country = dto.Country?.Trim(),
            Location = dto.Location?.Trim(),
            IsPrivate = dto.IsPrivate,                    
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

        
        var settings = new GroupSettings
        {
            GroupId = group.Id,
            IsPrivate = dto.IsPrivate,
            AllowMemberInvites = dto.AllowMemberInvites,
            AllowMemberPosts = dto.AllowMemberPosts,
            ModerateMessages = dto.ModerateMessages,
            AllowLinks = dto.AllowLinks,
            AllowMedia = dto.AllowMedia,
            NotifyOnNewEvent = dto.NotifyOnNewEvent,
            NotifyOnNewMember = dto.NotifyOnNewMember,
            UpdatedAt = DateTime.UtcNow
        };
        _context.GroupSettings.Add(settings);

        
        if (dto.Rules?.Any() == true)
        {
            foreach (var ruleDto in dto.Rules)
            {
                var rule = new GroupRule
                {
                    GroupId = group.Id,
                    Title = ruleDto.Title.Trim(),
                    Description = ruleDto.Description?.Trim(),
                    Order = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.GroupRules.Add(rule);
            }
        }

        if (dto.InterestTags?.Any() == true)
        {
            foreach (var tagName in dto.InterestTags)
            {
                if (string.IsNullOrWhiteSpace(tagName)) continue;

                var interest = await _context.Interests
                    .FirstOrDefaultAsync(i => i.Name.ToLower() == tagName.ToLower().Trim());

                if (interest == null)
                {
                    interest = new Interest
                    {
                        Name = tagName.Trim(),
                        Category = "Custom",
                        Icon = "tag",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Interests.Add(interest);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Final save for settings, member, rules
        await _context.SaveChangesAsync();

        return group;
    }













    //joining a group


    public async Task<(bool IsSuccess, string Message, bool IsPending)> JoinGroupAsync(int groupId, int userId)
    {
        // 1. Get group
        var group = await _context.Groups
            .Include(g => g.Settings)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

        if (group == null)
            return (false, "Group not found", false);

        // 2. Check if  a user is already a member
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

        if (isMember)
            return (false, "You are already a member of this group", false);

        // 3. Determine Privacy
        bool isPrivate = group.IsPrivate || (group.Settings?.IsPrivate ?? false);

        if (!isPrivate)
        {

            //  PUBLIC GROUP: Join directly
            var member = new GroupMemberModel
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
            return (true, "Joined group successfully!", false);
        }
        else
        {
            //  PRIVATE GROUP: Handle requests
            var existingRequest = await _context.GroupJoinRequests
                .FirstOrDefaultAsync(r => r.GroupId == groupId && r.UserId == userId);

            if (existingRequest != null)
            {
                // if Rejected ,Allow Re-apply
                if (existingRequest.Status == "rejected")
                {
                    existingRequest.Status = "pending";
                    existingRequest.RequestedAt = DateTime.UtcNow;
                    existingRequest.ReviewNotes = null;
                    await _context.SaveChangesAsync();
                    return (true, "Join request sent!", true);
                }
                // if Pending  Already pending
                if (existingRequest.Status == "pending")
                {
                    return (true, "Request is already pending.", true);
                }
                // if Approved = Should be member, but just in case
                if (existingRequest.Status == "approved")
                {
                    var member = new GroupMemberModel
                    {
                        GroupId = groupId,
                        UserId = userId,
                        Role = "member",
                        JoinedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.GroupMembers.Add(member);
                    group.MemberCount++;
                    await _context.SaveChangesAsync();
                    return (true, "Joined group successfully!", false);
                }
            }

            // if its iS A New Request
            var newRequest = new GroupJoinRequest
            {
                GroupId = groupId,
                UserId = userId,
                Status = "pending",
                Message = null,
                RequestedAt = DateTime.UtcNow
            };

            _context.GroupJoinRequests.Add(newRequest);
            await _context.SaveChangesAsync();
            return (true, "Join request sent!", true);
        }
    }



    // Getting exact join status for the frontend
    public async Task<string> GetJoinRequestStatusAsync(int groupId, int userId)
    {
        // 1. Check if member
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

        if (isMember) return "member";

        // 2. Check request
        var request = await _context.GroupJoinRequests
            .FirstOrDefaultAsync(r => r.GroupId == groupId && r.UserId == userId);

        if (request != null)
        {
            if (request.Status == "pending") return "pending";
            if (request.Status == "rejected") return "rejected";
            if (request.Status == "approved") return "member"; 
        }

        return "none"; // No request found
    }




    // Leave a group


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




    // Messaging

    public async Task<AuthResult> SendMessageAsync(int groupId, int userId, string content)
    {
        var group = await _context.Groups
            .Include(g => g.Settings)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

        if (group == null)
            return AuthResult.Failure("Group not found.");

        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

        if (!isMember)
            return AuthResult.Failure("You must be a member to send messages.");

        if (!group.Settings?.AllowMemberPosts ?? false)
        {
            var isOrganizer = group.OrganizerId == userId;
            if (!isOrganizer)
                return AuthResult.Failure("Posting is disabled for members in this group.");
        }

        var chat = await _context.GroupChats
            .FirstOrDefaultAsync(c => c.GroupId == groupId && c.IsActive);

        if (chat == null)
        {
            chat = new GroupChat { GroupId = groupId };
            _context.GroupChats.Add(chat);
            await _context.SaveChangesAsync();
        }

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

    public async Task<List<GroupMessage>> GetGroupMessagesAsync(int groupId, int userId, int limit = 50)
    {
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

    public async Task<GroupSettings?> GetSettingsAsync(int groupId)
    {
        return await _context.GroupSettings.FindAsync(groupId);
    }


    //    Update Group Settings (for the Organizer)

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

        if (dto.IsPrivate.HasValue) settings.IsPrivate = dto.IsPrivate.Value;
        if (dto.AllowMemberInvites.HasValue) settings.AllowMemberInvites = dto.AllowMemberInvites.Value;
        if (dto.AllowMemberPosts.HasValue) settings.AllowMemberPosts = dto.AllowMemberPosts.Value;
        if (dto.ModerateMessages.HasValue) settings.ModerateMessages = dto.ModerateMessages.Value;
        if (dto.AllowLinks.HasValue) settings.AllowLinks = dto.AllowLinks.Value;
        if (dto.AllowMedia.HasValue) settings.AllowMedia = dto.AllowMedia.Value;
        if (dto.NotifyOnNewEvent.HasValue) settings.NotifyOnNewEvent = dto.NotifyOnNewEvent.Value;
        if (dto.NotifyOnNewMember.HasValue) settings.NotifyOnNewMember = dto.NotifyOnNewMember.Value;

      
        if (group.IsPrivate != dto.IsPrivate)
        {
            group.IsPrivate = (bool)dto.IsPrivate;
            group.UpdatedAt = DateTime.UtcNow; 
        }

        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return AuthResult.Success("Settings updated.");
    }




    public async Task<AuthResult> AddRuleAsync(int groupId, int organizerId, CreateGroupRuleDto dto)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != organizerId)
            return AuthResult.Failure("Group not found or permission denied.");

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

    




    public async Task<List<GroupRule>> GetGroupRulesAsync(int groupId)
    {
        return await _context.GroupRules
            .Where(gr => gr.GroupId == groupId && gr.IsActive)
            .OrderBy(gr => gr.Order)
            .ToListAsync();
    }

    // Create a new discussion
    public async Task<AuthResult> CreateDiscussionAsync(int groupId, int userId, CreateDiscussionDto dto)
    {
        // Verify group exists and is active
        var group = await _context.Groups
            .Include(g => g.Settings)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

        if (group == null)
            return AuthResult.Failure("Group not found.");

        // Verify user is a member (or organizer)
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive);

        if (!isMember)
            return AuthResult.Failure("You must be a member to create discussions.");

        // Create discussion
        var discussion = new GroupDiscussion
        {
            GroupId = groupId,
            AuthorId = userId,
            Title = dto.Title.Trim(),
            Content = dto.Content?.Trim() ?? string.Empty,
            IsPinned = dto.IsPinned,
            IsLocked = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            SourceMessageId = dto.SourceMessageId 
        };

        _context.GroupDiscussions.Add(discussion);
        await _context.SaveChangesAsync();

        return AuthResult.Success("Discussion created.");
    }

    
    public async Task<List<DiscussionItemDto>> GetDiscussionsAsync(int groupId)
    {
        var discussions = await _context.GroupDiscussions
            .Where(d => d.GroupId == groupId && d.IsActive)
            .Include(d => d.Author)
            .OrderByDescending(d => d.IsPinned)
            .ThenByDescending(d => d.CreatedAt)
            .Take(50)
            .ToListAsync();

        return discussions.Select(d => new DiscussionItemDto
        {
            Id = d.Id,
            Title = d.Title,
            Content = d.Content,
            AuthorId = d.AuthorId,
            Author = new UserDto
            {
                Id = d.Author.Id,
                FirstName = d.Author.FirstName,
                LastName = d.Author.LastName,
                ProfilePicture = d.Author.ProfilePicture
            },
            ReplyCount = d.ReplyCount,
            IsPinned = d.IsPinned,
            IsLocked = d.IsLocked,
            CreatedAt = d.CreatedAt,
            Trending = d.ReplyCount > 5 || d.IsPinned
        }).ToList();
    }

    



    public async Task<DiscussionDetailDto?> GetDiscussionWithRepliesAsync(int discussionId, int userId)
    {
        var discussion = await _context.GroupDiscussions
            .Include(d => d.Group)
            .Include(d => d.Author)
            .FirstOrDefaultAsync(d => d.Id == discussionId && d.IsActive);

        if (discussion == null) return null;
        if (discussion.Group == null) return null; 

        if (discussion.Group.IsPrivate)
        {
            var isMember = await _context.GroupMembers.AnyAsync(gm =>
                gm.GroupId == discussion.GroupId && gm.UserId == userId && gm.IsActive);
            if (!isMember) return null;
        }

        var replies = await _context.GroupDiscussionReplies
            .Where(r => r.DiscussionId == discussionId && r.IsActive)
            .Include(r => r.Author)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        discussion.ViewCount++;
        discussion.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var discussionReaction = await GetReactionDataAsync("Discussion", discussionId, userId);

        

        var replyDtos = new List<ReplyDto>();
        foreach (var r in replies)
        {
            var replyReaction = await GetReactionDataAsync("Reply", r.Id, userId);
            replyDtos.Add(new ReplyDto
            {
                Id = r.Id,
                AuthorId = r.AuthorId,
                Author = new UserDto
                {
                    Id = r.Author?.Id ?? 0,
                    FirstName = r.Author?.FirstName ?? "Deleted",
                    LastName = r.Author?.LastName ?? "User",
                    ProfilePicture = r.Author?.ProfilePicture
                },
                Content = r.Content ?? "",
                ParentReplyId = r.ParentReplyId,
                CreatedAt = r.CreatedAt,
                UpvoteCount = replyReaction.Count,
                UserReaction = replyReaction.UserReaction
            });
        }

        return new DiscussionDetailDto
        {
            Id = discussion.Id,
            GroupId = discussion.GroupId,
            AuthorId = discussion.AuthorId,
            Author = new UserDto
            {
                Id = discussion.Author?.Id ?? 0,
                FirstName = discussion.Author?.FirstName ?? "Deleted",
                LastName = discussion.Author?.LastName ?? "User",
                ProfilePicture = discussion.Author?.ProfilePicture
            },
            Title = discussion.Title ?? "",
            Content = discussion.Content ?? "",
            IsPinned = discussion.IsPinned,
            IsLocked = discussion.IsLocked,
            ReplyCount = discussion.ReplyCount,
            UpvoteCount = discussionReaction.Count,
            UserReaction = discussionReaction.UserReaction,
            CreatedAt = discussion.CreatedAt,
            Replies = replyDtos 
        };
    }





    //  Toggle reaction on discussion or reply 
    public async Task<AuthResult> ToggleReactionAsync(string targetType, int targetId, int userId, string reactionType)
    {
        try
        {
           
            if (targetType != "Discussion" && targetType != "Reply")
                return AuthResult.Failure($"Invalid target type: {targetType}");

            
            if (targetType == "Reply")
            {
                var reply = await _context.GroupDiscussionReplies
                    .FirstOrDefaultAsync(r => r.Id == targetId && r.IsActive);

                if (reply == null)
                    return AuthResult.Failure("Reply not found or inactive.");

                
            }
            
            else if (targetType == "Discussion")
            {
                var discussion = await _context.GroupDiscussions
                    .FirstOrDefaultAsync(d => d.Id == targetId && d.IsActive);

                if (discussion == null)
                    return AuthResult.Failure("Discussion not found or inactive.");
            }

            
            var existing = await _context.GroupDiscussionReactions
                .FirstOrDefaultAsync(r =>
                    r.TargetType == targetType &&
                    r.TargetId == targetId &&
                    r.UserId == userId &&
                    r.ReactionType == reactionType);

            if (existing != null)
            {
                // Remove existing reaction
                _context.GroupDiscussionReactions.Remove(existing);
            }
            else
            {
                // Add new reaction
                var reaction = new GroupDiscussionReaction
                {
                    TargetType = targetType,
                    TargetId = targetId,
                    UserId = userId,
                    ReactionType = reactionType,
                    CreatedAt = DateTime.UtcNow
                };
                _context.GroupDiscussionReactions.Add(reaction);
            }

            await _context.SaveChangesAsync();
            return AuthResult.Success("Reaction updated.");
        }
        catch (Exception ex)
        {
            
            System.Diagnostics.Debug.WriteLine($"[ERROR] ToggleReactionAsync crashed: {ex.Message}\n{ex.StackTrace}");
            return AuthResult.Failure($"Server error: {ex.Message}");
        }
    }





    private async Task<(int Count, string? UserReaction)> GetReactionDataAsync(string targetType, int targetId, int userId)
    {
        try
        {
            var count = await _context.GroupDiscussionReactions
                .CountAsync(r => r.TargetType == targetType && r.TargetId == targetId && r.ReactionType == "upvote");

            var userReaction = await _context.GroupDiscussionReactions
                .Where(r => r.TargetType == targetType && r.TargetId == targetId && r.UserId == userId)
                .Select(r => r.ReactionType)
                .FirstOrDefaultAsync();

            return (count, userReaction == "upvote" ? "upvoted" : (userReaction ?? null));
        }
        catch
        {
           
            return (0, null);
        }
    }

    // Delete a reply (author or organizer only)
    public async Task<AuthResult> DeleteReplyAsync(int replyId, int userId)
    {
        var reply = await _context.GroupDiscussionReplies
            .Include(r => r.Discussion)
            .FirstOrDefaultAsync(r => r.Id == replyId && r.IsActive);

        if (reply == null)
            return AuthResult.Failure("Reply not found.");

        if (reply.Discussion == null || !reply.Discussion.IsActive)
            return AuthResult.Failure("Discussion not found or inactive.");

        var isAuthor = reply.AuthorId == userId;

        var isOrganizer = reply.Discussion?.Group?.OrganizerId == userId;

        if (!isAuthor && !isOrganizer)
            return AuthResult.Failure("Permission denied.");

        reply.IsActive = false;
        reply.Discussion.ReplyCount = Math.Max(0, reply.Discussion.ReplyCount - 1);
        reply.Discussion.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return AuthResult.Success("Reply deleted.");
    }

    //  Add a reply to discussion 
    public async Task<AuthResult> AddReplyAsync(int discussionId, int userId, CreateReplyDto dto)
    {
        var discussion = await _context.GroupDiscussions
            .Include(d => d.Group)
            .FirstOrDefaultAsync(d => d.Id == discussionId && d.IsActive);

        if (discussion == null)
            return AuthResult.Failure("Discussion not found.");
        if (discussion.IsLocked)
            return AuthResult.Failure("This discussion is locked.");

        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == discussion.GroupId && gm.UserId == userId && gm.IsActive);
        if (!isMember)
            return AuthResult.Failure("You must be a member to reply.");

        var reply = new GroupDiscussionReply
        {
            DiscussionId = discussionId,
            AuthorId = userId,
            Content = dto.Content.Trim(),
            ParentReplyId = dto.ParentReplyId,
            CreatedAt = DateTime.UtcNow
        };

        _context.GroupDiscussionReplies.Add(reply);
        discussion.ReplyCount++;
        discussion.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return AuthResult.Success("Reply posted.");
    }


    // request to join 
    public async Task<AuthResult> RequestJoinAsync(int groupId, int userId, string? message)
    {
        var group = await _context.Groups
            .Include(g => g.Settings)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

        if (group == null)
            return AuthResult.Failure("Group not found.");

        //if USER IS  already a member
        if (await _context.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.IsActive))
            return AuthResult.Failure("You are already a member of this group.");

        // Check for existing pending request
        if (await _context.GroupJoinRequests.AnyAsync(r => r.GroupId == groupId && r.UserId == userId && r.Status == "pending"))
            return AuthResult.Failure("You already have a pending request to join this group.");

        bool isPrivate = group.IsPrivate || (group.Settings?.IsPrivate ?? false);

        if (!isPrivate)
            return AuthResult.Success("This is a public group. Please use the Join button instead.");

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

        return AuthResult.Success("Join request submitted successfully. Waiting for organizer approval.");
    }







    public async Task<List<PendingRequestDto>> GetPendingJoinRequestsAsync(int groupId, int organizerId)
    {
        //  Verify group exists and user is organizer
        var group = await _context.Groups
            .Include(g => g.Settings)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null || group.OrganizerId != organizerId)
            return new List<PendingRequestDto>(); 


        // Fetch ONLY pending requests for this group
        var pendingRequests = await _context.GroupJoinRequests
            .Where(r => r.GroupId == groupId && r.Status == "pending")
            .Include(r => r.User) 
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        
        return pendingRequests.Select(r => new PendingRequestDto
        {
            Id = r.Id,
            User = new UserDto 
            {
                Id = r.User.Id,
                FirstName = r.User.FirstName,
                LastName = r.User.LastName,
                ProfilePicture = r.User.ProfilePicture
            },
            Message = r.Message,
            RequestedAt = r.RequestedAt
        }).ToList();
    }






    public async Task<AuthResult> ReviewJoinRequestAsync(int requestId, int organizerId, bool approve, string? notes)
    {
        var request = await _context.GroupJoinRequests
            .Include(r => r.Group)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            return AuthResult.Failure("Request not found or permission denied.");

        //  Verify organizer permission
        if (request.Group == null || request.Group.OrganizerId != organizerId)
            return AuthResult.Failure("Permission denied.");

        //  Only allow reviewing pending requests
        if (request.Status != "pending")
            return AuthResult.Failure("Request has already been reviewed.");

        //  Update request status
        request.Status = approve ? "approved" : "rejected";
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = organizerId;
        request.ReviewNotes = notes?.Trim();

        if (approve)
        {
            //  Check if user is already a member before adding
            var existingMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == request.GroupId && gm.UserId == request.UserId);

            if (existingMember != null)
            {
                //  Reactivate if was previously removed
                existingMember.IsActive = true;
                existingMember.JoinedAt = DateTime.UtcNow;
            }
            else
            {
                // Add a new member
                var member = new GroupMemberModel
                {
                    GroupId = request.GroupId,
                    UserId = request.UserId,
                    Role = "member",
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };
                _context.GroupMembers.Add(member);
            }

            //  Update group member count 
            if (request.Group.MemberCount < (await _context.GroupMembers.CountAsync(gm => gm.GroupId == request.GroupId && gm.IsActive)))
            {
                request.Group.MemberCount++;
            }
            request.Group.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return AuthResult.Success(approve ? "Member approved and added." : "Request rejected.");
    }



    public async Task<AuthResult> RemoveMemberAsync(int groupId, int organizerId, int targetUserId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != organizerId)
            return AuthResult.Failure("Group not found or permission denied.");

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

    //  Activity Feed 
    public async Task<List<ActivityItemDto>> GetActivityFeedAsync(int groupId)
    {
        var activities = new List<ActivityItemDto>();

        var joins = await _context.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.IsActive)
            .Include(gm => gm.User)
            .OrderByDescending(gm => gm.JoinedAt)
            .Take(10)
            .ToListAsync();

        foreach (var join in joins)
        {
            activities.Add(new ActivityItemDto
            {
                Id = join.Id,
                Type = "join",
                User = new UserDto { FirstName = join.User.FirstName, LastName = join.User.LastName },
                Description = "joined the group",
                CreatedAt = join.JoinedAt
            });
        }

        var messages = await _context.GroupMessages
            .Where(m => m.GroupChat.GroupId == groupId && !m.IsDeleted)
            .Include(m => m.Sender)
            .OrderByDescending(m => m.SentAt)
            .Take(10)
            .ToListAsync();

        foreach (var msg in messages)
        {
            activities.Add(new ActivityItemDto
            {
                Id = msg.Id,
                Type = "post",
                User = new UserDto { FirstName = msg.Sender.FirstName, LastName = msg.Sender.LastName },
                Description = "posted a message",
                CreatedAt = msg.SentAt
            });
        }

        return activities.OrderByDescending(a => a.CreatedAt).Take(20).ToList();
    }






    // Delete Message (for Organizer)
    public async Task<AuthResult> DeleteMessageAsync(int groupId, int messageId, int organizerId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != organizerId)
            return AuthResult.Failure("Permission denied");

        var message = await _context.GroupMessages.FindAsync(messageId);
        if (message == null || message.GroupChat.GroupId != groupId)
            return AuthResult.Failure("Message not found");

        message.IsDeleted = true;
        await _context.SaveChangesAsync();

        return AuthResult.Success("Message deleted");
    }

    //  Add Member 
    public async Task<AuthResult> AddMemberAsync(int groupId, int organizerId, int targetUserId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null || group.OrganizerId != organizerId)
            return AuthResult.Failure("Permission denied");

        var existing = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == targetUserId);

        if (existing != null)
        {
            existing.IsActive = true;
            await _context.SaveChangesAsync();
            return AuthResult.Success("Member added");
        }

        var member = new GroupMemberModel
        {
            GroupId = groupId,
            UserId = targetUserId,
            Role = "member",
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
        _context.GroupMembers.Add(member);

        group.MemberCount++;
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return AuthResult.Success("Member added");
    }



    // GET: Members of a group
    public async Task<List<GroupMemberResponseDto>> GetGroupMembersAsync(int groupId)
    {
        var members = await _context.GroupMembers
            .Where(gm => gm.GroupId == groupId && gm.IsActive)
            .Include(gm => gm.User)
            .Select(gm => new GroupMemberResponseDto
            {
                Id = gm.Id,
                UserId = gm.UserId,
                Role = gm.Role,
                JoinedAt = gm.JoinedAt,
                User = new UserDto
                {
                    Id = gm.User.Id,
                    FirstName = gm.User.FirstName,
                    LastName = gm.User.LastName,
                    ProfilePicture = gm.User.ProfilePicture
                }
            })
            .OrderByDescending(gm => gm.JoinedAt)
            .ToListAsync();

        return members;
    }





    public async Task<(bool IsSuccess, string? Message, GroupModel? Group)> UpdateGroupAsync(
    int groupId,
    int organizerId,
    UpdateGroupDto dto)
    {
        // Fetch the group - DbSet<Group> 
        var group = await _context.Groups
            .Include(g => g.Settings)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return (false, "Group not found", null);

        
        if (group.OrganizerId != organizerId)
            return (false, "Only the organizer can update group details", null);



        
        if (!string.IsNullOrEmpty(dto.Name)) group.Name = dto.Name;
        if (dto.Description != null) group.Description = dto.Description;
        if (dto.City != null) group.City = dto.City;
        if (dto.Country != null) group.Country = dto.Country;
        if (dto.Location != null) group.Location = dto.Location;
        if (dto.IsPrivate !=null) group.IsPrivate = dto.IsPrivate.Value;

        group.UpdatedAt = DateTime.UtcNow;

        
        if (group.Settings != null)
        {
            if (dto.AllowMemberInvites.HasValue) group.Settings.AllowMemberInvites = dto.AllowMemberInvites.Value;
            if (dto.AllowMemberPosts.HasValue) group.Settings.AllowMemberPosts = dto.AllowMemberPosts.Value;
            if (dto.ModerateMessages.HasValue) group.Settings.ModerateMessages = dto.ModerateMessages.Value;
        }




      
        await _context.SaveChangesAsync();

       
        var updatedGroup = await _context.Groups
            .Include(g => g.Organizer)
            .Include(g => g.Settings)
            .Include(g => g.GroupRules)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        return (true, null, updatedGroup!); 
    }








    public async Task<List<GroupModel>> GetGroupsByOrganizerAsync(int organizerId)
    {
        return await _context.Groups
            .Include(g => g.Organizer)
            .Include(g => g.GroupMembers)
            .Where(g => g.OrganizerId == organizerId && g.IsActive)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<GroupModel>> GetGroupsByMemberAsync(int userId, bool excludeOrganized = false)
    {
        var query = _context.GroupMembers
            .Include(gm => gm.Group)
                .ThenInclude(g => g.Organizer)
            .Include(gm => gm.Group)
                .ThenInclude(g => g.GroupMembers)
            .Where(gm => gm.UserId == userId && gm.Group.IsActive);

        if (excludeOrganized)
        {
            query = query.Where(gm => gm.Group.OrganizerId != userId);
        }

        return await query
            .Select(gm => gm.Group)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }


    // GET: /api/groups/join-statuses?userId={userId}
    public async Task<List<JoinStatusDto>> GetJoinStatusesAsync(int userId)
    {
        var requests = await _context.GroupJoinRequests
            .Where(r => r.UserId == userId && r.Status != "approved")
            .Select(r => new JoinStatusDto
            {
                GroupId = r.GroupId,
                Status = r.Status // "pending" or "rejected"
            })
            .ToListAsync();

        return requests;
    }

    public class JoinStatusDto
    {
        public int GroupId { get; set; }
        public string Status { get; set; } = string.Empty; // "none" | "pending" | "rejected"
    }



}

//  DTO Classes 

public class ActivityItemDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public UserDto User { get; set; } = new UserDto();
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}


public class DiscussionItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public UserDto Author { get; set; } = new UserDto();
    public int ReplyCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsPinned { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Trending { get; set; }
    public int? SourceMessageId { get; set; }
}




public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
}
public class AddMemberDto
{
    public int TargetUserId { get; set; }
}


public class GroupMemberResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public UserDto User { get; set; } = new UserDto();
}


public class CreateReplyDto
{
    [Required, StringLength(3000)]
    public string Content { get; set; } = string.Empty;

    public int? ParentReplyId { get; set; } // Optional: reply to a reply
}
public class ReplyDto
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public UserDto Author { get; set; } = new UserDto();
    public string Content { get; set; } = string.Empty;
    public int? ParentReplyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UpvoteCount { get; set; }
    public string? UserReaction { get; set; } // Current user's reaction
}

public class DiscussionDetailDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int AuthorId { get; set; }
    public UserDto Author { get; set; } = new UserDto();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public bool IsLocked { get; set; }
    public int ReplyCount { get; set; }
    public int UpvoteCount { get; set; }
    public string? UserReaction { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReplyDto> Replies { get; set; } = new List<ReplyDto>();
}




