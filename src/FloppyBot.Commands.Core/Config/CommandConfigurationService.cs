using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;

namespace FloppyBot.Commands.Core.Config;

public class CommandConfigurationService : ICommandConfigurationService
{
    private readonly IRepository<CommandConfiguration> _repository;

    public CommandConfigurationService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<CommandConfiguration>();
    }

    public NullableObject<CommandConfiguration> GetCommandConfiguration(
        string channelId,
        string commandName
    )
    {
        return _repository
            .GetAll()
            .FirstOrDefault(c => c.ChannelId == channelId && c.CommandName == commandName)
            .Wrap();
    }

    public IEnumerable<CommandConfiguration> GetCommandConfigurationsForChannel(string channelId)
    {
        return _repository.GetAll().Where(c => c.ChannelId == channelId);
    }

    public void SetCommandConfiguration(CommandConfiguration commandConfiguration)
    {
        var id = GenerateCommandConfigurationId(
            commandConfiguration.ChannelId,
            commandConfiguration.CommandName
        );
        if (_repository.GetById(id) is null)
        {
            _repository.Insert(commandConfiguration.WithId(id));
        }
        else
        {
            _repository.Update(commandConfiguration.WithId(id));
        }
    }

    public void DeleteCommandConfiguration(string channelId, string commandName)
    {
        _repository.Delete(GenerateCommandConfigurationId(channelId, commandName));
    }

    private static string GenerateCommandConfigurationId(string channelId, string commandName)
    {
        return $"{channelId},{commandName}";
    }
}
