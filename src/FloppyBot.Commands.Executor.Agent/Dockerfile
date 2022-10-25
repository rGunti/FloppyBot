ARG CODE_VERSION=2.0.0
ARG CODE_VERSION_SUFFIX=+noversion

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG CODE_VERSION
ARG CODE_VERSION_SUFFIX
ENV FLOPPYBOT_PROJECT=src/FloppyBot.Commands.Executor.Agent/FloppyBot.Commands.Executor.Agent.csproj

COPY src/ /src/
RUN dotnet restore ${FLOPPYBOT_PROJECT}

RUN dotnet build \
    --no-restore \
    --configuration Release \
    ${FLOPPYBOT_PROJECT}
RUN dotnet publish \
    --no-restore \
    --no-build \
    --configuration Release \
    /p:Version="${CODE_VERSION}" \
    /p:InformationalVersion="${CODE_VERSION}" \
    -o /out \
    ${FLOPPYBOT_PROJECT}
RUN echo "${CODE_VERSION}" > /app-version

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
COPY --from=build /out /app
COPY --from=build /app-version /app/version
WORKDIR /app
ENTRYPOINT [ "dotnet", "FloppyBot.Commands.Executor.Agent.dll" ]
