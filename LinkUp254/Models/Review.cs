using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class Review : BaseEntity
    {
        public int UserId { get; set; }
        public int EventId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public Users User { get; set; } = null!;

        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;

        public Review() { }

        public Review(int userId, int eventId, int rating, string comment) : base()
        {
            UserId = userId;
            EventId = eventId;
            Rating = rating;
            Comment = comment;
        }
    }
}
