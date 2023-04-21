namespace FloppyBot.Commands.Core.Attributes.Guards;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public abstract class GuardAttribute : Attribute { }
