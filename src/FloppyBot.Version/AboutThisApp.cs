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
            assembly.GetProductVersion()!,
            Guid.NewGuid().ToString()
        );
    }

    private AboutThisApp(string name, string serviceName, string version, string instanceId)
    {
        Name = name;
        ServiceName = serviceName;
        Version = version;
        InstanceId = instanceId;
    }

    public static AboutThisApp Info { get; }

    /// <summary>
    /// Name of the application
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Name of the service
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Version of the service
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// An ID identifying this exact instance
    /// which is randomly generated on startup
    /// </summary>
    public string InstanceId { get; }
}

internal static class AssemblyExtensions
{
    internal static string? GetProductName(this Assembly assembly) =>
        assembly.GetAssemblyAttribute<AssemblyProductAttribute>(v => v.Product);

    internal static string? GetProductVersion(this Assembly assembly) =>
        assembly.GetAssemblyAttribute<AssemblyInformationalVersionAttribute>(v =>
            v.InformationalVersion
        );

    private static string? GetAssemblyAttribute<T>(
        this Assembly assembly,
        Func<T, string> valueSelectorFn
    )
        where T : Attribute
    {
        return assembly.GetCustomAttributes<T>().Select(valueSelectorFn).FirstOrDefault();
    }
}
