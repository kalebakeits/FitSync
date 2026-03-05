#!/bin/bash
set -e

REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"

export REGISTRY TAG

echo "Building FitSync images..."
echo "Registry: $REGISTRY"
echo "Tag: $TAG"
echo ""

cd "$(dirname "$0")/.."

scripts=(
    scripts/images/migrate.sh
    scripts/images/api.sh
    scripts/images/gui.sh
    scripts/images/zwift-fetcher.sh
    scripts/images/garmin-uploader.sh
    scripts/images/wahoo-fetcher.sh
    scripts/images/purger.sh
)

for script in "${scripts[@]}"; do
    bash "$script"
done

echo ""
echo "All images built and pushed successfully."
