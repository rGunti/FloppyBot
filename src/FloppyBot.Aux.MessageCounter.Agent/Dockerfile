FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
COPY ./out /app
COPY ./app-version /app/version
WORKDIR /app
ENTRYPOINT [ "dotnet", "FloppyBot.Aux.MessageCounter.Agent.dll" ]
