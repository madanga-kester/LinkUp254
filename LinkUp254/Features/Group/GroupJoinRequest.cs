using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Groups
{
    public class GroupJoinRequest : BaseEntity
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected

        [ForeignKey("GroupId")]
        public Group Group { get; set; } = null!;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public GroupJoinRequest() { }

        public GroupJoinRequest(int groupId, int userId) : base()
        {
            GroupId = groupId;
            UserId = userId;
        }
    }
}

