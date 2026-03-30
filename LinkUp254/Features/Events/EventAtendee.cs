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



        
        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;




        public string? RsvpStatus { get; set; } = "Going";



        public EventAttendee() { }

        public EventAttendee(int eventId, int userId)
        {
            EventId = eventId;
            UserId = userId;
        }
    }
}