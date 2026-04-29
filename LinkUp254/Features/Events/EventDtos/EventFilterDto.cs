// src/Features/Events/DTOs/EventFilterDto.cs
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Events.DTOs;

public class EventFilterDto
{
    // Search
    [StringLength(500)]
    public string? Search { get; set; }

    // Location
    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(500)]
    public string? Location { get; set; }  // Venue name/address

    // Date range (filters on StartTime)
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Price filters
    public bool? IsFreeOnly { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    //   Event type filters (virtual/in-person)
    public bool? IsVirtual { get; set; }
    public bool? IsInPerson { get; set; }

    // Interest filtering (comma-separated IDs: "1,2,3")
    public string? InterestIds { get; set; }

    // Age restrictions
    public bool? AgeRestricted { get; set; }
    public int? MinAge { get; set; }

    // Pagination
    [Range(1, 100)]
    public int Limit { get; set; } = 20;

    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;

    // Sort options
    [RegularExpression("^(date_asc|date_desc|popularity|price_asc|price_desc|relevance|ending_soon)$")]
    public string SortBy { get; set; } = "relevance";
}