#!/usr/bin/env python3
"""Portable source validator for Sprint 47 with inherited Sprint 46 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint46

VERSION = "0.0.47"
INFORMATIONAL_VERSION = "0.0.47-s47-targeting-legs"
TEST_COUNT = 776

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 47 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint46.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 71, 72)
    require_tokens(read(root, "planning/SPRINT-47-ENTRY-CRITERIA.md"),
        ["full-round action", "costs 1 grit", "four or more legs",
         "native Trip", "trip-immune"], "Sprint 47 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/TargetingLegsRiderService.cs"),
        ["TargetingLegsRiderDecision", "immuneToSneakAttack",
         "immuneToTrip"], "Sprint 47 rider policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint47Tests.cs"),
        ["TargetingLegsEligibleRider", "TargetingLegsRiderGates",
         "TargetingLegsRiderObservations"], "Sprint 47 domain tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/TargetingLegsRuntime.cs"),
        ["CreateRuleDealDamage(false)", "CombatManeuver.Trip",
         "ReplaceAttackBonus = 1000", "ImmuneToCombatManeuvers",
         "ImmuneToSneakAttack"], "Sprint 47 runtime")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/TargetingLegsBlueprints.cs"),
        ["SetIsFullRoundAction(true)", "AbilityRange.Weapon",
         "TargetingLegsAbilityLogic"], "Sprint 47 blueprints")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["TargetingLegsBlueprints.Register", "targetingLegs.Feature"],
        "Sprint 47 progression")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerTargetingLegs",
         "targeting-legs-damage", "targeting-legs-trip"],
        "Sprint 47 guarded scenario")
    print("Sprint 47 source invariant validation passed with inherited Sprint 46 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 47 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
