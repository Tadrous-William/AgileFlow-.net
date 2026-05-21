using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class ScheduledReport : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int TemplateId { get; set; }
    public ReportTemplate Template { get; set; } = null!;
    public string ScheduleFrequency { get; set; } = string.Empty;
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Recipients { get; set; }
    public string? Parameters { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
