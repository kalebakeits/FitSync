# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

RUN dotnet tool install --global dotnet-ef --version 10.0.0-rc.2.25502.107
ENV PATH="${PATH}:/root/.dotnet/tools"

COPY src/FitSync.Database/ FitSync.Database/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore FitSync.Database/FitSync.Database.csproj

WORKDIR /src/FitSync.Database
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet build FitSync.Database.csproj -c Release

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet ef migrations bundle --configuration Release -o /app/efbundle

FROM mcr.microsoft.com/dotnet/nightly/runtime:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=build /app/efbundle .

ENTRYPOINT ["/app/efbundle"]
