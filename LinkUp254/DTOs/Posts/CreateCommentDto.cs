using System.ComponentModel.DataAnnotations;


namespace LinkUp254.DTOs.Posts
{
    public class CreateCommentDto
    {
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
