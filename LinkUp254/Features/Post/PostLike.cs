using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Post
{
    public class PostLike : BaseEntity
    {
        public int PostId { get; set; }
        public int UserId { get; set; }

        [ForeignKey(nameof(PostId))]
        public Post Post { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
