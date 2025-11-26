FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

# Install EF Core tools - match your project's EF Core version
RUN dotnet tool install --global dotnet-ef --version 10.0.0-rc.2.25502.107
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy source
COPY src/FitSync.Database/ FitSync.Database/

# Create migration bundle
WORKDIR /src/FitSync.Database
RUN dotnet restore FitSync.Database.csproj
RUN dotnet build FitSync.Database.csproj -c Release
RUN dotnet ef migrations bundle --configuration Release -o /app/efbundle

# Runtime
FROM mcr.microsoft.com/dotnet/nightly/runtime:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=build /app/efbundle .

ENTRYPOINT ["/app/efbundle"]
