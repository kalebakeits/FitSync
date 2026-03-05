# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

COPY src/FitSync.Api/FitSync.Api.csproj FitSync.Api/
COPY src/FitSync.Shared/FitSync.Shared.csproj FitSync.Shared/
COPY src/FitSync.Database/FitSync.Database.csproj FitSync.Database/
COPY src/FitSync.Garmin/Shared/FitSync.Garmin.Shared.csproj FitSync.Garmin/Shared/
COPY src/FitSync.Wahoo/Shared/FitSync.Wahoo.Shared.csproj FitSync.Wahoo/Shared/
COPY src/FitSync.Zwift/Shared/FitSync.Zwift.Shared.csproj FitSync.Zwift/Shared/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore FitSync.Api/FitSync.Api.csproj

COPY src/FitSync.Api/ FitSync.Api/
COPY src/FitSync.Shared/ FitSync.Shared/
COPY src/FitSync.Database/ FitSync.Database/
COPY src/FitSync.Garmin/Shared/ FitSync.Garmin/Shared/
COPY src/FitSync.Wahoo/Shared/ FitSync.Wahoo/Shared/
COPY src/FitSync.Zwift/Shared/ FitSync.Zwift/Shared/

WORKDIR /src/FitSync.Api
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish FitSync.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "FitSync.Api.dll"]
