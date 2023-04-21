using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Support.PreExecution;

/// <summary>
/// A task that is being executed before each command
/// </summary>
public interface IPreExecutionTask
{
    /// <summary>
    /// Method that is being executed before a command.
    /// To prevent a command from being executed, return false;
    /// </summary>
    /// <param name="info"></param>
    /// <param name="instruction"></param>
    /// <returns></returns>
    bool ExecutePre(CommandInfo info, CommandInstruction instruction);
}
