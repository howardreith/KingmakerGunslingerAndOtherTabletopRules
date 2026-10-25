#!/usr/bin/env python3
"""Generate the exhaustive deterministic custom-weapon visual audit."""

from __future__ import annotations

import json
import re
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
    "KMG.Firearms.RoadwardenItem": "Roadwarden",
    "KMG.Firearms.DeadReckoningItem": "Dead Reckoning",
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
        "source": "assets-source/third-party/models/firearm-long-gun-derivatives/musket-normalized.fbx",
        "provenance": "Modified Mesh Masters Flintlock Rifle, CC-BY-4.0; original FBX preserved; deterministic derivative recorded",
        "animation": "Crossbow; inherited native attachment slots",
        "grip": "two-handed; identity equipment root; canonical source frame converted to measured Heavy Crossbow held basis; SupportHandTarget follows the same semantic solve; independent BackMount",
    },
    "Blunderbuss": {
        "type": "KMG.Firearms.EarlyBlunderbussWeaponType",
        "source": "assets-source/third-party/models/firearm-long-gun-derivatives/blunderbuss-normalized.fbx",
        "provenance": "Modified ccotwist Blunderbuss Low Poly, CC-BY-4.0; original FBX preserved; deterministic derivative recorded",
        "animation": "Crossbow; inherited native attachment slots",
        "grip": "two-handed; identity equipment root; canonical source frame converted to measured Heavy Crossbow held basis; SupportHandTarget follows the same semantic solve; independent BackMount",
    },
    "Rifle": {
        "type": "KMG.Firearms.AdvancedRifleWeaponType",
        "source": "assets-source/third-party/models/firearm-long-gun-derivatives/rifle-normalized.fbx",
        "provenance": "Modified Killian Delias Winchester lever action rifle, CC-BY-4.0; original FBX preserved; deterministic derivative recorded",
        "animation": "Crossbow; inherited native attachment slots",
        "grip": "two-handed; identity equipment root; canonical source frame converted to measured Heavy Crossbow held basis; SupportHandTarget follows the same semantic solve; independent BackMount",
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


PROGRESSION_FIREARM = re.compile(
    r"^KMG\.Firearms\.(Reliable)?(Pistol|Musket|Blunderbuss)Plus([1-5])Item$")
PROGRESSION_EASTERN = re.compile(
    r"^KMG\.EasternWeapons\.(Wakizashi|Katana|Nodachi)\.Plus([2-5])Item$")
PROGRESSION_SPEAR = re.compile(r"^KMG\.ElvenBranchedSpear\.Plus([2-5])Item$")


def progression_display_name(symbol: str):
    """Better Vendors progression variants follow their family's generic name."""
    match = PROGRESSION_FIREARM.match(symbol)
    if match and (match.group(1) or match.group(3) != "1"):
        prefix = "Reliable " if match.group(1) else ""
        return f"{prefix}{match.group(2)} +{match.group(3)}"
    match = PROGRESSION_SPEAR.match(symbol)
    if match:
        return f"+{match.group(1)} Elven Branched Spear"
    return None


def firearm_kind(symbol: str) -> str:
    if symbol == "KMG.Test.TestMusketItem":
        return "Musket"
    if symbol == "KMG.Firearms.RoadwardenItem":
        return "Musket"
    if symbol == "KMG.Firearms.DeadReckoningItem":
        return "Pistol"
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
    match = PROGRESSION_EASTERN.match(symbol)
    if match:
        return f"+{match.group(2)} {family}"
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
    if (PROGRESSION_FIREARM.match(symbol) or PROGRESSION_EASTERN.match(symbol)
            or PROGRESSION_SPEAR.match(symbol)):
        return "enhanced"
    return "named"


def firearm_variant(symbol: str, kind: str) -> str:
    named = {
        "KMG.Firearms.DuelistsRebuttalItem": "Pistol.Duelist",
        "KMG.Firearms.TheLastWordItem": "Pistol.LastWord",
    }
    return named.get(symbol, f"{kind}.Service")


def firearm_prefab(variant: str) -> str:
    return {
        "Pistol.Service": "Pistol",
        "Pistol.Duelist": "PistolDuelist",
        "Pistol.LastWord": "PistolLastWord",
        "Musket.Service": "Musket",
        "Blunderbuss.Service": "Blunderbuss",
        "Rifle.Service": "Rifle",
        "Revolver.Service": "Revolver",
    }[variant]


def eastern_variant(symbol: str, family: str) -> str:
    if symbol.endswith(".ColdIronItem"):
        return {"Wakizashi": "Wakizashi.Petal",
                "Katana": "Katana.Reed",
                "Nodachi": "Nodachi.Cleaver"}[family]
    if (symbol.endswith((".BaseItem", ".MasterworkItem", ".Plus1Item"))
            or PROGRESSION_EASTERN.match(symbol)):
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
                        "Plus1ColdIronItem")) or PROGRESSION_SPEAR.match(symbol):
        return "ElvenBranchedSpear.ClassicBranch"
    if symbol.endswith(("Boughkeeper", "Thornstep", "MoonlitFork")):
        return "ElvenBranchedSpear.ThornBranch"
    return "ElvenBranchedSpear.CrownBranch"


def make_record(entry: dict) -> dict:
    symbol = entry["symbol"]
    common = {
        "symbolicIdentity": symbol,
        "assetGuid": entry["guid"],
        "displayedName": DISPLAY_NAMES.get(symbol) or
            progression_display_name(symbol) or symbol.rsplit(".", 1)[1],
        "tier": tier(symbol),
    }
    if symbol.startswith("KMG.Firearms.") or symbol.startswith("KMG.Test."):
        kind = firearm_kind(symbol)
        data = FIREARM_DATA[kind]
        variant = firearm_variant(symbol, kind)
        prefab = firearm_prefab(variant)
        project_variant = variant in ("Pistol.Duelist", "Pistol.LastWord")
        source_fbx = data["source"] if not project_variant else \
            "assets-source/original-models/firearm-pistol-variants/" + \
            ("pistol-duelist.fbx" if variant == "Pistol.Duelist" else
             "pistol-last-word.fbx")
        weapon_type = "KMG.Test.TestMusketWeaponType" if symbol.startswith(
            "KMG.Test.") else data["type"]
        concern = "human inventory/world readability and hand fit remain required" if kind == "Pistol" else \
            "runtime-qualified default Medium male held/stored/ready/fire; reload, locomotion, and body matrix open" if kind in \
            ("Musket", "Blunderbuss", "Rifle") else \
            "qualified structurally; lower-priority human visual review remains"
        common.update({
            "familyOrFirearmKind": kind,
            "weaponType": weapon_type,
            "currentItemLevelVisual": prefab,
            "currentTypeLevelVisual": kind,
            "effectiveEquippedPrefab": prefab,
            "sourceFbx": source_fbx,
            "sourceBlend": "assets-source/original-models/firearm-pistol-variants/firearm-pistol-variants.blend" if project_variant else "none; preserved licensed source imported directly",
            "deterministicGenerator": "assets-source/original-models/firearm-pistol-variants/generate_firearm_pistol_variants.py" if project_variant else ("assets-source/third-party/models/firearm-long-gun-derivatives/generate_long_gun_derivatives.py plus tools/unity/BuildFirearmBundles.cs" if kind in ("Musket", "Blunderbuss", "Rifle") else "tools/unity/BuildFirearmBundles.cs imports the preserved licensed source"),
            "animationDonorStyle": data["animation"],
            "gripHandednessContract": data["grip"],
            "currentMaterial": "project-owned three-material source forced to opaque Unity Standard" if project_variant else f"opaque Unity Standard; generated {kind}_* material assets",
            "currentBundle": "kingmakergunslinger.firearms",
            "sourceLicenseProvenance": "project-owned clean-room deterministic Blender geometry; repository LICENSE" if project_variant else data["provenance"],
            "currentManyToOneVisualGroup": variant,
            "proposedVisualVariant": variant,
            "clippingOrientationConcerns": concern,
            "mappingScope": "equipped project weapon",
        })
        return common

    if symbol.startswith("KMG.EasternWeapons."):
        family = symbol.split(".")[2]
        common["displayedName"] = DISPLAY_NAMES[symbol] if symbol in \
            DISPLAY_NAMES else generic_eastern_name(symbol, family)
        variant = eastern_variant(symbol, family)
        suffix = variant.split(".", 1)[1]
        prefab = family if suffix == "Classic" else family + suffix
        source_stem = family.lower() if suffix == "Classic" else \
            family.lower() + "-" + suffix.lower()
        common.update({
            "familyOrFirearmKind": family,
            "weaponType": f"KMG.EasternWeapons.{family}.WeaponType",
            "currentItemLevelVisual": prefab,
            "currentTypeLevelVisual": family,
            "effectiveEquippedPrefab": prefab,
            "sourceFbx": f"assets-source/original-models/eastern-weapons/{source_stem}.fbx",
            "sourceBlend": "assets-source/original-models/eastern-weapons/eastern-weapons.blend",
            "deterministicGenerator": "assets-source/original-models/eastern-weapons/generate_eastern_weapons.py",
            "animationDonorStyle": "exact native donor GUID from EasternWeaponBlueprints; AnimStyle preserved",
            "gripHandednessContract": "Wakizashi light; Katana versatile; Nodachi two-handed; +Z blade axis",
            "currentMaterial": "project-owned Blender materials forced to opaque Unity Standard",
            "currentBundle": "kingmakergunslinger.easternweapons",
            "sourceLicenseProvenance": "project-owned clean-room Blender geometry; repository provenance/build reports",
            "currentManyToOneVisualGroup": variant,
            "proposedVisualVariant": variant,
            "clippingOrientationConcerns": "preserve family length ordering, blade edge orientation, hand contact, and no cross-family substitution",
            "mappingScope": "equipped project weapon",
        })
        return common

    if symbol.startswith("KMG.ElvenBranchedSpear."):
        variant = spear_variant(symbol)
        suffix = variant.split(".", 1)[1]
        prefab = {"ClassicBranch": "ElvenBranchedSpear",
                  "ThornBranch": "ElvenBranchedSpearThorn",
                  "CrownBranch": "ElvenBranchedSpearCrown"}[suffix]
        source = {"ClassicBranch": "elven-branched-spear.fbx",
                  "ThornBranch": "elven-branched-spear-thorn.fbx",
                  "CrownBranch": "elven-branched-spear-crown.fbx"}[suffix]
        common.update({
            "familyOrFirearmKind": "Elven Branched Spear",
            "weaponType": "KMG.ElvenBranchedSpear.WeaponType",
            "currentItemLevelVisual": prefab,
            "currentTypeLevelVisual": "ElvenBranchedSpear",
            "effectiveEquippedPrefab": prefab,
            "effectiveStoredPrefab": prefab + "Back",
            "sourceFbx": "assets-source/original-models/elven-branched-spear/" + source,
            "sourceBlend": "assets-source/original-models/elven-branched-spear/elven-branched-spear.blend",
            "deterministicGenerator": "assets-source/original-models/elven-branched-spear/generate_elven_branched_spear.py",
            "animationDonorStyle": "native Longspear VisualParameters and animation style",
            "gripHandednessContract": "two-handed reach polearm; source +Z physical-tip/+Y head-normal frame with Grip 0, support +0.593016m, tip +1.14m, and butt -1.14m; semantic basis converted to the measured native Longspear held frame",
            "storedPresentationContract": "independent Back prefab converts the same authored frame to the measured native Longspear BeltModel basis and aligns the mesh-derived KMG_Back center to the donor renderer anchor",
            "supportHandContract": "held-only EquipmentOffsets.IkTargetLeftHand resolves exactly to SupportHandTarget; stored prefabs never drive hand IK",
            "currentMaterial": "project-owned Blender materials forced to opaque Unity Standard",
            "currentBundle": "kingmakergunslinger.elvenbranchedspear",
            "sourceLicenseProvenance": "project-owned clean-room Blender geometry; repository provenance/build reports",
            "currentManyToOneVisualGroup": variant,
            "proposedVisualVariant": variant,
            "clippingOrientationConcerns": "physical central spearhead must lead; branches must retain donor-matched roll and remain outside both hands, forearms, torso, and shaft grip region; held and stored frames are independent",
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
        if symbol.startswith("KMG.Summoning.OutsiderCelestial."):
            species, attack = symbol.split(".")[-2:]
            weapon_kind = attack.split("1d")[0].split("2d")[0]
            manufactured = weapon_kind in ("Greatsword", "Warhammer")
            common.update({
                "familyOrFirearmKind": "Sprints 30-31 outsider/celestial attack",
                "weaponType": symbol.rsplit(".", 1)[0] + "." + weapon_kind + "Type",
                "currentItemLevelVisual": "summon-owned native weapon reference" if manufactured else "natural attack; body-owned presentation",
                "effectiveEquippedPrefab": "summon-owned native manufactured weapon" if manufactured else "project creature body; no separate transferable visual",
                "gripHandednessContract": "two-handed manufactured weapon" if manufactured else "creature-specific natural attack role; alternative slam or bite where configured",
                "currentManyToOneVisualGroup": "ExpandedSummoning.OutsiderCelestial." + species,
                "clippingOrientationConcerns": "functional-first body and armament; rough anatomy, seams, rigid wings and modest clipping remain accepted visual debt",
            })
            if species == "HoundArchon" and manufactured:
                common.update({
                    "effectiveEquippedPrefab": "native greatsword visual attached to the owned Free Hands rig right palm by HoundArchonSwordView; no extra item",
                    "deterministicGenerator": "HoundArchonSwordView exact-owner runtime attachment of existing weapon visual",
                    "animationDonorStyle": "measured Free Hands rig; native hand equipment unavailable without Character avatar",
                })
        elif symbol.startswith("KMG.Summoning.Natural.Gorgon."):
            common.update({
                "effectiveEquippedPrefab": "original project Gorgon body: assets/ungulates/gorgon-mesh.json",
                "sourceFbx": "none; original procedural mesh data",
                "sourceBlend": "none; procedural geometry",
                "deterministicGenerator": "assets-source/original-models/sprint26-gorgon/build_gorgon.py",
                "animationDonorStyle": "native Horse 5bb9579fdb2b26b48bb10d61c81cfdfb; runtime rig/animation reference",
                "currentMaterial": "project gorgon-albedo.png on per-view native-compatible material",
                "sourceLicenseProvenance": "project-authored geometry/albedo; native rig/animation referenced only",
                "currentManyToOneVisualGroup": "ExpandedSummoning.GorgonOriginal",
                "clippingOrientationConcerns": "rough armor proportions, contact and secondary motion are deferred visual debt",
            })
        elif symbol.startswith("KMG.Summoning.Natural.Griffon."):
            common.update({
                "effectiveEquippedPrefab": "original project Griffon body: assets/sprint27-griffon/griffon-mesh.json",
                "sourceFbx": "none; original procedural mesh data",
                "sourceBlend": "none; procedural geometry",
                "deterministicGenerator": "assets-source/original-models/sprint27-griffon/build_griffon.py",
                "animationDonorStyle": "native Manticore da807351572c540478b703957d863e68; runtime rig/animation reference",
                "currentMaterial": "project griffon-albedo.png on per-view native-compatible material",
                "sourceLicenseProvenance": "project-authored geometry/albedo; native rig/animation referenced only",
                "currentManyToOneVisualGroup": "ExpandedSummoning.GriffonOriginal",
                "clippingOrientationConcerns": "coarse anatomy, rigid pointed wings and simplified transitions are deferred visual debt",
            })
        elif symbol.startswith("KMG.Summoning.Natural.Bulette."):
            common.update({
                "effectiveEquippedPrefab": "exact native adult Bulette prefab 46b4f98ae7d8ddf40878914741a42129",
                "animationDonorStyle": "native adult Bulette 18fa241bd1afd77438564ec92614f7f1",
                "clippingOrientationConcerns": "native body with above-ground legal charge/leap; no underground system",
            })
            if symbol.endswith(".Bite2d8"):
                common["weaponType"] = "KMG.Summoning.Natural.Bulette.BiteType"
        elif symbol.startswith("KMG.Summoning.Giant."):
            name = symbol.rsplit(".", 1)[-1]
            is_rock = "Rock" in name
            common.update({
                "familyOrFirearmKind": "Sprint 28 giant humanoid attack",
                "weaponType": ("KMG.Summoning.Giant." + name.replace("1d8", "Type")) if is_rock else "native donor type configured per giant",
                "currentItemLevelVisual": "summon-owned native weapon reference" if name.startswith(("Greatclub", "Greatsword")) else "native attack-mode presentation; no collectible item",
                "effectiveEquippedPrefab": "summon-owned native manufactured weapon" if name.startswith(("Greatclub", "Greatsword")) else "giant attack mode; no transferable item",
                "animationDonorStyle": "native giant attack animation",
                "gripHandednessContract": "two-handed manufactured weapon" if name.startswith(("Greatclub", "Greatsword")) else ("ranged thrown-rock attack" if is_rock else "primary slam alternative"),
                "currentManyToOneVisualGroup": "ExpandedSummoning.GiantNative" + ("Rock" if is_rock else "Melee"),
                "clippingOrientationConcerns": "usable native giant body and attack presentation; visual proportions and motion remain backlog",
            })
            sprint29_types = {
                "EttinMainFlail2d6": "Sprint29.EttinFlailType",
                "EttinOffFlail2d6": "Sprint29.EttinFlailType",
                "EttinMainJavelin1d8": "Sprint29.EttinJavelinType",
                "EttinOffJavelin1d8": "Sprint29.EttinJavelinType",
                "StormGreatsword4d6": "Sprint29.StormGreatswordType",
                "StormSlam2d6": "Sprint29.StormSlamType",
                "StormCompositeBow3d6": "Sprint29.StormCompositeBowType",
                "CloudMorningstar4d6": "Sprint29.CloudMorningstarType",
                "CloudSlam2d6": "Sprint29.CloudSlamType",
                "CloudRock2d6": "CloudRockType",
            }
            if name in sprint29_types:
                weapon_type = sprint29_types[name]
                common.update({
                    "familyOrFirearmKind": "Sprint 29 hidden giant humanoid attack",
                    "weaponType": "KMG.Summoning.Giant." + weapon_type,
                    "currentItemLevelVisual": "summon-owned native weapon reference",
                    "effectiveEquippedPrefab": "summon-owned native weapon; no transferable item",
                    "gripHandednessContract": (
                        "paired independent hands" if name.startswith("Ettin") else
                        "ranged bow attack" if "Bow" in name else
                        "ranged thrown attack" if "Javelin" in name or is_rock else
                        "primary slam alternative" if "Slam" in name else
                        "manufactured melee attack"),
                    "currentManyToOneVisualGroup": "ExpandedSummoning.Sprint29Native",
                    "clippingOrientationConcerns": "native giant armament; live body and attack presentation pending qualification",
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
    # 70 pre-existing identities, the Sprint 8 claw, the Sprint 10 Wasp
    # sting and hidden Stirge touch carrier, the Sprint 13 Poison Frog flat-1
    # bite, the Sprint 23 gore and spiked tail, the Sprint 24 talon and
    # powerful bite, the Sprint 25 Brachiosaurus 4d6 tail, and 43 Better
    # Vendors variants. Sprint26 adds exactly four creature-owned natural
    # weapons; the source/identity gate owns their exact approved set.
    if not records:
        raise RuntimeError("The active custom weapon manifest is empty")
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
