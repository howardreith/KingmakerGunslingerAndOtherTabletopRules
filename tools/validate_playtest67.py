#!/usr/bin/env python3
"""Portable validator for the 0.0.67 player-path repair."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_playtest66

VERSION = "0.0.67"
INFORMATIONAL_VERSION = "0.0.67-seventh-playtest-player-path-repair"


def validate(root: Path) -> None:
    validate_playtest66.VERSION = VERSION
    validate_playtest66.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_playtest66.validate(root)
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Reloading/ReloadAbilityPresentationPatches.cs",
        "ReloadAbilityCommandTypePatch", "ref UnitCommand.CommandType __0",
        "ReloadAbilityPresentation.Command(action)")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Diagnostics/CombatTracePatches.cs",
        "FirearmDischargeRuntime.BeforeAttackRoll")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Playtest {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Playtest {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
