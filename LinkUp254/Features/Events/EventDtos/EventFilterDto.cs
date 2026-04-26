using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Events.DTOs;

public class EventFilterDto
{
   
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
    public bool? IsFreeOnly { get; set; }  // Show only free events
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Interest filtering 
    public string? InterestIds { get; set; }

    // Pagination
    [Range(1, 100)]
    public int Limit { get; set; } = 20;

    

    [Range(0, int.MaxValue)]
    public int Offset { get; set; } = 0;
    public string? Search { get; set; }

    // Sort 
    [RegularExpression("^(date_asc|date_desc|popularity|price_asc|price_desc|relevance)$")]
    public string SortBy { get; set; } = "relevance";


}