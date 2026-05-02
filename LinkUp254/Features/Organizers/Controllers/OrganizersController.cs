using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LinkUp254.Features.Organizers.DTOs;
using LinkUp254.Features.Organizers.Services;

namespace LinkUp254.Features.Organizers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizersController : ControllerBase
{
    private readonly IOrganizerService _organizerService;
    private readonly ILogger<OrganizersController> _logger;

    public OrganizersController(IOrganizerService organizerService, ILogger<OrganizersController> logger)
    {
        _organizerService = organizerService;
        _logger = logger;
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
        try
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Invalid user session." });

            var success = await _organizerService.FollowOrganizerAsync(userId, dto.OrganizerId);

            if (success)
            {
                return Ok(new { message = "Followed successfully." });
            }
            else
            {
                
                return Ok(new { message = "Already following.", isAlreadyFollowing = true });
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Follow failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Follow endpoint error");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("unfollow")]
    [Authorize]
    public async Task<IActionResult> Unfollow([FromBody] FollowOrganizerDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Invalid user session." });

            var success = await _organizerService.UnfollowOrganizerAsync(userId, dto.OrganizerId);

            if (success)
            {
                return Ok(new { message = "Unfollowed successfully." });
            }
            else
            {
                
                return Ok(new { message = "Not following this organizer.", isNotFollowing = true });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unfollow endpoint error");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("rate")]
    [Authorize]
    public async Task<IActionResult> Rate([FromBody] RateOrganizerDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Invalid user session." });

            var success = await _organizerService.RateOrganizerAsync(userId, dto.OrganizerId, dto.Rating, dto.Comment);
            return success ? Ok(new { message = "Rating saved successfully." }) : StatusCode(500, new { message = "Failed to save rating." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rate endpoint error");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    [HttpPost("contact")]
    [Authorize]
    public async Task<IActionResult> Contact([FromBody] ContactOrganizerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Message)) return BadRequest(new { message = "Message cannot be empty." });

        try
        {
            var userId = GetUserId();
            if (userId <= 0) return Unauthorized(new { message = "Invalid user session." });

            var success = await _organizerService.ContactOrganizerAsync(userId, dto.OrganizerId, dto.Message);
            return success ? Ok(new { message = "Message sent successfully." }) : StatusCode(500, new { message = "Failed to send message." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Contact endpoint error");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("sub")?.Value;

        return int.TryParse(idClaim, out var userId) ? userId : throw new UnauthorizedAccessException("Invalid user token.");
    }
}