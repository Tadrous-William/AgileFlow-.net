using AgileTaskManager.Constants;
using AgileTaskManager.Data;
using AgileTaskManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ISprintService _sprintService;

    public AnalyticsApiController(ApplicationDbContext db, ISprintService sprintService)
    {
        _db = db;
        _sprintService = sprintService;
    }

    [HttpGet("sprint/{sprintId:int}")]
    public async Task<IActionResult> GetSprintAnalytics(int sprintId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var sprint = await _db.Sprints.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sprintId);
        if (sprint == null)
            return NotFound(new { error = "Sprint not found." });

        // Allow any authenticated user to view sprint analytics

        try
        {
            var analytics = await _sprintService.GetSprintAnalyticsAsync(sprintId);
            return Ok(analytics);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("project/{projectId:int}")]
    public async Task<IActionResult> GetProjectAnalytics(int projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Allow any authenticated user to view project analytics

        try
        {
            var analytics = await _sprintService.GetProjectAnalyticsAsync(projectId);
            return Ok(analytics);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
