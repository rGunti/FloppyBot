using FloppyBot.Commands.Core.Support.PostExecution;
using FloppyBot.Commands.Core.Support.PreExecution;

namespace FloppyBot.Commands.Core.Support.Hybrid;

public interface IHybridTask : IPreExecutionTask, IPostExecutionTask
{
}
