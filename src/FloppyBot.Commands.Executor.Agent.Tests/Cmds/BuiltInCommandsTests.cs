namespace FloppyBot.Commands.Executor.Agent.Tests.Cmds;

[TestClass]
public class BuiltInCommandsTests
{
    [TestMethod]
    public void Ping()
    {
        Assert.AreEqual(BuiltInCommands.REPLY_PING, BuiltInCommands.Ping());
    }
}
