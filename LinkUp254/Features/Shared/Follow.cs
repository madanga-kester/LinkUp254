using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Shared
{
    public class Follow : BaseEntity
    {
        public int FollowerId { get; set; }
        public int FollowingId { get; set; }

        [ForeignKey(nameof(FollowerId))]
        public User Follower { get; set; } = null!;

        [ForeignKey(nameof(FollowingId))]
        public User Following { get; set; } = null!;
    }
}
