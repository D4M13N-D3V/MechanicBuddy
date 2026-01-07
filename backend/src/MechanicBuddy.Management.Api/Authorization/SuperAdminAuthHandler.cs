using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MechanicBuddy.Management.Api.Authorization;

public class SuperAdminRequirement : IAuthorizationRequirement
{
}

public class SuperAdminAuthHandler : AuthorizationHandler<SuperAdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SuperAdminRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim != null && roleClaim.Value == "owner")
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
