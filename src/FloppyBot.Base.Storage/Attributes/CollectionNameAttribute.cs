namespace FloppyBot.Base.Storage.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class CollectionNameAttribute : Attribute
{
    public CollectionNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
