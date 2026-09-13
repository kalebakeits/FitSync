# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

COPY purger/FitSync.Purger/FitSync.Purger.csproj purger/FitSync.Purger/
COPY shared/FitSync.Shared/FitSync.Shared.csproj shared/FitSync.Shared/
COPY database/FitSync.Database/FitSync.Database.csproj database/FitSync.Database/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore purger/FitSync.Purger/FitSync.Purger.csproj

COPY purger/FitSync.Purger/ purger/FitSync.Purger/
COPY shared/FitSync.Shared/ shared/FitSync.Shared/
COPY database/FitSync.Database/ database/FitSync.Database/

WORKDIR /src/purger/FitSync.Purger
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish FitSync.Purger.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FitSync.Purger.dll"]
