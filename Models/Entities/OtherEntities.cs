using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class Project : ITenantEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public decimal TotalCost { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
}

public class ProjectMember : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public AgileTaskManager.Models.Enums.ProjectMemberRole Role { get; set; } =
        AgileTaskManager.Models.Enums.ProjectMemberRole.Developer;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class Sprint : ITenantEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public AgileTaskManager.Models.Enums.SprintStatus Status { get; set; } =
        AgileTaskManager.Models.Enums.SprintStatus.Planned;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
}

public class Comment : ITenantEntity
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}

public class Feedback : ITenantEntity
{
    public int Id { get; set; }
    public int Rating { get; set; }          // 1–5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public string ClientId { get; set; } = string.Empty;
    public ApplicationUser Client { get; set; } = null!;
}

public class Attachment : ITenantEntity
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;   // URL or local path
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    
    // Additional properties for Phase 5.2
    public string? UploadedByUserId { get; set; }
    public ApplicationUser? UploadedByUser { get; set; }
    public string? StoredFileName { get; set; }
}

public class Notification : ITenantEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public string? Link { get; set; }        // Optional deep-link URL
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Notification type for Phase 5.1
    public string Type { get; set; } = "TaskAssigned";

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}

public class ActivityLog : ITenantEntity
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;    // e.g. "StatusChanged"
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Multi-tenancy
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Audit trail properties for Phase 5.4
    public string EntityName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }

    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public string ActorId { get; set; } = string.Empty;
    public ApplicationUser Actor { get; set; } = null!;
}
