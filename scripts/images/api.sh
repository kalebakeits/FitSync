#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[api] Building..."
docker build -t "$REGISTRY/fitsync-api:$TAG" -f src/FitSync.Api/Dockerfile .
docker push "$REGISTRY/fitsync-api:$TAG"
echo "[api] Done."
