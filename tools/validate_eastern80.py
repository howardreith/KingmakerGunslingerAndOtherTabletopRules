#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_spear79

VERSION = "0.0.80"
INFORMATIONAL_VERSION = "0.0.80-eastern-weapons"
PACKAGE = "KingmakerGunslinger-0.0.80-local-runtime.zip"

EXPECTED_GENERIC_IDENTITIES = {
    "KMG.EasternWeapons.Wakizashi.WeaponType":
        ("86bd3d7faf1aec1c527fb9c0d87a395c", "BlueprintWeaponType"),
    "KMG.EasternWeapons.Katana.WeaponType":
        ("85d96c1dd1eb02b381c2b3f8ad345952", "BlueprintWeaponType"),
    "KMG.EasternWeapons.Nodachi.WeaponType":
        ("41c269cc820ee734f437ab3fa20de198", "BlueprintWeaponType"),
    "KMG.EasternWeapons.Wakizashi.BaseItem":
        ("b61ee7e62bc9288004eb0121c8f5d37e", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Wakizashi.MasterworkItem":
        ("58fd0f272f4523458016dc3656b778c3", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Wakizashi.ColdIronItem":
        ("1aa01a0528eb595b5cbf19ac7c71a64e", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Wakizashi.Plus1Item":
        ("83a507873a518b54793d0da632def246", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.BaseItem":
        ("aba40a9e8302b31e4daa2acf6ab48a46", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.MasterworkItem":
        ("37e8c76b2fc196e9f82e1196e918263c", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.ColdIronItem":
        ("599f8f45f325911ffcbdbd6544ba114f", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.Plus1Item":
        ("87b3d851726a4a9abd0baec6beca957c", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.BaseItem":
        ("35b7082d98ff45ba51dce536a1bc68a1", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.MasterworkItem":
        ("df5a3b333eab59c04028d88084d7ada9", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.ColdIronItem":
        ("0db026048052031be931b9701b3859ef", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.Plus1Item":
        ("38e31ba5cdbdc668f8dcd8985070c0b7", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.ProficiencyPolicyEnchantment":
        ("1264cc0bf069541d8c2191d05fc40c5d", "BlueprintWeaponEnchantment"),
    "KMG.EasternWeapons.Wakizashi.ExoticWeaponProficiency":
        ("b14f7d9b2b665801a9d5b916c6be4ea9", "BlueprintFeature"),
    "KMG.EasternWeapons.Katana.ExoticWeaponProficiency":
        ("93ef81404f085e2a8b261bdab15d5a08", "BlueprintFeature"),
    "KMG.EasternWeapons.Wakizashi.FinesseTraining":
        ("dfdc2a631ebb5980934181c86e1c43fd", "BlueprintFeature"),
    "KMG.EasternWeapons.Wakizashi.PaperLantern":
        ("fbb319cb67ae5657820548791a7a3733", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Wakizashi.QuietCurrent":
        ("be05a24b1b145e1ea008a4bf42b04c32", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Wakizashi.FallingPetal":
        ("c56dd11c12355a83b1cd9d833b2e5321", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Wakizashi.FoxfireWhisper":
        ("c7fc72c801e9506bb0c87e84eee8d313", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Wakizashi.EmptySleeve":
        ("a576839afc71574eb77203bf390fdf30", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Wakizashi.NightWithoutMoon":
        ("dc660fcebcc855bfb046336fc78a93ae", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.WayfarersOath":
        ("9ac64342cca85f72b0fe81cb6b9c53c0", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.WinterReed":
        ("060f933d8912594cbc3da731c4dae7a3", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.DrawnHorizon":
        ("d3f2a227bd335087805eb7225721dc83", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.ThunderAtTheGate":
        ("d5c7922d57a95025a977dd1ee59cb098", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.MoonlitCrossing":
        ("457e6f3694405f27999cf46047fafa52", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Katana.HeavensMeasure":
        ("dc086bdf8af25bceb569c8f5c627f560", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.BorderSentinel":
        ("c1c7a6746916504ebfdcb2b650a7145b", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.CloudCleaver":
        ("bb863dabbf655059af768723cf6226ba", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.StormOverStone":
        ("a7559dde16945f90aada81ecf9adb97a", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.MountainSunder":
        ("5867c9be30e15d3a8a22e0f442959d03", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.UnfixedForm":
        ("f4bed29f193e57f6826dc83a684e65db", "BlueprintItemWeapon"),
    "KMG.EasternWeapons.Nodachi.WorldTreeSeverer":
        ("e6e5cf56d3a259debd2f16a300bff115", "BlueprintItemWeapon"),
}


def validate(root: Path) -> None:
    validate_spear79.VERSION = VERSION
    validate_spear79.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_spear79.PACKAGE = PACKAGE
    validate_spear79.MILESTONE_LABEL = "EASTERN-WEAPONS"
    validate_spear79.PACKAGE_SUFFIX = "eastern-weapons"
    validate_spear79.validate(root)

    required = (
        "planning/EASTERN-WEAPONS-MISSION.md",
        "EASTERN-WEAPONS-JOURNAL.md",
        "EASTERN-WEAPONS-STATE.json",
        "docs/EASTERN-WEAPONS-IMPLEMENTATION-EVIDENCE.md",
        "src/KingmakerGunslinger/CustomWeapons/CustomWeaponCategoryDefinition.cs",
        "src/KingmakerGunslinger/CustomWeapons/CustomWeaponCategoryRegistry.cs",
        "src/KingmakerGunslinger/EasternWeapons/EasternWeaponCatalog.cs",
        "src/KingmakerGunslinger/EasternWeapons/EasternWeaponNamedCatalog.cs",
        "src/KingmakerGunslinger/EasternWeapons/EasternWeaponCategoryRuntime.cs",
        "src/KingmakerGunslinger/Blueprints/EasternWeaponBlueprints.cs",
        "src/KingmakerGunslinger/Blueprints/EasternWeaponNamedBlueprints.cs",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Eastern Weapons file missing: {relative}")

    manifest = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))
    eastern = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith("KMG.EasternWeapons.")]
    by_symbol = {entry["symbol"]: entry for entry in eastern}
    for symbol, (guid, planned_type) in EXPECTED_GENERIC_IDENTITIES.items():
        entry = by_symbol.get(symbol)
        if (entry is None or entry.get("guid") != guid
                or entry.get("plannedType") != planned_type
                or entry.get("status") != "active"):
            raise AssertionError(f"Eastern Weapons identity mismatch: {symbol}")
    if len(eastern) < len(EXPECTED_GENERIC_IDENTITIES):
        raise AssertionError("Eastern Weapons identity ledger is incomplete")

    bootstrap = (root / "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs") \
        .read_text(encoding="utf-8")
    for token in ("ExpectedRegisteredBlueprintCount = 320 +",
            "EasternWeaponBlueprints.Register",
            "internal static EasternWeaponBlueprintSet EasternWeapons"):
        if token not in bootstrap:
            raise AssertionError(f"Eastern Weapons bootstrap contract missing: {token}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Eastern Weapons {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Eastern Weapons {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
