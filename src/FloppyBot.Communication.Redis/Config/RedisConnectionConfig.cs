using StackExchange.Redis;

namespace FloppyBot.Communication.Redis.Config;

public class RedisConnectionConfig
{
    public RedisConnectionConfig(
        ConfigurationOptions options,
        string channel)
    {
        this.Options = options;
        this.Channel = channel;
    }

    public ConfigurationOptions Options { get; init; }
    public string Channel { get; init; }

    public void Deconstruct(
        out ConfigurationOptions configOptions,
        out string channel)
    {
        configOptions = this.Options;
        channel = this.Channel;
    }
}
