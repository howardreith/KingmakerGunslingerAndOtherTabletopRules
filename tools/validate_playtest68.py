#!/usr/bin/env python3
"""Portable validator for the 0.0.68 supply-item icon presentation repair."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_playtest67

VERSION = "0.0.68"
INFORMATIONAL_VERSION = "0.0.68-supply-item-icons"


def validate(root: Path) -> None:
    validate_playtest67.VERSION = VERSION
    validate_playtest67.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_playtest67.validate(root)
    development_ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs")
    text = development_ui.read_text(encoding="utf-8")
    required = "Kingmaker Gunslinger - 0.0.68 SUPPLY-ITEM-ICONS / DODGE-EXPIRATION-R3"
    if required not in text:
        raise AssertionError("The 0.0.68 supply-icon build label is missing.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Playtest {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Playtest {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
