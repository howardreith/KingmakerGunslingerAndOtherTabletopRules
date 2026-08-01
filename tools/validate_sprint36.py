#!/usr/bin/env python3
"""Portable source validator for Sprint 36 with inherited Sprint 35 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint35

VERSION = "0.0.36"
INFORMATIONAL_VERSION = "0.0.36-s36-core-deeds"
TEST_COUNT = 732

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 36 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT, active_blueprints: int = 39,
             manifest_entries: int = 40) -> None:
    root = root.resolve()
    validate_sprint35.validate(root, version, informational_version,
                               test_count, active_blueprints, manifest_entries)
    require_tokens(read(root, "planning/SPRINT-36-ENTRY-CRITERIA.md"),
        ["Deadeye", "Gunslinger's Dodge", "Quick Clear",
         "one grit per range increment beyond the first",
         "checkpoint, not a stopping condition"],
        "Sprint 36 entry criteria")
    require_tokens(read(root, "src/KingmakerGunslinger/Deeds/DeadeyeService.cs"),
        ["increment - 1", "InsufficientGrit", "WithinFirstIncrement",
         "RangeBoundaryToleranceMeters"], "Sprint 36 Deadeye policy")
    require_tokens(read(root, "tests/KingmakerGunslinger.DomainTests/Sprint36Tests.cs"),
        ["DeadeyeSecondIncrementCostsOne", "DeadeyeCostScalesBeyondFirst",
         "DeadeyeInsufficientGritFailsAtomic",
         "DeadeyeSpecialAndInvalidRangeFailClosed"],
        "Sprint 36 Deadeye tests")
    require_tokens(read(root, "src/KingmakerGunslinger/Deeds/GunslingerDodgeService.cs"),
        ["NotRangedAttack", "UnsupportedArmor", "Overloaded",
         "InsufficientGrit", "CannotDropProne", "DropProne"],
        "Sprint 36 Gunslinger's Dodge policy")
    require_tokens(read(root, "src/KingmakerGunslinger/Deeds/GunslingerDodgeMode.cs"),
        ["MoveFiveFeet", "DropProne"], "Sprint 36 dodge mode vocabulary")
    require_tokens(read(root, "src/KingmakerGunslinger/Deeds/QuickClearService.cs"),
        ["NotExactEquippedFirearm", "NotBroken", "NotMisfireBroken",
         "InsufficientGrit", "QuickClearMode.Move"], "Sprint 36 Quick Clear policy")
    print("Sprint 36 source invariant validation passed with inherited Sprint 35 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 36 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
