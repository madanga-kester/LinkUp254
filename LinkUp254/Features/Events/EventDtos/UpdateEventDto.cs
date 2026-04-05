using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Events.DTOs;

public class UpdateEventDto
{
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(500)]
    public string? Location { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public decimal? Price { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int? MaxAttendees { get; set; }

    public bool? IsPublished { get; set; }

    
    public List<int>? InterestIds { get; set; }
}