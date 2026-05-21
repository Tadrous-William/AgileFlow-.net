using System.Security.Claims;
using AgileTaskManager.Services;
using AgileTaskManager.Services.Interfaces;
using AgileTaskManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/time-tracking")]
[Authorize]
public class TimeTrackingController : ControllerBase
{
    private readonly ITimeTrackingService _timeTrackingService;

    public TimeTrackingController(ITimeTrackingService timeTrackingService)
    {
        _timeTrackingService = timeTrackingService;
    }

    [HttpPost("timer/start")]
    public async Task<IActionResult> StartTimer([FromBody] StartTimerViewModel model)
    {
        if (model.TaskId <= 0)
            return BadRequest(new { error = "Valid task ID is required." });

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var timeEntry = await _timeTrackingService.StartTimerAsync(model.TaskId, userId, model.Description);
            return Ok(timeEntry);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to start timer.", details = ex.Message });
        }
    }

    [HttpPost("timer/stop/{taskId:int}")]
    public async Task<IActionResult> StopTimer(int taskId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var timeEntry = await _timeTrackingService.StopTimerAsync(taskId, userId);
            return Ok(timeEntry);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to stop timer.", details = ex.Message });
        }
    }

    [HttpGet("timer/active")]
    public async Task<IActionResult> GetActiveTimer()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var activeTimer = await _timeTrackingService.GetActiveTimerAsync(userId);
            return Ok(activeTimer);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get active timer.", details = ex.Message });
        }
    }

    [HttpGet("entries")]
    public async Task<IActionResult> GetUserTimeEntries([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var entries = await _timeTrackingService.GetUserTimeEntriesAsync(userId, startDate, endDate);
            return Ok(entries);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get time entries.", details = ex.Message });
        }
    }

    [HttpPost("entries")]
    public async Task<IActionResult> CreateManualTimeEntry([FromBody] CreateTimeEntryViewModel model)
    {
        if (model.TaskId <= 0)
            return BadRequest(new { error = "Valid task ID is required." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var timeEntry = await _timeTrackingService.CreateManualTimeEntryAsync(model, userId);
            return CreatedAtAction(nameof(GetUserTimeEntries), new { }, timeEntry);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create time entry.", details = ex.Message });
        }
    }

    [HttpPut("entries/{entryId:int}")]
    public async Task<IActionResult> UpdateTimeEntry(int entryId, [FromBody] UpdateTimeEntryViewModel model)
    {
        if (entryId != model.Id)
            return BadRequest(new { error = "Entry ID mismatch." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var timeEntry = await _timeTrackingService.UpdateTimeEntryAsync(model, userId);
            return Ok(timeEntry);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update time entry.", details = ex.Message });
        }
    }

    [HttpDelete("entries/{entryId:int}")]
    public async Task<IActionResult> DeleteTimeEntry(int entryId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _timeTrackingService.DeleteTimeEntryAsync(entryId, userId);
            if (!success)
                return NotFound(new { error = "Time entry not found." });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete time entry.", details = ex.Message });
        }
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var dashboard = await _timeTrackingService.GetDashboardAsync(userId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get dashboard.", details = ex.Message });
        }
    }

    [HttpGet("timesheet")]
    public async Task<IActionResult> GetTimeSheet([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
            return BadRequest(new { error = "Start date must be before end date." });

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var timeSheet = await _timeTrackingService.GetTimeSheetAsync(userId, startDate, endDate);
            return Ok(timeSheet);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get timesheet.", details = ex.Message });
        }
    }

    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,TeamLead")]
    public async Task<IActionResult> GetTimeAnalytics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int? projectId = null, [FromQuery] string? userId = null)
    {
        if (startDate > endDate)
            return BadRequest(new { error = "Start date must be before end date." });

        try
        {
            var analytics = await _timeTrackingService.GetTimeAnalyticsAsync(startDate, endDate, projectId, userId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get time analytics.", details = ex.Message });
        }
    }

    [HttpPost("reports")]
    [Authorize(Roles = "Admin,TeamLead")]
    public async Task<IActionResult> GenerateTimeReport([FromBody] CreateTimeReportViewModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (model.StartDate > model.EndDate)
            return BadRequest(new { error = "Start date must be before end date." });

        try
        {
            var generatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(generatedBy))
                return Unauthorized();

            var report = await _timeTrackingService.GenerateTimeReportAsync(model, generatedBy);
            return CreatedAtAction(nameof(GetTimeAnalytics), new { }, report);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to generate time report.", details = ex.Message });
        }
    }
}

public class StartTimerViewModel
{
    public int TaskId { get; set; }
    public string? Description { get; set; }
}
