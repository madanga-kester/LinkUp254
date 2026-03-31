using LinkUp254.Features.Events;
using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using GroupEntity = LinkUp254.Features.Groups.Group;

namespace LinkUp254.Features.Group.Messages
{
    public class ChatMessage : BaseEntity
    {
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        public User? Sender { get; set; }

        public int? GroupId { get; set; }

        [ForeignKey("GroupId")]
        public GroupEntity? Group { get; set; }

        public int? EventId { get; set; }

        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}