using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Models
{
    public class ChatThread : BaseEntity
    {
        [Required]
        public string ThreadName { get; set; } = string.Empty;

        // Users participating in this thread
        public ICollection<Users> Participants { get; set; } = new List<Users>();

        // Messages in this thread
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public ChatThread() { }

        public ChatThread(string threadName) : base()
        {
            ThreadName = threadName;
        }
    }
}
