# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

COPY providers/wahoo/FitSync.Wahoo/FitSync.Wahoo.csproj providers/wahoo/FitSync.Wahoo/
COPY providers/wahoo/FitSync.Wahoo.Shared/FitSync.Wahoo.Shared.csproj providers/wahoo/FitSync.Wahoo.Shared/
COPY shared/FitSync.Shared/FitSync.Shared.csproj shared/FitSync.Shared/
COPY database/FitSync.Database/FitSync.Database.csproj database/FitSync.Database/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore providers/wahoo/FitSync.Wahoo/FitSync.Wahoo.csproj

COPY providers/wahoo/FitSync.Wahoo/ providers/wahoo/FitSync.Wahoo/
COPY providers/wahoo/FitSync.Wahoo.Shared/ providers/wahoo/FitSync.Wahoo.Shared/
COPY shared/FitSync.Shared/ shared/FitSync.Shared/
COPY database/FitSync.Database/ database/FitSync.Database/

WORKDIR /src/providers/wahoo/FitSync.Wahoo
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish FitSync.Wahoo.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FitSync.Wahoo.dll"]
