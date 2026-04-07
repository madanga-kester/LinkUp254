using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Groups.DTOs;

public class UpdateGroupSettingsDto
{
    public bool? IsPrivate { get; set; }
    public bool? AllowMemberInvites { get; set; }
    public bool? AllowMemberPosts { get; set; }
    public bool? ModerateMessages { get; set; }
    public bool? AllowLinks { get; set; }
    public bool? AllowMedia { get; set; }
    public bool? NotifyOnNewEvent { get; set; }
    public bool? NotifyOnNewMember { get; set; }
}

public class CreateGroupRuleDto
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int? Order { get; set; }
}

public class SendMessageDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public class ReviewJoinRequestDto
{
    public bool Approve { get; set; }
    public string? Notes { get; set; }
}