# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
make restore      # Restore NuGet dependencies
make build        # Build solution
make test         # Run all tests
make check        # Validate code style (csharpier + dotnet format)
make fix          # Auto-format code
```

Run a single test project:
```bash
dotnet test src/FloppyBot.Commands.Aux.Twitch.Tests/FloppyBot.Commands.Aux.Twitch.Tests.csproj
```

Filter by test name:
```bash
dotnet test --filter "ClassName=ShoutoutCommandTests" src/FloppyBot.Commands.Aux.Twitch.Tests
```

## Architecture

FloppyBot is a multi-platform streaming chat bot (Twitch, Discord). It uses an **agent-based microservice architecture** where independent processes communicate via **Redis pub/sub**.

### Message Flow

```
Chat Platform (Twitch/Discord)
        ↓ raw messages
Chat Agent  →  Redis  →  Command Parser Agent  →  Redis  →  Command Executor Agent
                                                                      ↓
                                                             MongoDB (storage)
                                                                      ↓
                                                              Redis  →  Chat Agent  →  Platform (replies)
```

### Agents

Each agent is an independent process (`BackgroundService`) in its own project under `src/`:

| Agent | Purpose |
|-------|---------|
| `FloppyBot.Chat.Agent` | Connects to chat platforms, routes messages in/out |
| `FloppyBot.Commands.Parser.Agent` | Parses raw `ChatMessage` → `CommandInstruction` |
| `FloppyBot.Commands.Executor.Agent` | Executes commands, enforces guards/privileges |
| `FloppyBot.WebApi.Agent` | ASP.NET Core REST API + SignalR hub |
| `FloppyBot.Auxiliary.*` agents | Message counter, Twitch alerts, etc. |

### Key Abstractions

- **`INotificationSender<T>` / `INotificationReceiver<T>`** — pub/sub transport (Redis-backed in prod, mock in tests)
- **`IRepository<TEntity>`** / **`IRepositoryFactory`** — storage (MongoDB in prod, LiteDb option available)
- **`ICommandGuard`** / **`ICommandGuardRegistry`** — privilege enforcement for command execution
- **`IAuditor`** — records changes as `AuditRecord` entities

### Project Layout

- `FloppyBot.Base.*` — Shared utilities: storage, logging, DI extensions, encryption, cron
- `FloppyBot.Chat.*` — Chat abstractions + Twitch/Discord/Console implementations
- `FloppyBot.Commands.Core` — Command execution framework (executor, guards, config)
- `FloppyBot.Commands.Aux.*` — Built-in commands (shoutout, currency, quotes, timer, translate, etc.)
- `FloppyBot.Commands.Custom.*` — User-defined command support
- `FloppyBot.Communication.Redis` — Redis pub/sub implementation

### DI Registration Pattern

All modules expose extension methods on `IServiceCollection`. Agents compose these at startup:

```csharp
services
    .AddRedisCommunication(config)
    .AddMongoDbStorage(config)
    .AddCommandExecutor()
    .AddGuards();
```

### Command Executor Plugin Loading

The executor agent uses `AssemblyPreloader.LoadAssembliesFromDirectory()` to discover command plugins at runtime via reflection — no compile-time reference required.

## Tech Stack

- **.NET 10 / C#** with nullable reference types and implicit usings
- **Redis** — inter-agent messaging (StackExchange.Redis)
- **MongoDB** — persistent storage
- **MSTest + FakeItEasy** — testing
- **Serilog** — structured logging
- **Csharpier + StyleCop** — code style enforcement (CI blocks non-conforming PRs)

## Additional remarks

- Do not make any changes until you have 95% confidence in what you need to build. Ask me follow-up questions until you reach that confidence.
