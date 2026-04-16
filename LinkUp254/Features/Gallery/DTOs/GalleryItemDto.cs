namespace LinkUp254.Features.Gallery.DTOs;

public class GalleryItemDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? Caption { get; set; }
}