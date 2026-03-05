#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[wahoo-fetcher] Building..."
docker build -t "$REGISTRY/fitsync-wahoo-fetcher:$TAG" -f src/FitSync.Wahoo/Fetcher/Dockerfile .
docker push "$REGISTRY/fitsync-wahoo-fetcher:$TAG"
echo "[wahoo-fetcher] Done."
