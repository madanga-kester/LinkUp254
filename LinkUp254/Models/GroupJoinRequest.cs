using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class GroupJoinRequest : BaseEntity
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected

        [ForeignKey("GroupId")]
        public Group Group { get; set; } = null!;

        [ForeignKey("UserId")]
        public Users User { get; set; } = null!;

        public GroupJoinRequest() { }

        public GroupJoinRequest(int groupId, int userId) : base()
        {
            GroupId = groupId;
            UserId = userId;
        }
    }
}
