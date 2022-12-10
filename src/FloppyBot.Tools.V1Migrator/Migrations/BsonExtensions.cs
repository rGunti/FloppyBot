using MongoDB.Bson;

namespace FloppyBot.Tools.V1Migrator.Migrations;

public static class BsonExtensions
{
    public static BsonValue? GetValueIfExists(this BsonDocument doc, string name)
    {
        return doc.TryGetValue(name, out BsonValue? value) ? value : null;
    }
}

