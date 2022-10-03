using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Communication.Redis.Config;

public static class Registration
{
    public static IServiceCollection AddRedisCommunication(this IServiceCollection services)
    {
        return services
            .AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>()
            .AddSingleton<INotificationSenderFactory, RedisNotificationInterfaceFactory>()
            .AddSingleton<INotificationReceiverFactory, RedisNotificationInterfaceFactory>();
    }
}
