using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities.Utils;

namespace FloppyBot.Commands.Playground;

[TestClass]
public class Main
{
    [DataTestMethod]
    [DataRow(PrivilegeLevel.Unknown, false)]
    [DataRow(PrivilegeLevel.Viewer, false)]
    [DataRow(PrivilegeLevel.Moderator, true)]
    [DataRow(PrivilegeLevel.Administrator, true)]
    [DataRow(PrivilegeLevel.Superuser, true)]
    public void Say(PrivilegeLevel privilegeLevel, bool expectMessage)
    {
        var instructions = MockCommandFactory.NewInstruction(
            "say",
            new[] { "Hello", "World" },
            privilegeLevel);
        var executor = CommandExecutor.Create();
        var response = executor.TryExecute(instructions);

        Assert.AreEqual(expectMessage, response != null);
        if (expectMessage)
        {
            Assert.AreEqual("Hello World", response!.Content);
        }
    }

    [TestMethod]
    public void Ping()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "ping",
            Array.Empty<string>(),
            PrivilegeLevel.Superuser);
        var executor = CommandExecutor.Create();
        var response = executor.TryExecute(instruction);

        Assert.IsNotNull(response);
        Assert.AreEqual("Pong!", response.Content);
    }
}
