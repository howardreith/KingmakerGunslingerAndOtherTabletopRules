#!/usr/bin/env python3
"""Portable source validator for Sprint 37 with inherited Sprint 36 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint36

VERSION = "0.0.37"
INFORMATIONAL_VERSION = "0.0.37-s37-class-integration"
TEST_COUNT = 737

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 37 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint36.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 44, 45)
    require_tokens(read(root, "planning/SPRINT-37-ENTRY-CRITERIA.md"),
        ["Nimble", "levels 2, 6, 10, 14, and 18",
         "light or no armor", "checkpoint, not a stopping condition"],
        "Sprint 37 entry criteria")
    require_tokens(read(root, "src/KingmakerGunslinger/Classes/NimbleService.cs"),
        ["gunslingerLevel - 2", "Math.Min(5", "NimbleArmor.Light"],
        "Sprint 37 Nimble policy")
    print("Sprint 37 source invariant validation passed with inherited Sprint 36 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 37 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
