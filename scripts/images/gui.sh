#!/bin/bash
set -e
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
echo "[gui] Building..."
docker build -t "$REGISTRY/fitsync-gui:$TAG" -f src/FitSync.Gui/Dockerfile .
docker push "$REGISTRY/fitsync-gui:$TAG"
echo "[gui] Done."
