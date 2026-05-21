using AgileTaskManager.Models.Enums;
using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class TaskItem : ITenantEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Models.Enums.TaskStatus Status { get; set; } = Models.Enums.TaskStatus.ToDo;
    public DateTime? StartDate { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Self-referencing dependency (single primary dependency)
    public int? DependsOnId { get; set; }
    public TaskItem? DependsOn { get; set; }
    public ICollection<TaskItem> Dependents { get; set; } = new List<TaskItem>();

    // Many-to-many dependencies via join table
    public ICollection<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>();
    public ICollection<TaskDependency> DependentOn { get; set; } = new List<TaskDependency>();

    // Assigned user FK
    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? SprintId { get; set; }
    public Sprint? Sprint { get; set; }

    // Navigation collections
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public Feedback? Feedback { get; set; }
}

public class TaskDependency : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    public int DependsOnTaskId { get; set; }
    public TaskItem DependsOnTask { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
