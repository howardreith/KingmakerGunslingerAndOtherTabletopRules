#!/usr/bin/env python3
"""Portable validator for the 0.0.61 first-playtest repair."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint60

VERSION = "0.0.61"
INFORMATIONAL_VERSION = "0.0.61-first-playtest-repair"

def validate(root: Path) -> None:
    validate_sprint60.validate(root, VERSION, INFORMATIONAL_VERSION, 841)

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Playtest 0.0.61 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
