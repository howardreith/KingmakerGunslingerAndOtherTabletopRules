#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_compatibility72

VERSION = "0.0.73"
INFORMATIONAL_VERSION = "0.0.73-pistolero-musket-master"
PACKAGE = "KingmakerGunslinger-0.0.73-local-runtime.zip"


def validate(root: Path) -> None:
    validate_compatibility72.VERSION = VERSION
    validate_compatibility72.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_compatibility72.validate_playtest71.VERSION = VERSION
    validate_compatibility72.validate_playtest71.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_compatibility72.validate(root)
    ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(
        encoding="utf-8")
    if "Kingmaker Gunslinger - 0.0.73 PISTOLERO-MUSKET-MASTER" not in ui:
        raise AssertionError("0.0.73 archetype build label missing")
    for relative in (
        "PISTOLERO-MUSKET-MASTER-JOURNAL.md",
        "PISTOLERO-MUSKET-MASTER-IMPLEMENTATION-REPORT.md",
        "docs/PISTOLERO-MUSKET-MASTER-QUALIFICATION.md",
        "planning/PISTOLERO-MUSKET-MASTER-MISSION.md",
        "planning/PISTOLERO-MUSKET-MASTER-REPLACEMENT-MATRIX.md",
        "src/KingmakerGunslinger/Blueprints/PistoleroBlueprints.cs",
        "src/KingmakerGunslinger/Blueprints/MusketMasterBlueprints.cs",
        "src/KingmakerGunslinger/Firearms/FirearmHandednessPolicy.cs",
    ):
        if not (root / relative).is_file():
            raise AssertionError(f"archetype mission file missing: {relative}")
    schema = json.loads((root / "compatibility/profiles.schema.json").read_text(
        encoding="utf-8"))
    package_const = schema["properties"]["profiles"]["items"]["properties"][
        "requiredGunslingerPackage"]["const"]
    if package_const != PACKAGE:
        raise AssertionError("compatibility schema package pin mismatch")
    profiles = json.loads((root / "compatibility/profiles.json").read_text(
        encoding="utf-8"))["profiles"]
    for profile in profiles:
        if profile["requiredGunslingerPackage"] != PACKAGE:
            raise AssertionError(f"profile package pin mismatch: {profile['id']}")
        if profile["runtimeLoadableRequired"]:
            required = {
                "observe-class-blueprint-contracts",
                "observe-gunslinger-presentation",
                "disposable-firearm-dependent-feats",
                "disposable-pistolero-deeds",
                "disposable-archetype-reconciliation",
            }
            if not required.issubset(set(profile["scenarios"])):
                raise AssertionError(
                    f"runtime profile lacks archetype scenarios: {profile['id']}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Archetype {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Archetype {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
