#!/usr/bin/env python3
"""Portable source validator for Sprint 41 with inherited Sprint 40 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint40

VERSION = "0.0.41"
INFORMATIONAL_VERSION = "0.0.41-s41-bonus-feats"
TEST_COUNT = 751

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 41 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT,
             active_manifest_count: int = 53,
             total_manifest_count: int = 54) -> None:
    root = root.resolve()
    validate_sprint40.validate(root, version, informational_version,
                               test_count, active_manifest_count,
                               total_manifest_count)
    require_tokens(read(root, "planning/SPRINT-41-ENTRY-CRITERIA.md"),
        ["levels 4, 8, 12, 16, and 20", "combat or grit feats",
         "prerequisite", "same native selection reference"],
        "Sprint 41 entry criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Classes/BonusFeatProgression.cs"),
        ["{ 4, 8, 12, 16, 20 }", "gunslingerLevel % 4 == 0",
         "(int[])ExactLevels.Clone()"], "Sprint 41 cadence")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["41c8486641f7d6d4283ca9dae4147a9f",
         "BlueprintFeatureSelection", "Features.Add(bonusFeats)"],
        "Sprint 41 native selection integration")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerBonusFeats", "bonus-feats-cadence",
         "bonus-feats-native-selection", "!selection.IgnorePrerequisites"],
        "Sprint 41 guarded scenario")
    print("Sprint 41 source invariant validation passed with inherited Sprint 40 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 41 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
