using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Events.DTOs
{
    public class EventFilterDto
    {
        
        public string? Interests { get; set; }

        // location 
        public string? City { get; set; }
        public string? Country { get; set; }

        //  date range
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Pagination
        [Range(1, 100)]
        public int Limit { get; set; } = 20;

        [Range(0, int.MaxValue)]
        public int Offset { get; set; } = 0;

        // Sort 
        [RegularExpression("^(date_asc|date_desc|popularity|relevance)$")]
        public string SortBy { get; set; } = "relevance";
    }
}