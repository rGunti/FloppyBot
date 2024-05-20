using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace FloppyBot.WebApi.Auth;

public static class Permissions
{
    public const string READ_COMMANDS = "read:commands";
    public const string READ_BOT = "read:bot";
    public const string RESTART_BOT = "restart:bot";

    public const string READ_QUOTES = "read:quotes";
    public const string EDIT_QUOTES = "edit:quotes";

    public const string READ_CUSTOM_COMMANDS = "read:custom-commands";
    public const string EDIT_CUSTOM_COMMANDS = "edit:custom-commands";

    public const string READ_CONFIG = "read:config";
    public const string EDIT_CONFIG = "edit:config";

    public const string READ_FILES = "read:files";
    public const string EDIT_FILES = "edit:files";

    public const string READ_LOGS = "read:logs";

    public static readonly string[] AllPermissions;

    static Permissions()
    {
        AllPermissions = typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => f.GetRawConstantValue() as string)
            .ToArray()!;
    }

    public static AuthorizationOptions AddPermissionAsPolicy(
        this AuthorizationOptions opts,
        string permission
    )
    {
        opts.AddPolicy(
            permission,
            p => p.Requirements.Add(new HasPermissionRequirement(permission))
        );
        return opts;
    }
}
