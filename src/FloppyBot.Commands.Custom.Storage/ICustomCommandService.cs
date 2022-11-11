using AutoMapper;
using FloppyBot.Base.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;

namespace FloppyBot.Commands.Custom.Storage;

public interface ICustomCommandService
{
    CustomCommandDescription? GetCommand(string channelId, string commandName);
    IEnumerable<CustomCommandDescription> GetCommandsOfChannel(string channelId);
}

public class CustomCommandService : ICustomCommandService
{
    private readonly IMapper _mapper;
    private readonly IRepository<CustomCommandDescriptionEo> _repository;

    public CustomCommandService(
        IRepositoryFactory repositoryFactory,
        IMapper mapper)
    {
        _repository = repositoryFactory.GetRepository<CustomCommandDescriptionEo>();
        _mapper = mapper;
    }

    public CustomCommandDescription? GetCommand(string channelId, string commandName)
    {
        return _repository.GetAll()
            .Where(c => c.Owners.Contains(channelId))
            .Where(c => c.Name == commandName || c.Aliases.Contains(commandName))
            .Select(eo => _mapper.Map<CustomCommandDescription>(eo))
            .FirstOrDefault();
    }

    public IEnumerable<CustomCommandDescription> GetCommandsOfChannel(string channelId)
    {
        return _repository.GetAll()
            .Where(c => c.Owners.Contains(channelId))
            .Select(eo => _mapper.Map<CustomCommandDescription>(eo));
    }
}
