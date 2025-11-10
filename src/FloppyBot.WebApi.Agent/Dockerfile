FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
COPY ./out /app
COPY ./app-version /app/version
WORKDIR /app
ENTRYPOINT [ "dotnet", "FloppyBot.WebApi.Agent.dll" ]
