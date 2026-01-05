namespace LinkUp254.DTOs.Posts
{
    public class CommentDto
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
