using System.Collections.Immutable;
using FloppyBot.Commands.Registry.Entities;

namespace FloppyBot.Commands.Registry;

public interface IDistributedCommandRegistry
{
    IImmutableList<string> GetCommands();
    IImmutableList<CommandAbstract> GetAllCommands();
    CommandAbstract? GetCommand(string commandName);
    void StoreCommand(string commandName, CommandAbstract commandAbstract);
    void RemoveCommand(string commandName);
}
