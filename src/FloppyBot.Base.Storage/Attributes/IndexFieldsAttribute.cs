namespace FloppyBot.Base.Storage.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class IndexFieldsAttribute : Attribute
{
    public IndexFieldsAttribute(string indexName, params string[] fields)
    {
        IndexName = indexName;
        Fields = fields;
    }

    public string IndexName { get; }
    public string[] Fields { get; }
}
