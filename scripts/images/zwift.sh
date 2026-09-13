#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[zwift] Building..."
docker build -t "$REGISTRY/fitsync-zwift:$TAG" -f providers/zwift/FitSync.Zwift/Dockerfile .
docker push "$REGISTRY/fitsync-zwift:$TAG"
echo "[zwift] Done."
