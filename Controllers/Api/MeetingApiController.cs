using System.Security.Claims;
using AgileTaskManager.Services;
using AgileTaskManager.Services.Interfaces;
using AgileTaskManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/meetings")]
[Authorize]
public class MeetingController : ControllerBase
{
    private readonly IMeetingService _meetingService;

    public MeetingController(IMeetingService meetingService)
    {
        _meetingService = meetingService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMeeting([FromBody] CreateMeetingViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var meeting = await _meetingService.CreateMeetingAsync(model, userId);
            return CreatedAtAction(nameof(GetMeeting), new { id = meeting.Id }, meeting);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create meeting.", details = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMeeting(int id)
    {
        try
        {
            var meeting = await _meetingService.GetMeetingAsync(id);
            return Ok(meeting);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get meeting.", details = ex.Message });
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateMeeting([FromBody] UpdateMeetingViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var meeting = await _meetingService.UpdateMeetingAsync(model, userId);
            return Ok(meeting);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update meeting.", details = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMeeting(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _meetingService.DeleteMeetingAsync(id, userId);
            if (!success)
                return NotFound(new { error = "Meeting not found or cannot be deleted." });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete meeting.", details = ex.Message });
        }
    }

    [HttpGet("project/{projectId:int}")]
    public async Task<IActionResult> GetMeetings(int projectId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var meetings = await _meetingService.GetMeetingsAsync(projectId, startDate, endDate);
            return Ok(meetings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get meetings.", details = ex.Message });
        }
    }

    [HttpPost("{id:int}/start")]
    [Authorize(Roles = "Admin,TeamLead")]
    public async Task<IActionResult> StartMeeting(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var meeting = await _meetingService.StartMeetingAsync(id, userId);
            return Ok(meeting);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to start meeting.", details = ex.Message });
        }
    }

    [HttpPost("{id:int}/end")]
    [Authorize(Roles = "Admin,TeamLead")]
    public async Task<IActionResult> EndMeeting(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var meeting = await _meetingService.EndMeetingAsync(id, userId);
            return Ok(meeting);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to end meeting.", details = ex.Message });
        }
    }

    [HttpPost("{id:int}/join")]
    public async Task<IActionResult> JoinMeeting(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _meetingService.JoinMeetingAsync(id, userId);
            if (!success)
                return BadRequest(new { error = "Cannot join meeting." });

            return Ok(new { message = "Successfully joined meeting." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to join meeting.", details = ex.Message });
        }
    }

    [HttpPost("{id:int}/leave")]
    public async Task<IActionResult> LeaveMeeting(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _meetingService.LeaveMeetingAsync(id, userId);
            if (!success)
                return BadRequest(new { error = "Cannot leave meeting." });

            return Ok(new { message = "Successfully left meeting." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to leave meeting.", details = ex.Message });
        }
    }

    // Standup Notes
    [HttpPost("standup")]
    public async Task<IActionResult> CreateStandupNote([FromBody] CreateStandupNoteViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var standup = await _meetingService.CreateStandupNoteAsync(model, userId);
            return Ok(standup);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create standup note.", details = ex.Message });
        }
    }

    [HttpGet("standup/project/{projectId:int}")]
    public async Task<IActionResult> GetStandupNotes(int projectId, [FromQuery] DateTime? date = null)
    {
        try
        {
            var standups = await _meetingService.GetStandupNotesAsync(projectId, date);
            return Ok(standups);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get standup notes.", details = ex.Message });
        }
    }

    // Retrospective Notes
    [HttpPost("retrospective")]
    public async Task<IActionResult> CreateRetrospectiveNote([FromBody] CreateRetrospectiveNoteViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var note = await _meetingService.CreateRetrospectiveNoteAsync(model, userId);
            return Ok(note);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create retrospective note.", details = ex.Message });
        }
    }

    [HttpGet("retrospective/{meetingId:int}")]
    public async Task<IActionResult> GetRetrospectiveNotes(int meetingId)
    {
        try
        {
            var notes = await _meetingService.GetRetrospectiveNotesAsync(meetingId);
            return Ok(notes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get retrospective notes.", details = ex.Message });
        }
    }

    // Action Items
    [HttpPost("{meetingId:int}/action-items")]
    public async Task<IActionResult> CreateActionItem(int meetingId, [FromBody] CreateActionItemViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var actionItem = await _meetingService.CreateActionItemAsync(meetingId, model, userId);
            return Ok(actionItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create action item.", details = ex.Message });
        }
    }

    [HttpPut("action-items")]
    public async Task<IActionResult> UpdateActionItem([FromBody] UpdateActionItemViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var actionItem = await _meetingService.UpdateActionItemAsync(model, userId);
            return Ok(actionItem);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update action item.", details = ex.Message });
        }
    }

    [HttpDelete("action-items/{actionItemId:int}")]
    public async Task<IActionResult> DeleteActionItem(int actionItemId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _meetingService.DeleteActionItemAsync(actionItemId, userId);
            if (!success)
                return NotFound(new { error = "Action item not found." });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete action item.", details = ex.Message });
        }
    }

    // Dashboard and Analytics
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dashboard = await _meetingService.GetDashboardAsync(userId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get dashboard.", details = ex.Message });
        }
    }

    [HttpGet("analytics/project/{projectId:int}")]
    [Authorize(Roles = "Admin,TeamLead")]
    public async Task<IActionResult> GetAnalytics(int projectId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
            return BadRequest(new { error = "Start date must be before end date." });

        try
        {
            var analytics = await _meetingService.GetAnalyticsAsync(projectId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get analytics.", details = ex.Message });
        }
    }

    // Quick Start Meeting
    [HttpPost("quick-start")]
    public async Task<IActionResult> QuickStartMeeting([FromBody] QuickStartMeetingViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var createModel = new CreateMeetingViewModel
            {
                Title = model.Title ?? MeetingFormattingHelper.GetMeetingTypeDisplayName(model.Type),
                Type = model.Type,
                ScheduledAt = model.ScheduledAt,
                ProjectId = model.ProjectId,
                SprintId = model.SprintId,
                ParticipantIds = model.ParticipantIds,
                Description = $"Quick start {MeetingFormattingHelper.GetMeetingTypeDisplayName(model.Type).ToLower()}"
            };

            var meeting = await _meetingService.CreateMeetingAsync(createModel, userId);
            return CreatedAtAction(nameof(GetMeeting), new { id = meeting.Id }, meeting);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to quick start meeting.", details = ex.Message });
        }
    }
}
