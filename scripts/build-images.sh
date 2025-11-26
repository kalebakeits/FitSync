#!/bin/bash
set -e

# Configuration
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"

echo "Building FitSync images..."
echo "Registry: $REGISTRY"
echo "Tag: $TAG"
echo ""

# Build from project root
cd "$(dirname "$0")/.."

echo "Building migration image..."
docker build --platform linux/amd64 -t ${REGISTRY}/fitsync-migrate:${TAG} -f src/FitSync.Database/Dockerfile .

echo "Building API image..."
docker build --platform linux/amd64 -t ${REGISTRY}/fitsync-api:${TAG} -f src/FitSync.Api/Dockerfile .

echo "Building GUI image..."
docker build --platform linux/amd64 -t ${REGISTRY}/fitsync-gui:${TAG} -f src/FitSync.Gui/Dockerfile .

echo "Building ZwiftFetcher image..."
docker build --platform linux/amd64 -t ${REGISTRY}/fitsync-zwiftfetcher:${TAG} -f src/FitSync.ZwiftFetcher/Dockerfile .

echo "Building Uploader image..."
docker build --platform linux/amd64 -t ${REGISTRY}/fitsync-garminuploader:${TAG} -f src/FitSync.Uploader/Dockerfile .

echo ""
echo "✅ All images built successfully!"
echo ""
echo "Images:"
echo "  - ${REGISTRY}/fitsync-migrate:${TAG}"
echo "  - ${REGISTRY}/fitsync-api:${TAG}"
echo "  - ${REGISTRY}/fitsync-gui:${TAG}"
echo "  - ${REGISTRY}/fitsync-zwiftfetcher:${TAG}"
echo "  - ${REGISTRY}/fitsync-uploader:${TAG}"
echo ""
echo "To push to registry, run:"
echo "  ./scripts/push-images.sh"
