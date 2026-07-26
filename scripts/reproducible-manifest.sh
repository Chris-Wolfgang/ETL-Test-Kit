#!/usr/bin/env bash
# Generate reproducible-build-manifest.json (#145): the canonical SHA-256 hashes
# of each shipped assembly, built deterministically the same way a third party
# would reproduce them (the command documented in docs/REPRODUCIBLE-BUILD.md).
#
# Usage: scripts/reproducible-manifest.sh <version> <output-file>
#
# The build uses -c Release -f net10.0 -p:ContinuousIntegrationBuild=true, which
# normalises source paths (/_/) so the output is independent of the checkout
# location — the same knobs reproducible-build.yaml (#135) verifies. The manifest
# records the SDK version and OS it was produced on so a verifier can match the
# reference environment.
set -euo pipefail

VERSION="${1:?usage: reproducible-manifest.sh <version> <output-file>}"
OUT_FILE="${2:?usage: reproducible-manifest.sh <version> <output-file>}"

PROJECTS=(
  "src/Wolfgang.Etl.TestKit/Wolfgang.Etl.TestKit.csproj"
  "src/Wolfgang.Etl.TestKit.Xunit/Wolfgang.Etl.TestKit.Xunit.csproj"
)

TFM="net10.0"
SDK_VERSION="$(dotnet --version)"
OS_NAME="$(uname -s)"

# Collect "assembly<TAB>sha256" lines for each shipped assembly.
hashes=""
for proj in "${PROJECTS[@]}"; do
  dotnet build "$proj" -c Release -f "$TFM" -p:ContinuousIntegrationBuild=true >/dev/null
  dir="$(dirname "$proj")/bin/Release/$TFM"
  asm="$dir/$(basename "$(dirname "$proj")").dll"
  sha="$(sha256sum "$asm" | cut -d' ' -f1)"
  hashes+="$(basename "$asm")	$sha"$'\n'
done

# Assemble the JSON (scripts/reproducible-manifest.py avoids a jq dependency and
# keeps the piped hashes on stdin — a heredoc-inlined script would collide with
# the interpreter's own stdin).
script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
printf '%s' "$hashes" \
  | python3 "$script_dir/reproducible-manifest.py" "$VERSION" "$TFM" "$SDK_VERSION" "$OS_NAME" "$OUT_FILE"
