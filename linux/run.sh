#!/usr/bin/env bash
# Run the Linux/Mono build of Git Extensions.
# Settings live in an own profile dir (default: ~/.config/gitextensions-linux) so an
# existing Git Extensions 2.x installation is left untouched. Override with GITEXT_PROFILE.
set -euo pipefail
REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONFIG="${GITEXT_CONFIGURATION:-Release}"
OUT="$REPO/artifacts/bin/GitExtensions/$CONFIG/net461"
PROFILE="${GITEXT_PROFILE:-$HOME/.config/gitextensions-linux}"
mkdir -p "$PROFILE/config" "$PROFILE/data"
export MONO_PATH="$REPO/artifacts/mono-facades${MONO_PATH:+:$MONO_PATH}"
export XDG_CONFIG_HOME="$PROFILE/config"
export XDG_DATA_HOME="$PROFILE/data"
cd "$OUT" && exec mono GitExtensions.exe "$@"
