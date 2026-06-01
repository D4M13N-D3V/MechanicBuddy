using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MechanicBuddy.Management.Api.Authorization;

public class SuperAdminRequirement : IAuthorizationRequirement
{
}

public class SuperAdminAuthHandler : AuthorizationHandler<SuperAdminRequirement>
{
    // Platform super-admin roles ONLY. "owner" (tenant owners created via the
    // public, unauthenticated /api/signup endpoint) MUST NOT appear here: doing
    // so let anyone self-register and obtain full control-plane access
    // (provision/deprovision/delete tenants, run migrations, impersonate tenants).
    // Tenant owners are authorized only for owner-facing [Authorize] endpoints.
    private static readonly string[] AllowedRoles = { "admin", "support" };

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperAdminRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim != null && AllowedRoles.Contains(roleClaim.Value))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class ActiveAdminRequirement : IAuthorizationRequirement
{
}

public class ActiveAdminAuthHandler : AuthorizationHandler<ActiveAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ActiveAdminRequirement requirement)
    {
        var isActiveClaim = context.User.FindFirst("is_active");
        if (isActiveClaim != null && bool.TryParse(isActiveClaim.Value, out var isActive) && isActive)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
