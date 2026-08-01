#!/usr/bin/env python3
"""Portable source validator for Sprint 38 with inherited Sprint 37 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint37

VERSION = "0.0.38"
INFORMATIONAL_VERSION = "0.0.38-s38-gunslinger-initiative"
TEST_COUNT = 740

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 38 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint37.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 45, 46)
    require_tokens(read(root, "planning/SPRINT-38-ENTRY-CRITERIA.md"),
        ["Gunslinger Initiative", "+2", "current grit", "Quick Draw",
         "Apply at most once"], "Sprint 38 entry criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Classes/GunslingerInitiativeBonus.cs"),
        ["IUnitInitiativeHandler", "GetResourceAmount", "rule.Initiator"],
        "Sprint 38 initiative adapter")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Classes/GunslingerInitiativeRuntime.cs"),
        ["<Modifier>k__BackingField", "ConditionalWeakTable", "checked(current + bonus)"],
        "Sprint 38 initiative rule mutation")
    print("Sprint 38 source invariant validation passed with inherited Sprint 37 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 38 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
