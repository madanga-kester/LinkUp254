using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LinkUp254.Features.Interests.DTOs;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using LinkUp254.Database;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace LinkUp254.Features.Interests
{
    [ApiController]
    [Route("api/[controller]")]
    public class InterestController : ControllerBase
    {
        private readonly LinkUpContext _context;

        public InterestController(LinkUpContext context)
        {
            _context = context;
        }

        // GET: api/interest/all - Geting all active interests (public or auth required)
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var interests = await _context.Interests
                .Where(i => i.IsActive)
                .OrderBy(i => i.Category)
                .ThenBy(i => i.Name)
                .ToListAsync();

            return Ok(interests);
        }

        // POST: api/interest/select - Save user's selected interests
        [HttpPost("select")]
        [Authorize]
        public async Task<IActionResult> SelectInterests([FromBody] SelectInterestsDto dto)
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                               ?? User.FindFirst("sub")?.Value;

                if (!int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { message = "Invalid token" });

                // Remove old selections for this user
                var existing = await _context.UserInterests
                    .Where(ui => ui.UserId == userId)
                    .ToListAsync();
                _context.UserInterests.RemoveRange(existing);

                // Add new selections
                var newInterests = dto.InterestIds.Select(id => new UserInterest
                {
                    UserId = userId,
                    InterestId = id,
                    SelectedAt = DateTime.UtcNow,
                    IsActive = true
                });
                await _context.UserInterests.AddRangeAsync(newInterests);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Interests saved successfully", count = dto.InterestIds.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to save interests: {ex.Message}" });
            }
        }

        // GET: api/interest/my - Get current user's selected interests
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyInterests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid token" });

            var interests = await _context.UserInterests
                .Where(ui => ui.UserId == userId && ui.IsActive)
                .Select(ui => ui.Interest)
                .ToListAsync();

            return Ok(interests);
        }






        // GET: api/interest/has-interests - Check if user has selected interests
        [HttpGet("has-interests")]
        [Authorize]
        public async Task<IActionResult> HasInterests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { hasInterests = false });

            var hasInterests = await _context.UserInterests
                .AnyAsync(ui => ui.UserId == userId && ui.IsActive);

            return Ok(new { hasInterests });
        }









    }
}