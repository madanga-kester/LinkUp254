using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class PostShare : BaseEntity
    {
        public int OriginalPostId { get; set; }
        public int UserId { get; set; }

        public string? Caption { get; set; }

        [ForeignKey(nameof(OriginalPostId))]
        public Post OriginalPost { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public Users User { get; set; } = null!;
    }
}
