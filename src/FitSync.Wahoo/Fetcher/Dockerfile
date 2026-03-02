FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

COPY src/FitSync.Wahoo/Fetcher/FitSync.Wahoo.Fetcher.csproj FitSync.Wahoo/Fetcher/
COPY src/FitSync.Wahoo/Shared/FitSync.Wahoo.Shared.csproj FitSync.Wahoo/Shared/
COPY src/FitSync.Shared/FitSync.Shared.csproj FitSync.Shared/
COPY src/FitSync.Database/FitSync.Database.csproj FitSync.Database/

RUN dotnet restore FitSync.Wahoo/Fetcher/FitSync.Wahoo.Fetcher.csproj

COPY src/FitSync.Wahoo/Fetcher/ FitSync.Wahoo/Fetcher/
COPY src/FitSync.Wahoo/Shared/ FitSync.Wahoo/Shared/
COPY src/FitSync.Shared/ FitSync.Shared/
COPY src/FitSync.Database/ FitSync.Database/

WORKDIR /src/FitSync.Wahoo/Fetcher
RUN dotnet build FitSync.Wahoo.Fetcher.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish FitSync.Wahoo.Fetcher.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FitSync.Wahoo.Fetcher.dll"]
