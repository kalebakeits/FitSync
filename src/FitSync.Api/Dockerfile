FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

# Copy csproj files and restore
COPY src/FitSync.Api/FitSync.Api.csproj FitSync.Api/
COPY src/FitSync.Shared/FitSync.Shared.csproj FitSync.Shared/
COPY src/FitSync.Database/FitSync.Database.csproj FitSync.Database/
COPY src/FitSync.ServiceDefaults/FitSync.ServiceDefaults.csproj FitSync.ServiceDefaults/

RUN dotnet restore FitSync.Api/FitSync.Api.csproj

# Copy source
COPY src/FitSync.Api/ FitSync.Api/
COPY src/FitSync.Shared/ FitSync.Shared/
COPY src/FitSync.Database/ FitSync.Database/
COPY src/FitSync.ServiceDefaults/ FitSync.ServiceDefaults/

# Build
WORKDIR /src/FitSync.Api
RUN dotnet build FitSync.Api.csproj -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish FitSync.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "FitSync.Api.dll"]
