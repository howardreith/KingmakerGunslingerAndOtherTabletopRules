#!/usr/bin/env python3
"""Portable source validator for Sprint 40 with inherited Sprint 39 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint39

VERSION = "0.0.40"
INFORMATIONAL_VERSION = "0.0.40-s40-utility-shot"
TEST_COUNT = 748

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 40 file is missing: {relative}")
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
    validate_sprint39.validate(root, version, informational_version,
                               test_count, active_manifest_count,
                               total_manifest_count)
    require_tokens(read(root, "planning/SPRINT-40-ENTRY-CRITERIA.md"),
        ["Blast Lock", "Scoot Unattended Object", "Stop Bleeding",
         "SpellDescriptor.Bleed", "consumes exactly one loaded chamber"],
        "Sprint 40 entry criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/StopBleedingService.cs"),
        ["FirearmCondition.Wrecked", "LoadedRounds < 1",
         "CurrentGrit < 1", "BleedCount < 1"], "Sprint 40 policy")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/StopBleedingRuntime.cs"),
        ["FirearmDischargeService", "SpellDescriptor.Bleed",
         "Buffs.RemoveFact", "current => before"], "Sprint 40 runtime adapter")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerStopBleeding", "stop-bleeding-self",
         "stop-bleeding-adjacent", "stop-bleeding-zero-grit"],
        "Sprint 40 guarded scenario")
    print("Sprint 40 source invariant validation passed with inherited Sprint 39 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 40 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
