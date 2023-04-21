using System.Reflection;
using FloppyBot.Base.Storage.Attributes;
using FloppyBot.Base.Storage.Indexing;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Base.Storage.Utils;

public static class RepositoryFactoryUtils
{
    public static string DetermineCollectionName<T>()
        where T : class, IEntity<T> => DetermineCollectionName(typeof(T));

    public static string DetermineCollectionName(MemberInfo type)
    {
        return type.GetCustomAttributes<CollectionNameAttribute>()
                .Select(a => a.Name)
                .FirstOrDefault() ?? type.Name;
    }

    public static IServiceCollection AddStorageImplementation<TRepositoryFactory, TIndexManager>(
        this IServiceCollection services
    )
        where TRepositoryFactory : class, IRepositoryFactory
        where TIndexManager : class, IIndexManager
    {
        return services
            .AddSingleton<IRepositoryFactory, TRepositoryFactory>()
            .AddSingleton<IIndexManager, TIndexManager>()
            .AddSingleton<IndexInitializer>();
    }
}
