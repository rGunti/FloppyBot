using FloppyBot.Chat.Mock;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Chat.Config;

public static class Registration
{
    public static IServiceCollection AddMockChatInterface(this IServiceCollection services)
    {
        return services
            .AddSingleton<IChatInterface, MockChatInterface>(_ => new MockChatInterface());
    }
}
