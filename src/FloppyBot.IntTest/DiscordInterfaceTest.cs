using AwesomeAssertions.Extensions;
using FloppyBot.Chat;
using FloppyBot.Chat.Discord;
using FloppyBot.Chat.Entities;
using FloppyBot.IntTest.Fixtures;
using FloppyBot.IntTest.Utils;

namespace FloppyBot.IntTest;

[Collection(DiscordFixtureCollection.NAME)]
public class DiscordInterfaceTest : IAsyncDisposable
{
    private readonly DiscordFixture _discordFixture;
    private readonly DiscordChatInterface _bot1;
    private readonly DiscordChatInterface _bot2;

    public DiscordInterfaceTest(DiscordFixture discordFixture)
    {
        _discordFixture = discordFixture;
        _bot1 = discordFixture.Bot1;
        _bot2 = discordFixture.Bot2;
    }

    [Fact(Skip = "only to be executed locally")]
    public async Task TestMessageSubmissions()
    {
        await _discordFixture.WaitForConnection();
        using var messageMonitor = _bot2.Monitor();

        _bot1.SendMessage(_discordFixture.DefaultChannelIdentifier, "$ping");

        await Wait.Until(
            () => messageMonitor.OccurredEvents.Length > 0,
            5.Seconds(),
            "Event was not raised"
        );

        var expectedChatMessage = new ChatMessage(
            $"Discord/{_discordFixture.ServerId}/_",
            new ChatUser(
                $"Discord/{_discordFixture.Bot1Id}",
                "FloppyBot Test 1",
                PrivilegeLevel.Unknown
            ),
            SharedEventTypes.CHAT_MESSAGE,
            "$ping",
            null,
            _bot1.SupportedFeatures
        );

        messageMonitor.Should().Raise(nameof(DiscordChatInterface.MessageReceived));
        messageMonitor
            .OccurredEvents.Select(e => e.Parameters.Last())
            .Cast<ChatMessage>()
            .Select(c => c with { Identifier = $"Discord/{_discordFixture.ServerId}/_" })
            .Should()
            .ContainInOrder(expectedChatMessage);

        _bot1.SendMessage(
            _discordFixture.DefaultChannelIdentifier,
            ":white_check_mark: Test complete"
        );
    }

    [Fact(Skip = "only to be executed locally")]
    public async Task ExpectNoMessageSubmissionsFromBotsIfNotEnabled()
    {
        await _discordFixture.WaitForConnection();

        using var messageMonitor = _bot1.Monitor();
        _bot2.SendMessage(_discordFixture.DefaultChannelIdentifier, "$ping");

        await Task.Delay(1.Seconds());

        messageMonitor.Should().NotRaise(nameof(DiscordChatInterface.MessageReceived));

        _bot2.SendMessage(
            _discordFixture.DefaultChannelIdentifier,
            ":white_check_mark: Test complete"
        );
    }

    public async ValueTask DisposeAsync()
    {
        await _discordFixture.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
