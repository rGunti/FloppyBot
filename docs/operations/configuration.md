# Configuration

FloppyBot uses primarily environment variables prefixed with `FLOPPYBOT_` for configuration. Below you can find a list
of configuration settings you may use to adjust the behavior of FloppyBot, separated by Agent.

## Global

| Variable                             | Default      | Purpose                                          |
|:-------------------------------------|--------------|--------------------------------------------------|
| `FLOPPY_ENV`                         | `Production` | The [environment][dotnet-env] of the application |
| `FLOPPYBOT_ConnectionStrings__Redis` | `localhost`  | Redis Host                                       |
| `FLOPPYBOT_InstanceName`             | `1`          | A name for the agent to be identified by         |

[dotnet-env]: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-7.0

## Chat Agent

| Variable                  | Default | Purpose                                    |
|:--------------------------|---------|--------------------------------------------|
| `FLOPPYBOT_InterfaceType` | n/a     | The interface to use (`Discord`, `Twitch`) |

### Discord

In order to run the Chat Agent in Discord mode, you must set `FLOPPYBOT_InterfaceType=Discord` and have the relevant
access credentials ready. Check the [Discord Developers page][discord-dev] for more information.

| Variable                           | Default | Purpose                                                                                                                                         |
|:-----------------------------------|---------|-------------------------------------------------------------------------------------------------------------------------------------------------|
| `FLOPPYBOT_Discord__ClientId`      | n/a     | Discord application Client ID                                                                                                                   |
| `FLOPPYBOT_Discord__ClientSecret`  | n/a     | Discord application Client secret                                                                                                               |
| `FLOPPYBOT_Discord__Token`         | n/a     | Discord Bot token                                                                                                                               |
| `FLOPPYBOT_Discord__CommandPrefix` | `-`     | Used prefix for commands to be executed, has to be matched with the configuration value from Command Parser to make Discord Slash commands work |

[discord-dev]: https://discord.com/developers

### Twitch

In order to run the Chat Agent in Twitch mode, you must set `FLOPPYBOT_InterfaceType=Twitch` and have the relevant
access credentials ready. You will require a separate Twitch account and a Twitch application in
the [Twitch Developer Console][twitch-dev].

?> **Note**: You may use [Twitch Chat OAuth Password Generator][twitch-oauth-pw-gen] to generate an OAuth token for your
configuration.

| Variable                                        | Default | Purpose                                                                                                                                      |
|:------------------------------------------------|---------|----------------------------------------------------------------------------------------------------------------------------------------------|
| `FLOPPYBOT_Twitch__Username`                    | n/a     | Username of the bot account                                                                                                                  |
| `FLOPPYBOT_Twitch__Token`                       | n/a     | OAuth token of the bot account to join Twitch chat (must start with `oauth:`)                                                                |
| `FLOPPYBOT_Twitch__ClientId`                    | n/a     | The client ID for the Twitch application                                                                                                     |
| `FLOPPYBOT_Twitch__AccessToken`                 | n/a     | The client token for the Twitch application                                                                                                  |
| `FLOPPYBOT_Twitch__Channel`                     | n/a     | The Twitch channel to join                                                                                                                   |
| `FLOPPYBOT_Twitch__DisableWhenChannelIsOffline` | `true`  | When enabled, will not accept any messages from Twitch chat, if the channel is offline (i.e. not streaming) (`true`, `false`)                |
| `FLOPPYBOT_Twitch__MonitorInterval`             | `30`    | An interval in seconds, in which the channel is checked for stream activity (only has an effect, if `DisableWhenChannelIsOffline` is active) |

[twitch-dev]: https://dev.twitch.tv/console/apps

[twitch-oauth-pw-gen]: https://twitchapps.com/tmi/

## Command Parser

| Variable                       | Default | Purpose                                         |
|:-------------------------------|---------|-------------------------------------------------|
| `FLOPPYBOT_CommandPrefixes__0` | `-`     | The prefix to use for commands in chat messages |

?> **Note**: You can configure multiple `CommandPrefixes` by setting multiple environment variables with an increasing
number at the end (i.e. `FLOPPYBOT_CommandPrefixes__1`, `FLOPPYBOT_CommandPrefixes__2`, etc.). You have to ensure that
there is no gap in between the numbers.

!> **Discord Slash Commands**: If you're using the Discord chat agent, make sure that one of the prefixes configured
here is used for `FLOPPYBOT_Discord__CommandPrefix`. Otherwise slash commands may not work.

## Command Executor

| Variable                        | Default | Purpose                                                                              |
|:--------------------------------|---------|--------------------------------------------------------------------------------------|
| `FLOPPYBOT_Currency__ApiKey`    | n/a     | An API key for [Currency API][currency-api] (used by the `currency`/`money` command) |
| `FLOPPYBOT_DeepL__ApiKey`       | n/a     | An API key for [DeepL API][deepl-api] (used by the `translate` command)              |
| `FLOPPYBOT_TwitchApi__ClientId` | n/a     | A client ID for the [Twitch API][twitch-dev] (used by `shoutout` command)            |
| `FLOPPYBOT_TwitchApi__Secret`   | n/a     | A client secret for the [Twitch API][twitch-dev] (used by `shoutout` command)        |

[currency-api]: https://www.currencyconverterapi.com/

[deepl-api]: https://www.deepl.com/pro-api?cta=header-pro-api

## Web API

The Web API uses JWT tokens for authentication. You may use any JWT-based authentication provider. [Auth0][auth0] is
recommended.

| Variable                                           | Default | Purpose                                                                                                            |
|:---------------------------------------------------|---------|--------------------------------------------------------------------------------------------------------------------|
| `FLOPPYBOT_Cors__0` (or `...Cors__1`, `__2`, etc.) | n/a     | one or more addresses that are allowed to use the FloppyBot API (see [here for more information about CORS][cors]) |
| `FLOPPYBOT_Jwt__Authority`                         | n/a     | The JWT authority to accept (for example: `https://example.eu.auth0.com/`)                                         |
| `FLOPPYBOT_Jwt__Audience`                          | n/a     | The JWT audience to accept                                                                                         |

[auth0]: https://auth0.com/

[cors]: https://en.wikipedia.org/wiki/Cross-origin_resource_sharing
