using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Events
{
    [PrimaryKey(nameof(EventId), nameof(UserId))]
    public class EventAttendee : BaseEntity
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int UserId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Optional extra info
        public string? RsvpStatus { get; set; } = "Going";

        // Constructors
        public EventAttendee() { }

        public EventAttendee(int eventId, int userId)
        {
            EventId = eventId;
            UserId = userId;
        }
    }
}