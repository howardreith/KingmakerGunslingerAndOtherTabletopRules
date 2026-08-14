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
        "src/KingmakerGunslinger/EasternWeapons/EasternWeaponCategoryRuntime.cs",
        "src/KingmakerGunslinger/Blueprints/EasternWeaponBlueprints.cs",
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
    for token in ("ExpectedRegisteredBlueprintCount = 298 +",
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
