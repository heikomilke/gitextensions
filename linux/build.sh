#!/usr/bin/env bash
# Build Git Extensions (Linux/Mono port) inside Docker.
# Output: artifacts/bin/GitExtensions/Release/net461/
set -euo pipefail
REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
IMAGE="${GITEXT_BUILD_IMAGE:-gitext-mono-build}"
CONFIG="${GITEXT_CONFIGURATION:-Release}"
NUGET_CACHE="$REPO/.nuget-cache"
mkdir -p "$NUGET_CACHE"

if ! docker image inspect "$IMAGE" >/dev/null 2>&1; then
  echo "==> building Docker image $IMAGE"
  docker build -t "$IMAGE" -f "$REPO/linux/Dockerfile" "$REPO/linux"
fi

# Plugins that can be built on Linux (TFS integration needs Visual Studio assemblies).
PLUGINS=$(cd "$REPO" && find Plugins -name '*.csproj' -not -path '*/GitUIPluginInterfaces/*' -not -name 'Tfs*' | sort | tr '\n' ' ')
MSBUILD_ARGS="/p:Configuration=$CONFIG /p:TreatWarningsAsErrors=false /p:EnableStyleCopAnalyzers=false -nologo -v:m -m"

echo "==> msbuild (restore + build) in $IMAGE"
docker run --rm \
  -v "$REPO:/src" \
  -v "$NUGET_CACHE:/root/.nuget/packages" \
  -w /src "$IMAGE" sh -c "
    git config --global --add safe.directory '*'
    for p in $PLUGINS; do msbuild /restore \$p $MSBUILD_ARGS; done
    msbuild /restore GitExtensions/GitExtensions.csproj $MSBUILD_ARGS
  "

"$REPO/linux/extract-facades.sh"
echo "==> done: $REPO/artifacts/bin/GitExtensions/$CONFIG/net461"
