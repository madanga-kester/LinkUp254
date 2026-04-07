using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Groups.Models;

public class Group : BaseEntity
{
  

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? CoverImage { get; set; }

    public int OrganizerId { get; set; }
    public User Organizer { get; set; } = null!;

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    public int MemberCount { get; set; }



    public ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
    public ICollection<GroupEvent> GroupEvents { get; set; } = new List<GroupEvent>();
    public GroupSettings? Settings { get; set; }
    public ICollection<GroupRule> GroupRules { get; set; } = new List<GroupRule>();
    public GroupChat? Chat { get; set; }
    public ICollection<GroupJoinRequest> JoinRequests { get; set; } = new List<GroupJoinRequest>();
}