using System;

namespace LinkUp254.Features.Post.Posts
{
    public class PostLikeDto
    {
        public int PostId { get; set; }
        public int TotalLikes { get; set; }
        public bool IsLiked { get; set; }
    }
}
