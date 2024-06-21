using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Base.Auditing.Abstraction;

public static class Registration
{
    public static IServiceCollection AddAuditor<T>(this IServiceCollection services)
        where T : class, IAuditor
    {
        return services.AddScoped<IAuditor, T>();
    }
}
