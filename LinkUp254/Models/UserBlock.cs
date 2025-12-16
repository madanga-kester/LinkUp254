using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class UserBlock : BaseEntity
    {
        public int BlockerId { get; set; }
        public int BlockedId { get; set; }

        [ForeignKey(nameof(BlockerId))]
        public Users Blocker { get; set; } = null!;

        [ForeignKey(nameof(BlockedId))]
        public Users Blocked { get; set; } = null!;
    }
}
