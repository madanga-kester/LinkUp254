using LinkUp254.Features.Groups.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    // GET: api/groups - All active groups
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
        return group != null ? Ok(group) : NotFound(new { message = "Group not found" });
    }

    // GET: api/groups/my-groups - Groups user belongs to
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
        return Ok(new { isSuccess = true, message = "Group created successfully", group });
    }

    // POST: api/groups/{id}/join - Join a group
    [HttpPost("{id:int}/join")]
    [Authorize]
    public async Task<IActionResult> JoinGroup(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.JoinGroupAsync(id, intUserId);
        return Ok(new
        {
            isSuccess = true,
            message = result ? "Joined group successfully" : "You are already a member"
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
}