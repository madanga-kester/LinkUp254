using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Organizers.DTOs;

public class FollowOrganizerDto
{
    public int OrganizerId { get; set; }
}

public class RateOrganizerDto
{
    public int OrganizerId { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}

public class ContactOrganizerDto
{
    public int OrganizerId { get; set; }

    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
}