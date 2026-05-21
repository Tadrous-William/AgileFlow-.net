using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.Enums;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using TaskStatus = AgileTaskManager.Models.Enums.TaskStatus;

namespace AgileTaskManager.Services;

public class SprintService : ISprintService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public SprintService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<SprintViewModel> CreateAsync(CreateSprintViewModel model, string createdBy)
    {
        // Verify project exists and user has access
        var project = await _db.Projects.FindAsync(model.ProjectId);
        if (project == null)
            throw new InvalidOperationException("Project not found.");

        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == model.ProjectId && pm.UserId == createdBy);
        if (member == null || (member.Role != ProjectMemberRole.TeamLead && member.Role != ProjectMemberRole.Admin))
            throw new UnauthorizedAccessException("Only team leads can create sprints.");

        var sprint = new Sprint
        {
            Name = model.Name,
            Description = model.Description,
            ProjectId = model.ProjectId,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Status = SprintStatus.Planned,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _db.Sprints.Add(sprint);
        await _db.SaveChangesAsync();

        // Sprint-level events are not logged to ActivityLogs (that table requires a valid Task FK)

        return (await GetByIdAsync(sprint.Id))
            ?? throw new InvalidOperationException("Sprint could not be loaded after save.");
    }

    public async Task<SprintViewModel> UpdateAsync(UpdateSprintViewModel model, string updatedBy)
    {
        var sprint = await _db.Sprints.FindAsync(model.Id);
        if (sprint == null)
            throw new InvalidOperationException("Sprint not found.");

        // Check permissions
        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == sprint.ProjectId && pm.UserId == updatedBy);
        if (member == null || (member.Role != ProjectMemberRole.TeamLead && member.Role != ProjectMemberRole.Admin))
            throw new UnauthorizedAccessException("Only team leads can update sprints.");

        // Can't update active sprint dates
        if (sprint.Status == SprintStatus.Active && (model.StartDate != sprint.StartDate || model.EndDate != sprint.EndDate))
            throw new InvalidOperationException("Cannot change dates of an active sprint.");

        sprint.Name = model.Name;
        sprint.Description = model.Description;
        sprint.StartDate = model.StartDate;
        sprint.EndDate = model.EndDate;
        sprint.UpdatedBy = updatedBy;
        sprint.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Sprint-level events are not logged to ActivityLogs (that table requires a valid Task FK)

        return (await GetByIdAsync(sprint.Id))
            ?? throw new InvalidOperationException("Sprint could not be loaded after save.");
    }

    public async Task<bool> DeleteAsync(int sprintId, string userId)
    {
        var sprint = await _db.Sprints.FindAsync(sprintId);
        if (sprint == null)
            return false;

        // Check permissions
        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == sprint.ProjectId && pm.UserId == userId);
        if (member == null || (member.Role != ProjectMemberRole.TeamLead && member.Role != ProjectMemberRole.Admin))
            throw new UnauthorizedAccessException("Only team leads can delete sprints.");

        // Can't delete active sprint
        if (sprint.Status == SprintStatus.Active)
            throw new InvalidOperationException("Cannot delete an active sprint.");

        _db.Sprints.Remove(sprint);
        await _db.SaveChangesAsync();

        // Sprint-level events are not logged to ActivityLogs (that table requires a valid Task FK)

        return true;
    }

    public async Task<SprintViewModel?> GetByIdAsync(int sprintId)
    {
        var sprint = await _db.Sprints
            .Include(s => s.Project)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == sprintId);

        if (sprint == null)
            return null;

        return new SprintViewModel
        {
            Id = sprint.Id,
            Name = sprint.Name,
            Description = sprint.Description,
            ProjectId = sprint.ProjectId,
            ProjectName = sprint.Project?.Name ?? "",
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            Status = sprint.Status,
            CreatedBy = sprint.CreatedBy,
            CreatedAt = sprint.CreatedAt,
            UpdatedAt = sprint.UpdatedAt,
            TotalTasks = sprint.Tasks.Count,
            CompletedTasks = sprint.Tasks.Count(t => t.Status == TaskStatus.Done),
            InProgressTasks = sprint.Tasks.Count(t => t.Status == TaskStatus.InProgress),
            BlockedTasks = sprint.Tasks.Count(t => t.DependsOnId.HasValue)
        };
    }

    public async Task<List<SprintViewModel>> GetByProjectAsync(int projectId)
    {
        var sprints = await _db.Sprints
            .Where(s => s.ProjectId == projectId)
            .Include(s => s.Project)
            .Include(s => s.Tasks)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return sprints.Select(s => new SprintViewModel
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            ProjectId = s.ProjectId,
            ProjectName = s.Project?.Name ?? "",
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status,
            CreatedBy = s.CreatedBy,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            TotalTasks = s.Tasks.Count,
            CompletedTasks = s.Tasks.Count(t => t.Status == TaskStatus.Done),
            InProgressTasks = s.Tasks.Count(t => t.Status == TaskStatus.InProgress),
            BlockedTasks = s.Tasks.Count(t => t.DependsOnId.HasValue)
        }).ToList();
    }

    public async Task<SprintViewModel?> GetCurrentSprintAsync(int projectId)
    {
        var sprint = await _db.Sprints
            .Include(s => s.Project)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Status == SprintStatus.Active);

        if (sprint == null)
            return null;

        return new SprintViewModel
        {
            Id = sprint.Id,
            Name = sprint.Name,
            Description = sprint.Description,
            ProjectId = sprint.ProjectId,
            ProjectName = sprint.Project?.Name ?? "",
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            Status = sprint.Status,
            CreatedBy = sprint.CreatedBy,
            CreatedAt = sprint.CreatedAt,
            UpdatedAt = sprint.UpdatedAt,
            TotalTasks = sprint.Tasks.Count,
            CompletedTasks = sprint.Tasks.Count(t => t.Status == TaskStatus.Done),
            InProgressTasks = sprint.Tasks.Count(t => t.Status == TaskStatus.InProgress),
            BlockedTasks = sprint.Tasks.Count(t => t.DependsOnId.HasValue)
        };
    }

    public async Task<List<SprintViewModel>> GetActiveSprintsAsync(int projectId)
    {
        var sprints = await _db.Sprints
            .Where(s => s.ProjectId == projectId && (s.Status == SprintStatus.Active || s.Status == SprintStatus.Planned))
            .Include(s => s.Project)
            .Include(s => s.Tasks)
            .OrderBy(s => s.StartDate)
            .ToListAsync();

        return sprints.Select(s => new SprintViewModel
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            ProjectId = s.ProjectId,
            ProjectName = s.Project?.Name ?? "",
            StartDate = s.StartDate,
            EndDate = s.EndDate,
            Status = s.Status,
            CreatedBy = s.CreatedBy,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            TotalTasks = s.Tasks.Count,
            CompletedTasks = s.Tasks.Count(t => t.Status == TaskStatus.Done),
            InProgressTasks = s.Tasks.Count(t => t.Status == TaskStatus.InProgress),
            BlockedTasks = s.Tasks.Count(t => t.DependsOnId.HasValue)
        }).ToList();
    }

    public async Task<bool> StartSprintAsync(int sprintId, string userId)
    {
        var sprint = await _db.Sprints.FindAsync(sprintId);
        if (sprint == null)
            return false;

        // Check permissions
        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == sprint.ProjectId && pm.UserId == userId);
        if (member == null || (member.Role != ProjectMemberRole.TeamLead && member.Role != ProjectMemberRole.Admin))
            throw new UnauthorizedAccessException("Only team leads can start sprints.");

        // Check if there's already an active sprint
        var activeSprint = await _db.Sprints
            .FirstOrDefaultAsync(s => s.ProjectId == sprint.ProjectId && s.Status == SprintStatus.Active);
        if (activeSprint != null)
            throw new InvalidOperationException("There is already an active sprint in this project.");

        sprint.Status = SprintStatus.Active;
        sprint.ActualStartDate = DateTime.UtcNow;
        sprint.UpdatedBy = userId;
        sprint.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Sprint-level events are not logged to ActivityLogs (that table requires a valid Task FK)

        return true;
    }

    public async Task<bool> CompleteSprintAsync(int sprintId, string userId)
    {
        var sprint = await _db.Sprints
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == sprintId);
        if (sprint == null)
            return false;

        // Check permissions
        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == sprint.ProjectId && pm.UserId == userId);
        if (member == null || (member.Role != ProjectMemberRole.TeamLead && member.Role != ProjectMemberRole.Admin))
            throw new UnauthorizedAccessException("Only team leads can complete sprints.");

        if (sprint.Status != SprintStatus.Active)
            throw new InvalidOperationException("Only active sprints can be completed.");

        sprint.Status = SprintStatus.Completed;
        sprint.ActualEndDate = DateTime.UtcNow;
        sprint.UpdatedBy = userId;
        sprint.UpdatedAt = DateTime.UtcNow;

        // Move incomplete tasks to next sprint or backlog
        var incompleteTasks = sprint.Tasks.Where(t => t.Status != TaskStatus.Done).ToList();
        foreach (var task in incompleteTasks)
        {
            task.SprintId = null; // Move to backlog
        }

        await _db.SaveChangesAsync();

        // Sprint-level events are not logged to ActivityLogs (that table requires a valid Task FK)

        return true;
    }

    public async Task<SprintAnalyticsViewModel> GetSprintAnalyticsAsync(int sprintId)
    {
        // Load the sprint itself (no nav-property includes — load tasks separately)
        var sprint = await _db.Sprints
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sprintId);

        if (sprint == null)
            throw new InvalidOperationException("Sprint not found.");

        // Explicit WHERE SprintId = sprintId — never relies on EF nav-property caching
        var tasks = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.AssignedTo)
            .Where(t => t.SprintId == sprintId)
            .ToListAsync();

        var completedTasks = tasks.Where(t => t.Status == TaskStatus.Done).ToList();
        var now = DateTime.UtcNow;

        var overdue = tasks.Count(t =>
            t.Deadline.HasValue &&
            t.Deadline.Value < now &&
            t.Status != TaskStatus.Done);

        var burndownRows = await GetBurndownDataAsync(sprintId);
        var total = tasks.Count;
        var burndownPoints = burndownRows.Select(b => new BurndownPointViewModel
        {
            Date = b.Date,
            IdealRemaining = b.IdealRemaining,
            ActualRemaining = b.ActualRemaining,
            CompletedTasks = total > 0 ? total - b.ActualRemaining : 0,
            TotalTasks = total
        }).ToList();

        var teamPerformance = tasks
            .GroupBy(t => t.AssignedToId ?? string.Empty)
            .Select(g =>
            {
                var list = g.ToList();
                var first = list[0];
                return new TeamMemberPerformanceViewModel
                {
                    UserId = g.Key,
                    UserName = first.AssignedTo?.FullName ?? "Unassigned",
                    AssignedTasks = list.Count,
                    CompletedTasks = list.Count(t => t.Status == TaskStatus.Done),
                    OverdueTasks = list.Count(t =>
                        t.Deadline.HasValue && t.Deadline.Value < now && t.Status != TaskStatus.Done)
                };
            })
            .OrderByDescending(m => m.AssignedTasks)
            .ToList();

        var recentActivity = new List<ActivityLogViewModel>();
        var taskIds = tasks.Select(t => t.Id).ToList();
        if (taskIds.Count > 0)
        {
            var logs = await _db.ActivityLogs
                .Where(a => taskIds.Contains(a.TaskId))
                .Include(a => a.Actor)
                .OrderByDescending(a => a.Timestamp)
                .Take(25)
                .AsNoTracking()
                .ToListAsync();

            recentActivity = logs.Select(a => new ActivityLogViewModel
            {
                Id = a.Id,
                Action = a.Action,
                EntityId = a.TaskId.ToString(),
                Timestamp = a.Timestamp,
                ActorName = a.Actor?.FullName ?? "Unknown user",
                EntityType = "Task",
                OldValue = a.OldValue,
                NewValue = a.NewValue
            }).ToList();
        }

        return new SprintAnalyticsViewModel
        {
            SprintId = sprint.Id,
            SprintName = sprint.Name,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            Status = sprint.Status,
            TotalTasks = tasks.Count,
            CompletedTasks = completedTasks.Count,
            InProgressTasks = tasks.Count(t => t.Status == TaskStatus.InProgress),
            ToDoTasks = tasks.Count(t => t.Status == TaskStatus.ToDo),
            TestingTasks = tasks.Count(t => t.Status == TaskStatus.Testing),
            OverdueTasks = overdue,
            CompletionPercentage = tasks.Count > 0 ? (double)completedTasks.Count / tasks.Count * 100 : 0,
            BlockedTasks = tasks.Count(t => t.DependsOnId.HasValue),
            AverageTaskCompletionTime = completedTasks.Any()
                ? Math.Round(completedTasks.Average(t => ((t.CompletedAt ?? now) - t.CreatedAt).TotalHours))
                : 0,
            TasksByStatus = tasks.GroupBy(t => t.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            TasksByPriority = tasks.GroupBy(t => t.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            BurndownData = burndownPoints,
            TeamPerformance = teamPerformance,
            RecentActivity = recentActivity
        };
    }

    public async Task<ProjectAnalyticsViewModel> GetProjectAnalyticsAsync(int projectId)
    {
        var project = await _db.Projects
            .Include(p => p.Tasks)
            .Include(p => p.Sprints)
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            throw new InvalidOperationException("Project not found.");

        var tasks = project.Tasks.ToList();
        var sprints = project.Sprints.ToList();
        var now = DateTime.UtcNow;
        var end = now.Date;
        var start = end.AddDays(-29);

        var trends = new List<TaskTrendViewModel>();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            trends.Add(new TaskTrendViewModel
            {
                Date = d,
                CreatedTasks = tasks.Count(t => t.CreatedAt.Date == d),
                CompletedTasks = tasks.Count(t =>
                    t.CompletedAt.HasValue && t.CompletedAt.Value.Date == d)
            });
        }

        var sprintSummaries = sprints
            .OrderByDescending(s => s.StartDate)
            .Select(s =>
            {
                var sprintTasks = tasks.Where(t => t.SprintId == s.Id).ToList();
                return new SprintSummaryViewModel
                {
                    SprintId = s.Id,
                    SprintName = s.Name,
                    Status = s.Status,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    TotalTasks = sprintTasks.Count,
                    CompletedTasks = sprintTasks.Count(t => t.Status == TaskStatus.Done)
                };
            })
            .ToList();

        var completedCount = tasks.Count(t => t.Status == TaskStatus.Done);

        return new ProjectAnalyticsViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            TotalTasks = tasks.Count,
            CompletedTasks = completedCount,
            ActiveSprintsCount = sprints.Count(s => s.Status == SprintStatus.Active),
            TotalSprintsCount = sprints.Count,
            TeamSize = project.Members.Count,
            SprintSummaries = sprintSummaries,
            TaskTrends = trends,
            PeriodStart = start,
            PeriodEnd = end,
            InProgressTasks = tasks.Count(t => t.Status == TaskStatus.InProgress),
            OverdueTasks = tasks.Count(t =>
                t.Deadline.HasValue && t.Deadline.Value < now && t.Status != TaskStatus.Done),
            TasksByStatus = tasks.GroupBy(t => t.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            TasksByPriority = tasks.GroupBy(t => t.Priority.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            BudgetUtilization = project.Budget > 0 ? project.TotalCost / project.Budget * 100m : 0m,
            ProjectVelocity = sprints.Count > 0 ? (double)completedCount / sprints.Count : 0d
        };
    }

    public async Task<List<SprintBurndownViewModel>> GetBurndownDataAsync(int sprintId)
    {
        var sprint = await _db.Sprints
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sprintId);

        if (sprint == null)
            throw new InvalidOperationException("Sprint not found.");

        // Direct query — no nav-property ambiguity
        var sprintTasks = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.SprintId == sprintId)
            .Select(t => new { t.Status, t.CompletedAt })
            .ToListAsync();

        var burndownData = new List<SprintBurndownViewModel>();
        var startDate = sprint.ActualStartDate ?? sprint.StartDate;
        var endDate = DateTime.UtcNow.Date > sprint.EndDate.Date ? sprint.EndDate.Date : DateTime.UtcNow.Date;
        if (endDate < startDate.Date)
            endDate = startDate.Date;

        var sprintSpanDays = Math.Max(1, (endDate - startDate.Date).Days + 1);
        var totalTaskCount = sprintTasks.Count;

        for (var date = startDate.Date; date <= endDate; date = date.AddDays(1))
        {
            var completedByDate = sprintTasks
                .Count(t => t.Status == TaskStatus.Done && t.CompletedAt.HasValue && t.CompletedAt.Value.Date <= date);

            burndownData.Add(new SprintBurndownViewModel
            {
                Date = date,
                IdealRemaining = Math.Max(0, totalTaskCount - (int)((date - startDate.Date).Days * (double)totalTaskCount / sprintSpanDays)),
                ActualRemaining = totalTaskCount - completedByDate,
                CompletedTasks = completedByDate,
                TotalTasks = totalTaskCount
            });
        }

        return burndownData;
    }
}
