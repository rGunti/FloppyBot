namespace FloppyBot.Tools.V1Migrator.Config;

public class MigrationConfiguration
{
    public bool Simulate { get; set; } = true;
    public string[] LimitToChannels { get; set; } = Array.Empty<string>();
    public bool DropDestinationBeforeExecution { get; set; }
}

