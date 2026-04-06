using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events.models;

public class Event : BaseEntity
{
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

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    public decimal? Price { get; set; }
    public bool IsFree => !Price.HasValue || Price == 0;

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

    // Navigation propertie
    public ICollection<EventAttendee> EventAttendees { get; set; } = new List<EventAttendee>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    //  Many-to-Many with Interests 
    public ICollection<LinkUp254.Features.Events.models.EventInterest> EventInterests { get; set; } = new List<LinkUp254.Features.Events.models.EventInterest>();

    
    public bool IsUpcoming => StartTime > DateTime.UtcNow;
    public bool IsOngoing => StartTime <= DateTime.UtcNow && EndTime >= DateTime.UtcNow;
    public bool HasEnded => EndTime < DateTime.UtcNow;

    public Event() { }

    public Event(string title, string description, string city, string country, string location,
                 DateTime startTime, DateTime endTime, int organizerId, string? CoverImage)
    {
        Title = title;
        Description = description;
        City = city;
        Country = country;
        Location = location;
        StartTime = startTime;
        EndTime = endTime;
        OrganizerId = organizerId;
        this.CoverImage = CoverImage;
        IsActive = true;
        IsPublished = false;
        IsPublished = true;  

        CreatedAt = DateTime.UtcNow;
    }
} 