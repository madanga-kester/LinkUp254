using LinkUp254.Features.Events.Models;
using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events.models;

public class TicketCheckIn : BaseEntity
{
   

    [Required]
    [ForeignKey(nameof(Ticket))]
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    [Required]
    public DateTime CheckedInAt { get; set; } = DateTime.UtcNow;

    // Who scanned the ticket
    [StringLength(100)]
    public string? ScannedByUserId { get; set; } // Organizer's user ID

    [StringLength(100)]
    public string? ScannedByDeviceId { get; set; } 

    
    [StringLength(100)]
    public string? CheckInPoint { get; set; } 

    [Required]
    public bool IsValid { get; set; } = true;

    [StringLength(200)]
    public string? FailureReason { get; set; } // "Already used", "Invalid signature", etc.

   
    public bool WasOffline { get; set; } = false;
    public DateTime? SyncedAt { get; set; } 
    
    [StringLength(2000)]
    public string? Metadata { get; set; } 
}