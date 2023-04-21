using Microsoft.AspNetCore.Authorization;

namespace FloppyBot.WebApi.Auth;

public class HasPermissionHandler : AuthorizationHandler<HasPermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasPermissionRequirement requirement
    )
    {
        if (!context.User.HasClaim(c => c.Type == "permissions"))
        {
            return Task.CompletedTask;
        }

        string[] requirements =
            context.User.Claims.Where(c => c.Type == "permissions")?.Select(c => c.Value).ToArray()
            ?? Array.Empty<string>();

        if (requirements.Any(s => s == requirement.Permission))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(
                new AuthorizationFailureReason(this, $"Missing permission {requirement.Permission}")
            );
        }

        return Task.CompletedTask;
    }
}
