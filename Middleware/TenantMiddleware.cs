using System.Security.Claims;
using AgileTaskManager.Services.Interfaces;

namespace AgileTaskManager.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirst("TenantId");
            if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var tenantId))
            {
                tenantContext.SetTenant(tenantId);
                await _next(context);
                return;
            }
        }

        tenantContext.SetTenant(1);
        await _next(context);
    }
}
