using AgileTaskManager.Models.Enums;
using TaskStatus = AgileTaskManager.Models.Enums.TaskStatus;

namespace AgileTaskManager.Models.ViewModels;

public class TaskDependencyViewModel
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public TaskStatus TaskStatus { get; set; }
    public TaskPriority Priority { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? Deadline { get; set; }
    public bool IsCompleted => TaskStatus == TaskStatus.Done;
    public bool IsBlocked => !IsCompleted;
    public bool IsOverdue => Deadline.HasValue && Deadline.Value < DateTime.UtcNow && !IsCompleted;
}

public class DependencyGraphViewModel
{
    public int RootTaskId { get; set; }
    public string RootTaskTitle { get; set; } = string.Empty;
    public TaskStatus RootTaskStatus { get; set; }
    public TaskPriority RootTaskPriority { get; set; }
    public List<TaskDependencyViewModel> Dependencies { get; set; } = new();
    public List<TaskDependencyViewModel> Dependents { get; set; } = new();
    public bool CanStart { get; set; }
    public bool IsBlockingDependents { get; set; }
    public int BlockedCount => Dependencies.Count(d => d.IsBlocked);
    public int DependentCount => Dependents.Count;
}

public class CreateDependencyViewModel
{
    public int TaskId { get; set; }
    public int DependsOnTaskId { get; set; }
}

public class RemoveDependencyViewModel
{
    public int TaskId { get; set; }
    public int DependsOnTaskId { get; set; }
}

public class BulkDependencyUpdateViewModel
{
    public int TaskId { get; set; }
    public List<int> DependsOnTaskIds { get; set; } = new();
}

public class DependencyValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> CircularDependencyPath { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
}

public class DependencyViewModel
{
    public int TaskId { get; set; }
    public int DependsOnTaskId { get; set; }
}
