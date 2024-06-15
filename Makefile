restore:
	dotnet restore src/FloppyBot.sln
	dotnet tool restore

check: restore
	dotnet csharpier --check src/
	dotnet format style --verify-no-changes src/
	dotnet format analyzers --verify-no-changes src/

fix: restore
	dotnet csharpier src/
	dotnet format style src/
	dotnet format analyzers src/

format: restore
	dotnet csharpier src/
	dotnet format style src/
	dotnet format analyzers src/

build: restore
	dotnet build --no-restore src/FloppyBot.sln

test: build
	dotnet test --no-restore --no-build src/FloppyBot.sln
