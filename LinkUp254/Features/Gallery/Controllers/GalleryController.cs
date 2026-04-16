using LinkUp254.Features.Gallery.DTOs;
using LinkUp254.Features.Gallery.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUp254.Features.Gallery.Controllers;

[ApiController]
[Route("api/gallery")]
public class GalleryController : ControllerBase
{
    private readonly IGalleryServices _galleryServices;

    public GalleryController(IGalleryServices galleryServices)
    {
        _galleryServices = galleryServices;
    }

    // GET: api/gallery/groups/{groupId} - Get all gallery images for a group
    [HttpGet("groups/{groupId:int}")]
    public async Task<IActionResult> GetGroupGallery(int groupId)
    {
        var images = await _galleryServices.GetGalleryAsync(groupId);
        return Ok(images);
    }

    // POST: api/gallery/groups/{groupId}/upload - Upload a new gallery image
    [HttpPost("groups/{groupId:int}/upload")]
    [Authorize]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
    public async Task<IActionResult> UploadGalleryImage(int groupId, [FromBody] UploadGalleryImageRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _galleryServices.UploadGalleryImageAsync(groupId, intUserId, request);

        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = "Image uploaded successfully", data = result.Data })
            : BadRequest(new { message = result.Message });
    }

    // DELETE: api/gallery/images/{imageId} - Delete a gallery image (soft delete)
    [HttpDelete("images/{imageId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteGalleryImage(int imageId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _galleryServices.DeleteGalleryImageAsync(imageId, intUserId);

        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = "Image deleted successfully" })
            : BadRequest(new { message = result.Message });
    }
}