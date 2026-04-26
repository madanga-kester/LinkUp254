using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.Events.Models;
using LinkUp254.Features.Events.Services;
using LinkUp254.Features.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LinkUp254.Features.Events.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly TicketServices _ticketServices;
    private readonly LinkUpContext _context;

    public TicketsController(TicketServices ticketServices, LinkUpContext context)
    {
        _ticketServices = ticketServices;
        _context = context;
    }

    [HttpPost("events/{eventId:int}/ticket-tiers")]
    [Authorize]
    public async Task<ActionResult<object>> CreateTicketTier(
        [FromRoute] int eventId,
        [FromBody] CreateTicketTierDto dto)
    {
        var organizerId = GetCurrentUserId();
        if (organizerId == null)
            return Unauthorized(new { message = "Authentication required" });

        var result = await _ticketServices.CreateTicketTierAsync(eventId, dto, organizerId.Value);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message });

        var newTier = await _context.TicketTiers
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(t => t.EventId == eventId && t.Name == dto.Name);

        return Ok(new
        {
            isSuccess = true,
            message = result.Message,
            tierId = newTier?.Id
        });
    }

    [HttpGet("events/{eventId:int}/ticket-tiers")]
    [AllowAnonymous]
    public async Task<ActionResult<List<TicketTierDto>>> GetAvailableTiers([FromRoute] int eventId)
    {
        var tiers = await _ticketServices.GetAvailableTiersAsync(eventId);
        return Ok(tiers);
    }

    [HttpPut("ticket-tiers/{id:int}")]
    [Authorize]
    public async Task<ActionResult<AuthResult>> UpdateTicketTier(
        [FromRoute] int id,
        [FromBody] UpdateTicketTierDto dto)
    {
        var organizerId = GetCurrentUserId();
        if (organizerId == null)
            return Unauthorized(new { message = "Authentication required" });

        var result = await _ticketServices.UpdateTicketTierAsync(id, dto, organizerId.Value);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("purchase")]
    [Authorize]
    public async Task<ActionResult<PurchaseTicketResult>> PurchaseTickets(
        [FromBody] PurchaseTicketDto dto,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        var userId = GetCurrentUserIdString();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        var effectiveIdempotencyKey = idempotencyKey ?? dto.IdempotencyKey;
        var result = await _ticketServices.PurchaseTicketsAsync(dto, userId, effectiveIdempotencyKey);

        if (!result.IsSuccess)
        {
            if (result.Message.Contains("availability changed", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("Not enough tickets", StringComparison.OrdinalIgnoreCase))
                return Conflict(result);

            return BadRequest(result);
        }

        if (result.RequiresPayment && result.PaymentProvider?.ToLower() == "mpesa")
            return Accepted(result);

        return Ok(result);
    }

    [HttpPost("payments/mpesa/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> MpesaCallback([FromBody] MpesaCallbackDto dto)
    {
        if (!VerifyMpesaWebhookSignature(dto))
            return BadRequest(new { message = "Invalid webhook signature" });

        var ticketId = ExtractTicketIdFromReference(dto.AccountReference);
        if (ticketId == null)
            return BadRequest(new { message = "Invalid account reference" });

        var result = await _ticketServices.ConfirmPaymentAsync(ticketId.Value, new ConfirmPaymentDto
        {
            IsSuccess = dto.ResultCode == 0,
            ProviderTransactionId = dto.MpesaReceiptNumber
        });

        return result.IsSuccess
            ? Ok(new { message = "Payment confirmed" })
            : BadRequest(new { message = result.Message });
    }

    [HttpPost("{code}/validate")]
    [Authorize]
    public async Task<ActionResult<ValidateTicketResult>> ValidateTicket(
        [FromRoute] string code,
        [FromBody] ValidateTicketRequest? dto = null)
    {
        var scannerUserId = GetCurrentUserIdString();
        var result = await _ticketServices.ValidateTicketAsync(code, scannerUserId);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("validate-offline")]
    [AllowAnonymous]
    public IActionResult ValidateTicketOffline([FromBody] OfflineValidateRequest dto)
    {
        var isValid = _ticketServices.VerifyTicketOffline(
            dto.TicketCode,
            dto.QRCodeData,
            dto.Signature);

        return isValid
            ? Ok(new { isValid = true, message = "Ticket signature valid" })
            : BadRequest(new { isValid = false, message = "Invalid ticket signature" });
    }

    [HttpPost("{id:int}/refund")]
    [Authorize]
    public async Task<ActionResult<AuthResult>> ProcessRefund(
        [FromRoute] int id,
        [FromBody] RefundTicketDto dto)
    {
        var organizerId = GetCurrentUserId();
        if (organizerId == null)
            return Unauthorized(new { message = "Authentication required" });

        var result = await _ticketServices.ProcessRefundAsync(id, dto, organizerId.Value);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id:int}/transfer")]
    [Authorize]
    public async Task<ActionResult<AuthResult>> TransferTicket(
        [FromRoute] int id,
        [FromBody] TransferTicketDto dto)
    {
        var userId = GetCurrentUserIdString();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _ticketServices.TransferTicketAsync(id, dto, userId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<TicketDetailsDto>> GetTicketByCode([FromRoute] string code)
    {
        var ticket = await _context.Tickets
            .Include(t => t.TicketTier)
            .Include(t => t.Event)
            .FirstOrDefaultAsync(t => t.TicketCode == code);

        if (ticket == null)
            return NotFound(new { message = "Ticket not found" });

        var dto = new TicketDetailsDto
        {
            TicketCode = ticket.TicketCode,
            EventTitle = ticket.Event?.Title ?? "Unknown Event",
            TicketTier = ticket.TicketTier?.Name ?? "Unknown Tier",
            AttendeeName = ticket.AttendeeName ?? ticket.BuyerName ?? "Unknown",
            EventStartTime = ticket.Event?.StartTime ?? DateTime.MinValue,
            Venue = ticket.Event?.Venue ?? "Unknown Venue",
            QRCodeImageUrl = ticket.QRCodeImageUrl,
            IsCheckedIn = ticket.CheckedIn,
            Status = ticket.TicketStatus.ToString().ToLowerInvariant()
        };

        return Ok(dto);
    }

    private int? GetCurrentUserId()
    {
        var claimValue =
            User.FindFirst("sub")?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(claimValue, out var id) ? id : null;
    }

    private string? GetCurrentUserIdString()
    {
        return User.FindFirst("sub")?.Value ??
               User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private bool VerifyMpesaWebhookSignature(MpesaCallbackDto dto) => true;

    private int? ExtractTicketIdFromReference(string accountReference)
    {
        if (string.IsNullOrEmpty(accountReference) || !accountReference.StartsWith("LINKUP-"))
            return null;

        var idPart = accountReference.Substring("LINKUP-".Length);
        return int.TryParse(idPart, out var id) ? id : null;
    }
}



