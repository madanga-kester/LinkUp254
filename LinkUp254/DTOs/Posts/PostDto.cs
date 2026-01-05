namespace LinkUp254.DTOs.Posts
{
    public class PostDto
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; }

        public int LikeCount { get; set; }
        public int CommentCount { get; set; }

        public bool IsLikedByCurrentUser { get; set; }
    }
}
