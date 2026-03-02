# FitSync 🚴‍♂️

Automatically syncs Zwift activities to Garmin Connect so they actually count towards Training Load, VO2 Max, and other metrics. It works by editing Zwift activities to look like they were recorded on a Garmin device before uploading them. Activites are fetched and uploaded in the background so you can just Ride On!

**Live**: [fitsync.kaleba.app](https://fitsync.kaleba.app/) 🌐

![Markdown Logo](./demo.gif)

## Running Locally 💻

**Prerequisites**: .NET 10.0 SDK
```bash
cd FitSync
dotnet run --project src/FitSync.AppHost
```

**Default credentials**: `default` / `default1`

Aspire handles everything — database migrations, service orchestration, test data seeding.

The mock fetcher auto-verifies every user every 30s so you don't need SMTP config.

### Configuration

The mock fetcher runs by default and processes test `.fit` files for the default user.

You can log in and change the Garmin credentials to test against a real Connect account.

Alternatively configure `src/FitSync.Mock/Fetcher/appsettings.json` to auto-populate on startup:
```json
"MockFetcherOptions": {
  "RunFetcher": false,
  "GarminConnectEmail": "your-email@example.com",
  "GarminConnectPassword": "your-password"
}
```

*Use a test account if you do this.*

## Architecture 🏗️

| Service | Description |
|---|---|
| `FitSync.Api` | REST API |
| `FitSync.Gui` | React frontend |
| `FitSync.Zwift/Fetcher` | Polls Zwift for new activities |
| `FitSync.Wahoo/Fetcher` | Receives Wahoo webhook events |
| `FitSync.Garmin/Uploader` | Uploads activities to Garmin Connect |
| `FitSync.Mock/Fetcher` | Dev-only fetcher using local `.fit` test files |
| `FitSync.Database` | EF Core migrations |

Kafka for activity queuing, PostgreSQL for persistence, Kubernetes + Helm for deployment.

## Building & Deploying 🚀

Each service has its own build script so they can run in parallel:

```bash
# Build all images in parallel
REGISTRY=localhost:5000 TAG=latest ./scripts/build.sh

# Or build individually
./scripts/build/api.sh
./scripts/build/zwift-fetcher.sh
./scripts/build/wahoo-fetcher.sh
./scripts/build/garmin-uploader.sh
./scripts/build/gui.sh
./scripts/build/migrate.sh

# Deploy to Kubernetes
./scripts/deploy.sh

# Build and deploy in one step
./scripts/build-and-deploy.sh
```

## Future Work 🚀

- Fetchers for Strava, TrainingPeaks, etc.
- Upload to multiple destinations
- Bi-directional sync
- Overall service health indicator (GUI)

---

PRs welcome. Dev environment is zero-config.