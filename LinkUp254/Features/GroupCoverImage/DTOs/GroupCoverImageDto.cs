using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LinkUp254.Features.GroupCoverImage.DTOs;

public class GroupCoverImageDto
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Id { get; set; }

    [Required]
    public int GroupId { get; set; }

    [Url]
    [StringLength(2048)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageUrl { get; set; }

    [Url]
    [StringLength(2048)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ThumbnailUrl { get; set; }

    [Required]
    public int UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }

    public bool IsActive { get; set; }
}