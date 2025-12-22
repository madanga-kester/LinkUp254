using System.ComponentModel.DataAnnotations;

namespace LinkUp254.DTOs.Posts
{
    public class CreatePostInPut
    

        {
            [Required]
            [MaxLength(2000)]
            public string Content { get; set; } = string.Empty;

            public bool IsPublic { get; set; } = true;
        }
    }
