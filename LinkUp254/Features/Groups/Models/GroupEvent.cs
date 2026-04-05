using System;
using System.ComponentModel.DataAnnotations;
using LinkUp254.Features.Events.models;

namespace LinkUp254.Features.Groups.Models;

public class GroupEvent
{
    [Key]
    public int Id { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
