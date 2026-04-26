using LinkUp254.Features.Events.Models;
using LinkUp254.Features.Groups.Models;
using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events.models;

public class Event : BaseEntity
{
    [StringLength(500)]
    public string? Venue { get; set; }

    [Key]
    public new int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    
    [StringLength(500)]
    public string Location { get; set; } = string.Empty;



    [StringLength(200)]
    public string? VenueName { get; set; }

    [StringLength(500)]
    public string? StreetAddress { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    [StringLength(100)]
    public string? MapProviderPlaceId { get; set; }


    public int LocationVisibility { get; set; } = 0;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    public decimal? Price { get; set; }

    public bool IsFree => !Price.HasValue || Price.Value == 0;

    [StringLength(500)]
    public string? CoverImage { get; set; }

    [Required]
    public int OrganizerId { get; set; }

    [ForeignKey(nameof(OrganizerId))]
    public User Organizer { get; set; } = null!;

    public new bool IsActive { get; set; } = true;
    public bool IsPublished { get; set; } = false;

    public int AttendeeCount { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;

    public int? MaxAttendees { get; set; }

    public bool IsFull => MaxAttendees.HasValue && AttendeeCount >= MaxAttendees.Value;

    public float RelevanceScore { get; set; } = 0f;

    // Content Visibility: 0=Public, 1=GroupOnly, 2=Private
    public int Visibility { get; set; } = 0;

    // Navigation properties
    public ICollection<EventAttendee> EventAttendees { get; set; } = new List<EventAttendee>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<EventInterest> EventInterests { get; set; } = new List<EventInterest>();
    public ICollection<GroupEvent> GroupEvents { get; set; } = new List<GroupEvent>();





    // Ticketing navigation properties
    public ICollection<TicketTier> TicketTiers { get; set; } = new List<TicketTier>();
   






    public bool IsUpcoming => StartTime > DateTime.UtcNow;
    public bool IsOngoing => StartTime <= DateTime.UtcNow && EndTime >= DateTime.UtcNow;
    public bool HasEnded => EndTime < DateTime.UtcNow;

    
    public string GetDisplayLocation()
    {
        return LocationVisibility switch
        {
            0 => !string.IsNullOrEmpty(VenueName) ? $"{VenueName}, {City}" : $"{City}, {Country}",
            1 => $"{City}, {Country}",
            _ => "Location shared upon RSVP"
        };
    }

    public Event() { }

    public Event(string title, string description, string city, string country,
                 string location, DateTime startTime, DateTime endTime, int organizerId)
    {
        Title = title;
        Description = description;
        City = city;
        Country = country;
        Location = location;
        StartTime = startTime;
        EndTime = endTime;
        OrganizerId = organizerId;

        IsActive = true;
        IsPublished = true;
        Visibility = 0;
        LocationVisibility = 0; // Default to public precise location
        CreatedAt = DateTime.UtcNow;
    }
}