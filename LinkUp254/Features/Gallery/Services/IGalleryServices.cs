using LinkUp254.Features.Gallery.DTOs;
using LinkUp254.Features.Shared; 

namespace LinkUp254.Features.Gallery.Services;

public interface IGalleryServices
{
    Task<List<GalleryItemDto>> GetGalleryAsync(int groupId);
    Task<ServiceResult<GalleryItemDto>> UploadGalleryImageAsync(int groupId, int uploaderId, UploadGalleryImageRequest request);
    Task<ServiceResult<bool>> DeleteGalleryImageAsync(int imageId, int requesterId);
}