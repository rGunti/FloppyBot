using Microsoft.AspNetCore.Authorization;

namespace FloppyBot.WebApi.Auth;

public class HasPermissionRequirement : IAuthorizationRequirement
{
    public HasPermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
