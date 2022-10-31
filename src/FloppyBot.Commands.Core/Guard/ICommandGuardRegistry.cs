using FloppyBot.Commands.Core.Attributes.Guards;

namespace FloppyBot.Commands.Core.Guard;

public interface ICommandGuardRegistry
{
    void RegisterGuard(
        Type attributeType,
        Type guardImplType);

    void RegisterGuard<TGuard, TGuardAttribute>()
        where TGuard : ICommandGuard<TGuardAttribute>
        where TGuardAttribute : GuardAttribute;

    IEnumerable<Type> FindGuardImplementation(GuardAttribute guardAttribute);
}
