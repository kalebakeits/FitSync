#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[purger] Building..."
docker build -t "$REGISTRY/fitsync-purger:$TAG" -f purger/FitSync.Purger/Dockerfile .
docker push "$REGISTRY/fitsync-purger:$TAG"
echo "[purger] Done."
