using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class PostLike : BaseEntity
    {
        public int PostId { get; set; }
        public int UserId { get; set; }

        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public Users User { get; set; } = null!;
    }
}
