#!/usr/bin/env python3
"""Portable source validator for the Sprint 58 Stunning Shot observer."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint57

VERSION = "0.0.58"
INFORMATIONAL_VERSION = "0.0.58-s58-stunning-shot"
TEST_COUNT = 819

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 58 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT, active_count: int = 106,
             total_count: int = 107) -> None:
    root = root.resolve()
    validate_sprint57.validate(root, version, informational_version,
                               test_count, active_count, total_count)
    require_tokens(read(root, "planning/SPRINT-58-ENTRY-CRITERIA.md"),
        ["ImmuneToCriticalHit", "exact installed Stunned", "Fortitude",
         "exactly 2 grit", "True Grit"], "Sprint 58 criteria")
    runner = read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs")
    require_tokens(runner,
        ["RunObserveStunningShotNativeStunned", "name == \"Stunned\"",
         "AddCondition{Condition=Stunned}", "ImmuneToCriticalHit",
         "stunning-shot-native-stunned-condition",
         "RunDisposableGunslingerStunningShot",
         "stunning-shot-save-failure", "stunning-shot-save-success",
         "stunning-shot-critical-immunity"],
        "Sprint 58 native Stunned observer")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs") +
        read(root, "scripts/RuntimeAutomation.Common.ps1"),
        ["observe-stunning-shot-native-stunned",
         "disposable-gunslinger-stunning-shot"],
        "Sprint 58 observer allowlists")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/StunningShotService.cs") + read(root,
        "src/KingmakerGunslinger/Deeds/StunningShotAttackHandler.cs"),
        ["10 + request.Level / 2 + request.WisdomModifier",
         "attack.ImmuneToCriticalHit", "SavingThrowType.Fortitude",
         "TimeSpan.FromSeconds(6d)", "Owner.Resources.Spend"],
        "Sprint 58 policy and native attack handler")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/StunningShotBlueprints.cs"),
        ["09d39b38bb7c6014394b6daced9bacd3",
         "BlueprintCloneService.Clone", "UnitCommand.CommandType.Free"],
        "Sprint 58 blueprints")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint58Tests.cs"),
        ["StunningShotEligibleHit", "StunningShotMissAndImmunity",
         "StunningShotDuplicateGate", "StunningShotInvalidInput"],
        "Sprint 58 tests")
    print("Sprint 58 source validation passed with inherited Sprint 57 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try: validate(args.root)
    except Exception as exception:
        print(f"Sprint 58 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
