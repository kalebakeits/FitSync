#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[zwift-fetcher] Building..."
docker build -t "$REGISTRY/fitsync-zwift-fetcher:$TAG" -f src/FitSync.Zwift/Fetcher/Dockerfile .
docker push "$REGISTRY/fitsync-zwift-fetcher:$TAG"
echo "[zwift-fetcher] Done."
