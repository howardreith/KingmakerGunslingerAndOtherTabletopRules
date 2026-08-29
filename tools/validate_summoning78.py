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
MILESTONE_LABEL = "EXPANDED-SUMMONING"


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
    expected_label = f"Kingmaker Gunslinger - {VERSION} {MILESTONE_LABEL}"
    if expected_label not in ui:
        raise AssertionError(f"{VERSION} {MILESTONE_LABEL} build label missing")
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
    eastern_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith("KMG.EasternWeapons.")]
    focused_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith("KMG.CustomWeapons.FocusedWeapon.")]
    martial_performance_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith(
            "KMG.CustomWeapons.MartialPerformance.")]
    martial_performance_active = [entry
        for entry in martial_performance_entries
        if entry["status"] == "active"]
    martial_performance_reserved = [entry
        for entry in martial_performance_entries
        if entry["status"] == "reserved"]
    brown_fur_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith("KMG.BrownFur.")]
    brown_fur_active = [entry for entry in brown_fur_entries
        if entry["status"] == "active"]
    brown_fur_reserved = [entry for entry in brown_fur_entries
        if entry["status"] == "reserved"]
    urban_barbarian_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith("KMG.UrbanBarbarian.")]
    urban_barbarian_active = [entry for entry in urban_barbarian_entries
        if entry["status"] == "active"]
    urban_barbarian_reserved = [entry for entry in urban_barbarian_entries
        if entry["status"] == "reserved"]
    bodyguard_entries = [entry for entry in manifest["entries"]
        if entry["symbol"] in {
            "KMG.Feats.Bodyguard", "KMG.Feats.UseBodyguard",
            "KMG.Feats.BodyguardModeMarker", "KMG.Feats.InHarmsWay",
            "KMG.Feats.UseInHarmsWay", "KMG.Feats.InHarmsWayModeMarker",
            "KMG.Feats.InHarmsWayImmediatePending",
            "KMG.Feats.InHarmsWayImmediateChargedTurn"}]
    bodyguard_active = [entry for entry in bodyguard_entries
        if entry["status"] == "active"]
    bodyguard_reserved = [entry for entry in bodyguard_entries
        if entry["status"] == "reserved"]
    helpful_entries = [entry for entry in manifest["entries"]
        if entry["symbol"] == "KMG.Traits.HelpfulCombat"]
    helpful_active = [entry for entry in helpful_entries
        if entry["status"] == "active"]
    helpful_reserved = [entry for entry in helpful_entries
        if entry["status"] == "reserved"]
    heirloom_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith(
            "KMG.Traits.HeirloomWeapon.Nodachi.")]
    heirloom_active = [entry for entry in heirloom_entries
        if entry["status"] == "active"]
    heirloom_reserved = [entry for entry in heirloom_entries
        if entry["status"] == "reserved"]
    if (len(manifest["entries"]) != 1439 + len(spear_entries) +
            len(eastern_entries) + len(focused_entries) +
            len(martial_performance_entries) + len(brown_fur_entries) +
            len(urban_barbarian_entries) + len(bodyguard_entries) +
            len(helpful_entries) + len(heirloom_entries)
            or len(active) != 1438 + len(spear_entries) +
            len(eastern_entries) + len(focused_entries) +
            len(martial_performance_active) +
            len(brown_fur_active) + len(urban_barbarian_active) +
            len(bodyguard_active) + len(helpful_active) + len(heirloom_active)
            or len(reserved) != 1 + len(martial_performance_reserved) +
            len(brown_fur_reserved) +
            len(urban_barbarian_reserved) + len(bodyguard_reserved) +
            len(helpful_reserved) + len(heirloom_reserved)):
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
        "KMG.ElvenBranchedSpear.Boughkeeper": ("4a084b0226e077b58d79e33184018002", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.Thornstep": ("676faa5f811d851c9f14204bf864e1ec", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.MoonlitFork": ("403d62f6d3bb415c86939430176e55c0", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.VipersReach": ("1cfe40563a9b816931bb35e69677ac27", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.BriarCrownedSpear": ("ee580f43f50a0f0afefaedb3ce7133f3", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.SpearOfTheFirstBranch": ("85c18b96ebee3fdc87eb33da93c8fdf6", "BlueprintItemWeapon"),
        "KMG.ElvenBranchedSpear.BoughkeeperEnchantment": ("c777f06ec91be851794518fcdcc9c596", "BlueprintWeaponEnchantment"),
        "KMG.ElvenBranchedSpear.ThornstepEnchantment": ("89a27b8a22715a0b609912bc728dcb31", "BlueprintWeaponEnchantment"),
        "KMG.ElvenBranchedSpear.VipersReachEnchantment": ("be3a16e947fe8496a8301cbb2476cbcb", "BlueprintWeaponEnchantment"),
        "KMG.ElvenBranchedSpear.BriarCrownedEnchantment": ("62ef4362d84631574bacc977ffdad3e1", "BlueprintWeaponEnchantment"),
        "KMG.ElvenBranchedSpear.FirstBranchEnchantment": ("2bba46654f15079769b0e6c741e8f803", "BlueprintWeaponEnchantment"),
        "KMG.ElvenBranchedSpear.BoughkeeperArmorClassBuff": ("064feb1123cfb1ae4f541ef5e4d138a1", "BlueprintBuff"),
        "KMG.ElvenBranchedSpear.ThornstepSpeedPenaltyBuff": ("339e83672ea2116e55640d175fec0c84", "BlueprintBuff"),
        "KMG.ElvenBranchedSpear.ThornstepRoundMarker": ("7e2b2d36433396535555d39cc4066763", "BlueprintBuff"),
        "KMG.ElvenBranchedSpear.VipersReachReflexPenaltyBuff": ("6ac410ab82b81915d64249a213e1815a", "BlueprintBuff"),
        "KMG.ElvenBranchedSpear.VipersReachRoundMarker": ("dcc7832d9ed7558111ee97da668522fe", "BlueprintBuff"),
        "KMG.ElvenBranchedSpear.BriarCrownedRoundMarker": ("89cea1f236074e36051a68ece37aa05c", "BlueprintBuff"),
        "KMG.ElvenBranchedSpear.FirstBranchRoundMarker": ("1bb02c32918071bfa8333a12de4d7e94", "BlueprintBuff"),
        "KMG.ElvenBranchedSpear.FirstBranchSpeedPenaltyBuff": ("27d76fe829cc0234b7e120b19462848b", "BlueprintBuff"),
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
    expected_registration = ("ExpectedRegisteredBlueprintCount = 341 +"
        if bodyguard_entries else "ExpectedRegisteredBlueprintCount = 333 +"
        if focused_entries else "ExpectedRegisteredBlueprintCount = 329 +"
        if eastern_entries else "ExpectedRegisteredBlueprintCount = 283 +"
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
