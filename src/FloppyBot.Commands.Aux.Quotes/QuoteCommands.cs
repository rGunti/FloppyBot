using System.Collections.Immutable;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Rng;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Quotes;

[CommandHost]
[PrivilegeGuard(PrivilegeLevel.Viewer)]
[CommandCategory("Quotes")]
public class QuoteCommands
{
    public const string REPLY_CREATED = "Created new {Quote}";
    public const string REPLY_QUOTE_NOT_FOUND = "Quote #{QuoteId} could not be found";
    public const string REPLY_NO_QUOTES = "There doesn't seem to be any quote.";
    public const string REPLY_EDITED = "Updated {Quote}";
    public const string REPLY_DELETED = "Deleted Quote #{QuoteId}";
    public const string REPLY_TEXT_MISSING = "Quote text is missing";
    public const string REPLY_CONTEXT_MISSING = "Quote context is missing";
    public const string REPLY_QUOTE_ID_INVALID = "Quote number has to be a number";

    private static readonly IImmutableSet<string> OpAdd = new[] { "add", "+" }.ToImmutableHashSet();
    private static readonly IImmutableSet<string> OpEdit = new[] { "edit", "*" }.ToImmutableHashSet();
    private static readonly IImmutableSet<string> OpEditContext = new[] { "editcontext", "ec" }.ToImmutableHashSet();
    private static readonly IImmutableSet<string> OpDelete = new[] { "del", "delete", "-" }.ToImmutableHashSet();

    private readonly ILogger<QuoteCommands> _logger;
    private readonly IQuoteService _quoteService;

    public QuoteCommands(
        ILogger<QuoteCommands> logger,
        IQuoteService quoteService)
    {
        _logger = logger;
        _quoteService = quoteService;
    }

    [Command("quote", "q")]
    [PrimaryCommandName("quote")]
    [CommandDescription("Returns a random quote or a specific one, if a quote number is given")]
    [CommandSyntax(
        "[<Quote No.>]",
        "",
        "123")]
    [CommandParameterHint(1, "input", CommandParameterType.String, false)]
    public string? Quote(
        [SourceChannel] string sourceChannel,
        [SourceContext]
        string sourceContext,
        [Author]
        ChatUser author,
        [ArgumentIndex(0, stopIfMissing: false)]
        string? op,
        [ArgumentIndex(1, stopIfMissing: false)]
        string? subOp,
        [ArgumentRange(1, stopIfMissing: false)]
        string? text,
        [ArgumentRange(2, stopIfMissing: false)]
        string? subOpText)
    {
        try
        {
            return DoQuote(
                sourceChannel,
                sourceContext,
                author,
                op,
                subOp,
                text,
                subOpText);
        }
        catch (MissingPrivilegeException)
        {
            return null;
        }
    }

    private string? DoQuote(
        string sourceChannel,
        string sourceContext,
        ChatUser author,
        string? op,
        string? subOp,
        string? text,
        string? subOpText)
    {
        if (string.IsNullOrWhiteSpace(op))
        {
            var randomQuote = _quoteService.GetRandomQuote(sourceChannel);
            return randomQuote != null
                ? randomQuote.ToString()
                : REPLY_NO_QUOTES;
        }

        if (int.TryParse(op, out int quoteId))
        {
            return _quoteService.GetQuote(sourceChannel, quoteId)?.ToString()
                   ?? REPLY_QUOTE_NOT_FOUND.Format(new
                   {
                       QuoteId = quoteId
                   });
        }

        if (OpAdd.Contains(op))
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return REPLY_TEXT_MISSING;
            }

            return AddQuote(
                sourceChannel,
                sourceContext,
                author,
                text);
        }

        if (OpEdit.Contains(op))
        {
            author.AssertLevel(PrivilegeLevel.Moderator);

            if (!int.TryParse(subOp, out int editQuoteId))
            {
                return REPLY_QUOTE_ID_INVALID;
            }

            if (string.IsNullOrWhiteSpace(subOpText))
            {
                return REPLY_TEXT_MISSING;
            }

            return EditQuote(
                sourceChannel,
                editQuoteId,
                subOpText);
        }

        if (OpEditContext.Contains(op))
        {
            author.AssertLevel(PrivilegeLevel.Moderator);

            if (!int.TryParse(subOp, out int editQuoteId))
            {
                return REPLY_QUOTE_ID_INVALID;
            }

            if (string.IsNullOrWhiteSpace(subOpText))
            {
                return REPLY_CONTEXT_MISSING;
            }

            return EditQuoteContext(
                sourceChannel,
                editQuoteId,
                subOpText);
        }

        if (OpDelete.Contains(op))
        {
            author.AssertLevel(PrivilegeLevel.Moderator);

            if (!int.TryParse(subOp, out int deleteQuoteId))
            {
                return REPLY_QUOTE_ID_INVALID;
            }

            return DeleteQuote(
                sourceChannel,
                deleteQuoteId);
        }

        _logger.LogInformation(
            "Ran into an unhandled quote command: {SourceChannel} {OperatorCode} {Text}",
            sourceChannel,
            op,
            text);
        return null;
    }

    [Command("quoteadd", "q+")]
    [PrimaryCommandName("quoteadd")]
    [CommandDescription("Adds a new quote")]
    [CommandSyntax("<Text>")]
    [CommandParameterHint(1, "quoteText", CommandParameterType.String)]
    public string AddQuote(
        [SourceChannel] ChannelIdentifier sourceChannel,
        [SourceContext]
        string? sourceContext,
        [Author]
        ChatUser author,
        [AllArguments]
        string quoteText)
    {
        var quote = _quoteService.AddQuote(
            sourceChannel,
            quoteText,
            sourceContext ?? sourceChannel.Interface,
            author.DisplayName);
        return REPLY_CREATED.Format(new
        {
            Quote = quote
        });
    }

    [Command("quoteedit", "qe", "q*")]
    [PrimaryCommandName("quoteedit")]
    [CommandDescription("Edits the text of an existing quote")]
    [CommandSyntax("<Quote No.> <New Text>")]
    [PrivilegeGuard(PrivilegeLevel.Moderator)]
    [CommandParameterHint(1, "id", CommandParameterType.Number)]
    [CommandParameterHint(2, "newText", CommandParameterType.String)]
    public string EditQuote(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0)]
        int quoteId,
        [ArgumentRange(1)]
        string newQuoteText)
    {
        var editedQuote = _quoteService.EditQuote(
            sourceChannel,
            quoteId,
            newQuoteText);
        if (editedQuote == null)
        {
            return REPLY_QUOTE_NOT_FOUND.Format(new
            {
                QuoteId = quoteId
            });
        }

        return REPLY_EDITED.Format(new
        {
            Quote = editedQuote
        });
    }

    [Command("quoteeditcontext", "qec", "q*c")]
    [PrimaryCommandName("quoteeditcontext")]
    [CommandDescription("Edits the context of an existing quote")]
    [CommandSyntax("<Quote No.> <New Context>")]
    [PrivilegeGuard(PrivilegeLevel.Moderator)]
    [CommandParameterHint(1, "id", CommandParameterType.Number)]
    [CommandParameterHint(2, "newContext", CommandParameterType.String)]
    public string EditQuoteContext(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0)]
        int quoteId,
        [ArgumentRange(1)]
        string newQuoteContext)
    {
        var editedQuote = _quoteService.EditQuoteContext(
            sourceChannel,
            quoteId,
            newQuoteContext);
        if (editedQuote == null)
        {
            return REPLY_QUOTE_NOT_FOUND.Format(new
            {
                QuoteId = quoteId
            });
        }

        return REPLY_EDITED.Format(new
        {
            Quote = editedQuote
        });
    }

    [Command("quotedel", "q-")]
    [PrimaryCommandName("quotedel")]
    [CommandDescription("Deletes an existing quote")]
    [CommandSyntax("<Quote No.>", "123")]
    [PrivilegeGuard(PrivilegeLevel.Moderator)]
    [CommandParameterHint(1, "id", CommandParameterType.Number)]
    public string DeleteQuote(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0)]
        int quoteId)
    {
        return _quoteService.DeleteQuote(sourceChannel, quoteId)
            ? REPLY_DELETED.Format(new
            {
                QuoteId = quoteId
            })
            : REPLY_QUOTE_NOT_FOUND.Format(new
            {
                QuoteId = quoteId
            });
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void RegisterDependencies(IServiceCollection services)
    {
        services
            .AddSingleton<ITimeProvider, RealTimeProvider>()
            .AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>()
            .AddScoped<IQuoteService, QuoteService>()
            .AddScoped<IQuoteChannelMappingService, QuoteChannelMappingService>();
    }
}
