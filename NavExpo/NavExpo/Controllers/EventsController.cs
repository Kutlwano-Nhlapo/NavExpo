using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NavExpo.DTOs;
using NavExpo.Models;
using NavExpo.Services;

namespace NavExpo.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly AttendeeService _attendeeService;

        public EventsController(EventService eventService, AttendeeService attendeeService)
        {
            _eventService = eventService;
            _attendeeService = attendeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            try
            {
                var events = await _eventService.GetAsync();
                return Ok(ApiResponse<object>.SuccessResponse(new { events }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to fetch events: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(string id)
        {
            try
            {
                var eventItem = await _eventService.GetAsync(id);
                if (eventItem == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Event not found"));
                }

                return Ok(ApiResponse<EventForm>.SuccessResponse(eventItem));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to fetch event: {ex.Message}"));
            }
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] EventForm newEvent)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                newEvent.OrganizerId = userId ?? "";
                newEvent.Organizer = userName ?? "";
                newEvent.CreatedAt = DateTime.UtcNow;
                newEvent.UpdatedAt = DateTime.UtcNow;

                await _eventService.CreateAsync(newEvent);

                return CreatedAtAction(nameof(GetEvent), new { id = newEvent.Id },
                    ApiResponse<EventForm>.SuccessResponse(newEvent, "Event created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to create event: {ex.Message}"));
            }
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(string id, [FromBody] EventForm updatedEvent)
        {
            try
            {
                var existingEvent = await _eventService.GetAsync(id);
                if (existingEvent == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Event not found"));
                }

                // Check if user is the organizer or admin
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && existingEvent.OrganizerId != userId)
                {
                    return Forbid();
                }

                updatedEvent.Id = id;
                updatedEvent.OrganizerId = existingEvent.OrganizerId;
                updatedEvent.Organizer = existingEvent.Organizer;
                updatedEvent.AttendeeCount = existingEvent.AttendeeCount;
                updatedEvent.CreatedAt = existingEvent.CreatedAt;

                await _eventService.UpdateAsync(id, updatedEvent);

                return Ok(ApiResponse<EventForm>.SuccessResponse(updatedEvent, "Event updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to update event: {ex.Message}"));
            }
        }

        [Authorize(Roles = "Admin,Organizer")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            try
            {
                var existingEvent = await _eventService.GetAsync(id);
                if (existingEvent == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Event not found"));
                }

                // Check if user is the organizer or admin
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (userRole != "Admin" && existingEvent.OrganizerId != userId)
                {
                    return Forbid();
                }

                await _eventService.RemoveAsync(id);

                return Ok(ApiResponse<object>.SuccessResponse(null, "Event deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to delete event: {ex.Message}"));
            }
        }

        [HttpPost("{eventId}/register")]
        public async Task<IActionResult> RegisterForEvent(string eventId, [FromBody] Attendee attendee)
        {
            try
            {
                // Check if event exists
                var eventItem = await _eventService.GetAsync(eventId);
                if (eventItem == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Event not found"));
                }

                // Check if event is full
                if (eventItem.AttendeeCount >= eventItem.Capacity)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Event is full"));
                }

                // Check if user already registered
                var existingAttendee = await _attendeeService.GetByEventAndEmailAsync(eventId, attendee.Email);
                if (existingAttendee != null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("You are already registered for this event"));
                }

                attendee.EventId = eventId;
                attendee.RegisteredAt = DateTime.UtcNow;

                await _attendeeService.CreateAsync(attendee);
                await _eventService.IncrementAttendeeCountAsync(eventId);

                return Ok(ApiResponse<Attendee>.SuccessResponse(attendee, "Successfully registered for event"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Registration failed: {ex.Message}"));
            }
        }

        [HttpGet("{eventId}/attendees")]
        public async Task<IActionResult> GetEventAttendees(string eventId)
        {
            try
            {
                var attendees = await _attendeeService.GetByEventAsync(eventId);
                return Ok(ApiResponse<object>.SuccessResponse(new { attendees }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Failed to fetch attendees: {ex.Message}"));
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchEvents([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Search term is required"));
                }

                var events = await _eventService.SearchAsync(q);
                return Ok(ApiResponse<object>.SuccessResponse(new { events }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Search failed: {ex.Message}"));
            }
        }
    }
    
}
