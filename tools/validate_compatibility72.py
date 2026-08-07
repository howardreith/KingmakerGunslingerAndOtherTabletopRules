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
        "compatibility/profiles.schema.json",
        "compatibility/profiles.json",
        "scripts/compatibility/Inspect-OptionalModReferences.ps1",
        "scripts/compatibility/Test-OptionalModReferenceInventory.ps1",
        "scripts/compatibility/Invoke-OptionalModStaticAudit.ps1",
        "scripts/compatibility/CompatibilityProfile.Common.ps1",
        "scripts/compatibility/Resolve-KingmakerCompatibilityProfile.ps1",
        "scripts/compatibility/Test-KingmakerCompatibilityProfileResolution.ps1",
        "scripts/compatibility/Enter-KingmakerCompatibilityProfile.ps1",
        "scripts/compatibility/Restore-KingmakerCompatibilityProfile.ps1",
        "scripts/compatibility/Test-KingmakerCompatibilityProfile.ps1",
        "tools/compatibility/scan_optional_mod_sources.py",
        "tools/compatibility/test_scan_optional_mod_sources.py",
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
    profiles = json.loads((root / "compatibility/profiles.json").read_text(encoding="utf-8"))["profiles"]
    profile_ids = [entry["id"] for entry in profiles]
    if len(profile_ids) != len(set(profile_ids)) or len(profile_ids) != 8:
        raise AssertionError("compatibility profile IDs must be eight unique values")
    craft = next(profile for profile in profiles if profile["id"] == "gunslinger-craft-magic-items")
    if craft["disposition"] != "STATIC-AUDITED-ONLY" or craft["runtimeLoadableRequired"]:
        raise AssertionError("source-only Craft Magic Items profile is not static-only")
    catalog_by_key = {entry["key"]: entry for entry in catalog["references"]}
    if catalog_by_key["kaz-asset-references"]["runtimeStagingAllowed"]:
        raise AssertionError("KAZ asset references must not be runtime staged")
    all_local = next(profile for profile in profiles if profile["id"] == "gunslinger-all-loadable-local")
    if "kaz-asset-references" in all_local["modKeys"] or any(value.startswith("KAZ_") for value in all_local["expectedUmmIds"]):
        raise AssertionError("KAZ asset references leaked into runtime profile")


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
