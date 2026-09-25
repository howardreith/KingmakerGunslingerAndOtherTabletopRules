#!/usr/bin/env python3
"""Allocate once and validate the frozen Expanded Summoning foundation IDs."""

import argparse
import json
import re
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CATALOG = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
DONORS = ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs"
MANIFEST = ROOT / "blueprints/blueprints.json"
ROSTER = ROOT / "planning/EXPANDED-SUMMONING-ROSTER.md"
ENTRY = re.compile(
    r'C\("([^"]+)","([^"]+)",(null|\d+),(true|false),(null|\d+)'
    r'(?:,"([^"]+)")?\)')
DONOR = re.compile(r'"([a-z0-9-]+)\|([0-9a-f]{32})\|([01])"')

DONOR_NAMES = {
    "028cc6f46e7998f46855a33ffde89567": "MastodonSummon",
    "03dd28e92faf2e44eb9564a6ba01fdd0": "DireWolfSummon",
    "04944455200bc224d955a8e9bbd64f3f": "SummonedAirElementalSmall",
    "0b214d8e81a563549ba0be37cd1c16d0": "CR4_BearStandard",
    "124f1c45ef24d654e9cd420fe84f7f36": "CR5_CyclopStandard",
    "0cc7a2526e4557945b1d8eb277d1fb3a": "CR7_Nymph",
    "10a820de0a417f345866f794324205ad": "MephitFireSummoned",
    "1832be68f9814254dbbdab6df7fd5d0b": "SoulEaterSummoned",
    "1ed9a630f0d9d7f44855d3d1d1b2cdf2": "GiantFrogSummoned",
    "24719a49b84c5cd43b894268d22d9c89": "CR6_WillOWispStandart",
    "260da5b557e3fb04bb4960a36a5d1dc4": "CR7_BearDire",
    "2e24256e459468743b91fbb9aa85e1ab": "SummonedAirElementalHuge",
    "2f65fd8032e5182418ee83dd4f7858dd": "CR0.5_GiantCentipedeStandard",
    "30080a8d8ae40bb43aca496b11b74c6b": "CR4_GiantFrogPoisonous",
    "313a17cbd273d1f40bd1654ee2ae186e": "CR2_WorgStandart",
    "33bb90ffd13c87b4c8e45d920313752a": "SummonedAirElementalElder",
    "3764b43791a00e1468257adbca43ce9b": "SummonedAirElementalLarge",
    "394610e32cfbc4f43a0efaab16faae49": "CR1_Nixie",
    "b98ae409beb5e8543a75b82ecda082a7": "CR6_ShamblingMound",
    "bf2216f48b3f4d24c9c502007649340d": "PurpleWormSummoned",
    "fb824352b7968fb4d8103ac439644633": "CR10_GiantFlytrapStandard",
    "3f95557fc806db741b500a5735990841": "PonySummoned",
    "3b86a449e7264174eaccef9b8f02fe20": "SummonedEarthElementalHuge",
    "3bd31a0b4d800f04a8c5b7b1a6d7061e": "SummonedWaterElementalElder",
    "406c1e1af5400ac4881e330502ccbd9e": "CR3_GiantEagleStandard",
    "4109b40f6bbb49640840644cc84ada67": "MonitorLizardSummoned",
    "4615328295cd7e84bb2ef09d3dba8403": "MephitWaterSummoned",
    "46779f56cab2cb0438161fec0129790d": "MephitEarthSummoned",
    "46cede83b1f34ad4fa46b8776e352b02": "SummonedFireElementalSmall",
    "50782bc4eb36aac4287023e20ee00808": "MephitAirSummoned",
    "51c66b0783a748c4b9538f0f0678c4d7": "CR7_GiantSpiderDoombringing",
    "56372b0a2749c224392a5ee74105c534": "SummonedWaterElementalSmall",
    "58574e8d1d4dc464c976f396d9115b1a": "AzataBralaniSummoned",
    "5bb9579fdb2b26b48bb10d61c81cfdfb": "HorseSummoned",
    "5f968d63d756f994ebff0d774e88e4ab": "CR2_BoarStandard",
    "62a3e860e6e72e6499c38bb8b2fe303e": "SummonedWaterElementalMedium",
    "640fb7efb7c916945837bbcab995267e": "SummonedFireElementalHuge",
    "651600a51edd20141adb67696986c582": "SummonedEarthElementalSmall",
    "676f8b7d0a170674cb6e504e0e30b4f0": "SummonedAirElementalMedium",
    "680b5b61c80af664daec46af7644486c": "SummonedWaterElementalLarge",
    "6b4cb9b6116f2194192e1e7e379c48d7": "SummonedEarthElementalElder",
    "6ea3a75279bab234aa723989e30cb15a": "CR8_ErinyesDevilStandard",
    "6ec9c63c41a1e754ea4dcd85557625b4": "DireBoarSummoned",
    "76597216769b0d540aafafa07edf0cec": "WolfSummon",
    "768275c9885dd954fb3c84ba69ac4281": "LeopardSummoned",
    "812c9a0348e004242ba4e46efa91e38e": "SummonedEarthElementalMedium",
    "877c154a296ee8e45be1a00668319923": "SummonedWaterElementalHuge",
    "9e120b5e0ad3c794491c049aa24b9fde": "GiantSpiderSummoned",
    "a0ab0c31b1a92554291a82e598f39ba4": "SummonedFireElementalMedium",
    "b0b4091bdaebb464e903857a95189dea": "SummonedFireElementalGreater",
    "ba5026596b06b204eb2efed2b411c5b9": "SummonedFireElementalLarge",
    "bc8ca1437c0f48948b317b7e64febf0d": "AzataGhaelSummoned",
    "beae4985629a6f64eb98081e3171e4c1": "SmilodonSummoned",
    "c3524f96954a1d94f8525b86e7626633": "CR6_HodagStandard",
    "cda7013db24f4c547b79bfc5c617066b": "SummonedEarthElementalGreater",
    "d3d9ab560534bd948b10ac00abbff083": "SummonedEarthElementalLarge",
    "d6e0acbdbdb56114898922063ae2cba0": "CR8_OwlbearStandard",
    "e770cfbb96b528c4db258d7d03fe6533": "SummonedAirElementalGreater",
    "e8276e28b2234a745900fed80670bfdb": "CR1_LizardfolkStandard",
    "ea0f0bbc6e5e471428d535501b21eb26": "SummonedFireElementalElder",
    "ece348345859351439e1263115f5fdb9": "HellhoundSummoned",
    "fcc939e3acf355b458ddf9617d8c6c28": "SummonedWaterElementalGreater",
}

SPECIAL_NOTES = {
    "lantern-archon": "Dual bounded light rays and archon defenses; optional exact Aura of Menace carrier; planar travel omitted.",
    "salamander": "Spear/tail, heat, and bounded grab/constrict; cold vulnerability omitted.",
    "invisible-stalker": "Attack-safe natural invisibility and twin slams; tracking/scent omitted.",
    "shadow-demon": "Incorporeal/shadow offense; possession and teleportation omitted.",
    "succubus": "Bounded domination and one-round temporary energy drain; profane gift omitted.",
    "bebelith": "Demon hunting and DC 25 bounded one-round armor dismantle; rot and permanent item damage omitted.",
    "pixie": "Sixteen no-damage sleep arrows and one bounded irresistible dance; no ammunition or loot.",
    "cyclops": "Greataxe, ferocity, Power Attack and Cleave on a humanoid chassis; Flash of Insight bounded to one swift-action automatic critical hit per summoning; armor and crossbow omitted.",
    "owlbear": "Magical-beast chassis with bite and two claws; claw grab on the shared summon grapple lifecycle (Sprint 4).",
    "monitor-lizard": "Animal chassis with bite and native Constitution-scaled poison; bite grab on the shared summon grapple lifecycle (Sprint 6).",
    "leopard": "Animal chassis with bite and two claws plus two rake claws; pounce; claw grab on the shared summon grapple lifecycle; charge-only rake (Sprint 7).",
    "tiger": "New Large animal on the leopard rig (1.25 view scale, procedural striped coat); 2d6 bite, two 1d8 claws and two 1d8 rake claws; pounce; claw grab on the shared lifecycle; charge-only rake (Sprint 8).",
    "cheetah": "Animal chassis on the leopard rig with a procedural spotted coat at a lean view scale; trip bite; bounded once-per-summoning sprint (+30 feet for one round) with its own brain (Sprint 8).",
    "lion": "Animal chassis on the leopard rig with a tawny visual tint; bite and two claws plus two rake claws; pounce; claw grab on the shared lifecycle; charge-only rake (Sprint 7).",
    "dire-lion": "Animal chassis with bite, two claws and a secondary rake pair; pounce; claw grab on the shared lifecycle; charge-only rake (Sprint 7).",
    "dire-tiger": "Smilodon chassis with bite, two claws and a secondary rake pair; pounce; claw grab on the shared lifecycle; charge-only rake (Sprint 7).",
    "grizzly-bear": "Animal chassis with bite and two claws; claw grab on the shared summon grapple lifecycle (Sprint 6).",
    "dire-bear": "Animal chassis with bite and two claws; claw grab on the shared summon grapple lifecycle (Sprint 6).",
    "giant-spider": "Vermin chassis with bite and native poison; native 60-foot blindsight for tremorsense; bounded 50-foot ranged Web (Reflex, native web-grappled state, two uses per summoning) with its own brain; immune to webs (Sprint 6).",
    "shambling-mound": "Plant chassis with two slams; slam grab and constrict on the shared summon grapple lifecycle; fire resistance 10 and electricity immunity; the native poison aura is not carried.",
    "giant-flytrap": "Huge plant chassis with four bites; bite grab on the shared summon grapple lifecycle, one held target at a time; acid resistance 20; tremorsense as native blindsight; engulf omitted.",
    "purple-worm": "Gargantuan magical-beast chassis with bite and sting; bite grab swallows whole through the native swallow-whole part; native Constitution-scaled sting poison; burrow omitted.",
    "dust-mephit": "Air mephit chassis with its own rim light colour, air subtype; 15-foot enemy-only slashing breath that sickens; blur once per summoning; project wind wall once per summoning (allies-only 15-foot shelter for 6 rounds: arrows and bolts deflected, other ranged weapons 30% miss).",
    "ice-mephit": "Water mephit chassis with its own rim light colour, air subtype; 15-foot enemy-only cold breath that sickens; magic missile once per summoning; cold immunity, fire vulnerability; project chill metal once per summoning (Will negates; seven-round cold table against metal armor, minimal against a metal weapon only).",
    "magma-mephit": "Fire mephit chassis with its own rim light colour, earth and fire subtypes; 15-foot enemy-only 1d8 fire breath; fire immunity, cold vulnerability; project pyrotechnics once per summoning (enemies within 20 feet blinded 1d4+1 rounds, Will negates); project magma form once per summoning (5 rounds: DR 20/magic, speed 10, no attacks).",
    "ooze-mephit": "Water mephit chassis with its own rim light colour, water subtype; 15-foot enemy-only acid breath that sickens (Reflex negates both); acid arrow once per summoning; stinking cloud once per summoning on a project ally-safe clone of the native cloud area (enemies of the caster only).",
    "salt-mephit": "Air mephit chassis with its own rim light colour, earth subtype; 15-foot enemy-only slashing breath that sickens; glitterdust (enemies only) once per summoning; dehydrate as a project 20-foot burst (2d8, Fortitude half) once per summoning.",
    "steam-mephit": "Water mephit chassis with its own rim light colour, fire and water subtypes; 15-foot enemy-only fire breath that sickens; fire immunity, cold vulnerability; blur once per summoning; boiling rain as a project 20-foot burst (2d6 fire, Fortitude half) once per summoning.",
}

NATIVE_EXPANDED_OPTIONS = (
    ("SM", 5, "Redcap", "One"),
    ("SM", 6, "Axiomite", "One"), ("SM", 6, "SoulEater", "One"),
    ("SM", 6, "Redcap", "OneD3"),
    ("SM", 7, "Bogeyman", "One"), ("SM", 7, "Axiomite", "OneD3"),
    ("SM", 7, "SoulEater", "OneD3"), ("SM", 7, "Redcap", "OneD4PlusOne"),
    ("SM", 8, "MovanicDeva", "One"), ("SM", 8, "FrostGiant", "One"),
    ("SM", 8, "Bogeyman", "OneD3"), ("SM", 8, "Axiomite", "OneD4PlusOne"),
    ("SM", 8, "SoulEater", "OneD4PlusOne"),
    ("SM", 9, "Thanadaemon", "One"), ("SM", 9, "MovanicDeva", "OneD3"),
    ("SM", 9, "FrostGiant", "OneD3"), ("SM", 9, "Bogeyman", "OneD4PlusOne"),
    ("SNA", 1, "Mite", "One"), ("SNA", 2, "Mite", "OneD3"),
    ("SNA", 3, "Mite", "OneD4PlusOne"),
    ("SNA", 5, "Manticore", "One"), ("SNA", 6, "Manticore", "OneD3"),
    ("SNA", 7, "Manticore", "OneD4PlusOne"),
    ("SNA", 8, "Nereid", "One"), ("SNA", 9, "Nereid", "OneD3"),
    ("SNA", 9, "Hamadryad", "One"),
    ("SNA", 7, "FrostGiant", "One"), ("SNA", 8, "FrostGiant", "OneD3"),
    ("SNA", 9, "FrostGiant", "OneD4PlusOne"),
)


def token(key):
    return "".join(part[:1].upper() + part[1:] for part in re.split(r"[^A-Za-z0-9]+", key) if part)


def parsed_creatures():
    values = []
    for key, name, monster, templated, ally, visual in ENTRY.findall(
            CATALOG.read_text(encoding="utf-8")):
        values.append({
            "key": key,
            "name": name,
            "monster": None if monster == "null" else int(monster),
            "templated": templated == "true",
            "ally": None if ally == "null" else int(ally),
            "visual": visual or name,
        })
    if len(values) != 81:
        raise SystemExit(f"Expected 81 parsed creatures; observed {len(values)}")
    return values


def planned():
    rows = []
    creatures = []
    for value in parsed_creatures():
        creatures.append((value["key"], value["monster"], value["templated"],
                          value["ally"]))
        rows.append((f"KMG.Summoning.Unit.{token(value['key'])}", "BlueprintUnit"))
    rows.extend((
        ("KMG.Summoning.Native.SM.Tier1", "BlueprintAbility"),
        ("KMG.Summoning.Native.SNA.Tier1", "BlueprintAbility"),
        ("KMG.Summoning.AlignmentMode.Feature", "BlueprintFeature"),
        ("KMG.Summoning.AlignmentMode.FiendishMarker", "BlueprintBuff"),
        ("KMG.Summoning.AlignmentMode.Toggle", "BlueprintActivatableAbility"),
    ))
    for family, index in (("SM", 1), ("SNA", 3)):
        for parent in range(1, 10):
            for creature in creatures:
                source = creature[index]
                if source is None or source > parent:
                    continue
                count = "One" if source == parent else "OneD3" if source == parent - 1 else "OneD4PlusOne"
                symbol = f"KMG.Summoning.Ability.{family}.Tier{parent}.{token(creature[0])}.{count}"
                rows.append((symbol, "BlueprintAbility"))
                if family == "SM" and creature[2]:
                    rows.append((symbol + ".Celestial", "BlueprintAbility"))
                    rows.append((symbol + ".Fiendish", "BlueprintAbility"))
    for family, tier, creature, multiplicity in NATIVE_EXPANDED_OPTIONS:
        rows.append((f"KMG.Summoning.NativeOption.{family}.Tier{tier}."
                     f"{creature}.{multiplicity}", "BlueprintAbility"))
    for alignment in ("Celestial", "Fiendish"):
        for band in ("Low", "Mid", "High"):
            rows.append((f"KMG.Summoning.Template.{alignment}.{band}", "BlueprintBuff"))
    for alignment in ("Celestial", "Fiendish"):
        rows.append((f"KMG.Summoning.Smite.{alignment}.Available", "BlueprintBuff"))
    rows.extend((
        ("KMG.Summoning.Special.LanternArchon.LightRay", "BlueprintAbility"),
        ("KMG.Summoning.Special.LanternArchon.LightRayAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.LanternArchon.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.LanternArchon.Defenses", "BlueprintBuff"),
        ("KMG.Summoning.Special.ShadowDemon.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Salamander.SpearType", "BlueprintWeaponType"),
        ("KMG.Summoning.Special.Salamander.Spear", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Salamander.Tail", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Salamander.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Succubus.Dominate", "BlueprintAbility"),
        ("KMG.Summoning.Special.Succubus.Domination", "BlueprintBuff"),
        ("KMG.Summoning.Special.Succubus.DominateAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Succubus.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.Succubus.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Bebelith.Claw", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Bebelith.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Bebelith.DismantledArmor", "BlueprintBuff"),
        ("KMG.Summoning.Special.Pixie.SleepBowType", "BlueprintWeaponType"),
        ("KMG.Summoning.Special.Pixie.SleepBow", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Pixie.IrresistibleDance", "BlueprintAbility"),
        ("KMG.Summoning.Special.Pixie.IrresistibleDanceState", "BlueprintBuff"),
        ("KMG.Summoning.Special.Pixie.IrresistibleDanceResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.Pixie.SleepArrowResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.Pixie.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Pixie.IrresistibleDanceAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Pixie.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.Cyclops.FlashOfInsight", "BlueprintAbility"),
        ("KMG.Summoning.Special.Cyclops.FlashOfInsightState", "BlueprintBuff"),
        ("KMG.Summoning.Special.Cyclops.FlashOfInsightResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.Cyclops.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Cyclops.FlashOfInsightAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Cyclops.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.Grapple.Hold", "BlueprintBuff"),
        ("KMG.Summoning.Special.Grapple.Grappled", "BlueprintBuff"),
        ("KMG.Summoning.Special.Owlbear.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.ShamblingMound.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.GiantFlytrap.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.PurpleWorm.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.PurpleWorm.Swallowed", "BlueprintBuff"),
        ("KMG.Summoning.Special.DustMephit.Breath", "BlueprintAbility"),
        ("KMG.Summoning.Special.DustMephit.BreathAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.DustMephit.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.DustMephit.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.DustMephit.SpellLikeOne", "BlueprintAbility"),
        ("KMG.Summoning.Special.DustMephit.SpellLikeOneResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.DustMephit.SpellLikeOneAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.IceMephit.Breath", "BlueprintAbility"),
        ("KMG.Summoning.Special.IceMephit.BreathAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.IceMephit.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.IceMephit.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.IceMephit.SpellLikeOne", "BlueprintAbility"),
        ("KMG.Summoning.Special.IceMephit.SpellLikeOneResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.IceMephit.SpellLikeOneAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.MagmaMephit.Breath", "BlueprintAbility"),
        ("KMG.Summoning.Special.MagmaMephit.BreathAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.MagmaMephit.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.MagmaMephit.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.OozeMephit.Breath", "BlueprintAbility"),
        ("KMG.Summoning.Special.OozeMephit.BreathAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.OozeMephit.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.OozeMephit.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.OozeMephit.SpellLikeOne", "BlueprintAbility"),
        ("KMG.Summoning.Special.OozeMephit.SpellLikeOneResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.OozeMephit.SpellLikeOneAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.OozeMephit.SpellLikeTwo", "BlueprintAbility"),
        ("KMG.Summoning.Special.OozeMephit.SpellLikeTwoResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.OozeMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.SaltMephit.Breath", "BlueprintAbility"),
        ("KMG.Summoning.Special.SaltMephit.BreathAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.SaltMephit.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.SaltMephit.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.SaltMephit.SpellLikeOne", "BlueprintAbility"),
        ("KMG.Summoning.Special.SaltMephit.SpellLikeOneResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.SaltMephit.SpellLikeOneAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.SaltMephit.SpellLikeTwo", "BlueprintAbility"),
        ("KMG.Summoning.Special.SaltMephit.SpellLikeTwoResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.SaltMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.SteamMephit.Breath", "BlueprintAbility"),
        ("KMG.Summoning.Special.SteamMephit.BreathAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.SteamMephit.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.SteamMephit.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.SteamMephit.SpellLikeOne", "BlueprintAbility"),
        ("KMG.Summoning.Special.SteamMephit.SpellLikeOneResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.SteamMephit.SpellLikeOneAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.SteamMephit.SpellLikeTwo", "BlueprintAbility"),
        ("KMG.Summoning.Special.SteamMephit.SpellLikeTwoResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.SteamMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.MonitorLizard.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.GrizzlyBear.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.DireBear.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.GiantSpider.Web", "BlueprintAbility"),
        ("KMG.Summoning.Special.GiantSpider.WebResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.GiantSpider.WebAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.GiantSpider.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.GiantSpider.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Leopard.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Lion.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.DireLion.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.DireTiger.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Tiger.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Cheetah.Sprint", "BlueprintAbility"),
        ("KMG.Summoning.Special.Cheetah.SprintResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.Cheetah.SprintState", "BlueprintBuff"),
        ("KMG.Summoning.Special.Cheetah.SprintAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Cheetah.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.Cheetah.CombatTraits", "BlueprintBuff"),
        # Correction order (2026-09-25): the multi-link hold, its held state and the Flytrap's engulfed state.
        ("KMG.Summoning.Special.Grapple.MultiHold", "BlueprintBuff"),
        ("KMG.Summoning.Special.Grapple.MultiHeld", "BlueprintBuff"),
        ("KMG.Summoning.Special.GiantFlytrap.Engulfed", "BlueprintBuff"),
        ("KMG.Summoning.Special.Cyclops.HideArmor", "BlueprintFeature"),
        ("KMG.Summoning.Special.Pony.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Horse.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.DustMephit.SpellLikeTwo", "BlueprintAbility"),
        ("KMG.Summoning.Special.DustMephit.SpellLikeTwoResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.DustMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.DustMephit.WindWallArea", "BlueprintAbilityAreaEffect"),
        ("KMG.Summoning.Special.DustMephit.WindWallState", "BlueprintBuff"),
        ("KMG.Summoning.Special.IceMephit.SpellLikeTwo", "BlueprintAbility"),
        ("KMG.Summoning.Special.IceMephit.SpellLikeTwoResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.IceMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.IceMephit.ChillMetalState", "BlueprintBuff"),
        ("KMG.Summoning.Special.MagmaMephit.SpellLikeOne", "BlueprintAbility"),
        ("KMG.Summoning.Special.MagmaMephit.SpellLikeOneResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.MagmaMephit.SpellLikeOneAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.MagmaMephit.SpellLikeTwo", "BlueprintAbility"),
        ("KMG.Summoning.Special.MagmaMephit.SpellLikeTwoResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.MagmaMephit.SpellLikeTwoAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.MagmaMephit.MagmaFormState", "BlueprintBuff"),
        ("KMG.Summoning.Special.MagmaMephit.PyrotechnicsBlindedState", "BlueprintBuff"),
        ("KMG.Summoning.Special.OozeMephit.StinkingCloudArea", "BlueprintAbilityAreaEffect"),
        ("KMG.Summoning.Natural.Bite1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Bite1d3", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Tail1d12", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Tail3d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Bite2d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Talon2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Claw1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Subtype.Extraplanar", "BlueprintFeature"),
    ))
    if len(rows) != 1472 or len({symbol for symbol, _ in rows}) != 1472:
        raise SystemExit(f"Foundation plan invariant failed: {len(rows)} rows")
    return rows


def generated_roster(manifest):
    by_symbol = {entry["symbol"]: entry for entry in manifest["entries"]}
    donors = {
        key: {"guid": guid, "dedicated": dedicated == "1"}
        for key, guid, dedicated in DONOR.findall(DONORS.read_text(encoding="utf-8"))
    }
    creatures = parsed_creatures()
    if len(donors) != 81 or set(donors) != {value["key"] for value in creatures}:
        raise SystemExit("Roster generation requires one exact donor per creature")
    lines = [
        "# Expanded Summoning roster and identity ledger",
        "",
        "Generated deterministically by `tools/expanded_summoning_manifest.py`; do not edit by hand.",
        "",
        "Frozen totals: 74 Summon Monster entries / 414 placements; 71 Summon Nature's Ally entries / 399 placements; 81 unique units; 813 logical placements (Phase 1 Sprint 3 added Pony, Horse, Owlbear and Cyclops; Sprint 4 added Shambling Mound, Giant Flytrap and Purple Worm; Sprint 5 added the Dust, Ice, Magma, Ooze, Salt and Steam Mephits; Sprint 8 added the Tiger; the Frost Giant is a retained native unit under Summon Monster VIII-IX and Summon Nature's Ally VII-IX wrappers).",
        "",
        "Final native qualification source: `5205805eab3fe0115d6888c53bce73c80474d1b7`. Structural run `20260812T1327062696968Z-bd09acfba08942df8f7c42e5c70252f4`; native cast run `20260812T1330147883834Z-ec8896f1d65b43e0913a6bea7cba4405`; visual run `20260812T1151394827201Z-add45a04f5de44c1a39e3251f7ff0778`; enabled/disabled persistence runs `20260812T1155220523013Z-6d2a18f9b33344d08d3127ffce7e5cb6` through `20260812T1208449380302Z-65c9b7056d97483fb48a4a9b76c22ea6`; all eight required final compatibility transactions PASS and restored their profiles.",
        "",
        "Every placement has a distinct frozen ability identity so parent spell level, caster context, metamagic, duration, UI, and save identity remain local to that parent. Templated Summon Monster placements additionally own celestial and fiendish execution identities. No runtime GUID generation or shared cross-parent ability is used.",
        "",
    ]
    for family, tier_field, short in (("Summon Monster", "monster", "SM"),
                                      ("Summon Nature's Ally", "ally", "SNA")):
        lines.extend((f"## {family}", "",
            "| Tier / creature | Template or alignment | Donor / view | KMG unit | Frozen placement abilities | Adaptation and sanitization | Qualification |",
            "|---|---|---|---|---|---|---|"))
        for creature in sorted((value for value in creatures if value[tier_field] is not None),
                               key=lambda value: (value[tier_field], value["name"])):
            source = creature[tier_field]
            donor = donors[creature["key"]]
            donor_name = DONOR_NAMES.get(donor["guid"])
            if donor_name is None:
                raise SystemExit(f"Missing runtime-proven donor name: {donor['guid']}")
            unit_symbol = f"KMG.Summoning.Unit.{token(creature['key'])}"
            unit_guid = by_symbol[unit_symbol]["guid"]
            if short == "SM" and creature["templated"]:
                policy = "Celestial/fiendish; good/evil restricted, neutral chooses"
            elif short == "SNA":
                policy = "Caster alignment; never celestial/fiendish"
            else:
                policy = "Native alignment/subtypes; no template"
            abilities = []
            for parent in range(source, 10):
                multiplicity = "One" if parent == source else (
                    "OneD3" if parent == source + 1 else "OneD4PlusOne")
                symbol = (f"KMG.Summoning.Ability.{short}.Tier{parent}."
                          f"{token(creature['key'])}.{multiplicity}")
                entry = by_symbol[symbol]
                text = f"T{parent} {multiplicity} `{entry['guid']}`"
                if short == "SM" and creature["templated"]:
                    celestial = by_symbol[symbol + ".Celestial"]["guid"]
                    fiendish = by_symbol[symbol + ".Fiendish"]["guid"]
                    text += f" (C `{celestial}`; F `{fiendish}`)"
                abilities.append(text)
            donor_kind = "dedicated summon" if donor["dedicated"] else "visual/body donor"
            donor_text = (f"`{donor['guid']}` {donor_name}; {donor_kind}; "
                          f"view policy: {creature['visual']}")
            removed = ("XP, loot, inventory, campaign, persistence, teleport, planar travel, "
                       "and summon/conjure surfaces removed; ")
            adaptation = SPECIAL_NOTES.get(creature["key"],
                "Dedicated mechanics reused only where exact; otherwise donor is view/rig only and the checked-in tabletop profile owns stats, attacks, facts, and deviations.")
            lines.append(
                f"| {source} / {creature['name']} | {policy} | {donor_text} | "
                f"`{unit_guid}` | {';<br>'.join(abilities)} | {removed}{adaptation} "
                f"See `planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md`. | "
                "Structural PASS; native cast PASS; visual contract PASS; required profiles PASS |")
        lines.append("")
    lines.extend((
        "## Explicit exclusions",
        "",
        "No aquatic-only entries, unapproved ants, apes, rhinoceroses, extra dinosaurs, campaign spawns, companions, pets, vendors, loot, or external assets are added. The Pony, Horse, Owlbear and Cyclops joined in Phase 1 Sprint 3, the Shambling Mound, Giant Flytrap and Purple Worm in Sprint 4, the six new mephits in Sprint 5 (no Lightning Mephit), the Tiger in Sprint 8, and the Frost Giant is reused, never duplicated, as a retained native unit under creature-named wrappers. Existing vanilla and third-party entries are preserved by reference and order.",
        "",
    ))
    return "\n".join(lines)


def validate(manifest, plan):
    entries = manifest["entries"]
    by_symbol = {entry["symbol"]: entry for entry in entries}
    if len(by_symbol) != len(entries):
        raise SystemExit("Manifest contains duplicate symbols")
    guids = [entry["guid"] for entry in entries]
    if len(set(guids)) != len(guids) or any(not re.fullmatch(r"[0-9a-f]{32}", value) for value in guids):
        raise SystemExit("Manifest GUID format/collision check failed")
    for symbol, planned_type in plan:
        entry = by_symbol.get(symbol)
        if entry is None:
            raise SystemExit(f"Missing planned identity: {symbol}")
        if entry["plannedType"] != planned_type or entry["status"] not in ("reserved", "active"):
            raise SystemExit(f"Wrong type/status for {symbol}")
    active = sum(entry["status"] == "active" for entry in entries)
    reserved = sum(entry["status"] == "reserved" for entry in entries)
    print(f"Expanded Summoning manifest PASS: foundation={len(plan)} active={active} reserved={reserved} total={len(entries)}")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--allocate", action="store_true")
    parser.add_argument("--activate", action="store_true")
    parser.add_argument("--emit-roster", action="store_true")
    args = parser.parse_args()
    plan = planned()
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    existing = {entry["symbol"]: entry for entry in manifest["entries"]}
    if args.allocate:
        used = {entry["guid"] for entry in manifest["entries"]}
        for symbol, planned_type in plan:
            if symbol in existing:
                continue
            guid = uuid.uuid4().hex
            while guid in used:
                guid = uuid.uuid4().hex
            used.add(guid)
            manifest["entries"].append({
                "symbol": symbol,
                "guid": guid,
                "plannedType": planned_type,
                "status": "reserved",
                "milestone": "Expanded Summoning",
                "notes": "Frozen foundation identity; activate only with exact deterministic runtime registration."
            })
        MANIFEST.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    if args.activate:
        planned_symbols = {symbol for symbol, _ in plan}
        for entry in manifest["entries"]:
            if entry["symbol"] in planned_symbols:
                entry["status"] = "active"
                entry["notes"] = "Registered in every feature-module state; live parent publication remains independently gated."
        MANIFEST.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    validate(manifest, plan)
    roster = generated_roster(manifest)
    if args.emit_roster:
        ROSTER.write_text(roster, encoding="utf-8")
    elif not ROSTER.is_file() or ROSTER.read_text(encoding="utf-8") != roster:
        raise SystemExit(
            "Expanded Summoning roster ledger is stale; run with --emit-roster")


if __name__ == "__main__":
    main()
