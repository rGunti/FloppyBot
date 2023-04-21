using FloppyBot.WebApi.Auth.UserProfiles;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.WebApi.Auth;

public static class Dependencies
{
    public static IServiceCollection AddAuthDependencies(this IServiceCollection services)
    {
        return services.AddScoped<IUserService, UserService>();
    }
}
