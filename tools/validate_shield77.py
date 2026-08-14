#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_feature75
import expanded_summoning_manifest

VERSION = "0.0.77"
INFORMATIONAL_VERSION = "0.0.77-shield-other"
PACKAGE = "KingmakerGunslinger-0.0.77-local-runtime.zip"


def validate(root: Path) -> None:
    validate_feature75.VERSION = VERSION
    validate_feature75.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.PACKAGE = PACKAGE
    validate_feature75.validate_paper74.VERSION = VERSION
    validate_feature75.validate_paper74.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate_paper74.PACKAGE = PACKAGE
    validate_feature75.validate_paper74.validate_compatibility72.VERSION = VERSION
    validate_feature75.validate_paper74.validate_compatibility72.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate_paper74.validate_compatibility72.validate_playtest71.VERSION = VERSION
    validate_feature75.validate_paper74.validate_compatibility72.validate_playtest71.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate(root)

    ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if "Kingmaker Gunslinger - 0.0.77 SHIELD-OTHER" not in ui:
        raise AssertionError("0.0.77 Shield Other build label missing")
    required = (
        "planning/SHIELD-OTHER-MISSION.md",
        "planning/SHIELD-OTHER-INVENTORY.md",
        "planning/SHIELD-OTHER-MATRIX.md",
        "SHIELD-OTHER-IMPLEMENTATION-JOURNAL.md",
        "SHIELD-OTHER-IMPLEMENTATION-REPORT.md",
        "docs/SHIELD-OTHER-QUALIFICATION.md",
        "src/KingmakerGunslinger/Spells/ShieldOther/ShieldOtherRuntime.cs",
        "src/KingmakerGunslinger/Spells/ShieldOther/ShieldOtherBuffComponent.cs",
        "scripts/Test-ShieldOtherWorkingSavePersistence.ps1",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Shield Other file missing: {relative}")
    manifest = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    entries = {entry["symbol"]: entry for entry in manifest["entries"]}
    expected = {
        "KMG.Spells.ShieldOther.Ability": "6a8c4c1d2fbe4d6a9a724988c1348401",
        "KMG.Spells.ShieldOther.TargetBuff": "7bd92e3c44ad42e7b523ee8ed7afc602",
    }
    for symbol, guid in expected.items():
        if entries.get(symbol, {}).get("guid") != guid:
            raise AssertionError(f"Shield Other identity mismatch: {symbol}")
    active = [entry for entry in manifest["entries"] if entry["status"] == "active"]
    reserved = [entry for entry in manifest["entries"] if entry["status"] == "reserved"]
    spear_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith("KMG.ElvenBranchedSpear.")]
    eastern_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith("KMG.EasternWeapons.")]
    if (len(manifest["entries"]) != 1439 + len(spear_entries) + len(eastern_entries)
            or len(active) != 1438 + len(spear_entries) + len(eastern_entries)
            or len(reserved) != 1):
        raise AssertionError("Expanded Summoning reservation ledger count mismatch")
    expanded_summoning_manifest.validate(manifest, expanded_summoning_manifest.planned())
    bootstrap = (root / "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs").read_text(encoding="utf-8")
    expected_registration = ("ExpectedRegisteredBlueprintCount = 320 +"
        if eastern_entries else "ExpectedRegisteredBlueprintCount = 283 +"
        if spear_entries else "ExpectedRegisteredBlueprintCount = 254 +")
    if expected_registration not in bootstrap:
        raise AssertionError("Shield Other registration count mismatch")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Shield Other {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Shield Other {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
