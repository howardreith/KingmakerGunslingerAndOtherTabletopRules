#!/usr/bin/env python3
"""Generate the exhaustive deterministic custom-weapon visual audit."""

from __future__ import annotations

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "blueprints" / "blueprints.json"
JSON_OUTPUT = ROOT / "docs" / "weapon-visual-mapping-audit.json"
MARKDOWN_OUTPUT = ROOT / "docs" / "WEAPON-VISUAL-MAPPING-AUDIT.md"

DISPLAY_NAMES = {
    "KMG.Test.TestMusketItem": "Test Musket",
    "KMG.Firearms.EarlyPistolItem": "Pistol",
    "KMG.Firearms.EarlyMusketItem": "Musket",
    "KMG.Firearms.EarlyBlunderbussItem": "Blunderbuss",
    "KMG.Firearms.AdvancedRifleItem": "Advanced Rifle",
    "KMG.Firearms.AdvancedRevolverItem": "Advanced Revolver",
    "KMG.Firearms.PistolPlus1Item": "Pistol +1",
    "KMG.Firearms.MusketPlus1Item": "Musket +1",
    "KMG.Firearms.BlunderbussPlus1Item": "Blunderbuss +1",
    "KMG.Firearms.DuelistsRebuttalItem": "Duelist's Rebuttal",
    "KMG.Firearms.RiverKingsMeasureItem": "The River King's Measure",
    "KMG.Firearms.IrovettisOvationItem": "Irovetti's Ovation",
    "KMG.Firearms.TheLastWordItem": "The Last Word",
    "KMG.Firearms.WatchAtTheWorldsEndItem": "Watch at the World's End",
    "KMG.Deeds.PistolWhipOneHandedItem": "Pistol-Whip (One-Handed)",
    "KMG.Deeds.PistolWhipTwoHandedItem": "Pistol-Whip (Two-Handed)",
    "KMG.ElvenBranchedSpear.BaseItem": "Elven Branched Spear",
    "KMG.ElvenBranchedSpear.MasterworkItem": "Masterwork Elven Branched Spear",
    "KMG.ElvenBranchedSpear.ColdIronItem": "Cold Iron Elven Branched Spear",
    "KMG.ElvenBranchedSpear.MasterworkColdIronItem":
        "Masterwork Cold Iron Elven Branched Spear",
    "KMG.ElvenBranchedSpear.Plus1Item": "+1 Elven Branched Spear",
    "KMG.ElvenBranchedSpear.Plus1ColdIronItem":
        "+1 Cold Iron Elven Branched Spear",
    "KMG.ElvenBranchedSpear.Boughkeeper": "Boughkeeper",
    "KMG.ElvenBranchedSpear.Thornstep": "Thornstep",
    "KMG.ElvenBranchedSpear.MoonlitFork": "Moonlit Fork",
    "KMG.ElvenBranchedSpear.VipersReach": "Viper's Reach",
    "KMG.ElvenBranchedSpear.BriarCrownedSpear": "Briar-Crowned Spear",
    "KMG.ElvenBranchedSpear.SpearOfTheFirstBranch":
        "Spear of the First Branch",
    "KMG.EasternWeapons.Wakizashi.PaperLantern": "Paper Lantern",
    "KMG.EasternWeapons.Wakizashi.QuietCurrent": "Quiet Current",
    "KMG.EasternWeapons.Wakizashi.FallingPetal": "Falling Petal",
    "KMG.EasternWeapons.Wakizashi.FoxfireWhisper": "Foxfire Whisper",
    "KMG.EasternWeapons.Wakizashi.EmptySleeve": "Empty Sleeve",
    "KMG.EasternWeapons.Wakizashi.NightWithoutMoon": "Night Without Moon",
    "KMG.EasternWeapons.Katana.WayfarersOath": "Wayfarer's Oath",
    "KMG.EasternWeapons.Katana.WinterReed": "Winter Reed",
    "KMG.EasternWeapons.Katana.DrawnHorizon": "Drawn Horizon",
    "KMG.EasternWeapons.Katana.ThunderAtTheGate": "Thunder at the Gate",
    "KMG.EasternWeapons.Katana.MoonlitCrossing": "Moonlit Crossing",
    "KMG.EasternWeapons.Katana.HeavensMeasure": "Heaven's Measure",
    "KMG.EasternWeapons.Nodachi.BorderSentinel": "Border Sentinel",
    "KMG.EasternWeapons.Nodachi.CloudCleaver": "Cloud-Cleaver",
    "KMG.EasternWeapons.Nodachi.StormOverStone": "Storm Over Stone",
    "KMG.EasternWeapons.Nodachi.MountainSunder": "Mountain-Sunder",
    "KMG.EasternWeapons.Nodachi.UnfixedForm": "Unfixed Form",
    "KMG.EasternWeapons.Nodachi.WorldTreeSeverer": "World-Tree Severer",
}

FIREARM_DATA = {
    "Pistol": {
        "type": "KMG.Firearms.EarlyPistolWeaponType",
        "source": "assets-source/third-party/models/cyril43-flintlock-pistol/source/pistol.zip!/model/model.dae",
        "provenance": "Cyril43, Flintlock pistol, CC-BY-4.0; original archive preserved",
        "animation": "PiercingOneHanded; inherited native attachment slots",
        "grip": "one-handed firing grip; no support-hand IK; +Z muzzle",
    },
    "Musket": {
        "type": "KMG.Firearms.EarlyMusketWeaponType",
        "source": "assets-source/third-party/models/mesh-masters-rifle-musket/source/Musket 01.fbx",
        "provenance": "Mesh Masters, Flintlock Rifle, CC-BY-4.0; original FBX preserved",
        "animation": "Crossbow; inherited native attachment slots",
        "grip": "two-handed; identity firing grip plus SupportHandTarget; +Z muzzle",
    },
    "Blunderbuss": {
        "type": "KMG.Firearms.EarlyBlunderbussWeaponType",
        "source": "assets-source/third-party/models/ccotwist-blunderbuss/source/Blunderbuss_Low_Poly.fbx",
        "provenance": "ccotwist, Blunderbuss Low Poly, CC-BY-4.0; original FBX preserved",
        "animation": "Crossbow; inherited native attachment slots",
        "grip": "two-handed; identity firing grip plus SupportHandTarget; +Z muzzle",
    },
    "Rifle": {
        "type": "KMG.Firearms.AdvancedRifleWeaponType",
        "source": "assets-source/third-party/models/killian-delias-winchester-lever-action-rifle/source/fusilALevier.fbx",
        "provenance": "Killian Delias, Winchester lever action rifle, CC-BY-4.0; corrected identity record",
        "animation": "Crossbow; inherited native attachment slots",
        "grip": "two-handed; identity firing grip plus SupportHandTarget; +Z muzzle",
    },
    "Revolver": {
        "type": "KMG.Firearms.AdvancedRevolverWeaponType",
        "source": "assets-source/third-party/models/1851-navy-colt-revolver/source/Final2 Sketchfab.fbx",
        "provenance": "Steven Jurriaans, 1851 Colt Navy Revolver, CC-BY-4.0; original FBX preserved",
        "animation": "PiercingOneHanded; inherited native attachment slots",
        "grip": "one-handed firing grip; no support-hand IK; +Z muzzle",
    },
}

ARTIFACTS = {
    "KMG.Firearms.TheLastWordItem",
    "KMG.Firearms.WatchAtTheWorldsEndItem",
    "KMG.ElvenBranchedSpear.SpearOfTheFirstBranch",
    "KMG.EasternWeapons.Wakizashi.NightWithoutMoon",
    "KMG.EasternWeapons.Katana.HeavensMeasure",
    "KMG.EasternWeapons.Nodachi.WorldTreeSeverer",
}


def firearm_kind(symbol: str) -> str:
    if symbol == "KMG.Test.TestMusketItem":
        return "Musket"
    for kind in FIREARM_DATA:
        if kind in symbol or (kind == "Musket" and "RiverKings" in symbol) or \
                (kind == "Blunderbuss" and "Irovettis" in symbol) or \
                (kind == "Pistol" and any(token in symbol for token in
                    ("Duelists", "LastWord"))) or \
                (kind == "Musket" and "WatchAt" in symbol):
            return kind
    raise ValueError(f"Unknown firearm family for {symbol}")


def generic_eastern_name(symbol: str, family: str) -> str:
    if symbol.endswith(".BaseItem"):
        return family
    if symbol.endswith(".MasterworkItem"):
        return f"Masterwork {family}"
    if symbol.endswith(".ColdIronItem"):
        return f"Cold Iron {family}"
    if symbol.endswith(".Plus1Item"):
        return f"+1 {family}"
    raise ValueError(f"Missing Eastern display name for {symbol}")


def tier(symbol: str) -> str:
    if symbol in ARTIFACTS:
        return "artifact-tier"
    if symbol.startswith("KMG.Deeds.") or symbol.startswith("KMG.Summoning."):
        return "mechanics-only"
    if any(token in symbol for token in
           ("BaseItem", "MasterworkItem", "ColdIronItem", "Early", "Advanced")):
        return "generic"
    if "Plus1" in symbol or "MasterworkColdIron" in symbol:
        return "enhanced"
    return "named"


def firearm_variant(symbol: str, kind: str) -> str:
    named = {
        "KMG.Firearms.DuelistsRebuttalItem": "Pistol.Duelist",
        "KMG.Firearms.TheLastWordItem": "Pistol.LastWord",
        "KMG.Firearms.RiverKingsMeasureItem": "Musket.RiverKing",
        "KMG.Firearms.WatchAtTheWorldsEndItem": "Musket.WorldsEnd",
        "KMG.Firearms.IrovettisOvationItem": "Blunderbuss.Ovation",
        "KMG.Test.TestMusketItem": "Musket.PassThroughDevelopment",
    }
    return named.get(symbol, f"{kind}.Service")


def eastern_variant(symbol: str, family: str) -> str:
    if symbol.endswith(".ColdIronItem"):
        return {"Wakizashi": "Wakizashi.Petal",
                "Katana": "Katana.Reed",
                "Nodachi": "Nodachi.Cleaver"}[family]
    if symbol.endswith((".BaseItem", ".MasterworkItem", ".Plus1Item")):
        return f"{family}.Classic"
    capstones = {
        "KMG.EasternWeapons.Wakizashi.NightWithoutMoon",
        "KMG.EasternWeapons.Katana.HeavensMeasure",
        "KMG.EasternWeapons.Nodachi.WorldTreeSeverer",
    }
    if symbol in capstones:
        return f"{family}.Capstone"
    family_named = {
        "Wakizashi": {
            "PaperLantern": "Petal", "QuietCurrent": "Petal",
            "FallingPetal": "Petal", "FoxfireWhisper": "Moon",
            "EmptySleeve": "Moon",
        },
        "Katana": {
            "WayfarersOath": "Reed", "WinterReed": "Reed",
            "DrawnHorizon": "Reed", "ThunderAtTheGate": "Regal",
            "MoonlitCrossing": "Regal",
        },
        "Nodachi": {
            "BorderSentinel": "Cleaver", "CloudCleaver": "Cleaver",
            "StormOverStone": "Cleaver", "MountainSunder": "Titan",
            "UnfixedForm": "Titan",
        },
    }
    key = symbol.rsplit(".", 1)[1]
    return f"{family}.{family_named[family][key]}"


def spear_variant(symbol: str) -> str:
    if symbol.endswith(("BaseItem", "MasterworkItem", "ColdIronItem",
                        "MasterworkColdIronItem", "Plus1Item",
                        "Plus1ColdIronItem")):
        return "ElvenBranchedSpear.ClassicBranch"
    if symbol.endswith(("Boughkeeper", "Thornstep", "MoonlitFork")):
        return "ElvenBranchedSpear.ThornBranch"
    return "ElvenBranchedSpear.CrownBranch"


def make_record(entry: dict) -> dict:
    symbol = entry["symbol"]
    common = {
        "symbolicIdentity": symbol,
        "assetGuid": entry["guid"],
        "displayedName": DISPLAY_NAMES.get(symbol, symbol.rsplit(".", 1)[1]),
        "tier": tier(symbol),
    }
    if symbol.startswith("KMG.Firearms.") or symbol.startswith("KMG.Test."):
        kind = firearm_kind(symbol)
        data = FIREARM_DATA[kind]
        prefab = f"KMG_Firearms_{kind}"
        weapon_type = "KMG.Test.TestMusketWeaponType" if symbol.startswith(
            "KMG.Test.") else data["type"]
        concern = "none accepted for regular Pistol" if kind == "Pistol" else \
            "residual torso/upper-arm clipping; human matrix required" if kind in \
            ("Musket", "Blunderbuss") else \
            "qualified structurally; lower-priority human visual review remains"
        common.update({
            "familyOrFirearmKind": kind,
            "weaponType": weapon_type,
            "currentItemLevelVisual": prefab,
            "currentTypeLevelVisual": prefab,
            "effectiveEquippedPrefab": prefab,
            "sourceFbx": data["source"],
            "sourceBlend": "none at baseline",
            "deterministicGenerator": "none at baseline; tools/unity/BuildFirearmBundles.cs imports the licensed source",
            "animationDonorStyle": data["animation"],
            "gripHandednessContract": data["grip"],
            "currentMaterial": f"opaque Unity Standard; generated {kind}_* material assets",
            "currentBundle": "kingmakergunslinger.firearms",
            "sourceLicenseProvenance": data["provenance"],
            "currentManyToOneVisualGroup": f"FirearmKind.{kind}",
            "proposedVisualVariant": firearm_variant(symbol, kind),
            "clippingOrientationConcerns": concern,
            "mappingScope": "equipped project weapon",
        })
        return common

    if symbol.startswith("KMG.EasternWeapons."):
        family = symbol.split(".")[2]
        common["displayedName"] = DISPLAY_NAMES[symbol] if symbol in \
            DISPLAY_NAMES else generic_eastern_name(symbol, family)
        prefab = f"KMG_EasternWeapons_{family}"
        common.update({
            "familyOrFirearmKind": family,
            "weaponType": f"KMG.EasternWeapons.{family}.WeaponType",
            "currentItemLevelVisual": prefab,
            "currentTypeLevelVisual": prefab,
            "effectiveEquippedPrefab": prefab,
            "sourceFbx": f"assets-source/original-models/eastern-weapons/{family.lower()}.fbx",
            "sourceBlend": "assets-source/original-models/eastern-weapons/eastern-weapons.blend",
            "deterministicGenerator": "assets-source/original-models/eastern-weapons/generate_eastern_weapons.py",
            "animationDonorStyle": "exact native donor GUID from EasternWeaponBlueprints; AnimStyle preserved",
            "gripHandednessContract": "Wakizashi light; Katana versatile; Nodachi two-handed; +Z blade axis",
            "currentMaterial": "project-owned Blender materials forced to opaque Unity Standard",
            "currentBundle": "kingmakergunslinger.easternweapons",
            "sourceLicenseProvenance": "project-owned clean-room Blender geometry; repository provenance/build reports",
            "currentManyToOneVisualGroup": f"EasternWeaponFamily.{family}",
            "proposedVisualVariant": eastern_variant(symbol, family),
            "clippingOrientationConcerns": "preserve family length ordering, blade edge orientation, hand contact, and no cross-family substitution",
            "mappingScope": "equipped project weapon",
        })
        return common

    if symbol.startswith("KMG.ElvenBranchedSpear."):
        common.update({
            "familyOrFirearmKind": "Elven Branched Spear",
            "weaponType": "KMG.ElvenBranchedSpear.WeaponType",
            "currentItemLevelVisual": "KMG_ElvenBranchedSpear",
            "currentTypeLevelVisual": "KMG_ElvenBranchedSpear",
            "effectiveEquippedPrefab": "KMG_ElvenBranchedSpear",
            "sourceFbx": "assets-source/original-models/elven-branched-spear/elven-branched-spear.fbx",
            "sourceBlend": "assets-source/original-models/elven-branched-spear/elven-branched-spear.blend",
            "deterministicGenerator": "assets-source/original-models/elven-branched-spear/generate_elven_branched_spear.py",
            "animationDonorStyle": "native Longspear VisualParameters and animation style",
            "gripHandednessContract": "two-handed reach polearm; Grip 0; support +0.48m; tip +2.01m; butt -0.915m",
            "currentMaterial": "project-owned Blender materials forced to opaque Unity Standard",
            "currentBundle": "kingmakergunslinger.elvenbranchedspear",
            "sourceLicenseProvenance": "project-owned clean-room Blender geometry; repository provenance/build reports",
            "currentManyToOneVisualGroup": "ElvenBranchedSpear.SingleBaseline",
            "proposedVisualVariant": spear_variant(symbol),
            "clippingOrientationConcerns": "branches must be unmistakable and remain outside both hands, forearms, torso, and shaft grip region",
            "mappingScope": "equipped project weapon",
        })
        return common

    if symbol.startswith("KMG.Deeds.PistolWhip"):
        handed = "OneHanded" if "OneHanded" in symbol else "TwoHanded"
        common.update({
            "familyOrFirearmKind": "Pistol-Whip rule-event weapon",
            "weaponType": f"KMG.Deeds.PistolWhip{handed}Type",
            "currentItemLevelVisual": "cloned source Pistol visual; never equipped",
            "currentTypeLevelVisual": "cloned source Pistol presentation",
            "effectiveEquippedPrefab": "not applicable; unowned RuleAttackWithWeapon item",
            "sourceFbx": FIREARM_DATA["Pistol"]["source"],
            "sourceBlend": "none",
            "deterministicGenerator": "none; blueprint clone only",
            "animationDonorStyle": "not applicable; synthetic melee rule event",
            "gripHandednessContract": "one-handed d6" if handed == "OneHanded" else "two-handed d10",
            "currentMaterial": "not instantiated",
            "currentBundle": "none",
            "sourceLicenseProvenance": FIREARM_DATA["Pistol"]["provenance"],
            "currentManyToOneVisualGroup": "MechanicsOnly.PistolWhip",
            "proposedVisualVariant": "not applicable",
            "clippingOrientationConcerns": "none; blueprint must remain unowned and unrendered",
            "mappingScope": "mechanics-only exclusion",
        })
        return common

    if symbol.startswith("KMG.Summoning."):
        weapon_type = "native donor type configured per summoned creature"
        if symbol.endswith("Special.Salamander.Spear"):
            weapon_type = "KMG.Summoning.Special.Salamander.SpearType"
        elif symbol.endswith("Special.Pixie.SleepBow"):
            weapon_type = "KMG.Summoning.Special.Pixie.SleepBowType"
        common.update({
            "familyOrFirearmKind": "Expanded Summoning creature weapon",
            "weaponType": weapon_type,
            "currentItemLevelVisual": "native donor clone configured by Expanded Summoning builders",
            "currentTypeLevelVisual": "native donor presentation",
            "effectiveEquippedPrefab": "creature/unit-view-owned native presentation",
            "sourceFbx": "native Kingmaker asset; no redistributed source FBX",
            "sourceBlend": "none",
            "deterministicGenerator": "none; exact native blueprint donor cloning",
            "animationDonorStyle": "native summoned-creature donor",
            "gripHandednessContract": "creature-specific natural or manufactured attack",
            "currentMaterial": "native donor material",
            "currentBundle": "none",
            "sourceLicenseProvenance": "Kingmaker runtime donor reference only; no proprietary asset redistribution",
            "currentManyToOneVisualGroup": "ExpandedSummoning.NativeDonor",
            "proposedVisualVariant": "not applicable",
            "clippingOrientationConcerns": "outside this cleanup; preserve qualified creature donor contract",
            "mappingScope": "summoning-only exclusion",
        })
        return common
    raise ValueError(f"Unclassified BlueprintItemWeapon {symbol}")


def markdown_escape(value: object) -> str:
    return str(value).replace("|", "\\|").replace("\n", " ")


def generate() -> None:
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    identity_by_symbol = {entry["symbol"]: entry["guid"]
                          for entry in manifest["entries"]}
    items = [entry for entry in manifest["entries"]
             if entry["plannedType"] == "BlueprintItemWeapon" and
             entry["status"] == "active"]
    records = [make_record(entry) for entry in items]
    for record in records:
        record["weaponTypeAssetGuid"] = identity_by_symbol.get(
            record["weaponType"], "native-runtime-donor")
    if len(records) != 68:
        raise RuntimeError(f"Expected all 68 active custom weapon items, got {len(records)}")
    symbols = [record["symbolicIdentity"] for record in records]
    guids = [record["assetGuid"] for record in records]
    if len(set(symbols)) != len(symbols) or len(set(guids)) != len(guids):
        raise RuntimeError("Audit identities are not unique")

    payload = {
        "schemaVersion": 1,
        "generatedFrom": "blueprints/blueprints.json plus repository source contracts",
        "generator": "scripts/generate_weapon_visual_audit.py",
        "activeBlueprintItemWeaponCount": len(records),
        "mappingPolicy": "exact deterministic blueprint identity; no runtime randomness or transient state",
        "items": records,
    }
    JSON_OUTPUT.write_text(json.dumps(payload, indent=2, ensure_ascii=False) + "\n",
                           encoding="utf-8", newline="\n")

    scope_counts = {}
    family_counts = {}
    for record in records:
        scope_counts[record["mappingScope"]] = scope_counts.get(
            record["mappingScope"], 0) + 1
        family_counts[record["familyOrFirearmKind"]] = family_counts.get(
            record["familyOrFirearmKind"], 0) + 1
    lines = [
        "# Weapon Visual Mapping Audit",
        "",
        "This file is generated by `scripts/generate_weapon_visual_audit.py`.",
        "The JSON counterpart is `docs/weapon-visual-mapping-audit.json`.",
        "Do not hand-edit either generated output.",
        "",
        "## Coverage and policy",
        "",
        f"The blueprint manifest contains exactly **{len(records)}** active",
        "`BlueprintItemWeapon` identities and every one is represented below.",
        "The audit includes equipped player/development weapons, mechanics-only",
        "Pistol-Whip items, and Expanded Summoning creature weapons. Cosmetic",
        "variant work is intentionally limited to equipped project weapons; the",
        "other identities remain explicit preserve-only exclusions.",
        "",
        "Variant assignment is by stable blueprint identity only. Runtime",
        "randomness, object hashes, iteration order, save state, transient item",
        "identity, character identity, and time are forbidden.",
        "",
        "### Scope counts",
        "",
        "| Scope | Count |",
        "|---|---:|",
    ]
    for key in sorted(scope_counts):
        lines.append(f"| {markdown_escape(key)} | {scope_counts[key]} |")
    lines += ["", "### Family counts", "", "| Family/kind | Count |",
              "|---|---:|"]
    for key in sorted(family_counts):
        lines.append(f"| {markdown_escape(key)} | {family_counts[key]} |")

    identity_columns = ["symbolicIdentity", "assetGuid", "displayedName",
                        "familyOrFirearmKind", "weaponType",
                        "weaponTypeAssetGuid", "tier",
                        "currentManyToOneVisualGroup", "proposedVisualVariant",
                        "mappingScope"]
    lines += ["", "## Exact identity and variant mapping", "",
              "| " + " | ".join(identity_columns) + " |",
              "|" + "|".join("---" for _ in identity_columns) + "|"]
    for record in records:
        lines.append("| " + " | ".join(markdown_escape(record[column])
            for column in identity_columns) + " |")

    source_columns = ["symbolicIdentity", "currentItemLevelVisual",
                      "currentTypeLevelVisual", "effectiveEquippedPrefab",
                      "sourceFbx", "sourceBlend", "deterministicGenerator",
                      "animationDonorStyle", "gripHandednessContract",
                      "currentMaterial", "currentBundle",
                      "sourceLicenseProvenance", "clippingOrientationConcerns"]
    lines += ["", "## Exact presentation and provenance contracts", "",
              "| " + " | ".join(source_columns) + " |",
              "|" + "|".join("---" for _ in source_columns) + "|"]
    for record in records:
        lines.append("| " + " | ".join(markdown_escape(record[column])
            for column in source_columns) + " |")
    lines += [
        "",
        "## Audit conclusion before asset authoring",
        "",
        "The baseline is many-to-one by family: one prefab per firearm kind, one",
        "per Eastern family, and one for all Elven Branched Spears. The proposed",
        "vocabulary is bounded and communicates family, origin, named importance,",
        "or artifact status without inventing one mesh per enhancement increment.",
        "The Musket and Blunderbuss proposed mappings remain candidates until the",
        "required graybox geometry decision gates and human side-by-side review.",
        "",
    ]
    MARKDOWN_OUTPUT.write_text("\n".join(lines), encoding="utf-8", newline="\n")


if __name__ == "__main__":
    generate()
