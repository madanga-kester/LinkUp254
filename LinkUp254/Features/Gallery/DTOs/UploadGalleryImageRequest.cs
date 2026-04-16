namespace LinkUp254.Features.Gallery.DTOs;

public class UploadGalleryImageRequest
{
    public string ImageData { get; set; } = string.Empty;  // Base64 data URL or direct URL
    public string? FileName { get; set; }
    public string? Caption { get; set; }
}