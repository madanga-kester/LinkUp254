
using System.ComponentModel.DataAnnotations;


namespace LinkUp254.Features.Post.Posts
{
    public class CreateCommentDto
    {
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
