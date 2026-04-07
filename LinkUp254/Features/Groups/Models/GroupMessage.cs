using System;
using System.ComponentModel.DataAnnotations;
using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Groups.Models;

public class GroupMessage
{
    [Key]
    public int Id { get; set; }

    public int GroupChatId { get; set; }
    public GroupChat GroupChat { get; set; } = null!;  

    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;  

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public int LikeCount { get; set; }


    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }
}