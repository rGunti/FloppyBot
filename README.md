# FloppyBot

[![FloppyBot][img-floppybot]][gh-floppybot]
[![FloppyBot][img-floppybot-ac]][floppybot-ac]
[![FloppyBot][img-floppybot-ss]][floppybot-ss]

---

[![BSD-3-Clause](https://img.shields.io/github/license/rGunti/FloppyBot)](https://github.com/rGunti/FloppyBot/blob/master/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/rGunti/FloppyBot)](https://github.com/rGunti/FloppyBot/issues)
[![GitHub pull requests](https://img.shields.io/github/issues-pr/rGunti/FloppyBot.svg?style=flat)](https://github.com/rGunti/FloppyBot/pulls)
[![GitHub Workflow Status (with event)](https://img.shields.io/github/actions/workflow/status/rGunti/FloppyBot/.github%2Fworkflows%2Fbuild-test.yml?event=push)](https://github.com/rGunti/FloppyBot/actions/workflows/build-test.yml)
[![GitHub stars](https://img.shields.io/github/stars/rGunti/FloppyBot.svg?style=social&label=Stars&style=plastic)]()
[![GitHub watchers](https://img.shields.io/github/watchers/rGunti/FloppyBot.svg?style=social&label=Watch&style=plastic)]()
[![GitHub forks](https://img.shields.io/github/forks/rGunti/FloppyBot.svg?style=social&label=Fork&style=plastic)]()

---

FloppyBot is an open-source, multi-platform chat bot. It was originally designed for the Twitch
streamer [pinsrlTrex](https://twitch.tv/pinsrltrex) but has since been extended to more users.

> You can also find this project on Codeberg: [FloppyBot/FloppyBot][cb-floppybot] ([FloppyBot Organisation][cb-org])

---

## Other repositories

FloppyBot consists of multiple components:

- Backend (*this repository*)
- [Admin Console](https://github.com/rGunti/FloppyBot-AdminConsole): A web UI for users to manage FloppyBot
- [StreamSource](https://github.com/rGunti/FloppyBot-StreamSource): Browser source designed for OBS
- [Legacy Admin Console](https://gitlab.com/rGunti/pinsrbot-admin-console): The legacy frontend for FloppyBot, to be replaced by the new FloppyBot Admin Console

---

## Currently available agents

- ![Chat Agent Version](https://img.shields.io/docker/v/floppybot/chat-agent?logo=docker&label=Chat%20Agent)
  ![Chat Agent Image Size](https://img.shields.io/docker/image-size/floppybot/chat-agent/latest)
- ![Command Executor Version](https://img.shields.io/docker/v/floppybot/command-executor?logo=docker&label=Command%20Executor)
  ![Command Executor Image Size](https://img.shields.io/docker/image-size/floppybot/command-executor/latest)
- ![Command Parser Version](https://img.shields.io/docker/v/floppybot/command-parser?logo=docker&label=Command%20Parser)
  ![Command Parser Image Size](https://img.shields.io/docker/image-size/floppybot/command-parser/latest)
- ![Message Counter Version](https://img.shields.io/docker/v/floppybot/message-counter?logo=docker&label=Message%20Counter)
  ![Message Counter Image Size](https://img.shields.io/docker/image-size/floppybot/message-counter/latest)
- ![Twitch Alerts Version](https://img.shields.io/docker/v/floppybot/twitch-alerts?logo=docker&label=Twitch%20Alert%20Agent)
  ![Twitch Alerts Image Size](https://img.shields.io/docker/image-size/floppybot/twitch-alerts/latest)
- ![Web API Version](https://img.shields.io/docker/v/floppybot/web-api?logo=docker&label=Web%20API%20Agent)
  ![Web API Image Size](https://img.shields.io/docker/image-size/floppybot/web-api/latest)

- ![FloppyBot Admin Console Version](https://img.shields.io/docker/v/floppybot/admin-console?logo=docker&label=FloppyBot%20Admin%20Console)
  ![FloppyBot Admin Console Image Size](https://img.shields.io/docker/image-size/floppybot/admin-console/main)

---

[gh-floppybot]: https://github.com/rgunti/floppybot
[cb-floppybot]: https://codeberg.org/FloppyBot/FloppyBot
[cb-org]: https://codeberg.org/FloppyBot
[floppybot-ac]: https://github.com/rGunti/FloppyBot-AdminConsole
[floppybot-ss]: https://github.com/rGunti/FloppyBot-StreamSource
[img-floppybot]: https://img.shields.io/badge/FloppyBot-blue?logo=.net
[img-floppybot-ac]: https://img.shields.io/badge/Admin_Console-gray?logo=googlechrome
[img-floppybot-ss]: https://img.shields.io/badge/Stream_Source-gray?logo=obsstudio
