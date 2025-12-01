using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Models
{
    public class Event : BaseEntity
    {
        [Required] public string EventId { get; set; } = string.Empty;
        [Required] public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required] public string Location { get; set; } = string.Empty;
        [Required] public DateTime StartTime { get; set; }
        [Required] public DateTime EndTime { get; set; }
        public decimal? Price { get; set; }
        public string? CoverImage { get; set; }

        // Foreign key only — no navigation property = no ambiguity
        [Required] public int HostId { get; set; }
        public string HostName { get; set; } = string.Empty;

        // Many-to-many via explicit join entity
        public ICollection<EventAtendee> EventAtendees { get; set; } = new List<EventAtendee>();

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        public Event() { }

        public Event(string eventId, string title, string description, string location, int hostId, string hostName)
        {
            EventId = eventId;
            Title = title;
            Description = description;
            Location = location;
            HostId = hostId;
            HostName = hostName;
        }
    }
}