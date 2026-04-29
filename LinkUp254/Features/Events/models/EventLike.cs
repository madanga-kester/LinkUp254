
using LinkUp254.Features.Events.models;
using LinkUp254.Features.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events.Models;

[Table("EventLikes")]
public class EventLike
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EventId { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(EventId))]
    public Event Event { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}