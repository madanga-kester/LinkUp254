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

   




    [StringLength(200)]
    public string? VenueName { get; set; }

    [StringLength(500)]
    public string? StreetAddress { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [StringLength(100)]
    public string? MapProviderPlaceId { get; set; }


    [Range(0, 2)]
    public int? LocationVisibility { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? Price { get; set; }

    public int? GroupId { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int? MaxAttendees { get; set; }

    public bool? IsPublished { get; set; } = false;

    public List<int>? InterestIds { get; set; }

    // Content Visibility (0=Public, 1=GroupOnly, 2=Private)
    public int? Visibility { get; set; }

    
    public bool? IsFree { get; set; }
}