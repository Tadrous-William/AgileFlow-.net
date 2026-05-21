using System.ComponentModel.DataAnnotations;
using AgileTaskManager.Models.Enums;

namespace AgileTaskManager.Models.ViewModels;

// ── Task ViewModels ─────────────────────────────────────────────────────────

public class TaskCreateViewModel : IValidatableObject
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? StartDate { get; set; }

    public DateTime? Deadline { get; set; }

    [Range(0.5, 999.9)]
    public double EstimatedHours { get; set; } = 2.0;

    public string? AssignedToId { get; set; }

    public int? DependsOnId { get; set; }

    public int? ProjectId { get; set; }

    public int? SprintId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && Deadline.HasValue && StartDate.Value > Deadline.Value)
        {
            yield return new ValidationResult("Start date/time must not be after end date/time.", new[] { nameof(StartDate), nameof(Deadline) });
        }
    }
}

public class TaskEditViewModel : TaskCreateViewModel
{
    public int Id { get; set; }
    public AgileTaskManager.Models.Enums.TaskStatus Status { get; set; }
}

public class TaskListViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public AgileTaskManager.Models.Enums.TaskStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? Deadline { get; set; }
    public string? AssignedToName { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsOverdue => Deadline.HasValue && Deadline.Value < DateTime.UtcNow
                            && Status != AgileTaskManager.Models.Enums.TaskStatus.Done;
}

public class TaskDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public AgileTaskManager.Models.Enums.TaskStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public int? DependsOnId { get; set; }
    public string? DependsOnTitle { get; set; }
    public bool IsBlocked { get; set; }
    public List<CommentViewModel> Comments { get; set; } = new();
    public List<AttachmentViewModel> Attachments { get; set; } = new();
    public List<ActivityLogViewModel> ActivityLogs { get; set; } = new();
    public FeedbackViewModel? Feedback { get; set; }
}

// ── Comment ViewModels ──────────────────────────────────────────────────────

public class CommentViewModel
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AddCommentViewModel
{
    public int TaskId { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

// ── Feedback ViewModels ─────────────────────────────────────────────────────

public class FeedbackViewModel
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
}

public class AddFeedbackViewModel
{
    public int TaskId { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}

// ── Dashboard ViewModels ────────────────────────────────────────────────────

public class DashboardViewModel
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int UnreadNotifications { get; set; }
    public List<TaskListViewModel> RecentTasks { get; set; } = new();
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public List<ActivityLogViewModel> RecentActivity { get; set; } = new();
}

// ── Notification ViewModels ─────────────────────────────────────────────────

public class NotificationViewModel
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string? Link { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ── Activity Log ViewModels ─────────────────────────────────────────────────

public class ActivityLogViewModel
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ActorName { get; set; }
    public string? EntityType { get; set; }
    public string? Description { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

// ── Attachment ViewModels ───────────────────────────────────────────────────

public class AttachmentViewModel
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}

// ── Account ViewModels ──────────────────────────────────────────────────────

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Compare("Password"), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Developer";

}

// ── User Management ─────────────────────────────────────────────────────────

public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int AssignedTasksCount { get; set; }
}

public class AssignRoleViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
}

public class ProjectCreateViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
