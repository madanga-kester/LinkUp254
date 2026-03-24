using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Post.Posts
{
    public class CreatePostDto
    

        {
            [Required]
            [MaxLength(2000)]
            public string Content { get; set; } = string.Empty;

            public bool IsPublic { get; set; } = true;
        }
    }
