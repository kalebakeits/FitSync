# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

COPY api/FitSync.Api/FitSync.Api.csproj api/FitSync.Api/
COPY shared/FitSync.Shared/FitSync.Shared.csproj shared/FitSync.Shared/
COPY database/FitSync.Database/FitSync.Database.csproj database/FitSync.Database/
COPY providers/garmin/FitSync.Garmin.Shared/FitSync.Garmin.Shared.csproj providers/garmin/FitSync.Garmin.Shared/
COPY providers/wahoo/FitSync.Wahoo.Shared/FitSync.Wahoo.Shared.csproj providers/wahoo/FitSync.Wahoo.Shared/
COPY providers/zwift/FitSync.Zwift.Shared/FitSync.Zwift.Shared.csproj providers/zwift/FitSync.Zwift.Shared/
COPY source-generators/FitSync.SourceGenerators/FitSync.SourceGenerators.csproj source-generators/FitSync.SourceGenerators/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore api/FitSync.Api/FitSync.Api.csproj

COPY api/FitSync.Api/ api/FitSync.Api/
COPY shared/FitSync.Shared/ shared/FitSync.Shared/
COPY database/FitSync.Database/ database/FitSync.Database/
COPY providers/garmin/FitSync.Garmin.Shared/ providers/garmin/FitSync.Garmin.Shared/
COPY providers/wahoo/FitSync.Wahoo.Shared/ providers/wahoo/FitSync.Wahoo.Shared/
COPY providers/zwift/FitSync.Zwift.Shared/ providers/zwift/FitSync.Zwift.Shared/
COPY source-generators/FitSync.SourceGenerators/ source-generators/FitSync.SourceGenerators/
COPY assets/ assets/

WORKDIR /src/api/FitSync.Api
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish FitSync.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "FitSync.Api.dll"]
