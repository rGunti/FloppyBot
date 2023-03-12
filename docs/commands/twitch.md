# Twitch Commands

!> These commands are limited to Twitch and are not available on other platforms.

## Channel Shoutouts

### `shoutout`

_Alias: `so`_

Allows you to shoutout another Twitch channel. 
Note that you will need to setup a shoutout message using `setshoutout`
(or the Admin Console) for this to work.

Syntax: `shoutout <Channel Name>`

```
> User:      so lazysoph
< FloppyBot: Check out lazysoph at https://twitch.tv/lazysoph!
```

### `setshoutout`

Sets a shoutout message template for your channel. The message can contain the following placeholders:

- `{AccountName}`: The name of the Twitch channel
- `{DisplayName}`: The display name of the Twitch channel (includes capitalization)
- `{LastGame}`: The last game the channel broadcasted
- `{Link}`: Link to the Twitch channel

Syntax: `setshoutout <Message>`

```
Example:
> setshoutout Check out {DisplayName} at {Link}! They last played {LastGame}, which surely was great!
```

### `clearshoutout`

Clears the shoutout message, effecifely disabling the shoutout command.
