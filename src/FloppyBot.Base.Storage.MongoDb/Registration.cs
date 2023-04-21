using FloppyBot.Base.Storage.MongoDb.Indexing;
using FloppyBot.Base.Storage.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace FloppyBot.Base.Storage.MongoDb;

public static class Registration
{
    public static IServiceCollection AddMongoDbStorage(
        this IServiceCollection services,
        string connectionStringName = "MongoDb"
    )
    {
        BsonSerializer.RegisterIdGenerator(typeof(string), StringObjectIdGenerator.Instance);
        ConventionRegistry.Register(
            "Enum2String",
            new ConventionPack { new EnumRepresentationConvention(BsonType.String) },
            _ => true
        );
        return services
            .AddSingleton<MongoUrl>(
                s =>
                    MongoUrl.Create(
                        s.GetRequiredService<IConfiguration>()
                            .GetConnectionString(connectionStringName)
                    )
            )
            .AddSingleton<IMongoClient>(s => new MongoClient(s.GetMongoUrl()))
            .AddSingleton<IMongoDatabase>(
                s => s.GetMongoClient().GetDatabase(s.GetMongoUrl().DatabaseName)
            )
            .AddStorageImplementation<MongoDbRepositoryFactory, MongoDbIndexManager>();
    }

    private static MongoUrl GetMongoUrl(this IServiceProvider provider) =>
        provider.GetRequiredService<MongoUrl>();

    private static IMongoClient GetMongoClient(this IServiceProvider provider) =>
        provider.GetRequiredService<IMongoClient>();
}
