using LinkUp254.Features.Groups.DTOs;
using LinkUp254.Features.Groups.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace LinkUp254.Features.Groups;

[ApiController]
[Route("api/groups")]
public class GroupController : ControllerBase
{
    private readonly GroupServices _groupServices;
    

    public GroupController(GroupServices groupServices)
    {
        _groupServices = groupServices;
    }

    // GET: api/groups  All active groups 
    [HttpGet]
    public async Task<IActionResult> GetAllGroups([FromQuery] string? city, [FromQuery] string? country)
    {
        var groups = await _groupServices.GetAllGroupsAsync(city, country);

    
        return Ok(groups);
    }

    // GET: api/groups/{id} - Single group details 
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGroupById(int id)
    {
        var group = await _groupServices.GetGroupByIdAsync(id);
        if (group == null) return NotFound(new { message = "Group not found" });

        
        var response = new
        {
            group.Id,
            group.Name,
            group.Description,
            group.CoverImage,
            group.OrganizerId,
            group.Organizer,
            group.City,
            group.Country,
            group.MemberCount,
            group.IsActive,
            group.IsPrivate,
            group.CreatedAt,
            group.UpdatedAt,
            group.GroupMembers,
            group.GroupEvents,
            group.Settings,
            group.GroupRules,
            group.Chat
        };

        return Ok(response);
    }

    // GET: api/groups/my-groups - Groups users belongs to
    [HttpGet("my-groups")]
    [Authorize]
    public async Task<IActionResult> GetMyGroups()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            return Unauthorized(new { message = "Authentication required" });

        var groups = await _groupServices.GetUserGroupsAsync(id);
        return Ok(groups);
    }

    // POST: api/groups - Create new group
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var id))
            return Unauthorized(new { message = "Authentication required" });

        var group = await _groupServices.CreateGroupAsync(dto, id);

        return Ok(new
        {
            isSuccess = true,
            message = "Group created successfully",
            group
        });
    }






    // cover images




    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    [HttpPut("{id:int}/cover-image")]
    [Authorize]
    public async Task<IActionResult> UpdateCoverImage(int id, [FromBody] UpdateCoverImageDto dto)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateCoverImage START: id={id}, CoverImage length={(dto.CoverImage?.Length ?? 0)}");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
                return Unauthorized(new { message = "Authentication required" });

            var result = await _groupServices.UpdateCoverImageAsync(id, intUserId, dto.CoverImage);

            System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateCoverImage END: IsSuccess={result.IsSuccess}");

            return result.IsSuccess
                ? Ok(new { isSuccess = true, message = "Cover image updated successfully", coverImage = result.CoverImage })
                : BadRequest(new { message = result.Message ?? "Failed to update cover image" });
        }
        catch (Exception ex)
        {
            // Loging  error to console
            System.Diagnostics.Debug.WriteLine($"[ERROR] UpdateCoverImage CRASHED: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(500, new { message = $"Server error: {ex.Message}" });
        }
    }

















    // PUT: api/groups/{id} - Update group details (organizer only)
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.UpdateGroupAsync(id, intUserId, dto);

        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = "Group updated successfully", group = result.Group })
            : BadRequest(new { message = result.Message ?? "Failed to update group" });
    }







    [HttpPost("{id:int}/join")]
    [Authorize]  
    public async Task<IActionResult> JoinGroup(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var (isSuccess, message, isPending) = await _groupServices.JoinGroupAsync(id, intUserId);

        if (!isSuccess)
        {
            if (isPending)
                return Ok(new { isSuccess = true, isPending = true, message });
            else
                return BadRequest(new { isSuccess = false, message });
        }

        return Ok(new
        {
            isSuccess = true,
            isPending = isPending,
            message
        });
    }











    // POST: api/groups/{id}/leave - Leave a group
    [HttpPost("{id:int}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveGroup(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.LeaveGroupAsync(id, intUserId);
        return result
            ? Ok(new { isSuccess = true, message = "Left group successfully" })
            : BadRequest(new { message = "Could not leave group (you may be the organizer)" });
    }

    // DELETE: api/groups/{id} - Delete group (organizer only)
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.DeleteGroupAsync(id, intUserId);
        return result
            ? Ok(new { isSuccess = true, message = "Group deleted successfully" })
            : BadRequest(new { message = "Could not delete group (only organizer can delete)" });


    }









    // GROUP CHAT 
    // POST: api/groups/{id}/chat/send - Send a message
    [HttpPost("{id:int}/chat/send")]
    [Authorize]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.SendMessageAsync(id, intUserId, dto.Content);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET: api/groups/{id}/chat/messages - Get recent messages
    [HttpGet("{id:int}/chat/messages")]
    [Authorize]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int limit = 50)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var messages = await _groupServices.GetGroupMessagesAsync(id, intUserId, limit);
        return Ok(messages);
    }

    // GROUP SETTINGS 
    // PUT: api/groups/{id}/settings - Update group settings
    [HttpPut("{id:int}/settings")]
    [Authorize]
    public async Task<IActionResult> UpdateSettings(int id, [FromBody] UpdateGroupSettingsDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.UpdateSettingsAsync(id, intUserId, dto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET: api/groups/{id}/settings - Get current settings
    [HttpGet("{id:int}/settings")]
    [Authorize]
    public async Task<IActionResult> GetSettings(int id)
    {
        var settings = await _groupServices.GetSettingsAsync(id);
        return settings != null ? Ok(settings) : NotFound(new { message = "Settings not found" });
    }

    // GROUP RULES
    // POST: api/groups/{id}/rules - Add a new rule
    [HttpPost("{id:int}/rules")]
    [Authorize]
    public async Task<IActionResult> AddRule(int id, [FromBody] CreateGroupRuleDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.AddRuleAsync(id, intUserId, dto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET: api/groups/{id}/rules - Get all rules
    [HttpGet("{id:int}/rules")]
    public async Task<IActionResult> GetRules(int id)
    {
        var rules = await _groupServices.GetGroupRulesAsync(id);
        return Ok(rules);
    }

    // MEMBER REQUESTS
    // POST: api/groups/{id}/join-request - Request to join (for PRIVATE groups)
    [HttpPost("{id:int}/join-request")]
    [Authorize]
    public async Task<IActionResult> RequestJoin(int id, [FromBody] JoinRequestDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.RequestJoinAsync(id, intUserId, dto.Message);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }






 







    // GET: api/groups/{id}/join-requests/pending - Get pending requests (organizer only)
    [HttpGet("{id:int}/join-requests/pending")]
    [Authorize]
    public async Task<IActionResult> GetPendingRequests(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var requests = await _groupServices.GetPendingJoinRequestsAsync(id, intUserId);
        return Ok(requests);
    }

    // PUT: api/groups/join-requests/{requestId}/review - Review a join request
    [HttpPut("join-requests/{requestId:int}/review")]
    [Authorize]
    public async Task<IActionResult> ReviewRequest(int requestId, [FromBody] ReviewJoinRequestDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.ReviewJoinRequestAsync(requestId, intUserId, dto.Approve, dto.Notes);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // MEMBER MANAGEMENT
    // DELETE: api/groups/{id}/members/{userId} - Remove a member
    [HttpDelete("{id:int}/members/{userId:int}")]
    [Authorize]
    public async Task<IActionResult> RemoveMember(int id, int userId)
    {
        var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(organizerId) || !int.TryParse(organizerId, out var intOrganizerId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.RemoveMemberAsync(id, intOrganizerId, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // PUT: api/groups/{id}/members/{userId}/role - Update member role
    [HttpPut("{id:int}/members/{userId:int}/role")]
    [Authorize]
    public async Task<IActionResult> UpdateMemberRole(int id, int userId, [FromBody] UpdateRoleDto dto)
    {
        var organizerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(organizerId) || !int.TryParse(organizerId, out var intOrganizerId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.UpdateMemberRoleAsync(id, intOrganizerId, userId, dto.NewRole);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET: api/groups/{id}/activity
    [HttpGet("{id:int}/activity")]
    public async Task<IActionResult> GetActivityFeed(int id)
    {
        var activities = await _groupServices.GetActivityFeedAsync(id);
        return Ok(activities);
    }

    // GET: api/groups/{id}/discussions
    [HttpGet("{id:int}/discussions")]
    public async Task<IActionResult> GetDiscussions(int id)
    {
        var discussions = await _groupServices.GetDiscussionsAsync(id);
        return Ok(discussions);
    }

    // GET: api/groups/{id}/gallery
    [HttpGet("{id:int}/gallery")]
    public async Task<IActionResult> GetGallery(int id)
    {
        var gallery = await _groupServices.GetGalleryAsync(id);
        return Ok(gallery);
    }

    // DELETE: api/groups/{id}/chat/messages/{messageId} - Delete message (organizer only)
    [HttpDelete("{id:int}/chat/messages/{messageId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMessage(int id, int messageId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.DeleteMessageAsync(id, messageId, intUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // POST: api/groups/{id}/members - Add member directly (organizer only)
    [HttpPost("{id:int}/members")]
    [Authorize]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.AddMemberAsync(id, intUserId, dto.TargetUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    // GET: api/groups/{id}/members - Get all active members
    [HttpGet("{id:int}/members")]
    public async Task<IActionResult> GetGroupMembers(int id)
    {
        var members = await _groupServices.GetGroupMembersAsync(id);
        return Ok(members);
    }







    //  Get Join Status
    [HttpGet("{id:int}/join-status")]
    [Authorize]
    public async Task<IActionResult> GetJoinStatus(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized();

        var status = await _groupServices.GetJoinRequestStatusAsync(id, intUserId);
        return Ok(new { status });
    }





}

// DTOs
public class JoinRequestDto
{
    public string? Message { get; set; }
}

public class UpdateRoleDto
{
    [Required]
    public string NewRole { get; set; } = string.Empty;
}


public class UpdateGroupDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public bool IsPrivate { get; set; }
    public bool AllowMemberInvites { get; set; } = true;
    public bool AllowMemberPosts { get; set; } = true;
    public bool ModerateMessages { get; set; }

    [StringLength(500)]
    public string? CoverImage { get; set; }
}
public class UpdateCoverImageDto
{
   /// <summary>
   /// /[StringLength(500)]
   /// </summary>
    public string? CoverImage { get; set; } // URL or base64 data URL
}