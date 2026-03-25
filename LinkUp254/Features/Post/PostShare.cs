using LinkUp254.Features.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Post
{
    public class PostShare : BaseEntity
    {
        public int OriginalPostId { get; set; }
        public int UserId { get; set; }

        public string? Caption { get; set; }

        [ForeignKey(nameof(OriginalPostId))]
        public Post OriginalPost { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
