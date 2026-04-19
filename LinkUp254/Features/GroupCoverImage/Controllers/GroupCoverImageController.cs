using LinkUp254.Features.Auth;
using LinkUp254.Features.GroupCoverImage.DTOs;
using LinkUp254.Features.GroupCoverImage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

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

    // GET: api/group-cover-images/{groupId}
    [HttpGet("{groupId:int}")]
    public async Task<IActionResult> GetCoverImage(int groupId)
    {
        var dto = await _coverImageServices.GetCoverImageDtoAsync(groupId);
        return dto != null ? Ok(dto) : NotFound(new { message = "Cover image not found" });
    }

    // PUT: api/group-cover-images/{groupId}
    [HttpPut("{groupId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateCoverImage(int groupId, [FromBody] UpdateCoverImageRequest dto)
    {
        var organizerId = GetCurrentUserId();
        if (!organizerId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var result = await _coverImageServices.UpdateCoverImageAsync(groupId, organizerId.Value, dto.ImageUrl);
        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = result.Message })
            : BadRequest(new { message = result.Message });
    }

    // DELETE: api/group-cover-images/{groupId}
    [HttpDelete("{groupId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteCoverImage(int groupId)
    {
        var organizerId = GetCurrentUserId();
        if (!organizerId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var result = await _coverImageServices.DeleteCoverImageAsync(groupId, organizerId.Value);
        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = result.Message })
            : BadRequest(new { message = result.Message });
    }
}
