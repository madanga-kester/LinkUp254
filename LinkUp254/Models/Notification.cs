using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Models
{
    public class Notification : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;
        // Like, Comment, Follow, Share, EventInvite

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        [ForeignKey(nameof(UserId))]
        public Users User { get; set; } = null!;
    }
}
