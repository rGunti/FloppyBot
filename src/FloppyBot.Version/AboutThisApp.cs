using System.Reflection;

namespace FloppyBot.Version;

public static class AboutThisApp
{
    static AboutThisApp()
    {
        var assembly = Assembly.GetEntryAssembly()!;
        Version = assembly.GetProductVersion()!;
        ServiceName = assembly.GetProductName()!;
    }

    public static string Name => "FloppyBot";
    public static string ServiceName { get; }
    public static string Version { get; }

    private static string? GetProductName(this Assembly assembly)
        => assembly.GetAssemblyAttribute<AssemblyProductAttribute>(v => v.Product);

    private static string? GetProductVersion(this Assembly assembly)
        => assembly.GetAssemblyAttribute<AssemblyInformationalVersionAttribute>(v => v.InformationalVersion);

    private static string? GetAssemblyAttribute<T>(
        this Assembly assembly,
        Func<T, string> valueSelectorFn) where T : Attribute
    {
        return assembly
            .GetCustomAttributes<T>()
            .Select(valueSelectorFn)
            .FirstOrDefault();
    }
}
