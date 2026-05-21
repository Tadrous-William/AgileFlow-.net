using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class ReportPermission : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string GrantedBy { get; set; } = string.Empty;
}
