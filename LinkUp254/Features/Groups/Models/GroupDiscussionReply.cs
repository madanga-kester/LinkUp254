using LinkUp254.Features.Auth;
using LinkUp254.Features.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Groups.Models;

[Table("GroupDiscussionReplies")]
public class GroupDiscussionReply
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DiscussionId { get; set; }

    [ForeignKey(nameof(DiscussionId))]
    public GroupDiscussion Discussion { get; set; } = null!;

    [Required]
    public int AuthorId { get; set; }

 

    [ForeignKey(nameof(AuthorId))]
    public User Author { get; set; } = null!;

    

    [Required, StringLength(3000)]
    public string Content { get; set; } = string.Empty;

    public int? ParentReplyId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}