using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Events
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

        



        [Required] public int HostId { get; set; }
        public string HostName { get; set; } = string.Empty;




        public ICollection<EventAttendee> EventAttendees { get; set; } = new List<EventAttendee>();

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