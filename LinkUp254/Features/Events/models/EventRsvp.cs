// src/Features/Events/Models/EventRsvp.cs
using LinkUp254.Features.Events.models;
using LinkUp254.Features.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events.Models;

[Table("EventRsvps")]
public class EventRsvp
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EventId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required, StringLength(50)]
    public string Status { get; set; } = "going"; // "going", "interested", "none"

    public DateTime RsvpedAt { get; set; } = DateTime.UtcNow;

    public int? TicketTierId { get; set; }
    public int GuestCount { get; set; } = 1;

    // Navigation properties
    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(TicketTierId))]
    public TicketTier? TicketTier { get; set; }
}