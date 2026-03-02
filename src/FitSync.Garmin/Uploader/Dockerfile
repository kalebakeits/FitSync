FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0.100-alpine3.22 AS build
WORKDIR /src

COPY src/FitSync.Garmin/Uploader/FitSync.Garmin.Uploader.csproj FitSync.Garmin/Uploader/
COPY src/FitSync.Garmin/Shared/FitSync.Garmin.Shared.csproj FitSync.Garmin/Shared/
COPY src/FitSync.Shared/FitSync.Shared.csproj FitSync.Shared/
COPY src/FitSync.Database/FitSync.Database.csproj FitSync.Database/

RUN dotnet restore FitSync.Garmin/Uploader/FitSync.Garmin.Uploader.csproj

COPY src/FitSync.Garmin/Uploader/ FitSync.Garmin/Uploader/
COPY src/FitSync.Garmin/Shared/ FitSync.Garmin/Shared/
COPY src/FitSync.Shared/ FitSync.Shared/
COPY src/FitSync.Database/ FitSync.Database/

WORKDIR /src/FitSync.Garmin/Uploader
RUN dotnet build FitSync.Garmin.Uploader.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish FitSync.Garmin.Uploader.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0.0-alpine3.22 AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FitSync.Garmin.Uploader.dll"]
