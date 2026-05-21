using AgileTaskManager.Models.Interfaces;

namespace AgileTaskManager.Models.Entities;

public class UserSecurityLog : ITenantEntity
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? AdditionalData { get; set; }
}
