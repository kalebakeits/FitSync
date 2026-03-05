#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[garmin-uploader] Building..."
docker build -t "$REGISTRY/fitsync-garmin-uploader:$TAG" -f src/FitSync.Garmin/Uploader/Dockerfile .
docker push "$REGISTRY/fitsync-garmin-uploader:$TAG"
echo "[garmin-uploader] Done."
