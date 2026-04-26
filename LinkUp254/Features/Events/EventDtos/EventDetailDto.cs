



using LinkUp254.Features.Shared;
using UserDto = LinkUp254.Features.Shared.UserDto;

namespace LinkUp254.Features.Events.DTOs;


public class EventDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Location { get; set; }

    
    public string? VenueName { get; set; }
    public string? StreetAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? MapProviderPlaceId { get; set; }
    public int LocationVisibility { get; set; }

    public string DisplayLocation { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal? Price { get; set; }
    public bool IsFree { get; set; }
    public string? CoverImage { get; set; }

    public int OrganizerId { get; set; }
    public UserDto? Organizer { get; set; }

    public bool IsActive { get; set; }
    public bool IsPublished { get; set; }
    public int AttendeeCount { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int? MaxAttendees { get; set; }
    public bool IsFull { get; set; }
    public float RelevanceScore { get; set; }
    public int Visibility { get; set; }

    public bool IsUpcoming { get; set; }
    public bool IsOngoing { get; set; }
    public bool HasEnded { get; set; }


  
    public double? DistanceKm { get; set; }
}