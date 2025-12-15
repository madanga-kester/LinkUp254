using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class Follow : BaseEntity
    {
        public int FollowerId { get; set; }
        public int FollowingId { get; set; }

        [ForeignKey(nameof(FollowerId))]
        public Users Follower { get; set; } = null!;

        [ForeignKey(nameof(FollowingId))]
        public Users Following { get; set; } = null!;
    }
}
