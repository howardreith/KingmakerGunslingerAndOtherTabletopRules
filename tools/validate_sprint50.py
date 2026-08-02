#!/usr/bin/env python3
"""Portable source validator for Sprint 50 with inherited Sprint 47 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint47

VERSION = "0.0.50"
INFORMATIONAL_VERSION = "0.0.50-s50-bleeding-wound"
TEST_COUNT = 780

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 50 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint47.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 84, 85)
    require_tokens(read(root, "planning/SPRINT-50-ENTRY-CRITERIA.md"),
        ["four personal free-action selections", "RuleDealStatDamage",
         "SpellDescriptor.Bleed", "living creature"], "Sprint 50 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/BleedingWoundService.cs"),
        ["BleedingWoundKind.HitPoints", "insufficient-grit",
         "nonliving-target", "sneak-immune"], "Sprint 50 policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint50Tests.cs"),
        ["BleedingWoundAllChoices", "BleedingWoundMarkerConsumption",
         "BleedingWoundGates", "BleedingWoundInvalid"], "Sprint 50 tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/BleedingWoundTick.cs"),
        ["ITickEachRound", "DirectDamage", "RuleDealStatDamage"],
        "Sprint 50 recurring delivery")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/BleedingWoundBlueprints.cs"),
        ["UnitCommand.CommandType.Free", "SpellDescriptor.Bleed",
         "StackingType.Replace", "BleedingWoundAbilityLogic"],
        "Sprint 50 blueprints")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["BleedingWoundBlueprints.Register", "bleedingWound.Feature"],
        "Sprint 50 progression")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerBleedingWound",
         "bleeding-wound-hit-points", "bleeding-wound-ability-damage"],
        "Sprint 50 guarded scenario")
    print("Sprint 50 source invariant validation passed with inherited Sprint 47 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 50 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
