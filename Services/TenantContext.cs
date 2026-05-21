using AgileTaskManager.Services.Interfaces;

namespace AgileTaskManager.Services;

public class TenantContext : ITenantContext
{
    private int _tenantId;

    public int TenantId => _tenantId;
    public bool IsTenantSet => _tenantId != 0;

    public void SetTenant(int tenantId)
    {
        _tenantId = tenantId;
    }
}
