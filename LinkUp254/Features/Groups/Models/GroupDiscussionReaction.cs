using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Groups.Models;

[Table("GroupDiscussionReactions")]
public class GroupDiscussionReaction
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string TargetType { get; set; } = ""; // "Discussion" or "Reply"

    public int TargetId { get; set; }
    public int UserId { get; set; }

    [Required, StringLength(20)]
    public string ReactionType { get; set; } = ""; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}