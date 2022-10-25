FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV FLOPPYBOT_PROJECT=src/FloppyBot.Commands.Parser.Agent/FloppyBot.Commands.Parser.Agent.csproj

RUN wget -O /semver.sh https://raw.githubusercontent.com/cloudflare/semver_bash/master/semver.sh

COPY version /app-version
COPY scripts/determine-version.sh /version.sh

RUN bash /version.sh

COPY src/ /src/

RUN dotnet restore ${FLOPPYBOT_PROJECT}
RUN dotnet build \
    --no-restore \
    --configuration Release \
    /p:Version=$(cat /app-short-version | xargs) \
    /p:InformationalVersion=$(cat /app-version | xargs) \
    ${FLOPPYBOT_PROJECT}
RUN dotnet publish \
    --no-restore \
    --no-build \
    --configuration Release \
    -o /out \
    ${FLOPPYBOT_PROJECT}

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
COPY --from=build /out /app
COPY --from=build /app-version /app/version
WORKDIR /app
ENTRYPOINT [ "dotnet", "FloppyBot.Commands.Parser.Agent.dll" ]
