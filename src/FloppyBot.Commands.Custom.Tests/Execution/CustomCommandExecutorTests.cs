using FloppyBot.Base.Clock;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Rng;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Cooldown;
using FloppyBot.Commands.Custom.Execution;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FloppyBot.Commands.Custom.Tests.Execution;

[TestClass]
public class CustomCommandExecutorTests
{
    private static readonly DateTimeOffset RefTime = DateTimeOffset.Parse("2022-11-01T12:30:00Z");

    private static readonly CustomCommandDescription CommandDescription = new()
    {
        Id = "id",
        Name = "mycommand",
        Aliases = Enumerable.Empty<string>().ToImmutableHashSetWithValueSemantics(),
        Owners = new[] { "Mock/Channel" }.ToImmutableHashSetWithValueSemantics(),
        Limitations = new CommandLimitation
        {
            Cooldown = new CooldownDescription[]
            {
                new(PrivilegeLevel.Viewer, 15 * 1000)
            }.ToImmutableHashSetWithValueSemantics()
        },
        ResponseMode = CommandResponseMode.First,
        Responses = new CommandResponse[]
        {
            new(ResponseType.Text, "Hello World")
        }.ToImmutableListWithValueSemantics()
    };

    private static readonly CommandInstruction CommandInstruction = new(
        "mycommand",
        Enumerable.Empty<string>().ToImmutableListWithValueSemantics(),
        new CommandContext(
            new ChatMessage(
                "Mock/Channel/abcdefg",
                new ChatUser(
                    "Mock/User",
                    "Mock User",
                    PrivilegeLevel.Viewer),
                SharedEventTypes.CHAT_MESSAGE,
                "content")));

    [DataTestMethod]
    [DataRow(0, false)]
    [DataRow(5, false)]
    [DataRow(10, false)]
    [DataRow(15, true)]
    [DataRow(20, true)]
    [DataRow(25, true)]
    [DataRow(30, true)]
    public void HandlesCooldownCorrectly(int advanceBySecond, bool expectResult)
    {
        var timeProvider = new FixedTimeProvider(RefTime);
        timeProvider.AdvanceTimeBy(advanceBySecond.Seconds());

        var cooldownServiceMock = new Mock<ICooldownService>();
        cooldownServiceMock
            .Setup(c => c.GetLastExecution(
                It.Is<string>(c => c == "Mock/Channel"),
                It.Is<string>(u => u == "Mock/User"),
                It.Is<string>(c => c == "mycommand")))
            .Returns<string, string, string>((_, _, _) => RefTime);

        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownServiceMock.Object);

        string?[] reply = executor.Execute(CommandInstruction, CommandDescription).ToArray();
        if (expectResult)
        {
            CollectionAssert.AreEqual(new[]
            {
                "Hello World"
            }, reply);
        }
        else
        {
            Assert.AreEqual(0, reply.Length);
        }
    }
}
