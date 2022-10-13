FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV FLOPPYBOT_PROJECT=src/FloppyBot.Chat.Agent/FloppyBot.Chat.Agent.csproj

COPY src/ /src/
RUN dotnet restore ${FLOPPYBOT_PROJECT}

RUN dotnet build --no-restore --configuration Release ${FLOPPYBOT_PROJECT}
RUN dotnet publish --no-restore --no-build --configuration Release -o /out ${FLOPPYBOT_PROJECT}

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
COPY --from=build /out /app
WORKDIR /app
ENTRYPOINT [ "dotnet", "FloppyBot.Chat.Agent.dll" ]
