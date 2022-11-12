namespace FloppyBot.Commands.Core.Support;

[AttributeUsage(AttributeTargets.Class)]
public class TaskOrderAttribute : Attribute
{
    public TaskOrderAttribute(int order)
    {
        Order = order;
    }

    public int Order { get; }
}
