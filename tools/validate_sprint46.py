#!/usr/bin/env python3
"""Portable source validator for Sprint 46 with inherited Sprint 45 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint45

VERSION = "0.0.46"
INFORMATIONAL_VERSION = "0.0.46-s46-targeting-torso"
TEST_COUNT = 773

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 46 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT, registered_count: int = 69,
             ledger_count: int = 70) -> None:
    root = root.resolve()
    validate_sprint45.validate(root, version, informational_version,
                               test_count, registered_count, ledger_count)
    require_tokens(read(root, "planning/SPRINT-46-ENTRY-CRITERIA.md"),
        ["full-round action", "costs 1 grit", "19 or 20",
         "sneak attacks", "deed-local marker"],
        "Sprint 46 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/TargetingTorsoThreatService.cs"),
        ["naturalRoll < 1", "naturalRoll > 20",
         "TargetingTorsoThreatDecision"], "Sprint 46 threat policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint46Tests.cs"),
        ["TargetingTorsoThreatRange", "TargetingTorsoThreatGates",
         "TargetingTorsoThreatInvalid"], "Sprint 46 domain tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/TargetingTorsoRuntime.cs"),
        ["ConditionalWeakTable<RuleAttackWithWeapon", "CriticalEdgeBonus",
         "ImmuneToSneakAttack", "CreateRuleDealDamage(false)",
         "finally { Cancel(attack); }"], "Sprint 46 runtime")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/TargetingTorsoBlueprints.cs"),
        ["SetIsFullRoundAction(true)", "AbilityRange.Weapon",
         "TargetingTorsoAbilityLogic"], "Sprint 46 blueprints")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["TargetingTorsoBlueprints.Register", "targetingTorso.Feature"],
        "Sprint 46 progression")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerTargetingTorso",
         "targeting-torso-natural-18", "targeting-torso-natural-19"],
        "Sprint 46 guarded scenario")
    print("Sprint 46 source invariant validation passed with inherited Sprint 45 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 46 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
