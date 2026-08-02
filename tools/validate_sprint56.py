#!/usr/bin/env python3
"""Portable source validator for Sprint 56 with inherited Sprint 55 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint55

VERSION = "0.0.56"
INFORMATIONAL_VERSION = "0.0.56-s56-cheat-death"
TEST_COUNT = 813

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 56 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT, active_count: int = 102,
             total_count: int = 103) -> None:
    root = root.resolve()
    validate_sprint55.validate(root, version, informational_version,
                               test_count, active_count, total_count)
    require_tokens(read(root, "planning/SPRINT-56-ENTRY-CRITERIA.md"),
        ["spend all remaining grit", "exactly 1 hit point",
         "RuleTargetLogicComponent<RuleDealDamage>"], "Sprint 56 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/CheatDeathService.cs") + read(root,
        "src/KingmakerGunslinger/Deeds/CheatDeathDecision.cs"),
        ["request.CurrentGrit, 1", "LevelTooLow", "InsufficientGrit",
         "WrongTarget", "Duplicate"], "Sprint 56 policy")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/CheatDeathDamageHandler.cs"),
        ["RuleTargetLogicComponent<RuleDealDamage>", "Owner.Unit.HPLeft",
         "Owner.Unit.MaxHP - 1", "Events.TryMark(rule)",
         "Owner.Resources.Spend(Grit, decision.GritCost)"],
        "Sprint 56 exact damage handler")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint56Tests.cs"),
        ["CheatDeathLethalApplies", "CheatDeathAllGritCosts",
         "CheatDeathResourceAndLevelGates", "CheatDeathInvalidInputRejected"],
        "Sprint 56 tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["CheatDeathBlueprints.Register", "LevelEntries[18]",
         "cheatDeath"], "Sprint 56 progression")
    print("Sprint 56 source invariant validation passed with inherited Sprint 55 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try: validate(args.root)
    except Exception as exception:
        print(f"Sprint 56 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
