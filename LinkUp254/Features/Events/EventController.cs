using LinkUp254.Features.Events.DTOs;
using LinkUp254.Features.Events.models;
using LinkUp254.Features.Shared;
using LinkUp254.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUp254.Features.Events;

[ApiController]
[Route("api/events")]
public class EventController : ControllerBase
{
    private readonly EventServices _eventServices;

    public EventController(EventServices eventServices)
    {
        _eventServices = eventServices;
    }

    //  Extract authenticated user ID from JWT claims
    private int? GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    // GET: api/events - All posted events (public + visibility-aware)
    [HttpGet]
    public async Task<IActionResult> GetAllEvents([FromQuery] EventFilterDto filters)
    {
        var userId = GetAuthenticatedUserId(); 
        var result = await _eventServices.GetEventsAsync(filters, userId);
        return Ok(result);
    }

    // GET: api/events/personalized - Interest + location filtered (auth required)
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

    // GET: api/events/trending - Most attended events (public + visibility-aware)
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingEvents([FromQuery] EventFilterDto filters)
    {
        var userId = GetAuthenticatedUserId(); 
        var result = await _eventServices.GetTrendingEventsAsync(filters, userId);
        return Ok(result);
    }

    // GET: api/events/{id} - Single event details (visibility-aware)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        var userId = GetAuthenticatedUserId(); 
        var result = await _eventServices.GetEventByIdAsync(id, userId);
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
        return result.IsSuccess ? Ok(result) : BadRequest(result);
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

    // DELETE: api/events/{id} - Soft delete event (organizer only)
    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Authentication required" });

        var result = await _eventServices.DeleteEventAsync(id, int.Parse(userId));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}