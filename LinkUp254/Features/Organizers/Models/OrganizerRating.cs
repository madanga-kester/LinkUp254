using System;
using System.ComponentModel.DataAnnotations;
using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Organizers.Models;

public class OrganizerRating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OrganizerId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual User Organizer { get; set; } = null!;
}