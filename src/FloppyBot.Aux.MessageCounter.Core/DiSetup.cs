using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Aux.MessageCounter.Core;

public static class DiSetup
{
    public static IServiceCollection AddMessageCounter(this IServiceCollection services)
    {
        return services
            .AddSingleton<MessageCounter>();
    }

    public static IServiceCollection AddMessageOccurrenceService(this IServiceCollection services)
    {
        return services
            .AddTransient<IMessageOccurrenceService, MessageOccurrenceService>();
    }
}


