FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
COPY ./out /app
COPY ./app-version /app/version
WORKDIR /app
ENTRYPOINT [ "dotnet", "FloppyBot.Commands.Parser.Agent.dll" ]
