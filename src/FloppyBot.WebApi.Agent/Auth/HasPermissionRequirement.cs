using Microsoft.AspNetCore.Authorization;

namespace FloppyBot.WebApi.Agent.Auth;

public class HasPermissionRequirement : IAuthorizationRequirement
{
    public HasPermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
