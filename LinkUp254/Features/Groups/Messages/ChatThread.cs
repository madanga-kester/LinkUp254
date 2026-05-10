using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Group.Messages
{
    public class ChatThread : BaseEntity
    {
        [Required]
        public string ThreadName { get; set; } = string.Empty;

      
        public ICollection<User> Participants { get; set; } = new List<User>();

     
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public ChatThread() { }

        public ChatThread(string threadName) : base()
        {
            ThreadName = threadName;
        }
    }
}
