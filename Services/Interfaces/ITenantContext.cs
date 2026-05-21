namespace AgileTaskManager.Services.Interfaces;

public interface ITenantContext
{
    int TenantId { get; }
    bool IsTenantSet { get; }
    void SetTenant(int tenantId);
}
