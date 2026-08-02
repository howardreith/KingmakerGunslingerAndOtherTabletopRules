#!/usr/bin/env python3
"""Portable source validator for Sprint 51 with inherited Sprint 50 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint50

VERSION = "0.0.51"
INFORMATIONAL_VERSION = "0.0.51-s51-expert-loading"
TEST_COUNT = 784

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 51 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT, active_count: int = 87,
             ledger_count: int = 88) -> None:
    root = root.resolve()
    validate_sprint50.validate(root, version, informational_version,
                               test_count, active_count, ledger_count)
    require_tokens(read(root, "planning/SPRINT-51-ENTRY-CRITERIA.md"),
        ["Broken-to-Wrecked", "spends no grit unless", "fails closed",
         "two independent fresh-process"], "Sprint 51 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/ExpertLoadingService.cs") + read(root,
        "src/KingmakerGunslinger/Deeds/ExpertLoadingDecision.cs"),
        ["ConsumeMarker", "SuppressExplosion", "WouldExplode"],
        "Sprint 51 policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint51Tests.cs"),
        ["ExpertLoadingSuppressesBrokenMisfire",
         "ExpertLoadingInsufficientGritFailsClosed",
         "ExpertLoadingGatesAreExact", "ExpertLoadingInvalidInputFailsClosed"],
        "Sprint 51 tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/ExpertLoadingBlueprints.cs"),
        ["UnitCommand.CommandType.Free", "StackingType.Replace",
         "ExpertLoadingAbilityLogic"], "Sprint 51 blueprints")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/ExpertLoadingRuntime.cs"),
        ["ExpertLoadingBrokenRemainsBroken", "Resources.Spend",
         "suppression.failed"], "Sprint 51 runtime")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs"),
        ["ExpertLoadingRuntime.Apply"], "Sprint 51 misfire integration")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["ExpertLoadingBlueprints.Register", "expertLoading.Feature"],
        "Sprint 51 progression")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerExpertLoading",
         "expert-loading-suppression", "expert-loading-fail-closed"],
        "Sprint 51 guarded scenario")
    print("Sprint 51 source invariant validation passed with inherited Sprint 50 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 51 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
