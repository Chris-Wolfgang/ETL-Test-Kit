#!/usr/bin/env python3
"""Assemble reproducible-build-manifest.json (#145) from "assembly<TAB>sha256"
lines on stdin plus build metadata.

Usage: reproducible-manifest.py <version> <tfm> <sdk> <os> <output-file>
"""
import json
import sys


def main() -> None:
    if len(sys.argv) != 6:
        print("usage: reproducible-manifest.py <version> <tfm> <sdk> <os> <output-file>", file=sys.stderr)
        sys.exit(2)

    version, tfm, sdk, os_name, out_file = sys.argv[1:6]

    assemblies = []
    for line in sys.stdin.read().splitlines():
        if not line.strip():
            continue
        name, sha = line.split("\t")
        assemblies.append({"assembly": name, "sha256": sha})

    manifest = {
        "schema": "wolfgang.reproducible-build-manifest/v1",
        "version": version,
        "targetFramework": tfm,
        "buildCommand": f"dotnet build <project> -c Release -f {tfm} -p:ContinuousIntegrationBuild=true",
        "referenceEnvironment": {"os": os_name, "dotnetSdk": sdk},
        "assemblies": sorted(assemblies, key=lambda a: a["assembly"]),
    }

    with open(out_file, "w", encoding="utf-8", newline="\n") as handle:
        json.dump(manifest, handle, indent=2)
        handle.write("\n")

    print(f"Wrote {len(assemblies)} assembly hashes to {out_file}")


if __name__ == "__main__":
    main()
