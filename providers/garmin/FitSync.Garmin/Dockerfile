# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

COPY providers/garmin/FitSync.Garmin/FitSync.Garmin.csproj providers/garmin/FitSync.Garmin/
COPY providers/garmin/FitSync.Garmin.Shared/FitSync.Garmin.Shared.csproj providers/garmin/FitSync.Garmin.Shared/
COPY shared/FitSync.Shared/FitSync.Shared.csproj shared/FitSync.Shared/
COPY database/FitSync.Database/FitSync.Database.csproj database/FitSync.Database/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore providers/garmin/FitSync.Garmin/FitSync.Garmin.csproj

COPY providers/garmin/FitSync.Garmin/ providers/garmin/FitSync.Garmin/
COPY providers/garmin/FitSync.Garmin.Shared/ providers/garmin/FitSync.Garmin.Shared/
COPY shared/FitSync.Shared/ shared/FitSync.Shared/
COPY database/FitSync.Database/ database/FitSync.Database/

WORKDIR /src/providers/garmin/FitSync.Garmin
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish FitSync.Garmin.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FitSync.Garmin.dll"]
