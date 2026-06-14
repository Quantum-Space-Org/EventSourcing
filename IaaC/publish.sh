#!/bin/bash
set -e

cd "$(dirname "$0")/.."

NUGET_SOURCE="https://nuget.pkg.github.com/Quantum-Space-Org/index.json"

if [ -z "$GITHUB_TOKEN" ]; then
    echo "❌ GITHUB_TOKEN is not set"
    exit 1
fi

echo "🔍 Finding NuGet packages in ./build..."
nupkgs=$(find ./build -name "*.nupkg" -type f | sort -V)

if [ -z "$nupkgs" ]; then
    echo "❌ No .nupkg files found"
    exit 1
fi

echo "📦 Publishing packages:"
echo "$nupkgs"

for PACKAGE in $nupkgs; do
    echo ""
    echo "📤 Publishing: $(basename "$PACKAGE")"
    
    if dotnet nuget push "$PACKAGE" \
        --source "$NUGET_SOURCE" \
        --api-key "$GITHUB_TOKEN" \
        --skip-duplicate; then
        echo "✅ Success: $(basename "$PACKAGE")"
    else
        echo "⚠️ Failed or duplicate: $(basename "$PACKAGE")"
        # Don't exit, continue with other packages
    fi
done

echo ""
echo "✅ Publishing process completed"