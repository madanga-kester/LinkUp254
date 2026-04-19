using LinkUp254.Features.Auth;
using LinkUp254.Features.GroupCoverImage.DTOs;
using LinkUp254.Features.GroupCoverImage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUp254.Features.GroupCoverImage.Controllers;

[ApiController]
[Route("api/group-cover-images")]
public class GroupCoverImageController : ControllerBase
{
    private readonly IGroupCoverImageServices _coverImageServices;

    public GroupCoverImageController(IGroupCoverImageServices coverImageServices)
    {
        _coverImageServices = coverImageServices;
    }

    private int? GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value
                  ?? User.FindFirst("userId")?.Value;
        return int.TryParse(userId, out var id) ? id : null;
    }

    [HttpGet("{groupId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoverImage(int groupId)
    {
        var dto = await _coverImageServices.GetCoverImageDtoAsync(groupId, HttpContext.RequestAborted);
        return dto != null ? Ok(dto) : NotFound(new { message = "Cover image not found" });
    }

    [HttpPut("{groupId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCoverImage(int groupId, [FromBody] UpdateCoverImageRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var organizerId = GetCurrentUserId();
        if (!organizerId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var result = await _coverImageServices.UpdateCoverImageAsync(groupId, organizerId.Value, dto.ImageUrl, HttpContext.RequestAborted);
        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = result.Message })
            : BadRequest(new { message = result.Message });
    }

    [HttpDelete("{groupId:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteCoverImage(int groupId)
    {
        var organizerId = GetCurrentUserId();
        if (!organizerId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var result = await _coverImageServices.DeleteCoverImageAsync(groupId, organizerId.Value, HttpContext.RequestAborted);
        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = result.Message })
            : BadRequest(new { message = result.Message });
    }
}