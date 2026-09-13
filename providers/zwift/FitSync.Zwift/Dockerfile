# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

COPY providers/zwift/FitSync.Zwift/FitSync.Zwift.csproj providers/zwift/FitSync.Zwift/
COPY providers/zwift/FitSync.Zwift.Shared/FitSync.Zwift.Shared.csproj providers/zwift/FitSync.Zwift.Shared/
COPY shared/FitSync.Shared/FitSync.Shared.csproj shared/FitSync.Shared/
COPY database/FitSync.Database/FitSync.Database.csproj database/FitSync.Database/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore providers/zwift/FitSync.Zwift/FitSync.Zwift.csproj

COPY providers/zwift/FitSync.Zwift/ providers/zwift/FitSync.Zwift/
COPY providers/zwift/FitSync.Zwift.Shared/ providers/zwift/FitSync.Zwift.Shared/
COPY shared/FitSync.Shared/ shared/FitSync.Shared/
COPY database/FitSync.Database/ database/FitSync.Database/

WORKDIR /src/providers/zwift/FitSync.Zwift
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish FitSync.Zwift.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FitSync.Zwift.dll"]
