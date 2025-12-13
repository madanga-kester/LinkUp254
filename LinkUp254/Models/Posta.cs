using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class Post : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(3000)]
        public string Content { get; set; } = string.Empty;

        public string? Visibility { get; set; } = "Public"; // Public, Followers, Private

        [ForeignKey(nameof(UserId))]
        public Users Author { get; set; } = null!;

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
       // public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
       // public ICollection<PostShare> Shares { get; set; } = new List<PostShare>();
        public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();
    }
}
