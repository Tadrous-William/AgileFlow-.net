using AgileTaskManager.Services;
using AgileTaskManager.Services.Interfaces;
using AgileTaskManager.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/task-dependencies")]
[Authorize]
public class TaskDependencyController : ControllerBase
{
    private readonly ITaskDependencyService _dependencyService;

    public TaskDependencyController(ITaskDependencyService dependencyService)
    {
        _dependencyService = dependencyService;
    }

    [HttpGet("graph/{taskId:int}")]
    public async Task<IActionResult> GetDependencyGraph(int taskId)
    {
        try
        {
            var graph = await _dependencyService.GetDependencyGraphAsync(taskId);
            return Ok(graph);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get dependency graph.", details = ex.Message });
        }
    }

    [HttpGet("blocking/{taskId:int}")]
    public async Task<IActionResult> GetBlockingTasks(int taskId)
    {
        try
        {
            var blockingTasks = await _dependencyService.GetBlockingTasksAsync(taskId);
            return Ok(blockingTasks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get blocking tasks.", details = ex.Message });
        }
    }

    [HttpGet("dependents/{taskId:int}")]
    public async Task<IActionResult> GetDependentTasks(int taskId)
    {
        try
        {
            var dependentTasks = await _dependencyService.GetDependentTasksAsync(taskId);
            return Ok(dependentTasks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get dependent tasks.", details = ex.Message });
        }
    }

    [HttpGet("can-start/{taskId:int}")]
    public async Task<IActionResult> CanStartTask(int taskId)
    {
        try
        {
            var canStart = await _dependencyService.CanStartTaskAsync(taskId);
            return Ok(new { canStart, taskId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to check if task can start.", details = ex.Message });
        }
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateDependency([FromBody] CreateDependencyViewModel model)
    {
        if (model.TaskId == model.DependsOnTaskId)
            return BadRequest(new { error = "Task cannot depend on itself." });

        try
        {
            var validation = await _dependencyService.ValidateDependencyAsync(model.TaskId, model.DependsOnTaskId);
            return Ok(validation);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to validate dependency.", details = ex.Message });
        }
    }

    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreateDependency([FromBody] CreateDependencyViewModel model)
    {
        if (model.TaskId == model.DependsOnTaskId)
            return BadRequest(new { error = "Task cannot depend on itself." });

        try
        {
            var validation = await _dependencyService.ValidateDependencyAsync(model.TaskId, model.DependsOnTaskId);
            if (!validation.IsValid)
                return BadRequest(new { error = validation.ErrorMessage, circularDependencyPath = validation.CircularDependencyPath });

            var success = await _dependencyService.CreateDependencyAsync(model.TaskId, model.DependsOnTaskId);
            if (!success)
                return BadRequest(new { error = "Failed to create dependency." });

            return Ok(new { message = "Dependency created successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create dependency.", details = ex.Message });
        }
    }

    [HttpDelete("remove")]
    [Authorize]
    public async Task<IActionResult> RemoveDependency([FromBody] RemoveDependencyViewModel model)
    {
        try
        {
            var success = await _dependencyService.RemoveDependencyAsync(model.TaskId, model.DependsOnTaskId);
            if (!success)
                return NotFound(new { error = "Dependency not found." });

            return Ok(new { message = "Dependency removed successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to remove dependency.", details = ex.Message });
        }
    }

    [HttpPut("update/{taskId:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateDependencies(int taskId, [FromBody] BulkDependencyUpdateViewModel model)
    {
        if (taskId != model.TaskId)
            return BadRequest(new { error = "Task ID mismatch." });

        if (model.DependsOnTaskIds.Contains(taskId))
            return BadRequest(new { error = "Task cannot depend on itself." });

        try
        {
            // Validate all dependencies before updating
            foreach (var dependsOnId in model.DependsOnTaskIds)
            {
                var validation = await _dependencyService.ValidateDependencyAsync(taskId, dependsOnId);
                if (!validation.IsValid)
                    return BadRequest(new { error = $"Invalid dependency on task {dependsOnId}: {validation.ErrorMessage}" });
            }

            var success = await _dependencyService.UpdateDependenciesAsync(taskId, model.DependsOnTaskIds);
            if (!success)
                return BadRequest(new { error = "Failed to update dependencies." });

            return Ok(new { message = "Dependencies updated successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update dependencies.", details = ex.Message });
        }
    }

    [HttpGet("project/{projectId:int}/dependency-tree")]
    public async Task<IActionResult> GetProjectDependencyTree(int projectId)
    {
        try
        {
            // This would require a more complex implementation to get the full dependency tree for a project
            // For now, return a simple response indicating this is a future feature
            return Ok(new { message = "Project dependency tree feature coming soon." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to get project dependency tree.", details = ex.Message });
        }
    }
}
