#!/bin/bash
set -e

cd "$(dirname "$0")/.."

echo "🔨 Building images..."
./scripts/build.sh

echo ""
echo "🚀 Deploying..."
./scripts/deploy.sh
