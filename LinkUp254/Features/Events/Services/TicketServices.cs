using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.Events.models;
using LinkUp254.Features.Events.Models;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace LinkUp254.Features.Events.Services;

public class TicketServices
{
    private readonly LinkUpContext _context;
    private readonly ILogger<TicketServices> _logger;
    private readonly IConfiguration _config;
    private readonly string _ticketingSecret;

    public TicketServices(LinkUpContext context, ILogger<TicketServices> logger, IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _config = config;

        _ticketingSecret = _config["TICKETING_SECRET"]
            ?? throw new InvalidOperationException("TICKETING_SECRET configuration is required.");

        if (_config["ASPNETCORE_ENVIRONMENT"] != "Development" && _ticketingSecret.StartsWith("LinkUp254_Dev_Secret"))
            throw new InvalidOperationException("Production environment detected but development fallback secret is in use.");
    }

    public async Task<AuthResult> CreateTicketTierAsync(int eventId, CreateTicketTierDto dto, int organizerId)
    {
        try
        {
            var eventEntity = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == eventId && e.OrganizerId == organizerId);

            if (eventEntity == null)
                return AuthResult.Failure("Event not found or you don't have permission to add tickets to it.");

            if (dto.Capacity < 1)
                return AuthResult.Failure("Capacity must be at least 1.");

            if (dto.SaleStartsAt.HasValue && dto.SaleEndsAt.HasValue && dto.SaleStartsAt.Value > dto.SaleEndsAt.Value)
                return AuthResult.Failure("Sale start date cannot be after sale end date.");

            var newTier = new TicketTier
            {
                EventId = eventId,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Price = dto.Price,
                Capacity = dto.Capacity,
                SoldCount = 0,
                MinPerOrder = dto.MinPerOrder ?? 1,
                MaxPerOrder = dto.MaxPerOrder ?? 10,
                SaleStartsAt = dto.SaleStartsAt,
                SaleEndsAt = dto.SaleEndsAt,
                RequirePhoneNumber = dto.RequirePhoneNumber ?? true,
                RequireStudentId = dto.RequireStudentId ?? false,
                IsTransferable = dto.IsTransferable ?? true,
                IsRefundable = dto.IsRefundable ?? true,
                RefundDeadlineHours = dto.RefundDeadlineHours,
                IsTierActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TicketTiers.Add(newTier);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket tier '{TierName}' created for event {EventId} by organizer {OrganizerId}",
                newTier.Name, eventId, organizerId);

            return AuthResult.Success("Ticket tier created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateTicketTierAsync failed for event {EventId}", eventId);
            return AuthResult.Failure("Failed to create ticket tier. Please try again.");
        }
    }

    public async Task<AuthResult> UpdateTicketTierAsync(int tierId, UpdateTicketTierDto dto, int organizerId)
    {
        try
        {
            var tier = await _context.TicketTiers
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == tierId);

            if (tier == null || tier.Event.OrganizerId != organizerId)
                return AuthResult.Failure("Ticket tier not found or you don't have permission to edit it.");

            if (dto.Capacity.HasValue && dto.Capacity.Value < tier.SoldCount)
                return AuthResult.Failure($"Cannot reduce capacity below {tier.SoldCount} (already sold).");

            if (!string.IsNullOrEmpty(dto.Name)) tier.Name = dto.Name.Trim();
            if (dto.Description != null) tier.Description = dto.Description.Trim();
            if (dto.Price.HasValue) tier.Price = dto.Price.Value;
            if (dto.Capacity.HasValue) tier.Capacity = dto.Capacity.Value;
            if (dto.MinPerOrder.HasValue) tier.MinPerOrder = dto.MinPerOrder.Value;
            if (dto.MaxPerOrder.HasValue) tier.MaxPerOrder = dto.MaxPerOrder.Value;
            if (dto.SaleStartsAt.HasValue) tier.SaleStartsAt = dto.SaleStartsAt.Value;
            if (dto.SaleEndsAt.HasValue) tier.SaleEndsAt = dto.SaleEndsAt.Value;
            if (dto.RequirePhoneNumber.HasValue) tier.RequirePhoneNumber = dto.RequirePhoneNumber.Value;
            if (dto.RequireStudentId.HasValue) tier.RequireStudentId = dto.RequireStudentId.Value;
            if (dto.IsTransferable.HasValue) tier.IsTransferable = dto.IsTransferable.Value;
            if (dto.IsRefundable.HasValue) tier.IsRefundable = dto.IsRefundable.Value;
            if (dto.RefundDeadlineHours.HasValue) tier.RefundDeadlineHours = dto.RefundDeadlineHours.Value;
            if (dto.IsActive.HasValue) tier.IsTierActive = dto.IsActive.Value;

            tier.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket tier '{TierName}' updated for event {EventId}", tier.Name, tier.EventId);
            return AuthResult.Success("Ticket tier updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateTicketTierAsync failed for tier {TierId}", tierId);
            return AuthResult.Failure("Failed to update ticket tier.");
        }
    }

    public async Task<List<TicketTierDto>> GetAvailableTiersAsync(int eventId)
    {
        var now = DateTime.UtcNow;

        return await _context.TicketTiers
            .Where(t => t.EventId == eventId
                     && t.IsTierActive
                     && t.SoldCount < t.Capacity
                     && (!t.SaleStartsAt.HasValue || now >= t.SaleStartsAt.Value)
                     && (!t.SaleEndsAt.HasValue || now <= t.SaleEndsAt.Value))
            .Select(t => new TicketTierDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Price = t.Price,
                RemainingCapacity = t.Capacity - t.SoldCount,
                MinPerOrder = t.MinPerOrder,
                MaxPerOrder = t.MaxPerOrder,
                RequirePhoneNumber = t.RequirePhoneNumber,
                RequireStudentId = t.RequireStudentId,
                IsTransferable = t.IsTransferable,
                IsRefundable = t.IsRefundable
            })
            .OrderBy(t => t.Price)
            .ToListAsync();
    }

    public async Task<PurchaseTicketResult> PurchaseTicketsAsync(PurchaseTicketDto dto, string? userId = null, string? idempotencyKey = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(idempotencyKey))
            {
                var existing = await _context.Tickets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey);

                if (existing != null)
                {
                    return new PurchaseTicketResult
                    {
                        IsSuccess = true,
                        TicketId = existing.Id,
                        TicketCode = existing.TicketCode,
                        RequiresPayment = existing.PaymentStatus == PaymentStatus.Pending,
                        PaymentProvider = existing.PaymentProvider,
                        Message = "Request already processed. Returning existing ticket."
                    };
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var tier = await _context.TicketTiers
                    .Include(t => t.Event)
                    .FirstOrDefaultAsync(t => t.Id == dto.TierId);

                if (tier == null || !tier.IsAvailableForPurchase())
                    return PurchaseTicketResult.Failure("Ticket tier not available.");

                if (dto.Quantity < tier.MinPerOrder || dto.Quantity > tier.MaxPerOrder)
                    return PurchaseTicketResult.Failure($"Quantity must be between {tier.MinPerOrder} and {tier.MaxPerOrder}.");

                if (tier.SoldCount + dto.Quantity > tier.Capacity)
                    return PurchaseTicketResult.Failure("Not enough tickets remaining. Please refresh and try again.");

                var totalPrice = tier.Price * dto.Quantity;
                var ticketCode = Ticket.GenerateTicketCode(tier.EventId, tier.Id);

                var newTicket = new Ticket
                {
                    TicketCode = ticketCode,
                    TicketTierId = tier.Id,
                    EventId = tier.EventId,
                    PricePaid = tier.Price,
                    Quantity = dto.Quantity,
                    BuyerName = dto.BuyerName?.Trim(),
                    BuyerEmail = dto.BuyerEmail?.Trim(),
                    BuyerPhoneNumber = dto.BuyerPhoneNumber?.Trim(),
                    AttendeeName = dto.AttendeeName?.Trim() ?? dto.BuyerName?.Trim(),
                    AttendeeEmail = dto.AttendeeEmail?.Trim() ?? dto.BuyerEmail?.Trim(),
                    AttendeePhoneNumber = dto.AttendeePhoneNumber?.Trim() ?? dto.BuyerPhoneNumber?.Trim(),
                    StudentIdImageUrl = dto.StudentIdImageUrl?.Trim(),
                    PaymentProvider = dto.PaymentProvider ?? "manual",
                    PaymentStatus = PaymentStatus.Pending,
                    TicketStatus = TicketStatus.Reserved,
                    PurchasedAt = DateTime.UtcNow,
                    VerificationStatus = VerificationStatus.Unverified,
                    BuyerUserId = userId,
                    IdempotencyKey = idempotencyKey,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                newTicket.QRCodeData = GenerateQRCodeData(newTicket);
                _context.Tickets.Add(newTicket);

                tier.SoldCount += dto.Quantity;
                tier.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (dto.PaymentProvider?.ToLower() == "mpesa" && !string.IsNullOrEmpty(dto.BuyerPhoneNumber))
                {
                    return new PurchaseTicketResult
                    {
                        IsSuccess = true,
                        TicketId = newTicket.Id,
                        TicketCode = newTicket.TicketCode,
                        RequiresPayment = true,
                        PaymentProvider = "mpesa",
                        MpesaPayload = new MpesaStkPayload
                        {
                            PhoneNumber = dto.BuyerPhoneNumber,
                            Amount = (int)Math.Round(totalPrice),
                            AccountReference = $"LINKUP-{newTicket.Id}",
                            TransactionDesc = $"Ticket: {tier.Event.Title}"
                        },
                        Message = "M-Pesa prompt sent to your phone. Approve to complete purchase."
                    };
                }

                return new PurchaseTicketResult
                {
                    IsSuccess = true,
                    TicketId = newTicket.Id,
                    TicketCode = newTicket.TicketCode,
                    RequiresPayment = dto.PaymentProvider != "manual",
                    PaymentProvider = dto.PaymentProvider,
                    Message = dto.PaymentProvider == "manual"
                        ? "Ticket reserved. Awaiting organizer approval."
                        : "Payment initiated. Ticket will be issued upon confirmation."
                };
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Concurrency conflict during purchase for tier {TierId}", dto.TierId);
                return PurchaseTicketResult.Failure("Ticket availability changed. Please refresh and try again.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PurchaseTicketsAsync failed for tier {TierId}", dto.TierId);
            return PurchaseTicketResult.Failure("Failed to purchase tickets. Please try again.");
        }
    }

    public async Task<AuthResult> ConfirmPaymentAsync(int ticketId, ConfirmPaymentDto dto)
    {
        try
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketTier)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null || ticket.PaymentStatus != PaymentStatus.Pending)
                return AuthResult.Failure("Ticket not found or payment already processed.");

            ticket.PaymentStatus = dto.IsSuccess ? PaymentStatus.Success : PaymentStatus.Failed;
            ticket.ProviderTransactionId = dto.ProviderTransactionId;

            if (dto.IsSuccess)
            {
                ticket.TicketStatus = TicketStatus.Active;
                ticket.QRCodeImageUrl = $"/api/tickets/{ticket.Id}/qr-code";

                if (!string.IsNullOrEmpty(ticket.BuyerPhoneNumber) && ticket.TicketTier.RequirePhoneNumber)
                {
                    ticket.SmsSent = true;
                    ticket.SmsSentAt = DateTime.UtcNow;
                    ticket.SmsDeliveryStatus = "sent";
                }

                _logger.LogInformation("Ticket {TicketCode} issued for event {EventId}",
                    ticket.TicketCode, ticket.EventId);
            }
            else
            {
                ticket.TicketStatus = TicketStatus.Cancelled;
                ticket.TicketTier.SoldCount -= ticket.Quantity;
                _logger.LogInformation("Ticket {TicketCode} cancelled due to payment failure", ticket.TicketCode);
            }

            ticket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return dto.IsSuccess
                ? AuthResult.Success("Payment confirmed. Ticket issued.")
                : AuthResult.Failure("Payment failed. Ticket cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConfirmPaymentAsync failed for ticket {TicketId}", ticketId);
            return AuthResult.Failure("Failed to process payment confirmation.");
        }
    }

    public async Task<ValidateTicketResult> ValidateTicketAsync(string ticketCode, string? scannerUserId = null)
    {
        try
        {
            var ticket = await _context.Tickets
                .Include(t => t.Event)
                .Include(t => t.TicketTier)
                .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);

            if (ticket == null)
                return ValidateTicketResult.Failure("Invalid ticket code.");

            if (!ticket.IsValidForEntry())
            {
                var reason = ticket.TicketStatus switch
                {
                    TicketStatus.Used => "Already checked in",
                    TicketStatus.Cancelled => "Cancelled",
                    TicketStatus.Refunded => "Refunded",
                    TicketStatus.Transferred => "Transferred",
                    _ => "Invalid status"
                };
                return ValidateTicketResult.Failure(reason);
            }

            if (!VerifyQRCodeSignature(ticket))
            {
                ticket.VerificationStatus = VerificationStatus.Invalid;
                ticket.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return ValidateTicketResult.Failure("Invalid ticket signature.");
            }

            ticket.CheckedIn = true;
            ticket.CheckedInAt = DateTime.UtcNow;
            ticket.CheckedInByUserId = scannerUserId;
            ticket.VerificationStatus = VerificationStatus.Verified;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var checkIn = new TicketCheckIn
            {
                TicketId = ticket.Id,
                CheckedInAt = ticket.CheckedInAt.Value,
                ScannedByUserId = scannerUserId,
                IsValid = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.TicketCheckIns.Add(checkIn);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketCode} validated for event {EventId} by {ScannerUserId}",
                ticketCode, ticket.EventId, scannerUserId ?? "unknown");

            return new ValidateTicketResult
            {
                IsSuccess = true,
                AttendeeName = ticket.AttendeeName ?? ticket.BuyerName,
                TicketTier = ticket.TicketTier.Name,
                Message = $"Welcome, {ticket.AttendeeName ?? ticket.BuyerName}!"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ValidateTicketAsync failed for code {TicketCode}", ticketCode);
            return ValidateTicketResult.Failure("Validation error. Please try again.");
        }
    }

    public bool VerifyTicketOffline(string ticketCode, string qrCodeData, string signature)
    {
        try
        {
            var expectedSignature = ComputeHMACSHA256($"{ticketCode}:{qrCodeData}", _ticketingSecret);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(signature));
        }
        catch
        {
            return false;
        }
    }

    public async Task<AuthResult> ProcessRefundAsync(int ticketId, RefundTicketDto dto, int organizerId)
    {
        try
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketTier)
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null || ticket.Event.OrganizerId != organizerId)
                return AuthResult.Failure("Ticket not found or you don't have permission to refund it.");

            if (!ticket.TicketTier.IsRefundable)
                return AuthResult.Failure("This ticket tier does not allow refunds.");

            if (ticket.IsRefunded)
                return AuthResult.Failure("Ticket already refunded.");

            if (ticket.TicketTier.RefundDeadlineHours.HasValue)
            {
                var deadline = ticket.Event.StartTime.AddHours(-ticket.TicketTier.RefundDeadlineHours.Value);
                if (DateTime.UtcNow > deadline)
                    return AuthResult.Failure($"Refunds not allowed within {ticket.TicketTier.RefundDeadlineHours} hours of event start.");
            }

            var refundAmount = dto.RefundPercentage.HasValue
                ? ticket.PricePaid * ticket.Quantity * (dto.RefundPercentage.Value / 100m)
                : ticket.PricePaid * ticket.Quantity;

            if (ticket.PaymentProvider?.ToLower() == "mpesa" && !string.IsNullOrEmpty(ticket.BuyerPhoneNumber))
            {
                _logger.LogWarning("M-Pesa refund requested for ticket {TicketCode} - manual processing required",
                    ticket.TicketCode);
            }

            ticket.IsRefunded = true;
            ticket.RefundedAt = DateTime.UtcNow;
            ticket.RefundAmount = refundAmount;
            ticket.RefundReason = dto.Reason?.Trim();
            ticket.TicketStatus = TicketStatus.Refunded;
            ticket.PaymentStatus = PaymentStatus.Refunded;
            ticket.UpdatedAt = DateTime.UtcNow;

            ticket.TicketTier.SoldCount -= ticket.Quantity;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketCode} refunded for event {EventId} by organizer {OrganizerId}",
                ticket.TicketCode, ticket.EventId, organizerId);

            return AuthResult.Success($"Refund of KSh {refundAmount:F2} processed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProcessRefundAsync failed for ticket {TicketId}", ticketId);
            return AuthResult.Failure("Failed to process refund. Please try again.");
        }
    }

    public async Task<AuthResult> TransferTicketAsync(int ticketId, TransferTicketDto dto, string currentUserId)
    {
        try
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketTier)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null || ticket.TicketTier == null ||
                (ticket.BuyerUserId != currentUserId && !ticket.TicketTier.IsTransferable))
                return AuthResult.Failure("Ticket not found or transfers not allowed.");

            if (ticket.IsTransferred || ticket.CheckedIn)
                return AuthResult.Failure("Ticket cannot be transferred after check-in or previous transfer.");

            ticket.AttendeeName = dto.NewAttendeeName?.Trim();
            ticket.AttendeeEmail = dto.NewAttendeeEmail?.Trim();
            ticket.AttendeePhoneNumber = dto.NewAttendeePhoneNumber?.Trim();
            ticket.IsTransferred = true;
            ticket.TransferredAt = DateTime.UtcNow;
            ticket.TransferredToEmail = dto.NewAttendeeEmail?.Trim();
            ticket.TicketStatus = TicketStatus.Transferred;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket {TicketCode} transferred to {NewAttendeeEmail}",
                ticket.TicketCode, dto.NewAttendeeEmail);

            return AuthResult.Success("Ticket transferred successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TransferTicketAsync failed for ticket {TicketId}", ticketId);
            return AuthResult.Failure("Failed to transfer ticket.");
        }
    }

    private string GenerateQRCodeData(Ticket ticket)
    {
        var data = $"{ticket.TicketCode}:{ticket.EventId}:{ticket.TicketTierId}:{ticket.PricePaid}:{ticket.PurchasedAt:yyyy-MM-ddTHH:mm:ssZ}";
        var signature = ComputeHMACSHA256(data, _ticketingSecret);
        return $"{data}|{signature}";
    }

    private bool VerifyQRCodeSignature(Ticket ticket)
    {
        if (string.IsNullOrEmpty(ticket.QRCodeData)) return false;

        var parts = ticket.QRCodeData.Split('|');
        if (parts.Length != 2) return false;

        var data = parts[0];
        var providedSignature = parts[1];
        var expectedSignature = ComputeHMACSHA256(data, _ticketingSecret);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedSignature),
            Encoding.UTF8.GetBytes(providedSignature));
    }

    private string ComputeHMACSHA256(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}



