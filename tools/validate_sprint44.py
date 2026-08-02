#!/usr/bin/env python3
"""Portable source validator for Sprint 44 with inherited Sprint 43 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint43

VERSION = "0.0.44"
INFORMATIONAL_VERSION = "0.0.44-s44-startling-shot"
TEST_COUNT = 765

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 44 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint43.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 64, 65)
    require_tokens(read(root, "planning/SPRINT-44-ENTRY-CRITERIA.md"),
        ["at least 1 grit", "standard action", "intentionally misses",
         "one loaded chamber", "start of its next turn"],
        "Sprint 44 entry criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/StartlingShotService.cs"),
        ["InsufficientGrit", "InvalidTarget", "Eligible, 1, 0, 1"],
        "Sprint 44 policy")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/StartlingShotRuntime.cs"),
        ["FirearmDischargeService", "TimeSpan.FromSeconds(6d",
         "AddBuff", "RemoveFact", "current => before"],
        "Sprint 44 runtime")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/StartlingShotBlueprints.cs"),
        ["UnitCondition.LoseDexterityToAC", "AbilityRange.Weapon",
         "UnitCommand.CommandType.Standard", "StackingType.Replace"],
        "Sprint 44 blueprints")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint44Tests.cs"),
        ["StartlingShotEligible", "StartlingShotPreconditionsAtomic",
         "StartlingShotInvalidInputs"], "Sprint 44 domain tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerStartlingShot",
         "startling-shot-progression", "startling-shot-delivery",
         "startling-shot-flat-footed"], "Sprint 44 guarded scenario")
    print("Sprint 44 source invariant validation passed with inherited Sprint 43 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 44 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
