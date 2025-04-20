#!/bin/bash
set -e

cd "$(dirname "$0")/.."

CONFIG="Release"
OUTPUT_DIR="./build"
 
echo "🔄 Restoring dependencies..."
dotnet restore Quantum.EventSourcing.sln
 
echo "🔨 Building solution in $(pwd) ..."
dotnet build Quantum.EventSourcing.sln --configuration $CONFIG


echo "📦 Packing..."
dotnet pack Quantum.EventSourcing.sln --configuration $CONFIG --output $OUTPUT_DIR
