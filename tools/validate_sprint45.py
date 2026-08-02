#!/usr/bin/env python3
"""Portable source validator for Sprint 45 with inherited Sprint 44 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint44

VERSION = "0.0.45"
INFORMATIONAL_VERSION = "0.0.45-s45-targeting-head"
TEST_COUNT = 770

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 45 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint44.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 67, 68)
    require_tokens(read(root, "planning/SPRINT-45-ENTRY-CRITERIA.md"),
        ["full-round action", "costs 1 grit", "confuses the target",
         "immune to sneak attacks", "mind-affecting"], "Sprint 45 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/TargetingHeadService.cs"),
        ["InsufficientGrit", "InvalidTarget", "EvaluateRider"],
        "Sprint 45 policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint45Tests.cs"),
        ["TargetingHeadEligible", "TargetingHeadPreconditions",
         "TargetingHeadHitRider", "TargetingHeadRiderGates"],
        "Sprint 45 domain tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/TargetingHeadRuntime.cs"),
        ["RuleAttackWithWeapon", "ImmuneToSneakAttack", "AddBuff",
         "TimeSpan.FromSeconds(6d)"], "Sprint 45 runtime")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/TargetingHeadBlueprints.cs"),
        ["SetIsFullRoundAction(true)", "UnitCondition.Confusion",
         "SpellDescriptor.MindAffecting", "AbilityRange.Weapon"],
        "Sprint 45 blueprints")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["TargetingHeadBlueprints.Register", "targetingHead.Feature"],
        "Sprint 45 progression")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerTargetingHead",
         "targeting-head-progression", "targeting-head-attack",
         "targeting-head-rider"], "Sprint 45 guarded scenario")
    print("Sprint 45 source invariant validation passed with inherited Sprint 44 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 45 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
