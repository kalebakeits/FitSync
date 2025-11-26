FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

# Copy csproj files and restore
COPY src/FitSync.ZwiftFetcher/FitSync.ZwiftFetcher.csproj FitSync.ZwiftFetcher/
COPY src/FitSync.Shared/FitSync.Shared.csproj FitSync.Shared/
COPY src/FitSync.Database/FitSync.Database.csproj FitSync.Database/
COPY src/FitSync.ServiceDefaults/FitSync.ServiceDefaults.csproj FitSync.ServiceDefaults/

RUN dotnet restore FitSync.ZwiftFetcher/FitSync.ZwiftFetcher.csproj

# Copy source
COPY src/FitSync.ZwiftFetcher/ FitSync.ZwiftFetcher/
COPY src/FitSync.Shared/ FitSync.Shared/
COPY src/FitSync.Database/ FitSync.Database/
COPY src/FitSync.ServiceDefaults/ FitSync.ServiceDefaults/

# Build
WORKDIR /src/FitSync.ZwiftFetcher
RUN dotnet build FitSync.ZwiftFetcher.csproj -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish FitSync.ZwiftFetcher.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FitSync.ZwiftFetcher.dll"]
