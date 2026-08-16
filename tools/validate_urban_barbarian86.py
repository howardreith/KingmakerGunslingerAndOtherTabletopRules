#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_urban_barbarian85 as baseline

VERSION = "0.0.86"
INFORMATIONAL_VERSION = "0.0.86-urban-barbarian-human-review-repair-3"
PACKAGE = "KingmakerGunslinger-0.0.86-local-runtime.zip"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.validate(root)

    observer = (root / "src/KingmakerGunslinger/RuntimeTesting/"
        "UrbanBarbarianRageInventoryObserver.cs").read_text(encoding="utf-8")
    required = (
        "73 unique exact identities",
        "urban.TierSelectors",
        "tierVariantCounts.SequenceEqual(new[] { 6, 10, 15 })",
        "urban.LegacySelector.Hidden",
        "final player-facing tier selector blueprint graph",
    )
    for token in required:
        if token not in observer:
            raise AssertionError(
                f"Urban Rage inventory observer contract is missing: {token}")
    if "urban.Selector.ComponentsArray" in observer:
        raise AssertionError(
            "Urban Rage inventory observer still enumerates the inert legacy selector")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Urban Barbarian {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Urban Barbarian {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
