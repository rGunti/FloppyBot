using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Base.Storage.Attributes;
using FloppyBot.Base.Storage.Utils;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Base.Storage.Indexing;

public class IndexInitializer
{
    private readonly IIndexManager _indexManager;
    private readonly ILogger<IndexInitializer> _logger;

    public IndexInitializer(ILogger<IndexInitializer> logger, IIndexManager indexManager)
    {
        _logger = logger;
        _indexManager = indexManager;
    }

    public void InitializeIndices()
    {
        if (!_indexManager.SupportsIndices)
        {
            _logger.LogInformation(
                "Your storage system does not support indices. Initialization is skipped"
            );
            return;
        }

        var interfaceType = typeof(IEntity);
        var entities = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t.GetInterfaces().Any(i => i == interfaceType)
                && t.GetCustomAttributes<IndexFieldsAttribute>().Any()
            )
            .ToImmutableArray();

        _logger.LogInformation("Found {CollectionCount} collection(s) to index", entities.Length);

        foreach (var type in entities)
        {
            var collectionName = RepositoryFactoryUtils.DetermineCollectionName(type);
            _logger.LogDebug(
                "Checking {TypeName} (Collection Name={CollectionName})",
                type,
                collectionName
            );

            foreach (var indexFieldsAttribute in type.GetCustomAttributes<IndexFieldsAttribute>())
            {
                if (
                    !_indexManager.IndexExists(
                        collectionName,
                        indexFieldsAttribute.IndexName,
                        indexFieldsAttribute.Fields
                    )
                )
                {
                    _logger.LogInformation(
                        "Creating index {CollectionName}.{IndexName}",
                        collectionName,
                        indexFieldsAttribute.IndexName
                    );
                    _indexManager.CreateIndex(
                        collectionName,
                        indexFieldsAttribute.IndexName,
                        indexFieldsAttribute.Fields
                    );
                }
            }
        }
    }
}
