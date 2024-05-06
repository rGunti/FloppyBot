using FloppyBot.Chat.Twitch.Config;
using Microsoft.Extensions.Logging;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace FloppyBot.Chat.Twitch.Monitor;

public sealed class TwitchChannelOnlineMonitor : ITwitchChannelOnlineMonitor, IDisposable
{
    private readonly ILogger<TwitchChannelOnlineMonitor> _logger;
    private readonly LiveStreamMonitorService _liveStreamMonitorService;

    private TwitchStream? _stream;

    public TwitchChannelOnlineMonitor(
        ILogger<TwitchChannelOnlineMonitor> logger,
        LiveStreamMonitorService liveStreamMonitorService,
        TwitchConfiguration twitchConfiguration
    )
    {
        _logger = logger;
        _liveStreamMonitorService = liveStreamMonitorService;

        if (twitchConfiguration.HasTwitchApiCredentials)
        {
            _liveStreamMonitorService.OnStreamOffline += OnStreamOffline;
            _liveStreamMonitorService.OnStreamOnline += OnStreamOnline;
            _liveStreamMonitorService.OnStreamUpdate += OnStreamUpdate;

            _liveStreamMonitorService.SetChannelsByName([twitchConfiguration.Channel]);

            _logger.LogInformation("Starting Live Stream Monitor ...");
            _liveStreamMonitorService.Start();
        }
        else
        {
            _logger.LogWarning(
                "Live Stream Monitor has not been started because no credentials have been provided"
            );
        }
    }

    public TwitchStream? Stream => _stream;

    public bool IsChannelOnline()
    {
        return _stream?.IsOnline ?? false;
    }

    public event TwitchChannelOnlineStatusChangedDelegate? OnlineStatusChanged;
    public event TwitchChannelStatusChangedDelegate? StatusChanged;

    public void Dispose()
    {
        _logger.LogInformation("Stopping and disposing Live Stream Monitor ...");

        _liveStreamMonitorService.Stop();
        _liveStreamMonitorService.OnStreamOffline -= OnStreamOffline;
        _liveStreamMonitorService.OnStreamOnline -= OnStreamOnline;
        _liveStreamMonitorService.OnStreamUpdate -= OnStreamUpdate;
    }

    private void OnStreamUpdate(object? sender, OnStreamUpdateArgs e)
    {
        _logger.LogTrace("Channel {ChannelName} has updated", e.Channel);
        var stream = e.Stream.ToTwitchStream(_stream?.IsOnline ?? false);
        var onlineChanged = _stream?.IsOnline != stream.IsOnline;
        _stream = _stream != null ? stream with { IsOnline = _stream.IsOnline } : stream;

        var eventArgs = new TwitchChannelOnlineStatusChangedEventArgs(_stream);
        StatusChanged?.Invoke(this, eventArgs);
        if (onlineChanged)
        {
            OnlineStatusChanged?.Invoke(this, eventArgs);
        }
    }

    private void OnStreamOnline(object? sender, OnStreamOnlineArgs e)
    {
        _logger.LogTrace("Channel {ChannelName} has come online", e.Channel);
        var changed = _stream?.IsOnline != true;
        _stream = e.Stream.ToTwitchStream(true);

        var eventArgs = new TwitchChannelOnlineStatusChangedEventArgs(_stream);
        StatusChanged?.Invoke(this, eventArgs);
        if (changed)
        {
            OnlineStatusChanged?.Invoke(this, eventArgs);
        }
    }

    private void OnStreamOffline(object? sender, OnStreamOfflineArgs e)
    {
        _logger.LogTrace("Channel {ChannelName} has come offline", e.Channel);
        var changed = _stream?.IsOnline != false;
        _stream = e.Stream.ToTwitchStream(false);

        var eventArgs = new TwitchChannelOnlineStatusChangedEventArgs(_stream);
        StatusChanged?.Invoke(this, eventArgs);
        if (changed)
        {
            OnlineStatusChanged?.Invoke(this, eventArgs);
        }
    }
}
