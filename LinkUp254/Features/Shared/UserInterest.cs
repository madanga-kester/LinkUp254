using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Shared
{
    public class UserInterest : BaseEntity
    {
        public int UserId { get; set; }
        public int InterestId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("InterestId")]
        public Interest Interest { get; set; } = null!;

        // Optional tracking fields
        public int EngagementTotals { get; set; } = 0;  // based on events attended or chats

        public UserInterest() { }

        public UserInterest(int userId, int interestId) : base()
        {
            UserId = userId;
            InterestId = interestId;
        }
    }
}
