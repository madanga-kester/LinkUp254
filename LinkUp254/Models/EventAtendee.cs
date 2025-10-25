using System.ComponentModel.DataAnnotations;    
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class EventAtendee : BaseEntity
    {
        [Key]
        public int EventAtendeeId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;


        [ForeignKey("UserId")]
        public Users User { get; set; } = null!;
    



        public EventAtendee() { }


        public EventAtendee(int eventId, int userId)
        {
            EventId = eventId;
            UserId = userId;
        }
    }
}
