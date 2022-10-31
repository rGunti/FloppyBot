using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Guard;

public class CommandGuardRegistry : ICommandGuardRegistry
{
    private readonly Dictionary<Type, HashSet<Type>> _guardTypeDictionary = new();
    private readonly ILogger<CommandGuardRegistry> _logger;

    public CommandGuardRegistry(ILogger<CommandGuardRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterGuard(Type attributeType, Type guardImplType)
    {
        _logger.LogDebug(
            "Registering {GuardImplType} for attribute {GuardAttributeType}",
            guardImplType,
            attributeType);
        if (!_guardTypeDictionary.ContainsKey(attributeType))
        {
            _guardTypeDictionary[attributeType] = new HashSet<Type>();
        }

        _guardTypeDictionary[attributeType].Add(guardImplType);
    }

    public void RegisterGuard<TGuard, TGuardAttribute>()
        where TGuard : ICommandGuard<TGuardAttribute>
        where TGuardAttribute : GuardAttribute
    {
        RegisterGuard(typeof(TGuard), typeof(TGuardAttribute));
    }

    public IEnumerable<Type> FindGuardImplementation(GuardAttribute guardAttribute)
    {
        var guardAttributeType = guardAttribute.GetType();
        return _guardTypeDictionary.GetValueOrDefault(guardAttributeType)
               ?? throw new MissingGuardException(guardAttributeType);
    }
}
