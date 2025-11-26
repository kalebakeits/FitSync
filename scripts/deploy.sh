#!/bin/bash
set -e

# Configuration
REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"
NAMESPACE="fitsync"

echo "======================================"
echo "FitSync K8s Deployment"
echo "======================================"
echo "Registry: $REGISTRY"
echo "Tag: $TAG"
echo "Namespace: $NAMESPACE"
echo ""

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "❌ kubectl not found. Please install kubectl."
    exit 1
fi

# Check if helm is available
if ! command -v helm &> /dev/null; then
    echo "❌ helm not found. Please install helm."
    exit 1
fi

# Check if we're in the project root
if [ ! -f "k8s/fitsync/Chart.yaml" ]; then
    echo "❌ Please run this script from the project root directory."
    exit 1
fi

echo "📦 Step 1: Building Docker images..."
./scripts/build-images.sh

echo ""
echo "📤 Step 2: Pushing images to registry..."
./scripts/push-images.sh

echo ""
echo "🚀 Step 3: Deploying to K8s with Helm..."

# Check if release exists
if helm list -n $NAMESPACE | grep -q "fitsync"; then
    echo "Upgrading existing release..."
    helm upgrade fitsync ./k8s/fitsync \
        -n $NAMESPACE \
        --set imageRegistry=$REGISTRY \
        --set migration.tag=$TAG \
        --set api.tag=$TAG \
        --set gui.tag=$TAG \
        --set zwiftFetcher.tag=$TAG \
        --set uploader.tag=$TAG
else
    echo "Installing new release..."
    helm install fitsync ./k8s/fitsync \
        -n $NAMESPACE \
        --create-namespace \
        --set imageRegistry=$REGISTRY \
        --set migration.tag=$TAG \
        --set api.tag=$TAG \
        --set gui.tag=$TAG \
        --set zwiftFetcher.tag=$TAG \
        --set uploader.tag=$TAG
fi

echo ""
echo "⏳ Waiting for postgres to be ready..."
kubectl wait --for=condition=ready pod -l app=postgres -n $NAMESPACE --timeout=300s

echo ""
echo "⏳ Waiting for kafka to be ready..."
kubectl wait --for=condition=ready pod -l app=kafka -n $NAMESPACE --timeout=300s

echo ""
echo "🔄 Step 4: Restarting deployments to pull new images..."
kubectl rollout restart deployment api -n $NAMESPACE
kubectl rollout restart deployment gui -n $NAMESPACE
kubectl rollout restart deployment zwiftfetcher -n $NAMESPACE
kubectl rollout restart deployment uploader -n $NAMESPACE

echo "Waiting for deployments to be ready..."
kubectl rollout status deployment api -n $NAMESPACE --timeout=300s
kubectl rollout status deployment gui -n $NAMESPACE --timeout=300s
kubectl rollout status deployment zwiftfetcher -n $NAMESPACE --timeout=300s
kubectl rollout status deployment uploader -n $NAMESPACE --timeout=300s

echo ""
echo "✅ Deployment complete!"
echo ""
echo "Next steps:"
echo "  1. Check pod status: kubectl get pods -n $NAMESPACE"
echo "  2. View logs: kubectl logs -n $NAMESPACE -l app=zwiftfetcher --tail=50"
echo "  3. Seed database with your Zwift credentials (see DEPLOYMENT.md)"
echo ""
echo "To access postgres:"
echo "  kubectl exec -it -n $NAMESPACE postgres-0 -- psql -U postgres -d FitSync"
