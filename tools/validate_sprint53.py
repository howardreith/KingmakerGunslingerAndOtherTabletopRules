#!/usr/bin/env python3
"""Portable source validator for Sprint 53 with inherited Sprint 52 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint52

VERSION = "0.0.53"
INFORMATIONAL_VERSION = "0.0.53-s53-evasive"
TEST_COUNT = 795

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 53 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint52.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 94, 95)
    require_tokens(read(root, "planning/SPRINT-53-ENTRY-CRITERIA.md"),
        ["Evasion", "Uncanny Dodge", "Improved Uncanny Dodge",
         "Gunslinger level", "two independent feature PASS"],
        "Sprint 53 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/EvasiveService.cs") + read(root,
        "src/KingmakerGunslinger/Deeds/EvasiveDecision.cs"),
        ["GunslingerLevel >= 15", "CurrentGrit >= 1",
         "NativeBenefitCount"], "Sprint 53 policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint53Tests.cs"),
        ["EvasivePositiveGritAtLevelFifteen",
         "EvasiveZeroGritRemovesBenefits", "EvasiveLevelGateAndStableState",
         "EvasiveUnitStateIsIndependent", "EvasiveInvalidInputRejected"],
        "Sprint 53 tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/EvasiveBlueprints.cs"),
        ["576933720c440aa4d8d42b0c54b77e80",
         "3c08d842e802c3e4eb19d15496145709",
         "485a18c05792521459c7d06c63128c79",
         "BlueprintCloneService.Clone"], "Sprint 53 exact native clones")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/EvasiveGrantController.cs"),
        ["OnTurnOn", "OnTurnOff", "SetBenefits",
         "GetResourceAmount"], "Sprint 53 conditional grants")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/EvasiveResourcePatches.cs"),
        ["UnitAbilityResourceCollection", "Spend", "Restore",
         "Postfix(BlueprintScriptableObject blueprint",
         "EvasiveRuntime.Refresh(___m_Owner, blueprint)"],
        "Sprint 53 resource refresh")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["EvasiveBlueprints.Register", "evasive.Feature"],
        "Sprint 53 progression")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerEvasive", "evasive-positive-grit",
         "evasive-grit-transitions", "evasive-unit-isolation"],
        "Sprint 53 guarded scenario")
    print("Sprint 53 source invariant validation passed with inherited Sprint 52 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 53 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
