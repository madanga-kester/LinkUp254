using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

using GroupModel = LinkUp254.Features.Groups.Models.Group;

namespace LinkUp254.Features.GroupCoverImage.Models;

[Table("GroupCoverImages")]
[Index(nameof(GroupId), Name = "IX_GroupCoverImages_GroupId")]
[Index(nameof(GroupId), nameof(IsActive), Name = "IX_GroupCoverImages_GroupId_IsActive")]
[Index(nameof(UploadedBy), Name = "IX_GroupCoverImages_UploadedBy")]
public class GroupCoverImageModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Group))]
    public int GroupId { get; set; }

    public GroupModel? Group { get; set; }

    [StringLength(2000)]
    [Url]
    public string? ImageUrl { get; set; }

    [StringLength(500)]
    [Url]
    public string? ThumbnailUrl { get; set; }

    [Required]
    public int UploadedBy { get; set; }

   // [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public DateTime? UpdatedAt { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }
}