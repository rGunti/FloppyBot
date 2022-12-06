using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using Microsoft.Extensions.DependencyInjection;
using UrbanDictionnet;

namespace FloppyBot.Commands.Aux.UrbanDictionary;

[CommandHost]
[CommandCategory("Tools")]
// ReSharper disable once UnusedType.Global
public class UrbanDictionaryCommands
{
    private const string REPLY_DEFINITION = "{Word}: {Definition} (Source: {Permalink})";
    private const string REPLY_NOT_FOUND = "Sorry, but I couldn't find anything for {Query}";

    private readonly UrbanClient _urbanClient;

    public UrbanDictionaryCommands(UrbanClient urbanClient)
    {
        _urbanClient = urbanClient;
    }

    [Command("urbandictionary", "define")]
    [PrimaryCommandName("define")]
    [CommandCooldown(PrivilegeLevel.Viewer, 30000)]
    public async Task<CommandResult> Define([AllArguments] string query)
    {
        WordDefine? definition = await _urbanClient.GetWordAsync(query);
        if (definition is { ResultType: ResultType.Exact } && definition.Definitions.Any())
        {
            return CommandResult.SuccessWith(REPLY_DEFINITION.Format(definition.Definitions.First()));
        }

        return CommandResult.FailedWith(REPLY_NOT_FOUND.Format(new
        {
            Query = query
        }));
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void DiSetup(IServiceCollection services)
    {
        services
            .AddSingleton<UrbanClient>();
    }
}

