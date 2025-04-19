using FakeItEasy;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Auditing.Abstraction.Entities;
using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Tests;

public class QuoteCommandTests
{
    private readonly QuoteCommands _quoteCommands;
    private readonly IQuoteService _quoteService;
    private readonly IAuditor _auditor;

    public QuoteCommandTests()
    {
        _quoteService = A.Fake<IQuoteService>();
        _auditor = A.Fake<IAuditor>();
        _quoteCommands = new QuoteCommands(
            LoggingUtils.GetLogger<QuoteCommands>(),
            _quoteService,
            _auditor
        );
    }

    [Fact]
    public void AddQuote()
    {
        A.CallTo(() => _quoteService.AddQuote(A<string>._, A<string>._, A<string?>._, A<string>._))
            .ReturnsLazily(
                (string _, string quoteText, string context, string author) =>
                    new Quote(
                        "someId",
                        "someChannelMapping",
                        1337,
                        quoteText,
                        context,
                        DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                        author
                    )
            );

        var reply = _quoteCommands.AddQuote(
            "Mock/Channel",
            "Cool Game",
            new ChatUser("Mock/UserName", "User Name", PrivilegeLevel.Viewer),
            "This is my quote"
        );
        Assert.Equal("Created new Quote #1337: This is my quote [Cool Game @ 2022-10-12]", reply);

        A.CallTo(
                () =>
                    _quoteService.AddQuote(
                        "Mock/Channel",
                        "This is my quote",
                        "Cool Game",
                        "User Name"
                    )
            )
            .MustHaveHappenedOnceExactly();
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Mock/UserName",
                            "Mock/Channel",
                            nameof(Quote),
                            "1337",
                            CommonActions.Created,
                            "Quote #1337: This is my quote [Cool Game @ 2022-10-12]"
                        )
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void EditQuote()
    {
        A.CallTo(() => _quoteService.EditQuote(A<string>._, An<int>._, A<string>._))
            .ReturnsLazily(
                (string _, int _, string newContent) =>
                    new Quote(
                        "someId",
                        "someChannelMapping",
                        1337,
                        newContent,
                        "Cool Game",
                        DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                        "User Name"
                    )
            );

        var reply = _quoteCommands.EditQuote(
            "Mock/Channel",
            1337,
            "This is my new text",
            new ChatUser("Mock/UserName", "User Name", PrivilegeLevel.Viewer)
        );

        Assert.Equal("Updated Quote #1337: This is my new text [Cool Game @ 2022-10-12]", reply);
        A.CallTo(() => _quoteService.EditQuote("Mock/Channel", 1337, "This is my new text"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Mock/UserName",
                            "Mock/Channel",
                            nameof(Quote),
                            "1337",
                            CommonActions.Updated,
                            "Quote #1337: This is my new text [Cool Game @ 2022-10-12]"
                        )
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void EditQuoteContext()
    {
        A.CallTo(() => _quoteService.EditQuoteContext(A<string>._, An<int>._, A<string>._))
            .ReturnsLazily(
                (string _, int quoteId, string _) =>
                    new Quote(
                        "someId",
                        "someMappingId",
                        quoteId,
                        "This is my quote",
                        "Uncool Game",
                        DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                        "User Name"
                    )
            );

        var reply = _quoteCommands.EditQuoteContext(
            "Mock/Channel",
            1337,
            "Uncool Game",
            new ChatUser("Mock/UserName", "User Name", PrivilegeLevel.Viewer)
        );

        Assert.Equal("Updated Quote #1337: This is my quote [Uncool Game @ 2022-10-12]", reply);

        A.CallTo(() => _quoteService.EditQuoteContext("Mock/Channel", 1337, "Uncool Game"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Mock/UserName",
                            "Mock/Channel",
                            nameof(Quote),
                            "1337",
                            CommonActions.Updated,
                            "Quote #1337: This is my quote [Uncool Game @ 2022-10-12]"
                        )
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void DeleteQuote()
    {
        A.CallTo(() => _quoteService.DeleteQuote(A<string>._, An<int>._)).Returns(true);

        var reply = _quoteCommands.DeleteQuote(
            "Mock/Channel",
            1337,
            new ChatUser("Mock/UserName", "User Name", PrivilegeLevel.Viewer)
        );
        Assert.Equal("Deleted Quote #1337", reply);

        A.CallTo(() => _quoteService.DeleteQuote("Mock/Channel", 1337))
            .MustHaveHappenedOnceExactly();
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Mock/UserName",
                            "Mock/Channel",
                            nameof(Quote),
                            "1337",
                            CommonActions.Deleted,
                            null
                        )
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(PrivilegeLevel.Unknown, false)]
    [InlineData(PrivilegeLevel.Viewer, false)]
    [InlineData(PrivilegeLevel.Moderator, true)]
    [InlineData(PrivilegeLevel.Administrator, true)]
    [InlineData(PrivilegeLevel.Superuser, true)]
    public void CanRunGenericDeleteQuoteCommandWithCorrectPermissions(
        PrivilegeLevel inputLevel,
        bool expectExecution
    )
    {
        A.CallTo(() => _quoteService.DeleteQuote(A<string>._, An<int>._)).Returns(true);

        var reply = _quoteCommands.Quote(
            "Mock/Channel",
            "Some Game",
            new ChatUser("Mock/User", "Mock User", inputLevel),
            "delete",
            "1",
            "1",
            string.Empty
        );

        if (expectExecution)
        {
            Assert.Equal("Deleted Quote #1", reply);
        }
        else
        {
            Assert.Null(reply);
        }

        A.CallTo(() => _quoteService.DeleteQuote("Mock/Channel", 1))
            .MustHaveHappened(expectExecution ? 1 : 0, Times.Exactly);
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Mock/User",
                            "Mock/Channel",
                            nameof(Quote),
                            "1",
                            CommonActions.Deleted,
                            null
                        )
                    )
            )
            .MustHaveHappened(expectExecution ? 1 : 0, Times.Exactly);
    }

    [Theory]
    [InlineData(PrivilegeLevel.Unknown, false)]
    [InlineData(PrivilegeLevel.Viewer, false)]
    [InlineData(PrivilegeLevel.Moderator, true)]
    [InlineData(PrivilegeLevel.Administrator, true)]
    [InlineData(PrivilegeLevel.Superuser, true)]
    public void CanRunGenericEditQuoteCommandWithCorrectPermissions(
        PrivilegeLevel inputLevel,
        bool expectExecution
    )
    {
        A.CallTo(() => _quoteService.EditQuote(A<string>._, An<int>._, A<string>._))
            .ReturnsLazily(
                (string _, int quoteId, string newContent) =>
                    new Quote(
                        "someId",
                        "someMappingId",
                        quoteId,
                        newContent,
                        "Some Game",
                        DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                        "Mock User"
                    )
            );

        var reply = _quoteCommands.Quote(
            "Mock/Channel",
            "Some Game",
            new ChatUser("Mock/User", "Mock User", inputLevel),
            "edit",
            "1",
            "1 My new text",
            "My new text"
        );

        if (expectExecution)
        {
            Assert.Equal("Updated Quote #1: My new text [Some Game @ 2022-10-12]", reply);
        }
        else
        {
            Assert.Null(reply);
        }

        A.CallTo(() => _quoteService.EditQuote("Mock/Channel", 1, "My new text"))
            .MustHaveHappened(expectExecution ? 1 : 0, Times.Exactly);
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Mock/User",
                            "Mock/Channel",
                            nameof(Quote),
                            "1",
                            CommonActions.Updated,
                            "Quote #1: My new text [Some Game @ 2022-10-12]"
                        )
                    )
            )
            .MustHaveHappened(expectExecution ? 1 : 0, Times.Exactly);
    }

    [Theory]
    [InlineData(PrivilegeLevel.Unknown, false)]
    [InlineData(PrivilegeLevel.Viewer, false)]
    [InlineData(PrivilegeLevel.Moderator, true)]
    [InlineData(PrivilegeLevel.Administrator, true)]
    [InlineData(PrivilegeLevel.Superuser, true)]
    public void CanRunGenericEditQuoteContextCommandWithCorrectPermissions(
        PrivilegeLevel inputLevel,
        bool expectExecution
    )
    {
        A.CallTo(() => _quoteService.EditQuoteContext(A<string>._, An<int>._, A<string>._))
            .ReturnsLazily(
                (string _, int quoteId, string newContext) =>
                    new Quote(
                        "someId",
                        "someMappingId",
                        quoteId,
                        "My text",
                        newContext,
                        DateTimeOffset.Parse("2022-10-12T12:34:56Z"),
                        "Mock User"
                    )
            );

        var reply = _quoteCommands.Quote(
            "Mock/Channel",
            "Some Game",
            new ChatUser("Mock/User", "Mock User", inputLevel),
            "editcontext",
            "1",
            "1 My new text",
            "My new text"
        );

        if (expectExecution)
        {
            Assert.Equal("Updated Quote #1: My text [My new text @ 2022-10-12]", reply);
        }
        else
        {
            Assert.Null(reply);
        }

        A.CallTo(() => _quoteService.EditQuoteContext("Mock/Channel", 1, "My new text"))
            .MustHaveHappened(expectExecution ? 1 : 0, Times.Exactly);
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Mock/User",
                            "Mock/Channel",
                            nameof(Quote),
                            "1",
                            CommonActions.Updated,
                            "Quote #1: My text [My new text @ 2022-10-12]"
                        )
                    )
            )
            .MustHaveHappened(expectExecution ? 1 : 0, Times.Exactly);
    }
}
