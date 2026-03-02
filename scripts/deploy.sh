#!/bin/bash
set -e

REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
NAMESPACE="fitsync"

echo "======================================"
echo "FitSync K8s Deploy"
echo "======================================"
echo "Registry: $REGISTRY"
echo "Tag: $TAG"
echo "Namespace: $NAMESPACE"
echo ""

if ! command -v kubectl &> /dev/null; then
    echo "kubectl not found. Please install kubectl."
    exit 1
fi

if ! command -v helm &> /dev/null; then
    echo "helm not found. Please install helm."
    exit 1
fi

cd "$(dirname "$0")/.."

if [ ! -f "k8s/fitsync/Chart.yaml" ]; then
    echo "Please run this script from the project root directory."
    exit 1
fi

HELM_ARGS=(
    -n "$NAMESPACE"
    --set imageRegistry="$REGISTRY"
    --set migration.tag="$TAG"
    --set api.tag="$TAG"
    --set gui.tag="$TAG"
    --set zwiftFetcher.tag="$TAG"
    --set garminUploader.tag="$TAG"
    --set wahooFetcher.tag="$TAG"
)

if helm list -n "$NAMESPACE" | grep -q "fitsync"; then
    echo "Upgrading existing release..."
    helm upgrade fitsync ./k8s/fitsync "${HELM_ARGS[@]}"
else
    echo "Installing new release..."
    helm install fitsync ./k8s/fitsync "${HELM_ARGS[@]}" --create-namespace
fi

echo ""
echo "Waiting for postgres to be ready..."
kubectl wait --for=condition=ready pod -l app=postgres -n "$NAMESPACE" --timeout=300s

echo ""
echo "Waiting for kafka to be ready..."
kubectl wait --for=condition=ready pod -l app=kafka -n "$NAMESPACE" --timeout=300s

echo ""
echo "Restarting deployments to pull new images..."
kubectl rollout restart deployment api -n "$NAMESPACE"
kubectl rollout restart deployment gui -n "$NAMESPACE"
kubectl rollout restart deployment zwift-fetcher -n "$NAMESPACE"
kubectl rollout restart deployment garmin-uploader -n "$NAMESPACE"
kubectl rollout restart deployment wahoo-fetcher -n "$NAMESPACE"

echo "Waiting for deployments to be ready..."
kubectl rollout status deployment api -n "$NAMESPACE" --timeout=300s
kubectl rollout status deployment gui -n "$NAMESPACE" --timeout=300s
kubectl rollout status deployment zwift-fetcher -n "$NAMESPACE" --timeout=300s
kubectl rollout status deployment garmin-uploader -n "$NAMESPACE" --timeout=300s
kubectl rollout status deployment wahoo-fetcher -n "$NAMESPACE" --timeout=300s

echo ""
echo "Deployment complete."
