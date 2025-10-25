using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LinkUp254.Models
{
    public class Event : BaseEntity
    {
        [Required]
        public string EventId { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        [Required]
        public string Location { get; set; } = string.Empty;
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }


        public decimal? Price { get; set; }

        public string? CoverImage { get; set; }

        public int HostId { get; set; }
       public string HostName { get; set; } = string.Empty;


        [ForeignKey("HostId")]
        public Users Host { get; set; } = null!;



        public ICollection<Users> Attendees { get; set; } = new List<Users>();


        public ICollection<EventAtendee> EventAtendee { get; set; } = new List<EventAtendee>();


        public Event() {}

        public Event(string eventid, string title, string description, string location, int hostId, string hostName)
        {
            EventId = eventid;
            Title = title;
            Description = description;
            Location = location;
            HostId = hostId;
            HostName = hostName;

        }
    }
}
