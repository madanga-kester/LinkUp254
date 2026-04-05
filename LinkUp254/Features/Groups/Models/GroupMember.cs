using System;
using System.ComponentModel.DataAnnotations;
using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Groups.Models;

public class GroupMember
{
    [Key]
    public int Id { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(50)]
    public string Role { get; set; } = "member"; // admin, moderator, member
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}