#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_compatibility72

VERSION = "0.0.74"
INFORMATIONAL_VERSION = "0.0.74-paper-cartridges-auto-reload"
PACKAGE = "KingmakerGunslinger-0.0.74-local-runtime.zip"


def validate(root: Path) -> None:
    validate_compatibility72.VERSION = VERSION
    validate_compatibility72.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_compatibility72.validate_playtest71.VERSION = VERSION
    validate_compatibility72.validate_playtest71.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_compatibility72.validate(root)
    ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if "Kingmaker Gunslinger - 0.0.74 PAPER-CARTRIDGES-AUTO-RELOAD" not in ui:
        raise AssertionError("0.0.74 Paper Cartridge build label missing")
    for relative in (
        "PAPER-CARTRIDGES-AUTO-RELOAD-JOURNAL.md",
        "PAPER-CARTRIDGES-AUTO-RELOAD-IMPLEMENTATION-REPORT.md",
        "docs/PAPER-CARTRIDGES-AUTO-RELOAD-QUALIFICATION.md",
        "planning/PAPER-CARTRIDGES-AUTO-RELOAD-MISSION.md",
        "planning/PAPER-CARTRIDGES-AUTO-RELOAD-MATRIX.md",
        "src/KingmakerGunslinger/Blueprints/PaperCartridgeModeBlueprints.cs",
        "src/KingmakerGunslinger/Reloading/FirearmReloadPlan.cs",
    ):
        if not (root / relative).is_file():
            raise AssertionError(f"Paper Cartridge mission file missing: {relative}")
    schema = json.loads((root / "compatibility/profiles.schema.json").read_text(encoding="utf-8"))
    package_const = schema["properties"]["profiles"]["items"]["properties"]["requiredGunslingerPackage"]["const"]
    if package_const != PACKAGE:
        raise AssertionError("compatibility schema package pin mismatch")
    profiles = json.loads((root / "compatibility/profiles.json").read_text(encoding="utf-8"))["profiles"]
    for profile in profiles:
        if profile["requiredGunslingerPackage"] != PACKAGE:
            raise AssertionError(f"profile package pin mismatch: {profile['id']}")
    manifest = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    symbols = {entry["symbol"] for entry in manifest["entries"]}
    for symbol in (
        "KMG.Ammunition.PaperCartridge",
        "KMG.Ammunition.PaperLoadedNormalStateToken",
        "KMG.Ammunition.PaperBrokenLoadedStateToken",
        "KMG.Ammunition.UsePaperCartridges",
        "KMG.Gunsmithing.CraftPaperCartridges",
    ):
        if symbol not in symbols:
            raise AssertionError(f"Paper Cartridge blueprint missing: {symbol}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Paper Cartridge {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Paper Cartridge {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
