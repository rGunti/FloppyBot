using System.Reflection;
using FloppyBot.Base.Storage.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Base.Storage.Utils;

public static class RepositoryFactoryUtils
{
    public static string DetermineCollectionName<T>() where T : class, IEntity<T>
        => DetermineCollectionName(typeof(T));

    private static string DetermineCollectionName(MemberInfo type)
    {
        return type.GetCustomAttributes<CollectionNameAttribute>()
            .Select(a => a.Name)
            .FirstOrDefault() ?? type.Name;
    }

    public static IServiceCollection AddStorageImplementation<TRepositoryFactory>(
        this IServiceCollection services)
        where TRepositoryFactory : class, IRepositoryFactory
    {
        return services
            .AddSingleton<IRepositoryFactory, TRepositoryFactory>();
    }
}
