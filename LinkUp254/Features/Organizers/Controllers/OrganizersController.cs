using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LinkUp254.Features.Organizers.DTOs;
using LinkUp254.Features.Organizers.Services;

namespace LinkUp254.Features.Organizers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizersController : ControllerBase
{
    private readonly IOrganizerService _organizerService;

    public OrganizersController(IOrganizerService organizerService)
    {
        _organizerService = organizerService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrganizer(int id)
    {
        try
        {
            var dto = await _organizerService.GetOrganizerAsync(id);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("follow")]
    [Authorize]
    public async Task<IActionResult> Follow([FromBody] FollowOrganizerDto dto)
    {
        var userId = GetUserId();
        var success = await _organizerService.FollowOrganizerAsync(userId, dto.OrganizerId);
        return success ? Ok(new { message = "Followed successfully." }) : BadRequest(new { message = "Already following." });
    }

    [HttpPost("unfollow")]
    [Authorize]
    public async Task<IActionResult> Unfollow([FromBody] FollowOrganizerDto dto)
    {
        var userId = GetUserId();
        var success = await _organizerService.UnfollowOrganizerAsync(userId, dto.OrganizerId);
        return success ? Ok(new { message = "Unfollowed successfully." }) : NotFound(new { message = "Not following this organizer." });
    }

    [HttpPost("rate")]
    [Authorize]
    public async Task<IActionResult> Rate([FromBody] RateOrganizerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetUserId();
        var success = await _organizerService.RateOrganizerAsync(userId, dto.OrganizerId, dto.Rating, dto.Comment);
        return success ? Ok(new { message = "Rating saved successfully." }) : StatusCode(500, new { message = "Failed to save rating." });
    }

    [HttpPost("contact")]
    [Authorize]
    public async Task<IActionResult> Contact([FromBody] ContactOrganizerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Message)) return BadRequest(new { message = "Message cannot be empty." });

        var userId = GetUserId();
        var success = await _organizerService.ContactOrganizerAsync(userId, dto.OrganizerId, dto.Message);
        return success ? Ok(new { message = "Message sent successfully." }) : StatusCode(500, new { message = "Failed to send message." });
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("sub")?.Value;

        return int.TryParse(idClaim, out var userId) ? userId : throw new UnauthorizedAccessException("Invalid user token.");
    }
}