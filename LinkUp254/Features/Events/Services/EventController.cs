using LinkUp254.Database;
using LinkUp254.Features.Auth;
using LinkUp254.Features.Events.DTOs;
using LinkUp254.Features.Events.models;
using LinkUp254.Features.Events.Services;
using LinkUp254.Features.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LinkUp254.Features.Events;

[ApiController]
[Route("api/events")]
public class EventController : ControllerBase
{
    private readonly EventServices _eventServices;
    private readonly LinkUpContext _context;

    public EventController(EventServices eventServices, LinkUpContext context)
    {
        _eventServices = eventServices;
        _context = context;
    }

    

  
    private int? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    // GET: api/events - All posted events (public + visibility
    [HttpGet]
    public async Task<IActionResult> GetAllEvents([FromQuery] EventFilterDto filters)
    {
        var userId = GetAuthenticatedUserId(); 
        var result = await _eventServices.GetEventsAsync(filters, userId);
        return Ok(result);
    }

    // GET: api/events/personalized - Interest + location 
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

    // GET: api/events/trending - Most attended events (public + visibility
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingEvents([FromQuery] EventFilterDto filters)
    {
        var userId = GetAuthenticatedUserId(); 
        var result = await _eventServices.GetTrendingEventsAsync(filters, userId);
        return Ok(result);
    }

    
    // GET: api/events/{id} - Single event details with ticket tiers (visibility
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var userId = GetAuthenticatedUserId();
        var result = await _eventServices.GetEventWithTicketsAsync(id, userId);
        return result != null ? Ok(result) : NotFound(new { message = "Event not found" });
    }
    // GET: api/events/my-events - Events created by authenticated user
    [HttpGet("my-events")]
    [Authorize]
    public async Task<IActionResult> GetMyEvents()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _eventServices.GetEventsByOrganizerAsync(int.Parse(userId));
        return Ok(result);
    }




    // POST: api/events - Create new event (auth required)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _eventServices.CreateEventAsync(dto, int.Parse(userId));

        if (result.IsSuccess)
        {
            // Fetch the newly created event to return its ID
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











    // PUT: api/events/{id} - Update event (organizer only)
    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _eventServices.UpdateEventAsync(id, dto, int.Parse(userId));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    //// DELETE: api/events/{id} - Soft delete event (organizer only)
    //[HttpDelete("{id:int}")]
    //[Authorize]
    //public async Task<IActionResult> DeleteEvent(int id)
    //{
    //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
    //              ?? User.FindFirst("sub")?.Value;

    //    if (string.IsNullOrEmpty(userId))
    //        return Unauthorized(new { message = "Authentication required" });

    //    var result = await _eventServices.DeleteEventAsync(id, int.Parse(userId));
    //    return result.IsSuccess ? Ok(result) : BadRequest(result);
    //}



    // DELETE: api/events/{id} - Soft delete event (organizer OR group admin/moderator)
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
    [FromQuery] int limit = 20,
    [FromQuery] int offset = 0,
    [FromQuery] string? sortBy = "distance")
    {
        // Validate GPS coordinates
        if (lat < -90 || lat > 90 || lng < -180 || lng > 180)
            return BadRequest(new { message = "Invalid latitude or longitude" });

        if (radius < 1 || radius > 100)
            return BadRequest(new { message = "Radius must be between 1 and 100 km" });

        // Build filter object
        var filters = new EventFilterDto
        {
            City = city,
            Country = country,
            StartDate = startDate,
            EndDate = endDate,
            IsFreeOnly = isFreeOnly,
            Limit = limit,
            Offset = offset,
            SortBy = sortBy
        };

        // Get user ID if authenticated (for visibility filtering)
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (int.TryParse(userIdClaim, out var parsedId))
                userId = parsedId;
        }

        // Call service method
        var result = await _eventServices.GetNearbyEventsAsync(lat, lng, radius, filters, userId);

        // Map to DTOs with safe location display
        var dtos = result.Items.Select(e => new EventDetailDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            City = e.City,
            Country = e.Country,
            Location = e.Location,
            // Safe venue fields based on visibility
            VenueName = e.LocationVisibility == 0 ? e.VenueName : null,
            StreetAddress = e.LocationVisibility == 0 ? e.StreetAddress : null,
            Latitude = e.LocationVisibility == 0 ? e.Latitude : null,
            Longitude = e.LocationVisibility == 0 ? e.Longitude : null,
            LocationVisibility = e.LocationVisibility,
            DisplayLocation = _eventServices.GetSafeDisplayLocation(e, userId),
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Price = e.Price,
            IsFree = e.IsFree,
            CoverImage = e.CoverImage,
            OrganizerId = e.OrganizerId,
            Organizer = e.Organizer != null ? new UserDto
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
            // Calculate distance for response
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

    // Helper: Haversine distance (same as in EventServices)
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


}