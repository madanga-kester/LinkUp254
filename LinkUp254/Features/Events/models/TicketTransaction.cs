using LinkUp254.Features.Events.Models;
using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events.models;

public class TicketTransaction : BaseEntity
{
   

    [Required]
    [ForeignKey(nameof(Ticket))]
    public new int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    // Payment provider details
    [Required]
    [StringLength(50)]
    public string Provider { get; set; } = string.Empty; // "mpesa"

    [StringLength(100)]
    public string? ProviderTransactionId { get; set; } // M-Pesa receipt 

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = "KES"; 

    public string Status { get; set; } = "pending"; // "pending", "success", "failed", "refunded"

    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // M-Pesa  fields
    [StringLength(20)]
    public string? MpesaPhoneNumber { get; set; } // +254712345678

    [StringLength(100)]
    public string? MpesaReceiptNumber { get; set; }

    public DateTime? MpesaTransactionDate { get; set; }

 
    [StringLength(4000)]
    public string? WebhookPayload { get; set; } 

   
    public int? RefundedTransactionId { get; set; } 

    [StringLength(500)]
    public string? RefundReason { get; set; }

   
    [StringLength(2000)]
    public string? Metadata { get; set; }

    // Navigation
    public ICollection<TicketTransactionRefund>? Refunds { get; set; }
}


public class TicketTransactionRefund : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(TicketTransaction))]
    public int TransactionId { get; set; }
    public TicketTransaction Transaction { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RefundAmount { get; set; }

    [Required]
    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = "pending"; // "pending", "success", "failed"

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }

    [StringLength(100)]
    public string? ProviderRefundId { get; set; } 

    [StringLength(500)]
    public string? Notes { get; set; }
}