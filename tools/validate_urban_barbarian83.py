#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_brown_fur82 as baseline

VERSION = "0.0.83"
INFORMATIONAL_VERSION = "0.0.83-urban-barbarian"
PACKAGE = "KingmakerGunslinger-0.0.83-local-runtime.zip"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.MILESTONE_LABEL = "URBAN-BARBARIAN"
    baseline.PACKAGE_SUFFIX = "urban-barbarian"
    baseline.validate(root)

    required = (
        "planning/URBAN-BARBARIAN-MISSION.md",
        "planning/URBAN-BARBARIAN-FIDELITY-MATRIX.md",
        "planning/URBAN-BARBARIAN-RUNTIME-MATRIX.md",
        "planning/URBAN-BARBARIAN-COTW-CONTRACT.md",
        "src/KingmakerGunslinger/RuntimeTesting/UrbanBarbarianRageInventoryObserver.cs",
        "tests/KingmakerGunslinger.DomainTests/UrbanBarbarianInventoryContractTests.cs",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Urban Barbarian gate file missing: {relative}")

    project = (root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj") \
        .read_text(encoding="utf-8")
    if "RuntimeTesting\\UrbanBarbarianRageInventoryObserver.cs" not in project:
        raise AssertionError("Urban Rage observer is absent from the old-style project")
    if "CallOfTheWild.dll" in project or 'Reference Include="CallOfTheWild' in project:
        raise AssertionError("Urban Barbarian acquired a compile-time CotW reference")


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
