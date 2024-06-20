using System.Collections.Immutable;
using FakeItEasy;
using FloppyBot.Base.Auditing.Abstraction.Impl;
using FloppyBot.Base.Clock;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Rng;
using FloppyBot.Base.Storage.LiteDb;
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

    private readonly ICounterStorageService _counterStorageService;

    public CustomCommandExecutorTests()
    {
        var liteDb = LiteDbRepositoryFactory.CreateMemoryInstance();
        _counterStorageService = A.Fake<ICounterStorageService>(x =>
            x.Wrapping(new CounterStorageService(liteDb))
        );
    }

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
            A.Fake<ISoundCommandInvocationSender>(),
            new NoopAuditor()
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
            _counterStorageService,
            A.Fake<ISoundCommandInvocationSender>(),
            new NoopAuditor()
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

        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownService,
            _counterStorageService,
            A.Fake<ISoundCommandInvocationSender>(),
            new NoopAuditor()
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

        A.CallTo(() => _counterStorageService.Next(CommandDescription.Id))
            .MustHaveHappenedOnceExactly();
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
            _counterStorageService,
            A.Fake<ISoundCommandInvocationSender>(),
            new NoopAuditor()
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
        var counter = 0;
        A.CallTo(() => _counterStorageService.Next(CommandDescription.Id))
            .ReturnsLazily(() => ++counter);
        var placeholder = new PlaceholderContainer(
            CommandInstruction,
            CommandDescription,
            RefTime,
            42,
            _counterStorageService
        );

        // Access counter
        Assert.AreEqual(1, placeholder.Counter);
        A.CallTo(() => _counterStorageService.Next(CommandDescription.Id))
            .MustHaveHappenedOnceExactly();

        // Access counter again (ensure it has not been called twice)
        Assert.AreEqual(1, placeholder.Counter);
        A.CallTo(() => _counterStorageService.Next(CommandDescription.Id))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void PeekCounterWillNotIncreaseCounterValue()
    {
        _counterStorageService.Set(CommandDescription.Id, 1);
        var placeholder = new PlaceholderContainer(
            CommandInstruction,
            CommandDescription,
            RefTime,
            42,
            _counterStorageService
        );

        // Access counter
        Assert.AreEqual(1, placeholder.PeekCounter);
        A.CallTo(() => _counterStorageService.Peek(A<string>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _counterStorageService.Next(A<string>.Ignored)).MustNotHaveHappened();
    }

    [TestMethod]
    public void AssumingOperationsAllowed_CounterWillIncrementWithCorrectCommand()
    {
        var timeProvider = new FixedTimeProvider(RefTime.Add(5.Seconds()));

        var cooldownService = A.Fake<ICooldownService>();
        var counterService = _counterStorageService;
        _counterStorageService.Set(CommandDescription.Id, 10);

        var executor = new CustomCommandExecutor(
            NullLogger<CustomCommandExecutor>.Instance,
            timeProvider,
            new RandomNumberGenerator(),
            cooldownService,
            counterService,
            A.Fake<ISoundCommandInvocationSender>(),
            new NoopAuditor()
        );

        var customizedCommandDescription = CommandDescription with
        {
            Responses = new[]
            {
                new CommandResponse(ResponseType.Text, "I am at level {PeekCounter} now!"),
            }.ToImmutableListWithValueSemantics(),
            AllowCounterOperations = true,
        };

        string?[] reply = executor
            .Execute(
                CommandInstruction with
                {
                    Parameters = new[] { "+" }.ToImmutableListWithValueSemantics(),
                    Context = new CommandContext(
                        SourceMessage: CommandInstruction.Context!.SourceMessage with
                        {
                            Author = CommandInstruction.Context!.SourceMessage.Author with
                            {
                                PrivilegeLevel = PrivilegeLevel.Moderator,
                            },
                        }
                    ),
                },
                customizedCommandDescription
            )
            .ToArray();

        Assert.AreEqual("I am at level 11 now!", reply.First());

        // Repeat
        reply = executor
            .Execute(
                CommandInstruction with
                {
                    Parameters = new[] { "-" }.ToImmutableListWithValueSemantics(),
                    Context = new CommandContext(
                        SourceMessage: CommandInstruction.Context!.SourceMessage with
                        {
                            Author = CommandInstruction.Context!.SourceMessage.Author with
                            {
                                PrivilegeLevel = PrivilegeLevel.Moderator,
                            },
                        }
                    ),
                },
                customizedCommandDescription
            )
            .ToArray();
        Assert.AreEqual("I am at level 10 now!", reply.First());
    }
}
