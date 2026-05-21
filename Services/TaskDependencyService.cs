using AgileTaskManager.Data;
using AgileTaskManager.Models.Entities;
using AgileTaskManager.Models.Enums;
using AgileTaskManager.Models.ViewModels;
using AgileTaskManager.Services.Interfaces;
using TaskStatus = AgileTaskManager.Models.Enums.TaskStatus;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AgileTaskManager.Services;

public class TaskDependencyService : ITaskDependencyService
{
    private readonly ApplicationDbContext _db;

    public TaskDependencyService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DependencyGraphViewModel> GetDependencyGraphAsync(int taskId)
    {
        var task = await _db.Tasks
            .Include(t => t.DependsOn)
            .Include(t => t.Dependents)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new ArgumentException("Task not found", nameof(taskId));

        var dependencies = await GetDependenciesRecursive(taskId, new HashSet<int>());
        var dependents = await GetDependentsRecursive(taskId, new HashSet<int>());

        return new DependencyGraphViewModel
        {
            RootTaskId = task.Id,
            RootTaskTitle = task.Title,
            RootTaskStatus = task.Status,
            RootTaskPriority = task.Priority,
            Dependencies = dependencies,
            Dependents = dependents,
            CanStart = dependencies.All(d => d.IsCompleted),
            IsBlockingDependents = !dependencies.All(d => d.IsCompleted)
        };
    }

    public async Task<DependencyValidationResult> ValidateDependencyAsync(int taskId, int dependsOnTaskId)
    {
        var result = new DependencyValidationResult { IsValid = true };

        // Check if tasks exist
        var task = await _db.Tasks.FindAsync(taskId);
        var dependsOnTask = await _db.Tasks.FindAsync(dependsOnTaskId);

        if (task == null || dependsOnTask == null)
        {
            result.IsValid = false;
            result.ErrorMessage = "One or both tasks not found";
            return result;
        }

        // Check if dependency already exists (either primary or join table)
        var alreadyExists = task.DependsOnId == dependsOnTaskId
            || await _db.TaskDependencies.AnyAsync(td => td.TaskId == taskId && td.DependsOnTaskId == dependsOnTaskId);
        if (alreadyExists)
        {
            result.IsValid = false;
            result.ErrorMessage = "Dependency already exists";
            return result;
        }

        // Check for circular dependency
        var circularPath = await CheckCircularDependencyAsync(taskId, dependsOnTaskId);
        if (circularPath.Count > 0)
        {
            result.IsValid = false;
            result.ErrorMessage = "Circular dependency detected";
            result.CircularDependencyPath = circularPath;
            return result;
        }

        // Check if the dependency task is completed (warning)
        if (dependsOnTask.Status == TaskStatus.Done)
        {
            result.Warnings.Add("The dependency task is already completed. This dependency may not be necessary.");
        }

        // Check if the dependency task is in the same project
        if (task.ProjectId != dependsOnTask.ProjectId)
        {
            result.Warnings.Add("Tasks are in different projects. Cross-project dependencies may be difficult to manage.");
        }

        // Check if the dependency task has a later deadline
        if (dependsOnTask.Deadline.HasValue && task.Deadline.HasValue && 
            dependsOnTask.Deadline.Value > task.Deadline.Value)
        {
            result.Warnings.Add("The dependency task has a later deadline than this task. This may cause scheduling issues.");
        }

        return result;
    }

    public async Task<bool> CreateDependencyAsync(int taskId, int dependsOnTaskId)
    {
        var validation = await ValidateDependencyAsync(taskId, dependsOnTaskId);
        if (!validation.IsValid)
            return false;

        var task = await _db.Tasks.FindAsync(taskId);
        if (task == null) return false;

        // Add to join table
        var existing = await _db.TaskDependencies
            .AnyAsync(td => td.TaskId == taskId && td.DependsOnTaskId == dependsOnTaskId);
        if (!existing)
        {
            _db.TaskDependencies.Add(new TaskDependency
            {
                TaskId = taskId,
                DependsOnTaskId = dependsOnTaskId,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Set as primary dependency if none set
        if (task.DependsOnId == null)
            task.DependsOnId = dependsOnTaskId;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveDependencyAsync(int taskId, int dependsOnTaskId)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.DependsOnId == dependsOnTaskId);
        if (task == null) return false;

        // Remove from join table
        var joinEntry = await _db.TaskDependencies
            .FirstOrDefaultAsync(td => td.TaskId == taskId && td.DependsOnTaskId == dependsOnTaskId);
        if (joinEntry != null)
            _db.TaskDependencies.Remove(joinEntry);

        // Clear primary dependency if it matches
        if (task.DependsOnId == dependsOnTaskId)
        {
            // Set primary to next available dependency if any
            var nextDep = await _db.TaskDependencies
                .Where(td => td.TaskId == taskId)
                .Select(td => td.DependsOnTaskId)
                .FirstOrDefaultAsync();
            task.DependsOnId = nextDep > 0 ? nextDep : null;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<TaskDependencyViewModel>> GetBlockingTasksAsync(int taskId)
    {
        return await GetDependenciesRecursive(taskId, new HashSet<int>());
    }

    public async Task<List<TaskDependencyViewModel>> GetDependentTasksAsync(int taskId)
    {
        return await GetDependentsRecursive(taskId, new HashSet<int>());
    }

    public async Task<bool> UpdateDependenciesAsync(int taskId, List<int> dependsOnTaskIds)
    {
        var task = await _db.Tasks.FindAsync(taskId);
        if (task == null) return false;

        // Validate all new dependencies first
        foreach (var depId in dependsOnTaskIds)
        {
            var validation = await ValidateDependencyAsync(taskId, depId);
            if (!validation.IsValid) return false;
        }

        // Remove existing join-table dependencies
        var existingDeps = await _db.TaskDependencies
            .Where(td => td.TaskId == taskId)
            .ToListAsync();
        _db.TaskDependencies.RemoveRange(existingDeps);

        // Clear primary dependency
        task.DependsOnId = null;

        // Add new dependencies via join table
        foreach (var depId in dependsOnTaskIds)
        {
            _db.TaskDependencies.Add(new TaskDependency
            {
                TaskId = taskId,
                DependsOnTaskId = depId,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Set first as primary dependency for backward compat
        if (dependsOnTaskIds.Count > 0)
        {
            task.DependsOnId = dependsOnTaskIds.First();
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanStartTaskAsync(int taskId)
    {
        var dependencies = await GetBlockingTasksAsync(taskId);
        return dependencies.All(d => d.IsCompleted);
    }

    private async Task<List<TaskDependencyViewModel>> GetDependenciesRecursive(int taskId, HashSet<int> visited)
    {
        var result = new List<TaskDependencyViewModel>();

        if (!visited.Add(taskId))
            return result;

        var task = await _db.Tasks
            .Include(t => t.DependsOn)
                .ThenInclude(d => d!.AssignedTo)
            .Include(t => t.Dependencies)
                .ThenInclude(d => d.DependsOnTask)
                    .ThenInclude(dt => dt.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) return result;

        var visitedDeps = new HashSet<int>();

        // Collect from primary dependency
        if (task.DependsOn != null && visitedDeps.Add(task.DependsOn.Id))
        {
            result.Add(new TaskDependencyViewModel
            {
                TaskId = task.DependsOn.Id,
                TaskTitle = task.DependsOn.Title,
                TaskStatus = task.DependsOn.Status,
                Priority = task.DependsOn.Priority,
                AssignedToName = task.DependsOn.AssignedTo?.FullName,
                Deadline = task.DependsOn.Deadline
            });
            var subDeps = await GetDependenciesRecursive(task.DependsOn.Id, visited);
            result.AddRange(subDeps);
        }

        // Collect from join table
        foreach (var dep in task.Dependencies)
        {
            if (!visitedDeps.Add(dep.DependsOnTaskId)) continue;

            result.Add(new TaskDependencyViewModel
            {
                TaskId = dep.DependsOnTask.Id,
                TaskTitle = dep.DependsOnTask.Title,
                TaskStatus = dep.DependsOnTask.Status,
                Priority = dep.DependsOnTask.Priority,
                AssignedToName = dep.DependsOnTask.AssignedTo?.FullName,
                Deadline = dep.DependsOnTask.Deadline
            });
            var subDeps = await GetDependenciesRecursive(dep.DependsOnTaskId, visited);
            result.AddRange(subDeps);
        }

        return result;
    }

    private async Task<List<TaskDependencyViewModel>> GetDependentsRecursive(int taskId, HashSet<int> visited)
    {
        var result = new List<TaskDependencyViewModel>();

        if (!visited.Add(taskId))
            return result;

        var dependents = await _db.Tasks
            .Include(t => t.AssignedTo)
            .Where(t => t.DependsOnId == taskId)
            .ToListAsync();

        var joinDependents = await _db.TaskDependencies
            .Include(td => td.Task)
                .ThenInclude(t => t.AssignedTo)
            .Where(td => td.DependsOnTaskId == taskId)
            .Select(td => td.Task)
            .ToListAsync();

        var allDependents = dependents.Concat(joinDependents)
            .DistinctBy(t => t.Id)
            .ToList();

        foreach (var dependent in allDependents)
        {
            var dependentVm = new TaskDependencyViewModel
            {
                TaskId = dependent.Id,
                TaskTitle = dependent.Title,
                TaskStatus = dependent.Status,
                Priority = dependent.Priority,
                AssignedToName = dependent.AssignedTo?.FullName,
                Deadline = dependent.Deadline
            };

            result.Add(dependentVm);

            var subDependents = await GetDependentsRecursive(dependent.Id, visited);
            result.AddRange(subDependents);
        }

        return result;
    }

    private async Task<List<string>> CheckCircularDependencyAsync(int taskId, int dependsOnTaskId)
    {
        var path = new List<string>();
        var visited = new HashSet<int>();

        // Check if we can reach taskId from dependsOnTaskId
        if (await HasCircularPath(dependsOnTaskId, taskId, visited))
        {
            path.Add($"Task {taskId} -> Task {dependsOnTaskId}");
            await BuildCircularPath(dependsOnTaskId, taskId, path, visited);
        }

        return path;
    }

    private async Task<bool> HasCircularPath(int fromTaskId, int toTaskId, HashSet<int> visited)
    {
        if (!visited.Add(fromTaskId))
            return false;

        if (fromTaskId == toTaskId)
            return true;

        // Check primary dependents
        var task = await _db.Tasks
            .Include(t => t.Dependents)
            .FirstOrDefaultAsync(t => t.Id == fromTaskId);

        if (task != null)
        {
            foreach (var dependent in task.Dependents)
            {
                if (await HasCircularPath(dependent.Id, toTaskId, visited))
                    return true;
            }
        }

        // Check join-table dependents
        var joinDependents = await _db.TaskDependencies
            .Where(td => td.DependsOnTaskId == fromTaskId)
            .Select(td => td.TaskId)
            .ToListAsync();

        foreach (var depId in joinDependents)
        {
            if (await HasCircularPath(depId, toTaskId, visited))
                return true;
        }

        return false;
    }

    private async Task BuildCircularPath(int fromTaskId, int toTaskId, List<string> path, HashSet<int> visited)
    {
        if (fromTaskId == toTaskId)
            return;

        var task = await _db.Tasks
            .Include(t => t.Dependents)
            .FirstOrDefaultAsync(t => t.Id == fromTaskId);

        if (task == null) return;

        foreach (var dependent in task.Dependents)
        {
            path.Add($"Task {dependent.Id}");
            if (dependent.Id == toTaskId)
                break;

            await BuildCircularPath(dependent.Id, toTaskId, path, visited);
        }

        // Also check join table for circular path
        if (!path.Any(p => p.Contains(toTaskId.ToString())))
        {
            var joinDeps = await _db.TaskDependencies
                .Where(td => td.DependsOnTaskId == fromTaskId)
                .Select(td => td.TaskId)
                .ToListAsync();

            foreach (var depId in joinDeps)
            {
                path.Add($"Task {depId}");
                if (depId == toTaskId)
                    break;
                await BuildCircularPath(depId, toTaskId, path, visited);
            }
        }
    }
}
