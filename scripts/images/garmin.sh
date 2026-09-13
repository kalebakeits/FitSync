#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[garmin] Building..."
docker build -t "$REGISTRY/fitsync-garmin:$TAG" -f providers/garmin/FitSync.Garmin/Dockerfile .
docker push "$REGISTRY/fitsync-garmin:$TAG"
echo "[garmin] Done."
