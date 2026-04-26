using LinkUp254.Features.Events.models;
using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events.Models;

public class Ticket : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string TicketCode { get; set; } = null!;

    [StringLength(200)]
    public string? BuyerName { get; set; }

    [StringLength(254)]
    public string? BuyerEmail { get; set; }

    [StringLength(20)]
    public string? BuyerPhoneNumber { get; set; }

    [StringLength(200)]
    public string? AttendeeName { get; set; }

    [StringLength(254)]
    public string? AttendeeEmail { get; set; }

    [StringLength(20)]
    public string? AttendeePhoneNumber { get; set; }

    [StringLength(500)]
    public string? StudentIdImageUrl { get; set; }
    public bool IsStudentIdVerified { get; set; } = false;

    [Required]
    [ForeignKey(nameof(TicketTier))]
    public int TicketTierId { get; set; }
    public TicketTier TicketTier { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(Event))]
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePaid { get; set; }

    [Required]
    public int Quantity { get; set; } = 1;

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? PaymentProvider { get; set; }

    [StringLength(100)]
    public string? ProviderTransactionId { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public TicketStatus TicketStatus { get; set; } = TicketStatus.Reserved;

    public bool CheckedIn { get; set; } = false;
    public DateTime? CheckedInAt { get; set; }

    [StringLength(100)]
    public string? CheckedInByUserId { get; set; }

    [StringLength(100)]
    public string? CheckInDeviceId { get; set; }

    public bool IsTransferred { get; set; } = false;
    public DateTime? TransferredAt { get; set; }

    [StringLength(254)]
    public string? TransferredToEmail { get; set; }

    public bool IsRefunded { get; set; } = false;
    public DateTime? RefundedAt { get; set; }

    [StringLength(500)]
    public string? RefundReason { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? RefundAmount { get; set; }

    [StringLength(500)]
    public string? QRCodeData { get; set; }

    [StringLength(500)]
    public string? QRCodeImageUrl { get; set; }

    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;

    public bool SmsSent { get; set; } = false;
    public DateTime? SmsSentAt { get; set; }
    public string? SmsDeliveryStatus { get; set; }

    [StringLength(2000)]
    public string? Metadata { get; set; }

    [StringLength(100)]
    public string? BuyerUserId { get; set; }

    [StringLength(100)]
    public string? IdempotencyKey { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public static string GenerateTicketCode(int eventId, int tierId)
    {
        var random = new Random();
        var chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new string(Enumerable.Range(0, 6)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
        return $"TKT-{eventId}-{code}";
    }

    public bool IsValidForEntry()
    {
        return TicketStatus == TicketStatus.Active
            && !CheckedIn
            && !IsRefunded
            && VerificationStatus != VerificationStatus.Invalid;
    }
}