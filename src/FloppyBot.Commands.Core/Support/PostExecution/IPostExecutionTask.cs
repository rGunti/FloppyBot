using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Support.PostExecution;

/// <summary>
/// A task that is being executed after each command
/// </summary>
public interface IPostExecutionTask
{
    /// <summary>
    /// Method that is being executed after a command.
    /// To prevent the submission of a reply, return false.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="instruction"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    bool ExecutePost(
        CommandInfo info,
        CommandInstruction instruction,
        CommandResult result);
}
