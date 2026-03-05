#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[migrate] Building..."
docker build -t "$REGISTRY/fitsync-migrate:$TAG" -f src/FitSync.Database/Dockerfile .
docker push "$REGISTRY/fitsync-migrate:$TAG"
echo "[migrate] Done."
