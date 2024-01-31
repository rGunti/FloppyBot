using System.Collections.Immutable;
using System.Reflection;

namespace FloppyBot.Base.BinLoader;

public static class AssemblyPreloader
{
    public static void LoadAssembliesFromDirectory(string? directory = null)
    {
        var loadedAssemblies = AppDomain
            .CurrentDomain.GetAssemblies()
            .ToImmutableDictionary(a => a.Location, a => a);

        foreach (
            var assemblyPath in Directory
                .GetFiles(directory ?? AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Where(f => !loadedAssemblies.ContainsKey(f))
        )
        {
            Assembly.LoadFile(assemblyPath);
        }
    }
}
