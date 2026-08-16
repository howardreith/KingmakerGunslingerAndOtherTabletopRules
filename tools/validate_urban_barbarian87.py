#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_urban_barbarian86 as baseline

VERSION = "0.0.87"
INFORMATIONAL_VERSION = "0.0.87-urban-barbarian-human-review-repair-4"
PACKAGE = "KingmakerGunslinger-0.0.87-local-runtime.zip"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.validate(root)

    runner = (root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestRunner.cs").read_text(encoding="utf-8")
    required = (
        "urbanSet.TierSelectors",
        "urbanLegacySelectorInert",
        "urbanTierSelectorVariants.SequenceEqual(",
        "new[] { 6, 10, 15 })",
        "73 identities; inert legacy selector",
        "73 identities and owner facts retained",
    )
    for token in required:
        if token not in runner:
            raise AssertionError(
                f"Generic module runtime observer contract is missing: {token}")
    forbidden = (
        "urbanSet.Selector.ComponentsArray",
        "urbanSelectorVariants == 31",
        "70 identities; exactly one appended",
        "70 identities and owner facts retained",
    )
    for token in forbidden:
        if token in runner:
            raise AssertionError(
                f"Generic module runtime observer retains stale Urban state: {token}")


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
