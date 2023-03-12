# Host your own FloppyBot

To host FloppyBot yourself, you require:
- a server that can run Docker
- a MongoDB database server
- a Redis server
- (optional) a Reverse Proxy to serve the Web API and frontend securely
- (required if Web API is used) a JWT authentication provider (i.e. [Auth0][auth0])

[auth0]: https://auth0.com

## Agents

FloppyBot is divided into multiple parts ("microservices") called "Agents". Agents perform various tasks to make FloppyBot work. Currently, FloppyBot has 5 agents:

- Chat Agent: This agent connects to a single platform and serves messages back and forth. Depending on the configuration, it may for example connect to a certain Twitch channel and only serve this channel, while Discord is served by a single instance.
- Command Parser: This agent analyses incoming messages for possible commands. If a command is found, it issues a control message that can be picked up by other agents to act upon.
- Command Executor: This agent receives parsed command messages from the Command Parser and executes them. This is _The Core_ of FloppyBot.
- Message Counter: This agent receives messages from Chat Agents and keeps count. This is used by other features.
- Web API: This agent serves a Web API for the FloppyBot Admin Console and is connected to the other services. Certain commands can interact with the Web API, i.e. sound commands, which have to be executed in a browser running on the streamers computer to work.

## Docker Compose

The easiest way to get FloppyBot running is to use a Docker Compose file. This not only makes it easy to spin FloppyBot up and down, it also allows your configuration to be reproducible. Below you can find an example for a FloppyBot cluster:

```yml
version: '3'

services:
  # - Base Infrastructure
  # -- Reverse Proxy
  reverse-proxy:
    image: traefik:${VERSION_TRAEFIK}
    restart: always
    ports:
      - "80:80"
      - "443:443"
      # The Web UI
      #- "8080:8080"
    volumes:
      - ./traefik:/etc/traefik:ro
      - v_traefik_certs:/certs
      # So that Traefik can listen to the Docker events
      - /var/run/docker.sock:/var/run/docker.sock

  # -- MongoDB
  mongodb:
    container_name: bot_db
    image: mongo:${VERSION_MONGODB}
    restart: always
    ports:
      - 27017:27017 # Port protected through external firewall
    env_file:
      - ./db/mongo.env
    volumes:
      - v_mongodb_data:/data/db

  # -- Redis
  redis:
    container_name: bot_redis
    image: redis:${VERSION_REDIS}
    restart: always
    ports:
      - 6379:6379 # Port protected through external firewall

  # - FloppyBot
  # -- Web UI
  bot_control_panel:
    container_name: bot_control_panel
    image: registry.gitlab.com/rgunti/pinsrbot-admin-console:${VERSION_FLOPPYBOT_UI}
    restart: always
    env_file:
      - ./bot/ui.env
    volumes:
      - ./logging/config.json:/app/appsettings.Production.json:ro
  # -- Web API
  agent-web-api:
    container_name: bot_web_api
    image: floppybot/web-api:${VERSION_FLOPPYBOT}
    restart: always
    depends_on:
      - mongodb
      - redis
    volumes:
      - ./logging/config.json:/app/appsettings.Production.json:ro
    env_file:
      - ./bot/base.env
      - ./bot/db.env
      - ./bot/redis.env
      - ./bot/api.env
    environment:
      FLOPPYBOT_InstanceName: api-1

  # -- Chat Agents
  # --- Discord
  agent-chat-discord:
    container_name: bot_chat_agent_discord
    image: floppybot/chat-agent:${VERSION_FLOPPYBOT}
    restart: always
    depends_on:
      - redis
      - agent-command-executor
    volumes:
      - ./logging/config.json:/app/appsettings.Production.json:ro
    env_file:
      - ./bot/base.env
      - ./bot/redis.env
      - ./bot/chat-discord.env
    environment:
      FLOPPYBOT_InstanceName: discord-1
  # --- Twitch
  # ---- floppypanda
  agent-chat-twitch-floppypandach:
    container_name: bot_chat_agent_twitch_floppypandach
    image: floppybot/chat-agent:${VERSION_FLOPPYBOT}
    restart: always
    depends_on:
      - redis
    volumes:
      - ./logging/config.json:/app/appsettings.Production.json:ro
    env_file:
      - ./bot/base.env
      - ./bot/redis.env
      - ./bot/chat-twitch.env
    environment:
      FLOPPYBOT_Twitch__Channel: floppypandach
      FLOPPYBOT_InstanceName: twitch-floppypandach

  # -- Executor
  agent-command-executor:
    container_name: bot_command_executor
    image: floppybot/command-executor:${VERSION_FLOPPYBOT}
    restart: always
    depends_on:
      - mongodb
      - redis
    volumes:
      - ./logging/config.json:/app/appsettings.Production.json:ro
    env_file:
      - ./bot/base.env
      - ./bot/db.env
      - ./bot/redis.env
      - ./bot/executor.env
    environment:
      FLOPPYBOT_InstanceName: executor-1

  # -- Helper Agents
  agent-command-parser:
    container_name: bot_command_parser
    image: floppybot/command-parser:${VERSION_FLOPPYBOT}
    restart: always
    depends_on:
      - redis
    volumes:
      - ./logging/config.json:/app/appsettings.Production.json:ro
    env_file:
      - ./bot/base.env
      - ./bot/redis.env
      - ./bot/parser.env
    environment:
      FLOPPYBOT_InstanceName: parser-1
  agent-message-counter:
    container_name: bot_message_counter
    image: floppybot/message-counter:${VERSION_FLOPPYBOT}
    restart: always
    depends_on:
      - mongodb
      - redis
    volumes:
      - ./logging/config.json:/app/appsettings.Production.json:ro
    env_file:
      - ./bot/base.env
      - ./bot/db.env
      - ./bot/redis.env
    environment:
      FLOPPYBOT_InstanceName: message-counter-1

volumes:
  v_traefik_certs: {}
  v_mongodb_data: {}
```

## Folder Structure and `.env` files

You may have noticed that this `docker-compose.yaml` file references several `.env` files. This is to ensure that this file doesn't contain any secrets.
Additionally, versions are collected in environment variables, which you can also control with a root-level `.env` file. Below, you can find an example of how to setup a project folder for your `docker-compose.yaml` file:

```
floppybot
├── bot
│   ├── api.env
│   ├── base.env
│   ├── chat-discord.env
│   ├── chat-twitch.env
│   ├── db.env
│   ├── executor.env
│   ├── parser.env
│   ├── redis.env
│   └── ui.env
├── db
│   └── mongo.env
├── docker-compose.yaml
├── .env
├── logging
│   └── config.json
└── traefik
    ├── reverse-proxy.yaml
    └── traefik.yaml
```

The root-level `.env` contains all the container versions. This makes updating components easier later.

```env
VERSION_TRAEFIK=v2.9
VERSION_MONGODB=5.0-focal
VERSION_REDIS=7.0-alpine
VERSION_FLOPPYBOT=2.2303.11-beta01
VERSION_FLOPPYBOT_UI=2023.2.2
```

?> Configuring other aspects, like MongoDB or Traefik Reverse Proxy, goes beyond the scope of this article.

## FloppyBot Configuration

FloppyBot uses Environment variables (and JSON files) for configuration. It is recommended to put your values in `.env` files as overriding JSON-based configuration can lead to issues during runtime. See [Configuration](operations/configuration.md) for more details about the available configuration values.
