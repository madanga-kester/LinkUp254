using LinkUp254.Features.Shared.Newsletter.Models.DTOs;
using LinkUp254.Features.Shared.Newsletter.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUp254.Features.Shared.Newsletter.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterService _newsletterService;

    public NewsletterController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    [HttpGet("status")]
    public async Task<ActionResult<NewsletterStatusResponse>> GetSubscriptionStatus([FromQuery] string? email)
    {
        var subClaim = User.FindFirstValue("sub");

        if (!string.IsNullOrEmpty(subClaim) && int.TryParse(subClaim, out int userId))
        {
            var isSubscribed = await _newsletterService.IsUserSubscribedAsync(userId);
            return Ok(new NewsletterStatusResponse { IsSubscribed = isSubscribed });
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new { error = "Email is required for anonymous requests" });
        }

        var isEmailSubscribed = await _newsletterService.IsEmailSubscribedAsync(email);
        return Ok(new NewsletterStatusResponse { IsSubscribed = isEmailSubscribed });
    }

    [HttpPost("subscribe")]
    public async Task<ActionResult<SubscribeResponse>> Subscribe([FromBody] SubscribeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { error = "Email is required" });
        }

        var subClaim = User.FindFirstValue("sub");

        if (!string.IsNullOrEmpty(subClaim) && int.TryParse(subClaim, out int userId))
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            if (!string.Equals(userEmail, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Email must match authenticated account" });
            }

            var result = await _newsletterService.SubscribeAsync(userId, request.Email);
            return Ok(result);
        }

        var emailResult = await _newsletterService.SubscribeByEmailAsync(request.Email);
        return Ok(emailResult);
    }
}