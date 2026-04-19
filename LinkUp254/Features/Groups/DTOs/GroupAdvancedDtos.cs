using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Groups.DTOs;

//GROUP CREATION

//public class CreateGroupDto
//{
//    [Required]
//    [MaxLength(200)]
//    public string Name { get; set; } = string.Empty;

//    [MaxLength(2000)]
//    public string? Description { get; set; }

//    [MaxLength(100)]
//    public string? City { get; set; }

//    [MaxLength(100)]
//    public string? Country { get; set; }

//    [MaxLength(500)]
//    public string? Location { get; set; }

//    // Privacy & membership settings
//    public bool IsPrivate { get; set; } = false;
//    public bool AllowMemberInvites { get; set; } = true;
//    public bool AllowMemberPosts { get; set; } = true;
//    public bool ModerateMessages { get; set; } = false;

//    [Range(1, 100000)]
//    public int? MaxMembers { get; set; }

//    // Content settings
//    public bool AllowLinks { get; set; } = true;
//    public bool AllowMedia { get; set; } = true;

//    // Cover image - either URL or base64
//    [MaxLength(500)]
//    public string? CoverImage { get; set; }

//    public string? CoverImageBase64 { get; set; }

//    // Group rules
//    public List<CreateRuleDto>? Rules { get; set; }

//    // Interest tags for discovery
//    public List<string>? InterestTags { get; set; }

//    // Notification preferences
//    public bool NotifyOnNewEvent { get; set; } = true;
//    public bool NotifyOnNewMember { get; set; } = false;
//}

public class CreateGroupDto
{
    [Required(ErrorMessage = "Group name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    // Privacy & Core Settings
    public bool IsPrivate { get; set; } = false;

    // Membership & Content Settings
    public bool AllowMemberInvites { get; set; } = true;
    public bool AllowMemberPosts { get; set; } = true;
    public bool ModerateMessages { get; set; } = false;

    [Range(1, 100000, ErrorMessage = "Max members must be between 1 and 100,000")]
    public int? MaxMembers { get; set; }

    // Content permissions
    public bool AllowLinks { get; set; } = true;
    public bool AllowMedia { get; set; } = true;

    // Cover Image (support both URL and Base64 from frontend)
    [MaxLength(500)]
    public string? CoverImage { get; set; }        // Direct URL

    public string? CoverImageBase64 { get; set; }  // Base64 data URL

    // Rules and Tags
    public List<CreateRuleDto>? Rules { get; set; }

    public List<string>? InterestTags { get; set; }

    // Notifications
    public bool NotifyOnNewEvent { get; set; } = true;
    public bool NotifyOnNewMember { get; set; } = false;
}





// Helper DTO for rules (used in CreateGroupDto)
public class CreateRuleDto
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }
}

//  GROUP SETTINGS 

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

// GROUP RULES (Standalone) 

public class CreateGroupRuleDto
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int? Order { get; set; }
}

// GROUP CHAT 

public class SendMessageDto
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

// MEMBER REQUESTS 

public class ReviewJoinRequestDto
{
    public bool Approve { get; set; }
    public string? Notes { get; set; }
}





public class CreateDiscussionDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(5000)]
    public string Content { get; set; } = string.Empty;

    public bool IsPinned { get; set; } = false;

    // Optional: Track if promoted from chat message
    public int? SourceMessageId { get; set; }
}