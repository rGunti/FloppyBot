using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.FileStorage;

public static class Extensions
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services)
    {
        return services.AddScoped<IFileService, FileService>();
    }
}
