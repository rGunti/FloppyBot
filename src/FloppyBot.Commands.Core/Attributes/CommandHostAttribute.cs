namespace FloppyBot.Commands.Core.Attributes;

/// <summary>
/// This attribute denotes classes that host command handlers.
/// To implement command handlers, you will have to add this class
/// to your host class.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CommandHostAttribute : Attribute { }
