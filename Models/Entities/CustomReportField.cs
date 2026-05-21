using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class CustomReportField : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Required { get; set; } = false;
    public string? DefaultValue { get; set; }
    public string? Options { get; set; }
    public int Order { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string ReportType { get; set; } = string.Empty;
}
