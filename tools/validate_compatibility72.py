#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_playtest71

VERSION = "0.0.72"
INFORMATIONAL_VERSION = "0.0.72-optional-mod-compatibility"


def validate(root: Path) -> None:
    validate_playtest71.VERSION = VERSION
    validate_playtest71.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_playtest71.validate(root)
    ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if "Kingmaker Gunslinger - 0.0.72 OPTIONAL-MOD-COMPATIBILITY" not in ui:
        raise AssertionError("0.0.72 build label missing")
    required = [
        "planning/OPTIONAL-MOD-COMPATIBILITY-MISSION.md",
        "docs/OPTIONAL-MOD-COMPATIBILITY-FORENSICS.md",
        "docs/OPTIONAL-MOD-COMPATIBILITY-JOURNAL.md",
        "docs/OPTIONAL-MOD-COMPATIBILITY-IMPLEMENTATION-REPORT.md",
        "docs/OPTIONAL-MOD-COMPATIBILITY-MANUAL-ACCEPTANCE.md",
        "compatibility/reference-catalog.schema.json",
        "compatibility/reference-catalog.json",
        "scripts/compatibility/Inspect-OptionalModReferences.ps1",
        "scripts/compatibility/Test-OptionalModReferenceInventory.ps1",
    ]
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"compatibility mission file missing: {relative}")
    catalog = json.loads((root / "compatibility/reference-catalog.json").read_text(encoding="utf-8"))
    keys = [entry["key"] for entry in catalog["references"]]
    if len(keys) != len(set(keys)):
        raise AssertionError("duplicate compatibility reference key")
    for key in ("call-of-the-wild", "craft-magic-items", "arms-armor",
                "toggle-custom-soundpacks", "eddic-respec", "bag-of-tricks"):
        if key not in keys:
            raise AssertionError(f"compatibility reference key missing: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Compatibility {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Compatibility {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
