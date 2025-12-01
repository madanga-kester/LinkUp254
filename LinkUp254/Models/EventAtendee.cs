using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LinkUp254.Models
{
    [PrimaryKey(nameof(EventId), nameof(UserId))]
    public class EventAtendee : BaseEntity
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int UserId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public Users User { get; set; } = null!;

        // Optional: extra payload (e.g., RSVP status, joined date, etc.)
        public string? RsvpStatus { get; set; } = "Going";

        // Constructors
        public EventAtendee() { }

        public EventAtendee(int eventId, int userId)
        {
            EventId = eventId;
            UserId = userId;
        }
    }
}