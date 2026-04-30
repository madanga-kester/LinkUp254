using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.Auth.DTOs;
using LinkUp254.Features.Events.DTOs;
using LinkUp254.Features.Events.EventDtos;
using LinkUp254.Features.Events.models;
using LinkUp254.Features.Events.Services;
using LinkUp254.Features.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging; 
using System.Reflection;
using System.Security.Claims;

namespace LinkUp254.Features.Events.Controllers;

[ApiController]
[Route("api/events")]
public class EventController : ControllerBase
{
    private readonly EventServices _eventServices;
    private readonly EventEngagementServices _engagementServices;
    private readonly LinkUpContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EventController> _logger;

    
  




    public EventController(
    EventServices eventServices,
    EventEngagementServices engagementServices,
    LinkUpContext context,
    IWebHostEnvironment env,
    ILogger<EventController> logger) 
    {
        _eventServices = eventServices;
        _engagementServices = engagementServices;
        _context = context;
        _env = env;
        _logger = logger; 
    }





    private int? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllEvents([FromQuery] EventFilterDto filters)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _eventServices.GetEventsAsync(filters, userId);
        return Ok(result);
    }

    [HttpGet("personalized")]
    [Authorize]
    public async Task<IActionResult> GetPersonalizedEvents([FromQuery] EventFilterDto filters)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _eventServices.GetPersonalizedEventsAsync(userId, filters);
        return Ok(result);
    }

    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingEvents([FromQuery] EventFilterDto filters)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _eventServices.GetTrendingEventsAsync(filters, userId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var userId = GetAuthenticatedUserId();

        // Track view count (fire-and-forget, non-critical)
        _ = _engagementServices.IncrementViewCountAsync(id); 

        var result = await _eventServices.GetEventWithTicketsAsync(id, userId);
        return result != null ? Ok(result) : NotFound(new { message = "Event not found" });
    }



    [HttpGet("my-events")]
    [Authorize]
    public async Task<IActionResult> GetMyEvents([FromQuery] int limit = 9, [FromQuery] int offset = 0)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        // Get all events for organizer (service returns List<Event>)
        var allEvents = await _eventServices.GetEventsByOrganizerAsync(int.Parse(userId));

        // Apply pagination manually
        var total = allEvents.Count;
        var pagedEvents = allEvents
            .OrderByDescending(e => e.CreatedAt) // Show newest first
            .Skip(offset)
            .Take(limit)
            .ToList();

        return Ok(new PagedResult<Event>
        {
            Items = pagedEvents,
            Total = total,
            Limit = limit,
            Offset = offset
        });
    }



    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        // Server-side validation for age restriction
        if (dto.AgeRestricted == true && (!dto.MinAge.HasValue || dto.MinAge < 0 || dto.MinAge > 120))
        {
            return BadRequest(new { message = "Please provide a valid minimum age between 0 and 120 for age-restricted events." });
        }

        var result = await _eventServices.CreateEventAsync(dto, int.Parse(userId));

        if (result.IsSuccess)
        {
            var newEvent = await _context.Events
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync(e => e.OrganizerId == int.Parse(userId));

            return Ok(new
            {
                isSuccess = true,
                message = result.Message,
                eventId = newEvent?.Id
            });
        }
        return BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        // Server-side validation for age restriction updates
        if (dto.AgeRestricted == true && (!dto.MinAge.HasValue || dto.MinAge < 0 || dto.MinAge > 120))
        {
            return BadRequest(new { message = "Please provide a valid minimum age between 0 and 120 for age-restricted events." });
        }

        var result = await _eventServices.UpdateEventAsync(id, dto, int.Parse(userId));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    






    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value
                  ?? User.FindFirst("nameid")?.Value
                  ?? User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var intUserId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _eventServices.DeleteEventAsync(id, intUserId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    //  LIKE ENDPOINTS

    [HttpPost("{id:int}/like")]
    [Authorize]
    public async Task<IActionResult> LikeEvent(int id)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var success = await _engagementServices.ToggleLikeAsync(id, userId.Value, like: true);
        return success
            ? Ok(new { isSuccess = true, message = "Event liked successfully" })
            : BadRequest(new { message = "Failed to like event" });
    }

    [HttpDelete("{id:int}/like")]
    [Authorize]
    public async Task<IActionResult> UnlikeEvent(int id)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var success = await _engagementServices.ToggleLikeAsync(id, userId.Value, like: false);
        return success
            ? Ok(new { isSuccess = true, message = "Event unliked successfully" })
            : BadRequest(new { message = "Failed to unlike event" });
    }

   






    [HttpGet("{id:int}/like/status")]
    [Authorize]
    public async Task<IActionResult> GetLikeStatus(int id)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var isLiked = await _engagementServices.IsLikedByUserAsync(id, userId.Value);
        var likeCount = await _engagementServices.GetLikeCountAsync(id);

        return Ok(new { isLiked, likeCount });
    }

    //  RSVP ENDPOINTS

    [HttpPost("{id:int}/rsvp")]
    [Authorize]
    public async Task<IActionResult> RsvpEvent(int id, [FromBody] RsvpRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        try
        {
            var result = await _engagementServices.UpsertRsvpAsync(id, userId.Value, request);
            return Ok(new
            {
                isSuccess = true,
                message = "RSVP updated successfully",
                rsvp = result
            });
        }
        catch (InvalidOperationException ex)
        {
            // Handle business logic errors (event full, not found, etc.)
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RsvpEvent failed for user {UserId} on event {EventId}", userId, id);
            return StatusCode(500, new { message = "Failed to process RSVP" });
        }
    }

    [HttpDelete("{id:int}/rsvp")]
    [Authorize]
    public async Task<IActionResult> CancelRsvpEvent(int id)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var success = await _engagementServices.CancelRsvpAsync(id, userId.Value);
        return success
            ? Ok(new { isSuccess = true, message = "RSVP cancelled successfully" })
            : BadRequest(new { message = "Failed to cancel RSVP" });
    }











    [HttpGet("{id:int}/rsvp/status")]
    [Authorize]
    public async Task<IActionResult> GetUserRsvpStatus(int id)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var rsvp = await _engagementServices.GetUserRsvpAsync(id, userId.Value);
        var goingCount = await _engagementServices.GetRsvpCountAsync(id, "going");

        return Ok(new
        {
            rsvp,
            goingCount,
            isGoing = rsvp?.Status == "going"
        });
    }

    // SOCIAL CONTEXT ENDPOINT

    [HttpGet("{id:int}/social")]
    [Authorize]
    public async Task<IActionResult> GetEventSocialContext(int id)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        try
        {
            var context = await _engagementServices.GetSocialContextAsync(id, userId.Value);
            return Ok(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetEventSocialContext failed for user {UserId} on event {EventId}", userId, id);
            // Return safe defaults on error (service already handles this, but extra safety)
            return Ok(new SocialContextDto
            {
                FriendsGoing = 0,
                NetworkInterest = "low",
                LiveViewers = null,
                MutualFriends = new List<UserSummaryDto>()
            });
        }
    }












    [HttpGet("nearby")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<EventDetailDto>>> GetNearbyEvents(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius = 10,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool? isFreeOnly = null,
        [FromQuery] int? minAge = null,
        [FromQuery] bool? isVirtual = null,        
        [FromQuery] bool? isInPerson = null,       
        [FromQuery] string? interestIds = null,

        [FromQuery] int limit = 20,
        [FromQuery] int offset = 0,
        [FromQuery] string? sortBy = "distance")
    {
        if (lat < -90 || lat > 90 || lng < -180 || lng > 180)
            return BadRequest(new { message = "Invalid latitude or longitude" });

        if (radius < 1 || radius > 100)
            return BadRequest(new { message = "Radius must be between 1 and 100 km" });

        var filters = new EventFilterDto
        {
            City = city,
            Country = country,
            StartDate = startDate,
            EndDate = endDate,
            IsFreeOnly = isFreeOnly,
            MinAge = minAge,
            IsVirtual = isVirtual,      
            IsInPerson = isInPerson,    
            InterestIds = interestIds,  
            Limit = limit,
            Offset = offset,
            SortBy = sortBy
        };

        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (int.TryParse(userIdClaim, out var parsedId))
                userId = parsedId;
        }

        var result = await _eventServices.GetNearbyEventsAsync(lat, lng, radius, filters, userId);

        var dtos = result.Items.Select(e => new EventDetailDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            City = e.City,
            Country = e.Country,
            Location = e.Location,
            VenueName = e.LocationVisibility == 0 ? e.VenueName : null,
            StreetAddress = e.LocationVisibility == 0 ? e.StreetAddress : null,
            Latitude = e.LocationVisibility == 0 ? e.Latitude : null,
            Longitude = e.LocationVisibility == 0 ? e.Longitude : null,
            LocationVisibility = e.LocationVisibility,
            AgeRestricted = e.AgeRestricted,
            MinAge = e.MinAge,
            DisplayLocation = _eventServices.GetSafeDisplayLocation(e, userId),
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Price = e.Price,
            IsFree = e.IsFree,
            CoverImage = e.CoverImage,
            OrganizerId = e.OrganizerId,


            Organizer = e.Organizer != null ? new Shared.UserDto
            {
                Id = e.Organizer.Id,
                FirstName = e.Organizer.FirstName,
                LastName = e.Organizer.LastName,
                ProfilePicture = e.Organizer.ProfilePicture
            } : null,


            IsActive = e.IsActive,
            IsPublished = e.IsPublished,
            AttendeeCount = e.AttendeeCount,
            ViewCount = e.ViewCount,
            LikeCount = e.LikeCount,
            MaxAttendees = e.MaxAttendees,
            IsFull = e.IsFull,
            Visibility = e.Visibility,
            IsUpcoming = e.IsUpcoming,
            IsOngoing = e.IsOngoing,
            HasEnded = e.HasEnded,
            DistanceKm = e.Latitude.HasValue && e.Longitude.HasValue
                ? CalculateHaversineDistance(lat, lng, e.Latitude.Value, e.Longitude.Value)
                : null
        }).ToList();

        return Ok(new PagedResult<EventDetailDto>
        {
            Items = dtos,
            Total = result.Total,
            Limit = result.Limit,
            Offset = result.Offset
        });
    }

    private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees) => degrees * (Math.PI / 180);

    [HttpGet("{id:int}/cover-image")]
    public async Task<IActionResult> GetCoverImage(int id)
    {
        var eventEntity = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

        if (eventEntity == null || string.IsNullOrEmpty(eventEntity.CoverImage))
            return NotFound(new { message = "Cover image not found" });

        return Ok(new { imageUrl = eventEntity.CoverImage });
    }

    [HttpPut("{id:int}/cover-image")]
    [Authorize]
    public async Task<IActionResult> UpdateCoverImage(int id, [FromBody] UpdateEventCoverImageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Invalid request data", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var result = await _eventServices.UpdateEventCoverImageAsync(id, userId.Value, dto.ImageUrl);
        return result.IsSuccess
            ? Ok(new { isSuccess = true, message = result.Message })
            : BadRequest(new { message = result.Message });
    }

    [HttpDelete("{id:int}/cover-image")]
    [Authorize]
    public async Task<IActionResult> DeleteCoverImage(int id)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        var result = await _eventServices.DeleteEventCoverImageAsync(id, userId.Value);
        return result.IsSuccess
     ? Ok(new { isSuccess = true, message = result.Message })
     : BadRequest(new { message = result.Message });
    }








    [HttpPost("{id:int}/cover-image-upload")]
    [Authorize]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadCoverImageFile(int id, IFormFile file)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return Unauthorized(new { message = "Authentication required" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(new { message = "Only JPG, PNG, WebP allowed" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "File too large (max 5MB)" });

        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null || eventEntity.OrganizerId != userId.Value)
            return NotFound(new { message = "Event not found" });


       
        var webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads", "events");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"event_{id}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);





        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"/uploads/events/{uniqueFileName}";
        eventEntity.CoverImage = imageUrl;
        eventEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { imageUrl = imageUrl, message = "Image uploaded successfully" });
    }






    
    [HttpPost("attendee-avatars")]
    [AllowAnonymous] 
    public async Task<IActionResult> GetAttendeeAvatars([FromBody] AttendeeAvatarsRequest request)
    {
        if (!ModelState.IsValid || !request.EventIds.Any())
            return BadRequest(new { message = "EventIds list is required" });

        
        if (request.EventIds.Count > 50)
            return BadRequest(new { message = "Maximum 50 event IDs per request" });

        try
        {
            var result = await _engagementServices.GetAttendeeAvatarsAsync(request.EventIds);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAttendeeAvatars endpoint failed");
            return StatusCode(500, new { message = "Failed to fetch attendee avatars" });
        }
    }
}
