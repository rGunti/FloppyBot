using System.Reflection;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Support.Hybrid;
using FloppyBot.Commands.Core.Support.PostExecution;
using FloppyBot.Commands.Core.Support.PreExecution;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Core.Support;

public static class SupportingTaskExtensions
{
    public static IServiceCollection AddPreExecutionTask<TTask>(this IServiceCollection services)
        where TTask : class, IPreExecutionTask
    {
        return services.AddScoped<IPreExecutionTask, TTask>();
    }

    public static IServiceCollection AddPostExecutionTask<TTask>(this IServiceCollection services)
        where TTask : class, IPostExecutionTask
    {
        return services.AddScoped<IPostExecutionTask, TTask>();
    }

    public static IServiceCollection AddHybridExecutionTask<TTask>(this IServiceCollection services)
        where TTask : class, IHybridTask
    {
        return services.AddPreExecutionTask<TTask>().AddPostExecutionTask<TTask>();
    }

    internal static IServiceCollection AddTasks(this IServiceCollection services)
    {
        return services
            .AddHybridExecutionTask<LogTask>()
            .AddPreExecutionTask<DisabledCommandTask>()
            .AddPreExecutionTask<GuardTask>()
            .AddHybridExecutionTask<CooldownTask>();
    }

    private static IOrderedEnumerable<T> GetSupportingTasks<T>(this IServiceProvider provider)
    {
        return provider
            .GetRequiredService<IEnumerable<T>>()
            .OrderBy(
                t => t!.GetType().GetCustomAttribute<TaskOrderAttribute>()?.Order ?? int.MaxValue
            )
            .ThenBy(t => t!.GetType().FullName);
    }

    internal static IPreExecutionTask? RunPreExecutionTasks(
        this IServiceScope scope,
        CommandInfo info,
        CommandInstruction instruction
    )
    {
        return scope.ServiceProvider
            .GetSupportingTasks<IPreExecutionTask>()
            .FirstOrDefault(t => !t.ExecutePre(info, instruction));
    }

    internal static IPostExecutionTask? RunPostExecutionTasks(
        this IServiceScope scope,
        CommandInfo info,
        CommandInstruction instruction,
        CommandResult result
    )
    {
        return scope.ServiceProvider
            .GetSupportingTasks<IPostExecutionTask>()
            .FirstOrDefault(t => !t.ExecutePost(info, instruction, result));
    }
}
