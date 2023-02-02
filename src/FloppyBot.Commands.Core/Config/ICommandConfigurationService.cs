using FloppyBot.Base.Extensions;

namespace FloppyBot.Commands.Core.Config;

public interface ICommandConfigurationService
{
    NullableObject<CommandConfiguration> GetCommandConfiguration(string channelId, string commandName);
    IEnumerable<CommandConfiguration> GetCommandConfigurationsForChannel(string channelId);
    void SetCommandConfiguration(CommandConfiguration commandConfiguration);
}
