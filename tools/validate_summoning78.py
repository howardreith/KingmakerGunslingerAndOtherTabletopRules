#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_feature75
import expanded_summoning_manifest

VERSION = "0.0.78"
INFORMATIONAL_VERSION = "0.0.78-expanded-summoning"
PACKAGE = "KingmakerGunslinger-0.0.78-local-runtime.zip"


def validate(root: Path) -> None:
    validate_feature75.VERSION = VERSION
    validate_feature75.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.PACKAGE = PACKAGE
    validate_feature75.validate_paper74.VERSION = VERSION
    validate_feature75.validate_paper74.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate_paper74.PACKAGE = PACKAGE
    compatibility = validate_feature75.validate_paper74.validate_compatibility72
    compatibility.VERSION = VERSION
    compatibility.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    compatibility.validate_playtest71.VERSION = VERSION
    compatibility.validate_playtest71.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate(root)

    ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if "Kingmaker Gunslinger - 0.0.78 EXPANDED-SUMMONING" not in ui:
        raise AssertionError("0.0.78 Expanded Summoning build label missing")
    required = (
        "planning/EXPANDED-SUMMONING-MISSION.md",
        "planning/EXPANDED-SUMMONING-ROSTER.md",
        "planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md",
        "planning/EXPANDED-SUMMONING-INVENTORY.md",
        "EXPANDED-SUMMONING-JOURNAL.md",
        "EXPANDED-SUMMONING-IMPLEMENTATION-REPORT.md",
        "EXPANDED-SUMMONING-STATE.json",
        "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
        "src/KingmakerGunslinger/Blueprints/ExpandedSummoningBlueprints.cs",
        "scripts/compatibility/Test-ExpandedSummoningCompatibilityProfiles.ps1",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Expanded Summoning file missing: {relative}")
    manifest = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    active = [entry for entry in manifest["entries"] if entry["status"] == "active"]
    reserved = [entry for entry in manifest["entries"] if entry["status"] == "reserved"]
    spear_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith("KMG.ElvenBranchedSpear.")]
    if (len(manifest["entries"]) != 1439 + len(spear_entries)
            or len(active) != 1438 + len(spear_entries) or len(reserved) != 1):
        raise AssertionError("Expanded Summoning blueprint ledger count mismatch")
    expected_spear_entries = {
        "KMG.ElvenBranchedSpear.WeaponType": ("77f72b0febaf212a5650e7193c00361f", "BlueprintWeaponType"),
        "KMG.ElvenBranchedSpear.BaseItem": ("6edc216d68810960f85417237748b042", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.MasterworkItem": ("9c9edabf91f2117fd1b642c4d39b9574", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.ColdIronItem": ("8c0de00a236fe0f532d31711dcaa00a2", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.MasterworkColdIronItem": ("b16c34215cae9d60345042157149a4c0", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.Plus1Item": ("66111becd22690a2a19444a5c6bd0c7b", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.Plus1ColdIronItem": ("25d8f6c6f4767b3168f4700a2890954f", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.ExoticWeaponProficiency": ("017d586ec4546feabf6eaaa67ce74a3f", "BlueprintFeature"),
        "KMG.ElvenBranchedSpear.FinesseTraining": ("3843c643ffcc617faf9121a5f801a70e", "BlueprintFeature"),
        "KMG.ElvenBranchedSpear.MovementOpportunityAccuracy": ("b0cabc2a4ac0135fab2f89c689dea389", "BlueprintWeaponEnchantment"),
    }
    by_symbol = {entry["symbol"]: entry for entry in spear_entries}
    for symbol, (guid, planned_type) in expected_spear_entries.items():
        entry = by_symbol.get(symbol)
        if (entry is None or entry.get("guid") != guid
                or entry.get("plannedType") != planned_type
                or entry.get("status") != "active"):
            raise AssertionError(f"Elven Branched Spear identity mismatch: {symbol}")
    if spear_entries and len(spear_entries) != len(expected_spear_entries):
        raise AssertionError("Elven Branched Spear blueprint ledger count mismatch")
    expanded_summoning_manifest.validate(manifest, expanded_summoning_manifest.planned())
    bootstrap = (root / "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs").read_text(encoding="utf-8")
    expected_registration = ("ExpectedRegisteredBlueprintCount = 264 +"
        if spear_entries else "ExpectedRegisteredBlueprintCount = 254 +")
    if expected_registration not in bootstrap:
        raise AssertionError("Expanded Summoning aggregate registration count mismatch")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Expanded Summoning {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Expanded Summoning {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
