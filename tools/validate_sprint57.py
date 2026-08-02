#!/usr/bin/env python3
"""Portable source validator for the Sprint 57 Death's Shot observer."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint56

VERSION = "0.0.57"
INFORMATIONAL_VERSION = "0.0.57-s57-deaths-shot-observer"
TEST_COUNT = 813

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 57 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint56.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 102, 103)
    require_tokens(read(root, "planning/SPRINT-57-ENTRY-CRITERIA.md"),
        ["normal damage", "Fortitude", "Death descriptor",
         "cannot restore grit", "True Grit"], "Sprint 57 criteria")
    runner = read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs")
    require_tokens(runner,
        ["RunObserveDeathsShotNativeDeath", "name == \"Destruction\"",
         "SpellDescriptor.Death", "ContextActionSavingThrow",
         "ContextActionKillTarget", "deaths-shot-native-save-kill-actions"],
        "Sprint 57 native death observer")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs") +
        read(root, "scripts/RuntimeAutomation.Common.ps1"),
        ["observe-deaths-shot-native-death"], "Sprint 57 observer allowlists")
    print("Sprint 57 observer source validation passed with inherited Sprint 56 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try: validate(args.root)
    except Exception as exception:
        print(f"Sprint 57 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
