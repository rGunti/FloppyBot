namespace FloppyBot.IntTest.Fixtures;

[CollectionDefinition(NAME)]
public class FloppyBotTestCollection : ICollectionFixture<FloppyBotTestContainerFixture>
{
    public const string NAME = "FloppyBot";
}
