using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class ReportTemplate : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string? Parameters { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string DefaultFormat { get; set; } = "PDF";
    public string ScheduleFrequency { get; set; } = string.Empty;
    public string? Recipients { get; set; }
}
