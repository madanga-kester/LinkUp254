using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


using GroupModel = LinkUp254.Features.Groups.Models.Group;

namespace LinkUp254.Features.GroupCoverImage.Models;

[Table("GroupCoverImages")]
public class GroupCoverImageModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int GroupId { get; set; }

    
    public GroupModel? Group { get; set; }

    [StringLength(2000)]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    public int UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
    public DateTime? UpdatedAt { get; set; }
}