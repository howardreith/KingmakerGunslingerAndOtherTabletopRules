#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_urban_barbarian84 as baseline

VERSION = "0.0.85"
INFORMATIONAL_VERSION = "0.0.85-urban-barbarian-human-review-repair-2"
PACKAGE = "KingmakerGunslinger-0.0.85-local-runtime.zip"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.validate(root)

    crowd = (root / "src/KingmakerGunslinger/UrbanBarbarian/"
        "CrowdControlComponent.cs").read_text(encoding="utf-8")
    if "evt.BonusSources.Add(new BonusSource(1, Fact))" not in crowd:
        raise AssertionError(
            "Crowd Control AC must publish its exact combat-log bonus source")


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
