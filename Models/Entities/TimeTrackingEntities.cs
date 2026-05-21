using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class TimeEntry : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
    public string? Description { get; set; }
    public bool IsActive => !EndTime.HasValue;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Time tracking metadata
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class TimeSheet : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan TotalTime { get; set; }
    public TimeSpan BillableTime { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalAmount => (decimal)BillableTime.TotalHours * HourlyRate;
    public string? Notes { get; set; }
    public TimeSheetStatus Status { get; set; } = TimeSheetStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    
    // Navigation collections
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}

public enum TimeSheetStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected
}

public class TimeTrackingSettings : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public bool EnableTimeTracking { get; set; } = true;
    public bool RequireDescription { get; set; } = false;
    public bool AllowManualTimeEntry { get; set; } = true;
    public int MinimumEntryMinutes { get; set; } = 1;
    public int MaximumHoursPerDay { get; set; } = 24;
    public decimal DefaultHourlyRate { get; set; } = 0;
    public string TimeZone { get; set; } = "UTC";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class TimeReport : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string GeneratedBy { get; set; } = string.Empty;
    public ApplicationUser GeneratedByUser { get; set; } = null!;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ReportType Type { get; set; }
    public ReportScope Scope { get; set; }
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Report data (JSON)
    public string ReportData { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public TimeSpan TotalHours { get; set; }
    public decimal TotalAmount { get; set; }
    
    public bool IsPublic { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }
}

public enum ReportType
{
    Daily,
    Weekly,
    Monthly,
    Custom
}

public enum ReportScope
{
    User,
    Project,
    Team,
    All
}
