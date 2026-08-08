#!/usr/bin/env python3
"""Portable source validator for Sprint 42 with inherited Sprint 41 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint41

VERSION = "0.0.42"
INFORMATIONAL_VERSION = "0.0.42-s42-gun-training"
TEST_COUNT = 756

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 42 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT,
             active_manifest_count: int = 59,
             total_manifest_count: int = 60) -> None:
    root = root.resolve()
    validate_sprint41.validate(root, version, informational_version,
                               test_count, active_manifest_count,
                               total_manifest_count)
    require_tokens(read(root, "planning/SPRINT-42-ENTRY-CRITERIA.md"),
        ["levels 5, 9, 13, and 17", "Dexterity modifier", "+2 rather than +4",
         "FirearmDefinition.Kind"], "Sprint 42 entry criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Classes/GunTrainingPolicy.cs"),
        ["UntrainedBrokenIncrease = 4", "TrainedBrokenIncrease = 2",
         "Math.Min", "DamageBonus"], "Sprint 42 domain policy")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunTrainingBlueprints.cs"),
        ["BlueprintFeatureSelection", "GunTrainingDamage", "AllFeatures",
         "FirearmKind.Revolver"], "Sprint 42 selection blueprints")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs"),
        ["EffectiveMisfireValue", "FirearmTrainingRuntime.Resolve"],
        "Sprint 42 misfire integration")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerGunTraining", "gun-training-progression",
         "gun-training-damage", "gun-training-misfire"],
        "Sprint 42 guarded scenario")
    print("Sprint 42 source invariant validation passed with inherited Sprint 41 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 42 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
