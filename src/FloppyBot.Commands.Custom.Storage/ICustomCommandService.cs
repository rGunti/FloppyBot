using AutoMapper;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;

namespace FloppyBot.Commands.Custom.Storage;

public interface ICustomCommandService
{
    CustomCommandDescription? GetCommand(string channelId, string commandName);
    IEnumerable<CustomCommandDescription> GetCommandsOfChannel(string channelId);
    bool CreateSimpleCommand(string channelId, string commandName, string response);
    bool CreateCommand(CustomCommandDescription commandDescription);
    bool DeleteCommand(string channelId, string commandName);
    void UpdateCommand(CustomCommandDescription commandDescription);
}

public class CustomCommandService : ICustomCommandService
{
    private readonly IMapper _mapper;
    private readonly IRepository<CustomCommandDescriptionEo> _repository;

    public CustomCommandService(IRepositoryFactory repositoryFactory, IMapper mapper)
    {
        _repository = repositoryFactory.GetRepository<CustomCommandDescriptionEo>();
        _mapper = mapper;
    }

    public CustomCommandDescription? GetCommand(string channelId, string commandName)
    {
        return GetCommandEo(channelId, commandName)
            .Select(eo => _mapper.Map<CustomCommandDescription>(eo))
            .FirstOrDefault();
    }

    public IEnumerable<CustomCommandDescription> GetCommandsOfChannel(string channelId)
    {
        return _repository
            .GetAll()
            .Where(c => c.Owners.Contains(channelId))
            .Select(eo => _mapper.Map<CustomCommandDescription>(eo));
    }

    public bool CreateSimpleCommand(string channelId, string commandName, string response)
    {
        if (GetCommand(channelId, commandName) != null)
        {
            return false;
        }

        var command = new CustomCommandDescriptionEo
        {
            Name = commandName,
            Aliases = Array.Empty<string>(),
            Limitations = new CommandLimitationEo
            {
                MinLevel = PrivilegeLevel.Unknown,
                Cooldown = Array.Empty<CooldownDescriptionEo>(),
            },
            Owners = new[] { channelId },
            Responses = new[]
            {
                new CommandResponseEo { Type = $"{ResponseType.Text}", Content = response },
            },
            ResponseMode = CommandResponseMode.First,
        };
        _repository.Insert(command);
        return true;
    }

    public bool CreateCommand(CustomCommandDescription commandDescription)
    {
        if (
            commandDescription
                .Owners
                .Select(
                    channel =>
                        ExistsAnyWithName(
                            channel,
                            commandDescription.Name,
                            commandDescription.Aliases.ToArray()
                        )
                )
                .Any(exists => exists)
        )
        {
            return false;
        }

        _repository.Insert(_mapper.Map<CustomCommandDescriptionEo>(commandDescription));
        return true;
    }

    public bool DeleteCommand(string channelId, string commandName)
    {
        NullableObject<CustomCommandDescriptionEo> command = GetCommandEo(channelId, commandName);
        if (!command.HasValue)
        {
            return false;
        }

        _repository.Delete(command.Value);
        return true;
    }

    public void UpdateCommand(CustomCommandDescription commandDescription)
    {
        _repository.Update(_mapper.Map<CustomCommandDescriptionEo>(commandDescription));
    }

    private NullableObject<CustomCommandDescriptionEo> GetCommandEo(
        string channelId,
        string commandName
    )
    {
        return _repository
            .GetAll()
            .Where(c => c.Owners.Contains(channelId))
            .FirstOrDefault(c => c.Name == commandName || c.Aliases.Contains(commandName))
            .Wrap();
    }

    private bool ExistsAnyWithName(string channelId, string commandName, params string[] aliases)
    {
        return aliases
            .Concat(new[] { commandName })
            .Distinct()
            .Select(name => GetCommandEo(channelId, name))
            .Any(cmd => cmd.HasValue);
    }
}
