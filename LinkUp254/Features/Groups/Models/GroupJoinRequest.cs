using System;
using System.ComponentModel.DataAnnotations;
using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Groups.Models;

public class GroupJoinRequest
{
    [Key]
    public int Id { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(500)]
    public string? Message { get; set; }  // Optional message from requester

    public string Status { get; set; } = "pending";  // pending, approved, rejected
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedBy { get; set; }  // Admin rewiwer admin

    [MaxLength(500)]
    public string? ReviewNotes { get; set; }
}