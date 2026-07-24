#!/usr/bin/env python3
"""Compute the Stryker mutation score from mutation-report.json files and emit a
github-action-benchmark `customBiggerIsBetter` payload for the gh-pages trend (#124).

Aggregates every mutation-report.json under the given root (a repo may have more
than one test project's report), applying Stryker's own score formula:

    score = (Killed + Timeout) / (Killed + Timeout + Survived + NoCoverage) * 100

Ignored / CompileError mutants are excluded from the denominator, exactly as
Stryker does when it prints "The final mutation score".

Usage: python scripts/stryker-score.py <search-root> <output-json>
"""
import glob
import json
import os
import sys
from collections import Counter

DETECTED = ("Killed", "Timeout")
UNDETECTED = ("Survived", "NoCoverage")


def compute(root: str) -> float:
    counts: Counter = Counter()
    reports = glob.glob(os.path.join(root, "**", "mutation-report.json"), recursive=True)
    if not reports:
        raise SystemExit(f"No mutation-report.json found under {root!r}")
    for path in reports:
        with open(path, encoding="utf-8") as handle:
            data = json.load(handle)
        for file_data in data.get("files", {}).values():
            for mutant in file_data.get("mutants", []):
                counts[mutant["status"]] += 1

    detected = sum(counts[s] for s in DETECTED)
    denom = detected + sum(counts[s] for s in UNDETECTED)
    score = round(100 * detected / denom, 2) if denom else 0.0
    print(f"Mutation status counts: {dict(counts)}")
    print(f"Mutation score: {score}% (from {len(reports)} report(s))")
    return score


def main(root: str, out_file: str) -> None:
    score = compute(root)
    payload = [{"name": "Mutation score", "unit": "%", "value": score}]
    with open(out_file, "w", encoding="utf-8", newline="\n") as handle:
        json.dump(payload, handle, indent=2)
        handle.write("\n")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("usage: stryker-score.py <search-root> <output-json>", file=sys.stderr)
        sys.exit(2)
    main(sys.argv[1], sys.argv[2])
