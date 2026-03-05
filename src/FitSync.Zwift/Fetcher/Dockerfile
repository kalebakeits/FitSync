# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

COPY src/FitSync.Zwift/Fetcher/FitSync.Zwift.Fetcher.csproj FitSync.Zwift/Fetcher/
COPY src/FitSync.Zwift/Shared/FitSync.Zwift.Shared.csproj FitSync.Zwift/Shared/
COPY src/FitSync.Shared/FitSync.Shared.csproj FitSync.Shared/
COPY src/FitSync.Database/FitSync.Database.csproj FitSync.Database/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore FitSync.Zwift/Fetcher/FitSync.Zwift.Fetcher.csproj

COPY src/FitSync.Zwift/Fetcher/ FitSync.Zwift/Fetcher/
COPY src/FitSync.Zwift/Shared/ FitSync.Zwift/Shared/
COPY src/FitSync.Shared/ FitSync.Shared/
COPY src/FitSync.Database/ FitSync.Database/

WORKDIR /src/FitSync.Zwift/Fetcher
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish FitSync.Zwift.Fetcher.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FitSync.Zwift.Fetcher.dll"]
