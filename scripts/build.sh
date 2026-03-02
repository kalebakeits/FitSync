#!/bin/bash
set -e

REGISTRY="${REGISTRY:-localhost:5000}"
TAG="${TAG:-latest}"

export REGISTRY TAG

echo "Building FitSync images..."
echo "Registry: $REGISTRY"
echo "Tag: $TAG"
echo ""

cd "$(dirname "$0")"

pids=()
scripts=(
    build/migrate.sh
    build/api.sh
    build/gui.sh
    build/zwift-fetcher.sh
    build/garmin-uploader.sh
    build/wahoo-fetcher.sh
)

for script in "${scripts[@]}"; do
    bash "$script" &
    pids+=($!)
done

failed=0
for i in "${!pids[@]}"; do
    if ! wait "${pids[$i]}"; then
        echo "FAILED: ${scripts[$i]}"
        failed=1
    fi
done

if [ $failed -ne 0 ]; then
    echo ""
    echo "One or more builds failed."
    exit 1
fi

echo ""
echo "All images built and pushed successfully."
