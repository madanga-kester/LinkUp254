using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LinkUp254.Features.Shared;

namespace LinkUp254.Features.Gallery.Models;

[Table("GroupGallery")]
public class GroupGallery : BaseEntity
{
    // Id, CreatedAt, UpdatedAt, and IsActive are inherited from BaseEntity.

    public int GroupId { get; set; }

    [Required, MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public int UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;

    [MaxLength(200)]
    public string? Caption { get; set; }

    // ADDED: Upload timestamp (required by service mapping)
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}