using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.Enums;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgileTaskManager.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly INotificationService _notifications;

    public TaskService(ApplicationDbContext db, IAuditService audit, INotificationService notifications)
    {
        _db = db;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<List<TaskListViewModel>> GetAllAsync(string? userId = null, string? role = null)
    {
        var query = _db.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.DependsOn)
            .AsQueryable();

        // Members see only their own tasks; clients see all (read-only filtered elsewhere)
        if (role == "Developer" && userId != null)
            query = query.Where(t => t.AssignedToId == userId);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskListViewModel
            {
                Id             = t.Id,
                Title          = t.Title,
                Priority       = t.Priority,
                Status         = t.Status,
                StartDate      = t.StartDate,
                Deadline       = t.Deadline,
                AssignedToName = t.AssignedTo != null ? t.AssignedTo.FullName : "Unassigned",
                IsBlocked      = t.DependsOn != null && t.DependsOn.Status != Models.Enums.TaskStatus.Done
            })
            .ToListAsync();
    }

    public async Task<TaskDetailsViewModel?> GetDetailsAsync(int id)
    {
        var task = await _db.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.DependsOn)
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .Include(t => t.Attachments)
            .Include(t => t.ActivityLogs).ThenInclude(a => a.Actor)
            .Include(t => t.Feedback).ThenInclude(f => f!.Client)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null) return null;

        return new TaskDetailsViewModel
        {
            Id             = task.Id,
            Title          = task.Title,
            Description    = task.Description,
            Priority       = task.Priority,
            Status         = task.Status,
            StartDate      = task.StartDate,
            Deadline       = task.Deadline,
            CreatedAt      = task.CreatedAt,
            AssignedToId   = task.AssignedToId,
            AssignedToName = task.AssignedTo?.FullName ?? "Unassigned",
            DependsOnId    = task.DependsOnId,
            DependsOnTitle = task.DependsOn?.Title,
            IsBlocked      = task.DependsOn != null && task.DependsOn.Status != Models.Enums.TaskStatus.Done,
            Comments       = task.Comments.OrderBy(c => c.CreatedAt).Select(c => new CommentViewModel
            {
                Id         = c.Id,
                Content    = c.Content,
                AuthorName = c.User.FullName,
                CreatedAt  = c.CreatedAt
            }).ToList(),
            Attachments  = task.Attachments.Select(a => new AttachmentViewModel
            {
                Id            = a.Id,
                FileName      = a.FileName,
                FilePath      = a.FilePath,
                FileSizeBytes = a.FileSizeBytes,
                UploadedAt    = a.UploadedAt
            }).ToList(),
            ActivityLogs = task.ActivityLogs.OrderByDescending(a => a.Timestamp).Select(a => new ActivityLogViewModel
            {
                Id        = a.Id,
                Action    = a.Action,
                OldValue  = a.OldValue,
                NewValue  = a.NewValue,
                ActorName = a.Actor.FullName,
                Timestamp = a.Timestamp
            }).ToList(),
            Feedback = task.Feedback == null ? null : new FeedbackViewModel
            {
                Id         = task.Feedback.Id,
                Rating     = task.Feedback.Rating,
                Comment    = task.Feedback.Comment,
                ClientName = task.Feedback.Client.FullName,
                CreatedAt  = task.Feedback.CreatedAt
            }
        };
    }

    public async Task<TaskItem> CreateAsync(TaskCreateViewModel vm, string creatorId)
    {
        if (vm.ProjectId.HasValue)
        {
            var projectExists = await _db.Projects.AnyAsync(p => p.Id == vm.ProjectId.Value);
            if (!projectExists)
                throw new InvalidOperationException("Project not found.");
        }
        if (vm.SprintId.HasValue)
        {
            var sprintExists = await _db.Sprints.AnyAsync(s => s.Id == vm.SprintId.Value);
            if (!sprintExists)
                throw new InvalidOperationException("Sprint not found.");
        }

        var task = new TaskItem
        {
            Title        = vm.Title,
            Description  = vm.Description,
            Priority     = vm.Priority,
            StartDate    = vm.StartDate,
            Deadline     = vm.Deadline,
            AssignedToId = vm.AssignedToId,
            DependsOnId  = vm.DependsOnId,
            ProjectId    = vm.ProjectId,
            SprintId     = vm.SprintId,
            Status       = Models.Enums.TaskStatus.ToDo
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(task.Id, creatorId, "Created", null, task.Title);

        if (task.AssignedToId != null)
            await _notifications.CreateAsync(task.AssignedToId, $"You have been assigned task: {task.Title}", $"/Task/Details/{task.Id}");

        return task;
    }

    public async Task UpdateAsync(TaskEditViewModel vm, string actorId)
    {
        var task = await _db.Tasks.FindAsync(vm.Id)
            ?? throw new KeyNotFoundException($"Task {vm.Id} not found.");

        var oldTitle = task.Title;
        task.Title       = vm.Title;
        task.Description = vm.Description;
        task.Priority    = vm.Priority;
        task.Status      = vm.Status;
        task.StartDate   = vm.StartDate;
        task.Deadline    = vm.Deadline;
        task.DependsOnId = vm.DependsOnId;

        if (task.AssignedToId != vm.AssignedToId)
        {
            task.AssignedToId = vm.AssignedToId;
            if (vm.AssignedToId != null)
                await _notifications.CreateAsync(vm.AssignedToId, $"Task reassigned to you: {task.Title}", $"/Task/Details/{task.Id}");
        }

        await _db.SaveChangesAsync();
        await _audit.LogAsync(task.Id, actorId, "Updated", oldTitle, task.Title);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return false;
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task AssignAsync(int taskId, string userId, string actorId)
    {
        var task = await _db.Tasks.FindAsync(taskId)
            ?? throw new KeyNotFoundException();
        var oldAssignee = task.AssignedToId;
        task.AssignedToId = userId;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(taskId, actorId, "Assigned", oldAssignee, userId);
        await _notifications.CreateAsync(userId, $"Task assigned to you: {task.Title}", $"/Task/Details/{taskId}");
    }

    public async Task UpdateStatusAsync(int taskId, Models.Enums.TaskStatus newStatus, string actorId)
    {
        var task = await _db.Tasks.Include(t => t.DependsOn).FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException();

        if (task.DependsOn != null && task.DependsOn.Status != Models.Enums.TaskStatus.Done
            && newStatus == Models.Enums.TaskStatus.InProgress)
            throw new InvalidOperationException("Cannot start a task that has unfinished dependencies.");

        var oldStatus = task.Status;
        task.Status = newStatus;

        if (newStatus == Models.Enums.TaskStatus.Done)
        {
            task.CompletedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        await _audit.LogAsync(taskId, actorId, "StatusChanged", oldStatus.ToString(), newStatus.ToString());

        // Task completed - no event publishing needed
    }

    public async Task<bool> IsBlockedAsync(int taskId)
    {
        var task = await _db.Tasks.Include(t => t.DependsOn).FirstOrDefaultAsync(t => t.Id == taskId);
        return task?.DependsOn != null && task.DependsOn.Status != Models.Enums.TaskStatus.Done;
    }

    public async Task<List<TaskListViewModel>> GetByUserAsync(string userId)
        => await GetAllAsync(userId, "Developer");
}
