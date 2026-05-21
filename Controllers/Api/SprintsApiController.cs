using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using AgileTaskManager.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/projects/{projectId:int}/sprints")]
[Authorize]
public class SprintsApiController : ControllerBase
{
    private readonly ISprintService _sprintService;

    public SprintsApiController(ISprintService sprintService)
    {
        _sprintService = sprintService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByProject(int projectId)
    {
        try
        {
            var sprints = await _sprintService.GetByProjectAsync(projectId);
            return Ok(sprints);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sprint = await _sprintService.GetByIdAsync(id);
        if (sprint == null)
            return NotFound();

        return Ok(sprint);
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(int projectId)
    {
        var sprint = await _sprintService.GetCurrentSprintAsync(projectId);
        if (sprint == null)
            return NotFound();

        return Ok(sprint);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(int projectId)
    {
        var sprints = await _sprintService.GetActiveSprintsAsync(projectId);
        return Ok(sprints);
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> Create(int projectId, [FromBody] CreateSprintViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var sprint = await _sprintService.CreateAsync(vm, userId!);
            return CreatedAtAction(nameof(GetByProject), new { projectId }, sprint);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSprintViewModel vm)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var sprint = await _sprintService.UpdateAsync(vm, userId!);
            return Ok(sprint);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _sprintService.DeleteAsync(id, userId!);
            if (!result)
                return NotFound();

            return Ok(new { message = "Sprint deleted successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:int}/start")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> Start(int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _sprintService.StartSprintAsync(id, userId!);
            if (!result)
                return NotFound();

            return Ok(new { message = "Sprint started successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.TeamLead}")]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _sprintService.CompleteSprintAsync(id, userId!);
            if (!result)
                return NotFound();

            return Ok(new { message = "Sprint completed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("{id:int}/analytics")]
    public async Task<IActionResult> GetAnalytics(int id)
    {
        try
        {
            var analytics = await _sprintService.GetSprintAnalyticsAsync(id);
            return Ok(analytics);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id:int}/burndown")]
    public async Task<IActionResult> GetBurndown(int id)
    {
        try
        {
            var burndown = await _sprintService.GetBurndownDataAsync(id);
            return Ok(burndown);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
