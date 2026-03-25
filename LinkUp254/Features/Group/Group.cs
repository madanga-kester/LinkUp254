using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LinkUp254.Features.Shared;
using Microsoft.EntityFrameworkCore;

namespace LinkUp254.Features.Groups
{
   
    public class Group : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? CoverImage { get; set; }

        // The user who created the group
        [Required]
        public int CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public User? CreatedBy { get; set; }

        // Many-to-many relationship: Users → Groups (Members)
        public ICollection<User> Members { get; set; } = new List<User>();

        // posts, events, or join requests in the future
        // public ICollection<Post> Posts { get; set; } = new List<Post>();
        // public ICollection<GroupJoinRequest> JoinRequests { get; set; } = new List<GroupJoinRequest>();
    }
}