#!/bin/bash
set -e

cd "$(dirname "$0")/.."

NUGET_SOURCE="https://nuget.pkg.github.com/Quantum-Space-Org/index.json"
NUGET_SOURCE_NAME="github"

# Check for required token
if [ -z "$GITHUB_TOKEN" ]; then
    echo "❌ GITHUB_TOKEN environment variable is not set"
    echo "💡 Make sure GITHUB_TOKEN is passed to this script"
    exit 1
fi

# Find build directory (could be ./build, ./output, or ./nupkg)
BUILD_DIR=""
for dir in "./build" "./output" "./nupkg" "./artifacts"; do
    if [ -d "$dir" ] && [ -n "$(find "$dir" -name "*.nupkg" -type f 2>/dev/null)" ]; then
        BUILD_DIR="$dir"
        break
    fi
done

if [ -z "$BUILD_DIR" ]; then
    echo "❌ No build directory with .nupkg files found"
    echo "📁 Checked: ./build, ./output, ./nupkg, ./artifacts"
    exit 1
fi

echo "📁 Using build directory: $BUILD_DIR"

# Find all .nupkg files
nupkgs=$(find "$BUILD_DIR" -name "*.nupkg" -type f | sort -V)

if [ -z "$nupkgs" ]; then
    echo "❌ No .nupkg files found in $BUILD_DIR"
    exit 1
fi

echo ""
echo "📦 Found packages to publish:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
for PKG in $nupkgs; do
    echo "  📄 $(basename "$PKG")"
done
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Ensure the NuGet source exists
if ! dotnet nuget list source | grep -q "$NUGET_SOURCE_NAME"; then
    echo "➕ Adding NuGet source: $NUGET_SOURCE_NAME"
    dotnet nuget add source "$NUGET_SOURCE" \
        --name "$NUGET_SOURCE_NAME" \
        --username "Quantum-Space-Org" \
        --password "$GITHUB_TOKEN" \
        --store-password-in-clear-text
fi

# Publish each package
SUCCESS_COUNT=0
FAIL_COUNT=0

for PACKAGE in $nupkgs; do
    PACKAGE_NAME=$(basename "$PACKAGE")
    echo "📤 Publishing: $PACKAGE_NAME"
    
    if dotnet nuget push "$PACKAGE" \
        --source "$NUGET_SOURCE_NAME" \
        --api-key "$GITHUB_TOKEN" \
        --skip-duplicate 2>&1; then
        echo "  ✅ Success: $PACKAGE_NAME"
        ((SUCCESS_COUNT++))
    else
        EXIT_CODE=$?
        if [ $EXIT_CODE -eq 1 ]; then
            echo "  ⚠️  Skipped (likely duplicate): $PACKAGE_NAME"
            ((FAIL_COUNT++))
        else
            echo "  ❌ Failed with exit code $EXIT_CODE: $PACKAGE_NAME"
            ((FAIL_COUNT++))
        fi
    fi
    echo ""
done

# Summary
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📊 Publishing Summary:"
echo "  ✅ Success: $SUCCESS_COUNT"
echo "  ⚠️  Skipped/Failed: $FAIL_COUNT"
echo "  📦 Total: $((SUCCESS_COUNT + FAIL_COUNT))"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ $FAIL_COUNT -gt 0 ] && [ $SUCCESS_COUNT -eq 0 ]; then
    echo "❌ No packages were published successfully"
    exit 1
fi

echo "✅ Publishing process completed"