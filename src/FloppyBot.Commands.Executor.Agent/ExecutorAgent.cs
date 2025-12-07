using System.Text.Json;
using FloppyBot.Base.Configuration;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage.Indexing;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Commands.Aux.Twitch.Helpers;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Core.Replier;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Executor.Agent;

public class ExecutorAgent : BackgroundService
{
    private readonly ICommandExecutor _commandExecutor;

    private readonly IndexInitializer _indexInitializer;
    private readonly INotificationReceiver<CommandInstruction> _instructionReceiver;
    private readonly INotificationReceiver<ChatMessage> _notificationReceiver;
    private readonly ILogger<ExecutorAgent> _logger;
    private readonly ITwitchRewardConverter _twitchRewardConverter;

    private readonly IMessageReplier _replier;

    public ExecutorAgent(
        ILogger<ExecutorAgent> logger,
        IConfiguration configuration,
        INotificationReceiverFactory receiverFactory,
        ICommandExecutor commandExecutor,
        IndexInitializer indexInitializer,
        IMessageReplier replier,
        ITwitchRewardConverter twitchRewardConverter
    )
    {
        _logger = logger;
        _instructionReceiver = receiverFactory.GetNewReceiver<CommandInstruction>(
            configuration.GetParsedConnectionString("CommandInput")
        );
        _notificationReceiver = receiverFactory.GetNewReceiver<ChatMessage>(
            configuration.GetParsedConnectionString("MessageInput")
        );

        _instructionReceiver.NotificationReceived += OnCommandReceived;
        _notificationReceiver.NotificationReceived += OnMessageReceived;

        _commandExecutor = commandExecutor;
        _indexInitializer = indexInitializer;

        _replier = replier;
        _twitchRewardConverter = twitchRewardConverter;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _indexInitializer.InitializeIndices();

        _logger.LogInformation("Starting Command Executor Agent ...");
        _instructionReceiver.StartListening();
        _notificationReceiver.StartListening();
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Shutting down Command Executor Agent ...");
        _instructionReceiver.StopListening();
        _notificationReceiver.StopListening();
        return base.StopAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    private void OnCommandReceived(CommandInstruction commandInstruction)
    {
#if DEBUG
        _logger.LogDebug("Received command instruction {@CommandInstruction}", commandInstruction);
#endif

        ChatMessage? reply;
        try
        {
            reply = _commandExecutor.ExecuteCommand(commandInstruction);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Command execution failed! {@CommandInstruction}",
                commandInstruction
            );
            return;
        }

        if (reply == null || string.IsNullOrWhiteSpace(reply.Content))
        {
            _logger.LogDebug("Reply was empty (null or whitespace), discarding");
            return;
        }

        _replier.SendMessage(reply);
    }

    private void OnMessageReceived(ChatMessage notification)
    {
        if (notification.EventName != TwitchEventTypes.CHANNEL_POINTS_REWARD_REDEEMED)
        {
            return;
        }

        _logger.LogDebug("Received message notification {@ChatMessage}", notification);

        JsonSerializer
            .Deserialize<TwitchChannelPointsRewardRedeemedEvent>(notification.Content)
            .Wrap()
            .Select(rewardEvent =>
                _twitchRewardConverter.ConvertToCommandInstruction(rewardEvent, notification)
            )
            .Complete(OnCommandReceived);
    }
}
