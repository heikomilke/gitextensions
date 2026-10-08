#!/usr/bin/env bash
# Some distro Mono packages (e.g. Ubuntu's mono-libraries 6.14) ship without the
# .NET facade assemblies and netstandard.dll that Git Extensions' dependencies need.
# This copies them from the official mono:6.12 image into artifacts/mono-facades,
# where linux/run.sh puts them on MONO_PATH.
set -euo pipefail
REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DEST="$REPO/artifacts/mono-facades"
if [ -f "$DEST/netstandard.dll" ]; then
  echo "==> facades already present in $DEST"
  exit 0
fi
mkdir -p "$DEST"
echo "==> extracting Mono facade assemblies into $DEST"
docker run --rm -v "$DEST:/out" mono:6.12 sh -c \
  'cp /usr/lib/mono/4.5/Facades/*.dll /out/ && cp /usr/lib/mono/4.5/netstandard.dll /out/ && chown -R '"$(id -u):$(id -g)"' /out'
