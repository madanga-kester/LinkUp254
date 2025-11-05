using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LinkUp254.Models
{
    public class ChatMessage : BaseEntity
    {
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        
        
        //Message sender

       

        // Sender of the message (link to Users)
        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        public Users? Sender { get; set; }

        // Optional: Message sent in a Group
        public int? GroupId { get; set; }




        [ForeignKey("GroupId")]
        public Group? Group { get; set; }

        // Optional: Message sent in an Event
        public int? EventId { get; set; }




        [ForeignKey("EventId")]
        public Event? Event { get; set; }

     
        
        
        // Optional: Track if receiver read it
        public bool IsRead { get; set; } = false;

        public bool IsDeleted { get; set; } = false;



    }
}
