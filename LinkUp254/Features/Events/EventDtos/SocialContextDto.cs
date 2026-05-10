
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkUp254.Features.Events.DTOs;

public class SocialContextDto
{
    public int FriendsGoing { get; set; }

    [RegularExpression("^(low|medium|high)$")]
    public string NetworkInterest { get; set; } = "low";

    public int? LiveViewers { get; set; }

    public List<UserSummaryDto> MutualFriends { get; set; } = new();
}

public class UserSummaryDto
{
    public int Id { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(500)]
    public string? ProfilePicture { get; set; } 
}