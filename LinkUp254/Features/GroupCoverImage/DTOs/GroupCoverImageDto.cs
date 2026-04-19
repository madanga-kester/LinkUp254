
using System;

namespace LinkUp254.Features.GroupCoverImage.DTOs;

public class GroupCoverImageDto
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsActive { get; set; }
}