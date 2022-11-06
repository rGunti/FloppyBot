﻿using FloppyBot.Base.Storage.MongoDb.Indexing;
using FloppyBot.Base.Storage.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace FloppyBot.Base.Storage.MongoDb;

public static class Registration
{
    private static MongoUrl GetMongoUrl(this IServiceProvider provider) =>
        provider.GetRequiredService<MongoUrl>();

    private static IMongoClient GetMongoClient(this IServiceProvider provider) =>
        provider.GetRequiredService<IMongoClient>();

    public static IServiceCollection AddMongoDbStorage(
        this IServiceCollection services,
        string connectionStringName = "MongoDb")
    {
        BsonSerializer.RegisterIdGenerator(typeof(string), StringObjectIdGenerator.Instance);
        return services
            .AddSingleton<MongoUrl>(s => MongoUrl.Create(s.GetRequiredService<IConfiguration>()
                .GetConnectionString(connectionStringName)))
            .AddSingleton<IMongoClient>(s => new MongoClient(s.GetMongoUrl()))
            .AddSingleton<IMongoDatabase>(s => s.GetMongoClient().GetDatabase(s.GetMongoUrl().DatabaseName))
            .AddStorageImplementation<MongoDbRepositoryFactory, MongoDbIndexManager>();
    }
}