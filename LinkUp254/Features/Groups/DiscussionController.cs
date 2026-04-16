using LinkUp254.Features.Auth;
using LinkUp254.Features.Groups.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUp254.Features.Groups;

[ApiController]
[Route("api/discussions")]  
public class DiscussionController : ControllerBase
{
    private readonly GroupServices _groupServices;

    public DiscussionController(GroupServices groupServices)
    {
        _groupServices = groupServices;
    }

    // GET: api/discussions/{id} - Full thread + replies
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetDiscussionDetail(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var detail = await _groupServices.GetDiscussionWithRepliesAsync(id, intUserId);
        return detail != null
            ? Ok(detail)
            : NotFound(new { message = "Discussion not found or access denied" });
    }



    // POST: api/discussions/{id}/replies - Add a reply
    [HttpPost("{id:int}/replies")]
    [Authorize]
    public async Task<IActionResult> AddReply(int id, [FromBody] CreateReplyDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.AddReplyAsync(id, intUserId, dto);
        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = result.Message })
            : BadRequest(new { message = result.Message });
    }

    // DELETE: api/discussions/replies/{replyId} - Delete a reply
    [HttpDelete("replies/{replyId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteReply(int replyId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.DeleteReplyAsync(replyId, intUserId);
        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = result.Message })
            : BadRequest(new { message = result.Message });
    }



    // POST: api/discussions/reactions/{targetType}/{targetId} - Toggle reaction
    //[HttpPost("reactions/{targetType}/{targetId:int}")]
    //[Authorize]
    //public async Task<IActionResult> ToggleReaction(string targetType, int targetId, [FromBody] ToggleReactionDto dto)
    //{
    //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value



    // POST: api/discussions/reactions/{targetType}/{targetId} - Toggle reaction
    [HttpPost("reactions/{targetType}/{targetId:int}")]
    [Authorize]
    public async Task<IActionResult> ToggleReaction(string targetType, int targetId, [FromBody] DiscussionToggleReactionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value

            //
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _groupServices.ToggleReactionAsync(targetType, targetId, intUserId, dto.Type);
        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = result.Message })
            : BadRequest(new { message = result.Message });
    }





}