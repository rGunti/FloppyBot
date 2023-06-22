FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime
COPY --from=build ./out /app
COPY --from=build ./app-version /app/version
WORKDIR /app
ENTRYPOINT [ "dotnet", "FloppyBot.Aux.MessageCounter.Agent.dll" ]
