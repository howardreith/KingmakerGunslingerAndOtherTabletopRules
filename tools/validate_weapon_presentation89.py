#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_urban_barbarian88 as baseline

VERSION = "0.0.89"
INFORMATIONAL_VERSION = "0.0.89-weapon-presentation-calibration"
PACKAGE = "KingmakerGunslinger-0.0.89-local-runtime.zip"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.validate(root)

    required = (
        "WEAPON-PRESENTATION-MISSION.md",
        "WEAPON-PRESENTATION-JOURNAL.md",
        "WEAPON-PRESENTATION-BLOCKERS.md",
        "WEAPON-PRESENTATION-RESUME.md",
        "planning/WEAPON-PRESENTATION-MATRIX.md",
        "src/KingmakerGunslinger/RuntimeTesting/"
        "WeaponPresentationEvidenceScenario.cs",
        "tests/KingmakerGunslinger.DomainTests/"
        "WeaponPresentationMissionTests.cs",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(
                f"Weapon-presentation gate file missing: {relative}")

    scenario = (root / required[-2]).read_text(encoding="utf-8")
    for token in (
        "weapon-presentation-body-matrix-index.json",
        "male-medium-heavy-armor",
        "male-medium-cloak",
        "actor.Descriptor.Progression.Race.RaceId",
        "336 PNG/JSON pairs",
        "1,344 labelled views",
    ):
        if token not in scenario:
            raise AssertionError(
                f"Weapon-presentation final matrix contract missing: {token}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Weapon Presentation {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Weapon Presentation {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
