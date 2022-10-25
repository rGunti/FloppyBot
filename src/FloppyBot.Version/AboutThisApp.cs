using System.Reflection;

namespace FloppyBot.Version;

public class AboutThisApp
{
    static AboutThisApp()
    {
        var assembly = Assembly.GetEntryAssembly()!;
        Info = new AboutThisApp(
            "FloppyBot",
            assembly.GetProductName()!,
            assembly.GetProductVersion()!);
    }

    private AboutThisApp(
        string name,
        string serviceName,
        string version)
    {
        Name = name;
        ServiceName = serviceName;
        Version = version;
    }

    public static AboutThisApp Info { get; }

    public string Name { get; }
    public string ServiceName { get; }
    public string Version { get; }
}

internal static class AssemblyExtensions
{
    internal static string? GetProductName(this Assembly assembly)
        => assembly.GetAssemblyAttribute<AssemblyProductAttribute>(v => v.Product);

    internal static string? GetProductVersion(this Assembly assembly)
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
