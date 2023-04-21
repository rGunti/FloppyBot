using System.Collections.Immutable;
using FloppyBot.Base.Clock;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Rng;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Cooldown;
using FloppyBot.Commands.Custom.Communication;
using FloppyBot.Commands.Custom.Execution;
using FloppyBot.Commands.Custom.Execution.InternalEntities;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FloppyBot.Commands.Custom.Tests.Execution;

[TestClass]
public class CustomCommandExecutorTests
{
    private static readonly DateTimeOffset RefTime = DateTimeOffset.Parse("2022-11-01T12:30:00Z");

    private static readonly CustomCommandDescription CommandDescription =
        new()
        {
            Id = "id",
            Name = "mycommand",
            Aliases = Enumerable.Empty<string>().ToImmutableHashSetWithValueSemantics(),
            Owners = new[] { "Mock/Channel" }.ToImmutableHashSetWithValueSemantics(),
            Limitations = new CommandLimitation
            {
                Cooldown = new CooldownDescription[]
                {
                    new(PrivilegeLevel.Moderator, 0),
                    new(PrivilegeLevel.Viewer, 15 * 1000)
                }.ToImmutableHashSetWithValueSemantics()
            },
            ResponseMode = CommandResponseMode.First,
            Responses = new CommandResponse[]
            {
                new(ResponseType.Text, "Hello World")
            }.ToImmutableListWithValueSemantics()
        };

    private static readonly CommandInstruction CommandInstruction =
        new(
            "mycommand",
            Enumerable.Empty<string>().ToImmutableListWithValueSemantics(),
            new CommandContext(
                new ChatMessage(
                    "Mock/Channel/abcdefg",
                    new ChatUser("Mock/User", "Mock User", PrivilegeLevel.Viewer),
                    SharedEventTypes.CHAT_MESSAGE,
                    "content"
                )
            )
        );

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
            .Setup(
                c =>
                    c.GetLastExecution(
                        It.Is<string>(c => c == "Mock/Channel"),
                        It.Is<string>(u => u == "Mock/User"),
                        It.Is<string>(c => c == "mycommand")
                    )
            )
            .Returns<string, string, string>((_, _, _) => RefTime);

        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownServiceMock.Object,
            Mock.Of<ICounterStorageService>(),
            Mock.Of<ISoundCommandInvocationSender>()
        );

        string?[] reply = executor.Execute(CommandInstruction, CommandDescription).ToArray();
        if (expectResult)
        {
            CollectionAssert.AreEqual(new[] { "Hello World" }, reply);
        }
        else
        {
            Assert.AreEqual(0, reply.Length);
        }
    }

    [DataTestMethod]
    [DataRow(PrivilegeLevel.Administrator, true)]
    [DataRow(PrivilegeLevel.Moderator, true)]
    [DataRow(PrivilegeLevel.Viewer, false)]
    public void HandlesCooldownWithPrivilegeLevelsCorrectly(
        PrivilegeLevel userPrivilegeLevel,
        bool expectResult
    )
    {
        var timeProvider = new FixedTimeProvider(RefTime.Add(5.Seconds()));

        var cooldownServiceMock = new Mock<ICooldownService>();
        cooldownServiceMock
            .Setup(
                c =>
                    c.GetLastExecution(
                        It.Is<string>(ch => ch == "Mock/Channel"),
                        It.Is<string>(u => u == "Mock/User"),
                        It.Is<string>(cmd => cmd == "mycommand")
                    )
            )
            .Returns<string, string, string>((_, _, _) => RefTime);

        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownServiceMock.Object,
            Mock.Of<ICounterStorageService>(),
            Mock.Of<ISoundCommandInvocationSender>()
        );

        string?[] reply = executor
            .Execute(
                CommandInstruction with
                {
                    Context = new CommandContext(
                        CommandInstruction.Context!.SourceMessage with
                        {
                            Author = CommandInstruction.Context!.SourceMessage.Author with
                            {
                                PrivilegeLevel = userPrivilegeLevel
                            }
                        }
                    )
                },
                CommandDescription
            )
            .ToArray();
        if (expectResult)
        {
            CollectionAssert.AreEqual(new[] { "Hello World" }, reply);
        }
        else
        {
            Assert.AreEqual(0, reply.Length);
        }
    }

    [TestMethod]
    public void HandlesCounterCorrectly()
    {
        var timeProvider = new FixedTimeProvider(RefTime.Add(5.Seconds()));

        var cooldownServiceMock = new Mock<ICooldownService>();
        var counterMock = new Mock<ICounterStorageService>();
        var counter = 0;
        counterMock
            .Setup(s => s.Next(It.Is<string>(c => c == CommandDescription.Id)))
            .Returns((string _) => ++counter);

        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownServiceMock.Object,
            counterMock.Object,
            Mock.Of<ISoundCommandInvocationSender>()
        );

        string?[] reply = executor
            .Execute(
                CommandInstruction,
                CommandDescription with
                {
                    Responses = new[]
                    {
                        new CommandResponse(ResponseType.Text, "I am at level {Counter} now!")
                    }.ToImmutableListWithValueSemantics()
                }
            )
            .ToArray();

        counterMock.Verify(s => s.Next(It.Is<string>(c => c == CommandDescription.Id)), Times.Once);
        Assert.AreEqual("I am at level 1 now!", reply.First());

        // Repeat
        reply = executor
            .Execute(
                CommandInstruction,
                CommandDescription with
                {
                    Responses = new[]
                    {
                        new CommandResponse(ResponseType.Text, "I am at level {Counter} now!")
                    }.ToImmutableListWithValueSemantics()
                }
            )
            .ToArray();
        Assert.AreEqual("I am at level 2 now!", reply.First());
    }

    [DataTestMethod]
    [DataRow("Mock/CoolUser", true)]
    [DataRow("Mock/UncoolUser", false)]
    [DataRow("Mock/SomeOtherUser", false)]
    public void HandlesUserLimitationCorrectly(string inputUser, bool expectResult)
    {
        var timeProvider = new FixedTimeProvider(RefTime.Add(5.Seconds()));

        var cooldownServiceMock = new Mock<ICooldownService>();
        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownServiceMock.Object,
            Mock.Of<ICounterStorageService>(),
            Mock.Of<ISoundCommandInvocationSender>()
        );

        string?[] reply = executor
            .Execute(
                CommandInstruction with
                {
                    Context = new CommandContext(
                        CommandInstruction.Context!.SourceMessage with
                        {
                            Author = CommandInstruction.Context!.SourceMessage.Author with
                            {
                                Identifier = inputUser
                            }
                        }
                    )
                },
                CommandDescription with
                {
                    Limitations = CommandDescription.Limitations with
                    {
                        LimitedToUsers = ImmutableHashSet
                            .Create<string>()
                            .Add("CoolUser".ToLowerInvariant())
                    }
                }
            )
            .ToArray();
        if (expectResult)
        {
            CollectionAssert.AreEqual(new[] { "Hello World" }, reply);
        }
        else
        {
            Assert.AreEqual(0, reply.Length);
        }
    }

    [TestMethod]
    public void CounterWillOnlyIncrementOnce()
    {
        var counterMock = new Mock<ICounterStorageService>();
        var counter = 0;
        counterMock.Setup(s => s.Next(It.IsAny<string>())).Returns((string _) => ++counter);
        var placeholder = new PlaceholderContainer(
            CommandInstruction,
            CommandDescription,
            RefTime,
            42,
            counterMock.Object
        );

        // Access counter
        Assert.AreEqual(1, placeholder.Counter);
        counterMock.Verify(s => s.Next(It.IsAny<string>()), Times.Once);

        // Access counter again (ensure it has not been called twice)
        Assert.AreEqual(1, placeholder.Counter);
        counterMock.Verify(s => s.Next(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public void PeekCounterWillNotIncreaseCounterValue()
    {
        var counterMock = new Mock<ICounterStorageService>();
        counterMock.Setup(s => s.Peek(It.IsAny<string>())).Returns((string _) => 1);
        counterMock.Setup(s => s.Next(It.IsAny<string>())).Verifiable();
        var placeholder = new PlaceholderContainer(
            CommandInstruction,
            CommandDescription,
            RefTime,
            42,
            counterMock.Object
        );

        // Access counter
        Assert.AreEqual(1, placeholder.PeekCounter);
        counterMock.Verify(s => s.Peek(It.IsAny<string>()), Times.Once);
        counterMock.Verify(s => s.Next(It.IsAny<string>()), Times.Never);
    }
}
