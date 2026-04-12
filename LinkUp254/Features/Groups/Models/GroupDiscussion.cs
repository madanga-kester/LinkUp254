using LinkUp254.Features.Auth;
using LinkUp254.Features.Shared;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkUp254.Features.Groups.Models;

[Table("GroupDiscussions")]
public class GroupDiscussion
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int GroupId { get; set; }

    [ForeignKey(nameof(GroupId))]
    public Group Group { get; set; } = null!;

    [Required]
    public int AuthorId { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public User Author { get; set; } = null!;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(5000)]
    public string Content { get; set; } = string.Empty;

    public bool IsPinned { get; set; } = false;
    public bool IsLocked { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Tracking
    public int ReplyCount { get; set; } = 0;
    public int ViewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Optional: Track if promoted from chat
    public int? SourceMessageId { get; set; }
}