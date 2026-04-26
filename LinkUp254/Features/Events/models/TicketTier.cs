using LinkUp254.Features.Events.models;
using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events.Models;

public class TicketTier : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public int Capacity { get; set; }

    public int SoldCount { get; set; } = 0;

    public int? MinPerOrder { get; set; } = 1;
    public int? MaxPerOrder { get; set; } = 10;

    public DateTime? SaleStartsAt { get; set; }
    public DateTime? SaleEndsAt { get; set; }

    public bool RequirePhoneNumber { get; set; } = true;
    public bool RequireStudentId { get; set; } = false;
    public bool IsTransferable { get; set; } = true;
    public bool IsRefundable { get; set; } = true;
    public int? RefundDeadlineHours { get; set; } = 24;

    public bool IsTierActive { get; set; } = true;

    [Required]
    [ForeignKey(nameof(Event))]
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public bool IsAvailableForPurchase()
    {
        if (!IsTierActive) return false;
        if (SoldCount >= Capacity) return false;

        var now = DateTime.UtcNow;
        if (SaleStartsAt.HasValue && now < SaleStartsAt.Value) return false;
        if (SaleEndsAt.HasValue && now > SaleEndsAt.Value) return false;

        return true;
    }

    public int RemainingCapacity => Math.Max(0, Capacity - SoldCount);
}