using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LinkUp254.Features.GroupCoverImage.DTOs;

public class UpdateCoverImageRequest
{
    
    public string? ImageUrl { get; set; }
}