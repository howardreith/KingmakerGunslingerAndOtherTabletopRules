#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_paper74

VERSION = "0.0.75"
INFORMATIONAL_VERSION = "0.0.75-feature-modules-acadamae-graduate"
PACKAGE = "KingmakerGunslinger-0.0.75-local-runtime.zip"


def validate(root: Path) -> None:
    validate_paper74.VERSION = VERSION
    validate_paper74.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_paper74.PACKAGE = PACKAGE
    validate_paper74.validate_compatibility72.VERSION = VERSION
    validate_paper74.validate_compatibility72.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_paper74.validate_compatibility72.validate_playtest71.VERSION = VERSION
    validate_paper74.validate_compatibility72.validate_playtest71.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_paper74.validate(root)
    ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if VERSION == "0.0.75" and "Kingmaker Gunslinger - 0.0.75 FEATURE-MODULES / ACADAMAE-GRADUATE / CORD-OF-STUBBORN-RESOLVE" not in ui:
        raise AssertionError("0.0.75 feature-module build label missing")
    for relative in (
        "FEATURE-MODULES-ACADAMAE-GRADUATE-JOURNAL.md",
        "FEATURE-MODULES-ACADAMAE-GRADUATE-IMPLEMENTATION-REPORT.md",
        "docs/FEATURE-MODULES-ACADAMAE-GRADUATE-QUALIFICATION.md",
        "planning/FEATURE-MODULES-ACADAMAE-GRADUATE-MISSION.md",
        "planning/FEATURE-MODULES-ACADAMAE-GRADUATE-MATRIX.md",
        "planning/FEATURE-MODULE-BOUNDARY-INVENTORY.md",
        "planning/ACADAMAE-GRADUATE-SPELL-AND-PREREQUISITE-INVENTORY.md",
        "planning/CORD-OF-STUBBORN-RESOLVE-INVENTORY.md",
    ):
        if not (root / relative).is_file():
            raise AssertionError(f"Feature-module mission file missing: {relative}")
    manifest = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    symbols = {entry["symbol"] for entry in manifest["entries"]}
    for symbol in ("KMG.Feats.AcadamaeGraduate", "KMG.Items.CordOfStubbornResolve"):
        if symbol not in symbols:
            raise AssertionError(f"Feature-module blueprint missing: {symbol}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Feature modules {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Feature modules {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
