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
    public string? Location { get; set; }

    // Date range
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // Price filters
    public bool? IsFreeOnly { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Event type filters
    public bool? IsVirtual { get; set; }
    public bool? IsInPerson { get; set; }

    // Interest filtering
    public string? InterestIds { get; set; }

    // Age restrictions
    public bool? AgeRestricted { get; set; }
    public int? MinAge { get; set; }

    //  Publish/Active state filters (required by frontend)
    public bool? IsPublished { get; set; }
    public bool? IsActive { get; set; }

    // Pagination
    [Range(1, 100)]
    public int Limit { get; set; } = 20;
    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;

    // Sort
    public string? SortBy { get; set; } = "date_desc";

    public string GetSortField() => SortBy?.ToLower() switch
    {
        "starttime" or "start" or "date" or "time" => "StartTime",
        "title" or "name" => "Title",
        "city" or "location" => "City",
        "price" or "cost" => "Price",
        "popularity" or "relevance" => "AttendeeCount",
        "ending" or "ending_soon" => "EndTime",
        _ => "StartTime"
    };
}