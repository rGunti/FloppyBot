# Connect Quote Databases from two channels

FloppyBot allows you to connect the quote database from multiple channels together.
For example, this might be useful if you want quotes from your Twitch channel to be available on Discord as well.

!> Note that the channel to be added should not have any quotes as you might run into numbering conflicts otherwise.
Joining two channels with pre-existing quote databases is therefore not supported.

In this guide, we're going to assume you have an existing Twitch channel (let's call it `mytwitch`) and you want your
Discord server to access your quotes. We're also going to assume that FloppyBot is already setup on both the Twitch
channel and Discord server and that there are already some quotes stored.

1. On your Discord server, run `quotejoin Twitch/mytwitch`. FloppyBot will reply with a message like this:

> Join process started. To confirm the link, run the following command in one of your already joined channels:<br>
> `!quoteconfirm Discord/xxxxxxxxxxxxxxxxxx xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`<br>
> Please note that this code will expire in 5 minutes.

2. Go to your Twitch chat and execute the `quoteconfirm` command you got in Step 1. FloppyBot will then confirm the
   connection:

> Join confirmed! `Discord/xxxxxxxxxxxxxxxxxx` now shares quotes with `Twitch/mytwitch`.

3. You can now try to get a quote on your Discord server by running `quote` on your server.

Congratulations! You have connected your Twitch channel and Discord server and are now able to share the quotes over
both platforms.

?> Keep in mind that you can also connect two Twitch accounts or two Discord servers with each other. Additionally you
are not limited to just one connection between two accounts. You can connect as many accounts as you want to.
