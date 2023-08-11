using System.Collections.Immutable;
using FakeItEasy;
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
                    new(PrivilegeLevel.Viewer, 15 * 1000),
                }.ToImmutableHashSetWithValueSemantics(),
            },
            ResponseMode = CommandResponseMode.First,
            Responses = new CommandResponse[]
            {
                new(ResponseType.Text, "Hello World"),
            }.ToImmutableListWithValueSemantics(),
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

        var cooldownService = A.Fake<ICooldownService>();
        A.CallTo(() => cooldownService.GetLastExecution("Mock/Channel", "Mock/User", "mycommand"))
            .Returns(RefTime);

        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownService,
            A.Fake<ICounterStorageService>(),
            A.Fake<ISoundCommandInvocationSender>()
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

        var cooldownService = A.Fake<ICooldownService>();
        A.CallTo(() => cooldownService.GetLastExecution("Mock/Channel", "Mock/User", "mycommand"))
            .Returns(RefTime);

        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownService,
            A.Fake<ICounterStorageService>(),
            A.Fake<ISoundCommandInvocationSender>()
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
                                PrivilegeLevel = userPrivilegeLevel,
                            },
                        }
                    ),
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

        var cooldownService = A.Fake<ICooldownService>();
        var counterService = A.Fake<ICounterStorageService>();
        var counter = 0;
        A.CallTo(() => counterService.Next(CommandDescription.Id)).ReturnsLazily(() => ++counter);

        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownService,
            counterService,
            A.Fake<ISoundCommandInvocationSender>()
        );

        string?[] reply = executor
            .Execute(
                CommandInstruction,
                CommandDescription with
                {
                    Responses = new[]
                    {
                        new CommandResponse(ResponseType.Text, "I am at level {Counter} now!"),
                    }.ToImmutableListWithValueSemantics(),
                }
            )
            .ToArray();

        A.CallTo(() => counterService.Next(CommandDescription.Id)).MustHaveHappenedOnceExactly();
        Assert.AreEqual("I am at level 1 now!", reply.First());

        // Repeat
        reply = executor
            .Execute(
                CommandInstruction,
                CommandDescription with
                {
                    Responses = new[]
                    {
                        new CommandResponse(ResponseType.Text, "I am at level {Counter} now!"),
                    }.ToImmutableListWithValueSemantics(),
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

        var cooldownService = A.Fake<ICooldownService>();
        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownService,
            A.Fake<ICounterStorageService>(),
            A.Fake<ISoundCommandInvocationSender>()
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
                                Identifier = inputUser,
                            },
                        }
                    ),
                },
                CommandDescription with
                {
                    Limitations = CommandDescription.Limitations with
                    {
                        LimitedToUsers = ImmutableHashSet
                            .Create<string>()
                            .Add("CoolUser".ToLowerInvariant()),
                    },
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
        var counterService = A.Fake<ICounterStorageService>();
        var counter = 0;
        A.CallTo(() => counterService.Next(CommandDescription.Id)).ReturnsLazily(() => ++counter);
        var placeholder = new PlaceholderContainer(
            CommandInstruction,
            CommandDescription,
            RefTime,
            42,
            counterService
        );

        // Access counter
        Assert.AreEqual(1, placeholder.Counter);
        A.CallTo(() => counterService.Next(CommandDescription.Id)).MustHaveHappenedOnceExactly();

        // Access counter again (ensure it has not been called twice)
        Assert.AreEqual(1, placeholder.Counter);
        A.CallTo(() => counterService.Next(CommandDescription.Id)).MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void PeekCounterWillNotIncreaseCounterValue()
    {
        var counterService = A.Fake<ICounterStorageService>();
        A.CallTo(() => counterService.Peek(A<string>.Ignored)).ReturnsLazily(() => 1);
        var placeholder = new PlaceholderContainer(
            CommandInstruction,
            CommandDescription,
            RefTime,
            42,
            counterService
        );

        // Access counter
        Assert.AreEqual(1, placeholder.PeekCounter);
        A.CallTo(() => counterService.Peek(A<string>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => counterService.Next(A<string>.Ignored)).MustNotHaveHappened();
    }
}
