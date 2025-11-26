#!/bin/bash
set -e

# Configuration
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"

echo "Pushing FitSync images to $REGISTRY..."
echo ""

# Push images
docker push ${REGISTRY}/fitsync-migrate:${TAG}
docker push ${REGISTRY}/fitsync-api:${TAG}
docker push ${REGISTRY}/fitsync-gui:${TAG}
docker push ${REGISTRY}/fitsync-zwiftfetcher:${TAG}
docker push ${REGISTRY}/fitsync-garminuploader:${TAG}

echo ""
echo "✅ All images pushed successfully!"
