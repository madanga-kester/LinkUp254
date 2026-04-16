using LinkUp254.Database;
using LinkUp254.Features.Gallery.DTOs;
using LinkUp254.Features.Gallery.Models;
using LinkUp254.Features.Shared; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LinkUp254.Features.Gallery.Services;



public class GalleryServices : IGalleryServices
{
    private readonly LinkUpContext _context;
    private readonly ILogger<GalleryServices> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration; 

    public GalleryServices(
        LinkUpContext context,
        ILogger<GalleryServices> logger,
        IWebHostEnvironment env,
        IConfiguration configuration)  
    {
        _context = context;
        _logger = logger;
        _env = env ?? throw new ArgumentNullException(nameof(env));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));  
    }

    public async Task<List<GalleryItemDto>> GetGalleryAsync(int groupId)
    {
        return await _context.GroupGallery
            .Where(gg => gg.GroupId == groupId && gg.IsActive)
            .Include(gg => gg.UploadedBy)
            .Select(gg => new GalleryItemDto
            {
                Id = gg.Id,
                Url = gg.ImageUrl,
                UploadedBy = $"{gg.UploadedBy.FirstName} {gg.UploadedBy.LastName}",
                UploadedAt = gg.UploadedAt,
                Caption = gg.Caption
            })
            .OrderByDescending(gg => gg.UploadedAt)
            .ToListAsync();
    }

    public async Task<ServiceResult<GalleryItemDto>> UploadGalleryImageAsync(int groupId, int uploaderId, UploadGalleryImageRequest request)
    {
        // 1. Validate group exists and is active
        var group = await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive);

        if (group == null)
            return ServiceResult<GalleryItemDto>.Failure("Group not found");

        // 2. Validate user is member or organizer
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == uploaderId && gm.IsActive);

        var isOrganizer = group.OrganizerId == uploaderId;

        if (!isMember && !isOrganizer)
            return ServiceResult<GalleryItemDto>.Failure("You must be a member to upload images");

      
        string imageUrl;

        if (!string.IsNullOrEmpty(request.ImageData))
        {
            if (request.ImageData.StartsWith("image", StringComparison.OrdinalIgnoreCase) ||
                request.ImageData.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var commaIndex = request.ImageData.IndexOf(',');
                    if (commaIndex == -1)
                        return ServiceResult<GalleryItemDto>.Failure("Invalid image data format");

                    var base64Data = request.ImageData.Substring(commaIndex + 1);
                    var imageBytes = Convert.FromBase64String(base64Data);

                    
                    if (imageBytes.Length > 10 * 1024 * 1024)
                        return ServiceResult<GalleryItemDto>.Failure("Image too large (max 10MB)");

                   
                    imageUrl = await UploadImageToStorageAsync(imageBytes, request.FileName, groupId);

                    if (string.IsNullOrEmpty(imageUrl))
                        return ServiceResult<GalleryItemDto>.Failure("Failed to upload image");
                }
                catch (FormatException)
                {
                    return ServiceResult<GalleryItemDto>.Failure("Invalid image format");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing uploaded image for group {GroupId}", groupId);
                    return ServiceResult<GalleryItemDto>.Failure("Server error processing image");
                }
            }
            else
            {
                
                if (!Uri.IsWellFormedUriString(request.ImageData, UriKind.Absolute))
                    return ServiceResult<GalleryItemDto>.Failure("Invalid image URL");

                imageUrl = request.ImageData;
            }
        }
        else
        {
            return ServiceResult<GalleryItemDto>.Failure("Image data is required");
        }

        // 4. Create gallery record
        var galleryItem = new GroupGallery
        {
            GroupId = groupId,
            ImageUrl = imageUrl,
            UploadedById = uploaderId,
            Caption = request.Caption?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.GroupGallery.Add(galleryItem);
        await _context.SaveChangesAsync();

        // 5. Return DTO
        var uploader = await _context.Users.FindAsync(uploaderId);

        
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5260";
        var absoluteUrl = galleryItem.ImageUrl.StartsWith("http")
            ? galleryItem.ImageUrl
            : $"{baseUrl}{galleryItem.ImageUrl}";


        return ServiceResult<GalleryItemDto>.Success(new GalleryItemDto
        {
            Id = galleryItem.Id,
            Url = absoluteUrl,  // Returns "http://localhost:5260/images/groups/1007/xyz.jpg"
            UploadedBy = uploader != null ? $"{uploader.FirstName} {uploader.LastName}" : "Unknown User",
            UploadedAt = galleryItem.UploadedAt,
            Caption = galleryItem.Caption
      
        
        
        
       
        
        });
    }

    public async Task<ServiceResult<bool>> DeleteGalleryImageAsync(int imageId, int requesterId)
    {
        var galleryItem = await _context.GroupGallery
            .Include(gg => gg.UploadedBy)
            .FirstOrDefaultAsync(gg => gg.Id == imageId && gg.IsActive);

        if (galleryItem == null)
            return ServiceResult<bool>.Failure("Image not found");

      
        var group = await _context.Groups.FindAsync(galleryItem.GroupId);
        if (group == null)
            return ServiceResult<bool>.Failure("Associated group not found");

       
        var isOrganizer = group.OrganizerId == requesterId;
        var isUploader = galleryItem.UploadedById == requesterId;

        if (!isOrganizer && !isUploader)
            return ServiceResult<bool>.Failure("You don't have permission to delete this image");

        
        galleryItem.IsActive = false;
        galleryItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();


        return ServiceResult<bool>.Success(true);
    }

    // HELPER: Storage Upload 

    private async Task<string> UploadImageToStorageAsync(byte[] imageBytes, string? fileName, int groupId)
    {
        //  CONFIGURE STORAGE PROVIDER HERE

        //  Local Storage (Development Only)
        var relativePath = $"/images/groups/{groupId}";

       
        var wwwRoot = _env.WebRootPath;
        if (string.IsNullOrEmpty(wwwRoot))
        {
           
            wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _logger.LogWarning("WebRootPath was null, using fallback: {FallbackPath}", wwwRoot);
        }

       
        var safeGroupId = groupId.ToString();
        var physicalPath = Path.Combine(wwwRoot, "images", "groups", safeGroupId);

        Directory.CreateDirectory(physicalPath);

  
        var fileExt = ".jpg"; 
        if (!string.IsNullOrEmpty(fileName))
        {
            var ext = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(ext) && new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext.ToLower()))
            {
                fileExt = ext;
            }
        }

        var uniqueFileName = $"{Guid.NewGuid()}{fileExt}";
        var filePath = Path.Combine(physicalPath, uniqueFileName);

        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
        _logger.LogDebug("Saved image to: {FilePath}", filePath);

        // Return URL that your static file middleware can serve
        return $"{relativePath}/{uniqueFileName}";
    }
}