using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LinkUp254.Features.GroupCoverImage.DTOs;

public class UpdateCoverImageRequest
{
    [Required(ErrorMessage = "ImageUrl is required")]
    [Url(ErrorMessage = "ImageUrl must be a valid URL")]
    [StringLength(2048, ErrorMessage = "ImageUrl cannot exceed 2048 characters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ImageUrl { get; set; }
}