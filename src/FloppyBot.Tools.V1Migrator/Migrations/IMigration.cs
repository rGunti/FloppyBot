namespace FloppyBot.Tools.V1Migrator.Migrations;

public interface IMigration
{
    uint Order { get; }
    bool CanExecute();
    void Execute();
}

