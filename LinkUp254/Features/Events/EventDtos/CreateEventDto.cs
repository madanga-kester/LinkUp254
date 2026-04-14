using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Events.DTOs;

public class CreateEventDto
{
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(500)]
    public string? Location { get; set; }  

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? Price { get; set; }

    public int? GroupId { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }  

    public int? MaxAttendees { get; set; }

    public bool? IsPublished { get; set; } = false;  

    // Interest IDs to associate with event
    public List<int>? InterestIds { get; set; }

    // Visibility setting (0=Public, 1=GroupOnly, 2=Private)
    public int? Visibility { get; set; }

    // : Explicit IsFree flag (in case Price=0 but not free)
    public bool? IsFree { get; set; }
}