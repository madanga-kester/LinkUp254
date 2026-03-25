using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Shared
{
    public class UserBlock : BaseEntity
    {
        public int BlockerId { get; set; }
        public int BlockedId { get; set; }

        [ForeignKey(nameof(BlockerId))]
        public User Blocker { get; set; } = null!;

        [ForeignKey(nameof(BlockedId))]
        public User Blocked { get; set; } = null!;
    }
}
