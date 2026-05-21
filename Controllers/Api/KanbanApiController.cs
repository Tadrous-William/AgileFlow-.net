using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.Enums;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using AgileTaskManager.Hubs;
using TaskStatus = AgileTaskManager.Models.Enums.TaskStatus;
using System.Security.Claims;

namespace AgileTaskManager.Controllers.Api;

[ApiController]
[Route("api/kanban")]
[Authorize]
public class KanbanController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<TaskHub> _taskHub;

    public KanbanController(ApplicationDbContext db, IHubContext<TaskHub> taskHub)
    {
        _db = db;
        _taskHub = taskHub;
    }

    [HttpGet("board")]
    public async Task<IActionResult> GetBoard([FromQuery] int projectId, [FromQuery] int? sprintId = null)
    {
        // Validate project exists
        var project = await _db.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound(new { error = "Project not found." });

        // Check if user has access to this project
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isMember = await _db.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

        if (!isMember && !User.IsInRole(RoleConstants.Admin))
            return Forbid();

        // Validate sprint if specified
        Sprint? sprint = null;
        if (sprintId.HasValue)
        {
            sprint = await _db.Sprints
                .FirstOrDefaultAsync(s => s.Id == sprintId.Value && s.ProjectId == projectId);
            
            if (sprint == null)
                return NotFound(new { error = "Sprint not found." });
        }

        // Get tasks for the board
        var tasksQuery = _db.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.DependsOn)
            .Where(t => t.ProjectId == projectId);

        if (sprintId.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.SprintId == sprintId.Value);
        }

        var tasks = await tasksQuery.ToListAsync();

        // Group tasks by status
        var allStatuses = Enum.GetValues<TaskStatus>();
        var columns = new List<KanbanColumnViewModel>();

        foreach (var status in allStatuses)
        {
            var statusTasks = tasks
                .Where(t => t.Status == status)
                .Select(t => new KanbanTaskViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority.ToString(),
                    Status = t.Status.ToString(),
                    StartDate = t.StartDate,
                    Deadline = t.Deadline,
                    CreatedAt = t.CreatedAt,
                    AssignedToId = t.AssignedToId,
                    AssignedToName = t.AssignedTo?.FullName,
                    IsBlocked = t.DependsOnId.HasValue && t.DependsOn?.Status != TaskStatus.Done,
                    IsOverdue = t.Deadline.HasValue && t.Deadline.Value < DateTime.UtcNow && t.Status != TaskStatus.Done,
                    DependsOnId = t.DependsOnId,
                    DependsOnTitle = t.DependsOn?.Title,
                    ProjectName = project.Name,
                    SprintName = sprint?.Name
                })
                .OrderBy(t => (int)Enum.Parse(typeof(TaskPriority), t.Priority))
                .ThenBy(t => t.CreatedAt)
                .ToList();

            columns.Add(new KanbanColumnViewModel
            {
                Status = status.ToString(),
                Title = GetStatusTitle(status),
                Tasks = statusTasks.ToArray()
            });
        }

        var board = new KanbanBoardViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            SprintId = sprintId,
            SprintName = sprint?.Name,
            Columns = columns.ToArray()
        };

        return Ok(board);
    }

    [HttpPost("move")]
    public async Task<IActionResult> MoveTask([FromBody] MoveTaskViewModel model)
    {
        var task = await _db.Tasks
            .Include(t => t.Project)
            .Include(t => t.DependsOn)
            .FirstOrDefaultAsync(t => t.Id == model.TaskId);

        if (task == null)
            return NotFound(new { error = "Task not found." });

        // Check if user has access to this project
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isMember = await _db.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);

        if (!isMember && !User.IsInRole(RoleConstants.Admin))
            return Forbid();

        // Validate status transition
        if (!IsValidStatusTransition(task.Status, model.NewStatus))
            return BadRequest(new { error = "Invalid status transition." });

        // Check dependencies for tasks moving to InProgress or Testing
        if ((model.NewStatus == TaskStatus.InProgress || model.NewStatus == TaskStatus.Testing) && 
            task.DependsOnId.HasValue)
        {
            var dependency = await _db.Tasks
                .FirstOrDefaultAsync(t => t.Id == task.DependsOnId.Value);

            if (dependency?.Status != TaskStatus.Done)
                return BadRequest(new { error = "Cannot move task: dependency is not completed." });
        }

        var oldStatus = task.Status;
        task.Status = model.NewStatus;

        // Track completion timestamp — required for burndown & avg completion time
        if (model.NewStatus == TaskStatus.Done && task.CompletedAt == null)
            task.CompletedAt = DateTime.UtcNow;
        else if (model.NewStatus != TaskStatus.Done)
            task.CompletedAt = null; // task reopened

        // Create activity log
        var activityLog = new ActivityLog
        {
            TaskId = task.Id,
            ActorId = userId ?? string.Empty,
            Action = "StatusChanged",
            OldValue = oldStatus.ToString(),
            NewValue = model.NewStatus.ToString()
        };
        _db.ActivityLogs.Add(activityLog);

        await _db.SaveChangesAsync();

        // Send SignalR notification for real-time board update
        await _taskHub.Clients.Group(task.SprintId.HasValue 
            ? $"kanban-project-{task.ProjectId}-sprint-{task.SprintId.Value}"
            : $"kanban-project-{task.ProjectId}")
            .SendAsync("ReceiveTaskMove", new { 
                taskId = task.Id, 
                oldStatus = oldStatus.ToString(), 
                newStatus = model.NewStatus.ToString(), 
                movedBy = User.Identity?.Name 
            });

        return NoContent();
    }

    private static string GetStatusTitle(TaskStatus status)
    {
        return status switch
        {
            TaskStatus.ToDo => "To Do",
            TaskStatus.InProgress => "In Progress",
            TaskStatus.Testing => "Testing",
            TaskStatus.Done => "Done",
            _ => status.ToString()
        };
    }

    private static bool IsValidStatusTransition(TaskStatus from, TaskStatus to)
    {
        // Allow any transition for now, but you can implement strict rules here
        // For example: Done -> Testing should not be allowed
        if (from == TaskStatus.Done && to != TaskStatus.Done)
            return false;

        return true;
    }
}
