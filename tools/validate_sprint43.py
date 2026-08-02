#!/usr/bin/env python3
"""Portable source validator for Sprint 43 with inherited Sprint 42 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint42

VERSION = "0.0.43"
INFORMATIONAL_VERSION = "0.0.43-s43-dead-shot"
TEST_COUNT = 762

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 43 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT,
             active_manifest_count: int = 61,
             total_manifest_count: int = 62) -> None:
    root = root.resolve()
    validate_sprint42.validate(root, version, informational_version,
                               test_count, active_manifest_count,
                               total_manifest_count)
    require_tokens(read(root, "planning/SPRINT-43-ENTRY-CRITERIA.md"),
        ["full-round action", "base attack bonus", "every attack roll is a misfire",
         "One loaded chamber", "one critical confirmation"],
        "Sprint 43 entry criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/DeadShotService.cs"),
        ["BaseAttackBonus", "(index * 5)", "DeadShotStatus.Eligible"],
        "Sprint 43 policy")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/DeadShotOutcomeService.cs"),
        ["allMisfire", "hits++", "threats++", "Math.Min(0, -5"],
        "Sprint 43 outcome aggregation")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/DeadShotRuntime.cs"),
        ["RegisterProbe", "ShouldBypassDischarge", "WeaponDamageDiceOverride",
         "CriticalConfirmationBonus", "FirearmExplosionRuntime.Apply"],
        "Sprint 43 native runtime")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/DeadShotBlueprints.cs"),
        ["SetIsFullRoundAction(true)", "AbilityRange.Weapon",
         "DeadShotAbilityLogic"], "Sprint 43 blueprints")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerDeadShot", "dead-shot-progression",
         "dead-shot-mixed-volley", "dead-shot-all-misfire"],
        "Sprint 43 guarded scenario")
    print("Sprint 43 source invariant validation passed with inherited Sprint 42 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 43 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
