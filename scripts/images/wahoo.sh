#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[wahoo] Building..."
docker build -t "$REGISTRY/fitsync-wahoo:$TAG" -f providers/wahoo/FitSync.Wahoo/Dockerfile .
docker push "$REGISTRY/fitsync-wahoo:$TAG"
echo "[wahoo] Done."
