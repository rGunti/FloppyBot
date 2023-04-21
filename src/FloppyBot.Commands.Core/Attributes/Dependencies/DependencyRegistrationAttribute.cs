namespace FloppyBot.Commands.Core.Attributes.Dependencies;

/// <summary>
/// This attribute denotes methods that registers dependencies
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class DependencyRegistrationAttribute : Attribute { }
