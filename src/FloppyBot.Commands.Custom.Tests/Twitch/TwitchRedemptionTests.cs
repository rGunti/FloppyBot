using System.Text.Json;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Auditing.Abstraction.Impl;
using FloppyBot.Base.Clock;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Rng;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Commands.Aux.Twitch.Helpers;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Core.Scan;
using FloppyBot.Commands.Custom.Execution;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Communication;
using FloppyBot.Communication.Mock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Custom.Tests.Twitch;

[TestClass]
public class TwitchRedemptionTests
{
    private static readonly CustomCommandDescription CommandDescription = new()
    {
        Id = "id",
        Name = "lurk",
        Aliases = Enumerable.Empty<string>().ToImmutableHashSetWithValueSemantics(),
        Owners = new[] { "Twitch/pinsrltrex" }.ToImmutableHashSetWithValueSemantics(),
        Limitations = new CommandLimitation
        {
            Cooldown = new CooldownDescription[]
            {
                new(PrivilegeLevel.Moderator, 0),
                new(PrivilegeLevel.Viewer, 15_000),
            }.ToImmutableHashSetWithValueSemantics(),
            MinLevel = PrivilegeLevel.Viewer,
        },
        ResponseMode = CommandResponseMode.First,
        Responses = new CommandResponse[]
        {
            new(ResponseType.Text, "Hello from Lurk"),
        }.ToImmutableListWithValueSemantics(),
    };

    private readonly ServiceProvider _services;

    private readonly ITwitchRewardConverter _rewardConverter;
    private readonly ICustomCommandService _customCommandService;
    private readonly ICommandExecutor _commandExecutor;

    public TwitchRedemptionTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { "Key", "Value" },
                    { "ConnectionStrings:SoundCommandInvocation", "sleep" },
                }
            )
            .Build();
        var serviceCollection = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddLogging()
            .AddInMemoryStorage()
            .ScanAndAddCommandDependencies()
            .AddAutoMapper(typeof(CustomCommandStorageProfile))
            .AddSingleton<ICustomCommandService, CustomCommandService>()
            .AddSingleton<ITwitchRewardConverter, TwitchRewardConverter>()
            .AddSingleton<ITimeProvider>(new FixedTimeProvider())
            .AddSingleton<IRandomNumberGenerator>(new StaticNumberGenerator(42069))
            .AddSingleton<INotificationReceiverFactory, MockNotificationInterfaceFactory>()
            .AddSingleton<INotificationSenderFactory, MockNotificationInterfaceFactory>()
            .AddAuditor<NoopAuditor>();

        CustomCommandHost.DiSetup(serviceCollection);

        _services = serviceCollection.BuildServiceProvider();

        _customCommandService = _services.GetRequiredService<ICustomCommandService>();
        _customCommandService.CreateCommand(CommandDescription);

        _rewardConverter = _services.GetRequiredService<ITwitchRewardConverter>();
        _commandExecutor = _services.GetRequiredService<ICommandExecutor>();
    }

    [TestMethod]
    public void ConvertToCommandInstruction()
    {
        _customCommandService.LinkTwitchReward(
            "0314bae9-dd69-4664-b3f8-81a688f51d1a",
            "Twitch/pinsrltrex",
            "lurk"
        );

        var input =
            "{\"Identifier\":{\"Interface\":\"Twitch\",\"Channel\":\"pinsrltrex\",\"MessageId\":\"reward-09cd234d-c64f-423f-8f99-e04d0ed18184\",\"IsNewMessage\":false},\"Author\":{\"Identifier\":{\"Interface\":\"Twitch\",\"Channel\":\"floppypandach\"},\"DisplayName\":\"FloppyPandaCH\",\"PrivilegeLevel\":0},\"EventName\":\"Twitch.ChannelPointRewardRedeemed\",\"Content\":\"{\\u0022EventId\\u0022:\\u002209cd234d-c64f-423f-8f99-e04d0ed18184\\u0022,\\u0022User\\u0022:{\\u0022Identifier\\u0022:{\\u0022Interface\\u0022:\\u0022Twitch\\u0022,\\u0022Channel\\u0022:\\u0022floppypandach\\u0022},\\u0022DisplayName\\u0022:\\u0022FloppyPandaCH\\u0022,\\u0022PrivilegeLevel\\u0022:0},\\u0022Reward\\u0022:{\\u0022RewardId\\u0022:\\u00220314bae9-dd69-4664-b3f8-81a688f51d1a\\u0022,\\u0022Title\\u0022:\\u0022Hello from Lurk\\u0022,\\u0022Prompt\\u0022:\\u0022if you don\\\\u0027t feel like chatting, or got stuffs to do, but want to let me know you\\\\u0027re there. As long as you don\\\\u0027t chat, T-rexes cannot see you, so you\\\\u0027re safe. \\u0022,\\u0022PointCost\\u0022:7},\\u0022EventName\\u0022:\\u0022Twitch.ChannelPointRewardRedeemed\\u0022}\",\"Context\":null,\"SupportedFeatures\":0}";
        var chatMessage = JsonSerializer.Deserialize<ChatMessage>(input);

        Assert.IsNotNull(chatMessage);

        var rewardEvent = JsonSerializer.Deserialize<TwitchChannelPointsRewardRedeemedEvent>(
            chatMessage.Content
        );
        Assert.IsNotNull(rewardEvent);

        var result = _rewardConverter
            .ConvertToCommandInstruction(rewardEvent, chatMessage)
            .SingleOrDefault();

        Assert.IsNotNull(result);
        Assert.AreEqual("lurk", result.CommandName);

        var commandResult = _commandExecutor.ExecuteCommand(result);
        Assert.IsNotNull(commandResult);
        Assert.AreEqual("Hello from Lurk", commandResult.Content);
    }
}
