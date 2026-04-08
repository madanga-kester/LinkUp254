using System;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Groups.Models;

public class GroupSettings
{
    [Key]
    
    public int GroupId { get; set; }
    public LinkUp254.Features.Groups.Models.Group Group { get; set; } = null!;

    // Membership controls
    public bool IsPrivate { get; set; } = false;  // Private = requires approval to join
    public bool AllowMemberInvites { get; set; } = true;
    public bool AllowMemberPosts { get; set; } = true;

    // Content controls
    public bool ModerateMessages { get; set; } = false;  // Messages require approval
    public bool AllowLinks { get; set; } = true;
    public bool AllowMedia { get; set; } = true;

    // Notifications
    public bool NotifyOnNewEvent { get; set; } = true;
    public bool NotifyOnNewMember { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}