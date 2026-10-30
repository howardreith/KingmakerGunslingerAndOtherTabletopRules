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
VISIBILITY = ROOT / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs"
ADAPTERS = ROOT / "src/KingmakerGunslinger/Summoning/AlternateSummoningSourceCatalog.cs"
MANIFEST = ROOT / "blueprints/blueprints.json"
ADAPTER_ROW = re.compile(
    r'I\("([A-Za-z0-9]+)", SummonFamily\.(Monster|NaturesAlly), '
    r'"([0-9,]+)", AlternateSummoningPreservation\.(PerQuantity|TierOneOnly)\)')
ADAPTER_PREFIX = "KMG.Summoning.Adapter."
# Adapter GUIDs are name-based (RFC 4122 version 5) under this fixed namespace,
# so allocating a symbol always yields the same GUID on any checkout. They are
# still written once into the manifest; the runtime never derives an ID.
ADAPTER_NAMESPACE = uuid.UUID("5f1c8e0b-6d63-4c55-9a41-2b7e3f9c0d18")
ADAPTER_MILESTONE = "Expanded Summoning alternate sources"
ROSTER = ROOT / "planning/EXPANDED-SUMMONING-ROSTER.md"
ENTRY = re.compile(
    r'C\("([^"]+)","([^"]+)",(null|\d+),(true|false),(null|\d+)'
    r'(?:,"([^"]+)")?\)')
DONOR = re.compile(r'"([a-z0-9-]+)\|([0-9a-f]{32})\|([01])"')
SUPPRESSED = re.compile(
    r"SuppressedCreatureKeys\s*=\s*\n?\s*new HashSet<string>\((.*?)\);", re.S)

# Published creatures whose qualification is worth more than the structural
# line. A creature that is withheld never appears here: what it is waiting for
# is decided by the visibility catalog, not by this table.
QUALIFICATION_NOTES = {
    "invisible-stalker": "Sprint33 original body, native RTWP/TB Slam and natural-invisibility/Purge observations recorded on original development artifacts; four routes retained. Current qualification/restoration: planning/SPRINT33-STATUS.md; owner visual approval not claimed",
    "shadow-demon": "Sprint33 original winged/clawed body, native motion/attack and private teardown recorded on original development artifact; four routes retained. Current qualification/restoration: planning/SPRINT33-STATUS.md; owner visual approval not claimed",
    "succubus": "Sprint33 preserves usable Nymph presentation and icon; native control/protection/source cleanup and attack fallback observed. Temporary energy drain unresolved; persistence and release pending. Current evidence: planning/SPRINT33-STATUS.md; historical structural labels do not qualify this sprint",
    "satyr": "Sprint32 scoped public-route, control/support, technical presentation, lifecycle, persistence and integration qualification complete through source-bound native evidence and reviewed reuse; v169 publication authorized by Howie. Exact adaptations, evidence limits and current status: planning/SPRINT32-STATUS.md",
    "lillend-azata": "Sprint32 scoped public-route, control/support, technical presentation, lifecycle, persistence and integration qualification complete through source-bound native evidence and reviewed reuse; v169 publication authorized by Howie. Exact adaptations, evidence limits and current status: planning/SPRINT32-STATUS.md",
    "ettin": "Public candidate; scoped hidden native combat, ownership, AI, body and persistence observed on recorded development artifacts; exact public routes and release artifact still require qualification; owner visual review pending",
    "cloud-giant": "Public candidate; scoped hidden native combat, fog, AI, body and persistence observed on recorded development artifacts; exact public routes and release artifact still require qualification; owner visual review pending",
    "storm-giant": "Public candidate; scoped hidden native combat, lightning, AI, body and persistence observed on recorded development artifacts; exact public routes and release artifact still require qualification; owner visual review pending",
    "viper": "Independently qualified/published: all hidden gates47e8c121 PASS and all18 public roots996c5fe7 PASS; HumanReview NOT_PERFORMED_NONBLOCKING",
    "constrictor-snake": "Independently qualified/published: all hidden gates47e8c121 PASS and all14 public roots996c5fe7 PASS; HumanReview NOT_PERFORMED_NONBLOCKING",
    "salamander": "Five existing roots/identities preserved; mechanics/view/UI/crowd/persistence47e8c121 and public996c5fe7 PASS; final observation-only reach correctiondb1da016 profile/view73/73 PASS; Sprint17 technically qualified; HumanReview NOT_PERFORMED_NONBLOCKING",
    "crocodile": "Existing fourteen roots and identity retained; Sprint 16 complete: exact e3aeae63 hidden mechanics/original visual and 155ada89 public-route PASS; owner visual review pending",
    "dire-crocodile": "Six preserved roots published and qualified on exact 155ada89 after complete e3aeae63 hidden PASS; SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED; owner visual review pending",
    "fire-beetle": "Published; Sprint 14 qualified; owner visual review pending",
    "giant-ant-worker": "Published; Sprint 14 qualified under PASSIVE_CREATURE_SENSES_UNMODELED; owner visual review pending",
    "giant-ant-soldier": "Published; Sprint 14 qualified under PASSIVE_CREATURE_SENSES_UNMODELED; owner visual review pending",
    "giant-ant-drone": "Published; Sprint 15 qualified under PASSIVE_CREATURE_SENSES_UNMODELED; owner visual review pending",
    "giant-stag-beetle": "Published; Sprint 15 qualified; owner visual review pending",
    "giant-wasp": "Published; Sprint 10 Wasp mechanics, view, menu and player path technical PASS; owner visual review pending",
    "stirge": "Published; Stirge attach, drain, visual contact, icon, player path and live menu technical PASS; owner visual review pending",
    "aurochs": "Published; Sprint 11 mechanics, original visual, quantity and lifecycle technical PASS; owner visual review pending",
    "bison": "Published; Sprint 11 mechanics, original visual, quantity and lifecycle technical PASS; owner visual review pending",
    "rhinoceros": "Published; Sprint 11 mechanics, original visual, quantity and lifecycle technical PASS; owner visual review pending",
    "woolly-rhinoceros": "Published; Sprint 11 mechanics, original visual, quantity and lifecycle technical PASS; owner visual review pending",
    "dire-rat": "Published; Sprint 12 mechanics, disease lifetime, original visual, navigation, quantity and persistence technical PASS; owner visual review pending",
    "dog": "Published; Sprint 12 mechanics, original visual, navigation, quantity and persistence technical PASS; owner visual review pending",
    "hyena": "Published; Sprint 12 mechanics, original visual, navigation, quantity and persistence technical PASS; owner visual review pending",
    "goblin-dog": "Published; Sprint 12 mechanics, original visual, navigation, quantity and persistence technical PASS; owner visual review pending",
    "wolverine": "Published; Sprint 13 rage lifetime, original visual, navigation, both combat modes and persistence technical PASS; owner visual review pending",
    "shadow-mastiff": "Published; Sprint 13 bay, shadow blend, original visual, both combat modes and persistence technical PASS; owner visual review pending",
    "poisonous-frog": "Published; Sprint 13 mechanics, original visual, navigation, quantity and persistence technical PASS; owner visual review pending",
}


def suppressed_keys():
    """The creatures the visibility catalog withholds, read from the catalog.

    Registering ahead of qualification is what allocates a creature's
    identities once and keeps them from ever moving, so the roster has to be
    able to say that a row exists and is not yet published. That is decided in
    exactly one place and this reads it rather than repeating it.
    """
    text = VISIBILITY.read_text(encoding="utf-8")
    match = SUPPRESSED.search(text)
    if match is None:
        raise SystemExit("cannot read SuppressedCreatureKeys")
    return set(re.findall(r'"([a-z0-9-]+)"', match.group(1)))

DONOR_NAMES = {
    "6114adae73a1338498412b3c63c946f8": "CR4_SatyrShortbowStandard",
    "86dc43534645e234eb35431131e3b669": "GuardCaptain (retained human rig research only)",
    "afe56099ff6046b40a359b7562c0424e": "MovanicDevaSummoned",
    "da807351572c540478b703957d863e68": "CR5_ManticoreStandard",
    "61dd71e874b16f742a0f783314105553": "CR7_HillGiantStandart",
    "590cd3d5e76fdc649a5f97bc984cd3c4": "FrostGiantSummoned",
    "028cc6f46e7998f46855a33ffde89567": "MastodonSummon",
    "03dd28e92faf2e44eb9564a6ba01fdd0": "DireWolfSummon",
    "04944455200bc224d955a8e9bbd64f3f": "SummonedAirElementalSmall",
    "0b214d8e81a563549ba0be37cd1c16d0": "CR4_BearStandard",
    # Chosen by the guarded Sprint 18 primate donor census: the only
    # Large surveyed rig with real hands, a separate jaw and toes.
    "b98735a1737ae494dbe5cbeca1c7c083": "CR10_FerociousTrollGuard",
    # Chosen by the bounded Sprint 24 theropod census: independent hind
    # legs/feet/toes, jaw and forearms plus a four-joint balancing tail.
    "a8cbd4cdb0feaf64eab1452257c3c971": "LizardfolkFreeHands",
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
    "18fa241bd1afd77438564ec92614f7f1": "CR7_BuletteStandart",
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
    "77f3f2ddf1ec2da45ab956c433e3b557": "DogSummoned",
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
    "cyclops": "Greataxe, ferocity, Power Attack and Cleave on a humanoid chassis; +4 hide armor as an armor-descriptor fact (AC 19); Flash of Insight bounded to one use per summoning: a swift action arms the cyclops until its next attack roll, whose own d20 is a natural 20 with an ordinary confirmation; crossbow omitted.",
    "owlbear": "Magical-beast chassis with bite and two claws; claw grab on the shared summon grapple lifecycle (Sprint 4).",
    "monitor-lizard": "Animal chassis with bite and native Constitution-scaled poison; bite grab on the shared summon grapple lifecycle (Sprint 6).",
    "leopard": "Animal chassis with bite and two claws plus two rake claws; pounce; bite grab by limb identity on the shared summon grapple lifecycle (a foe of its size or smaller); rake gate: a charge, or the foe held since the round began (Sprint 7; corrected 2026-09-25).",
    "tiger": "New Large animal on the leopard rig (1.25 view scale, procedural striped coat); 2d6 bite, two 1d8 claws and two 1d8 rake claws; pounce; grab with the bite and both foreclaws by limb identity on the shared lifecycle; rake gate: a charge, or the foe held since the round began (Sprint 8; corrected 2026-09-25).",
    "cheetah": "Animal chassis on the leopard rig with a procedural spotted coat at a lean view scale; trip bite; bounded once-per-summoning sprint (+30 feet for one round) with its own brain (Sprint 8).",
    "dire-rat": "Sprint 12 Small animal profile with its printed bite and trip defense on the native Dog locomotion rig. An exact damaging bite makes the printed DC 11 Fortitude save before applying native Filth Fever. Its original compact rat silhouette and full runtime matrix remain qualification-gated; every Dire Rat placement stays hidden until those contracts pass.",
    "goblin-dog": "Sprint 12 Medium animal profile with disease immunity and an exact damaging-bite allergic reaction (Fortitude DC 12; one nonstacking day at -2 Dexterity and -2 Charisma; positive magical healing or remove disease ends it). The disclosed bounded Kingmaker adaptation exempts only the exact native Goblin unit type because no broader Goblinoid subtype exists. Its original silhouette and full runtime matrix remain qualification-gated.",
    "lion": "Animal chassis on the leopard rig with a tawny visual tint; bite and two claws plus two rake claws; pounce; bite grab by limb identity on the shared lifecycle; rake gate: a charge, or the foe held since the round began (Sprint 7; corrected 2026-09-25).",
    "giant-wasp": "Sprint 10 Phase 2 native sting, original flying visual and dedicated Dexterity poison graph; all twelve placements published after two-mode contact, lifecycle, player-path and live-menu qualification.",
    "stirge": "Sprint 10 Phase 2 Tiny flyer with a zero-damage touch carrier and original icon; a successful touch creates a Stirge-specific session attachment while the prey remains free to move and act. Actual blood drain triggers one disclosed 10% Filth Fever exposure check per victim from that particular Stirge, as the primary Paizo stat block requires. Reload resets attachment safely instead of restoring it.",
    "aurochs": "Sprint 11 original Aurochs visual; printed gore and Trample with the disclosed automatic-AoO-or-Reflex adaptation; Stampede requires three adjacent allied Stampede owners actively executing their own Trample in the same round and is rechecked for later contacts; Endurance omitted.",
    "bison": "Sprint 11 original Bison visual; printed gore and Trample with the disclosed automatic-AoO-or-Reflex adaptation; Stampede requires three adjacent allied Stampede owners actively executing their own Trample in the same round and is rechecked for later contacts; Endurance and Improved Bull Rush omitted.",
    "rhinoceros": "Sprint 11 original Rhinoceros visual and summon-local powerful charge restricted to its gore on a native charge; Endurance omitted.",
    "woolly-rhinoceros": "Sprint 11 original Woolly Rhinoceros visual; summon-local powerful charge restricted to its gore on a native charge; printed Trample with the disclosed automatic-AoO-or-Reflex adaptation; Diehard and Endurance omitted.",
    "dire-lion": "Animal chassis with bite, two claws and a secondary rake pair; pounce; bite grab by limb identity on the shared lifecycle; rake gate: a charge, or the foe held since the round began (Sprint 7; corrected 2026-09-25).",
    "dire-tiger": "Smilodon chassis with bite, two claws and a secondary rake pair; pounce; grab with the bite and both foreclaws by limb identity on the shared lifecycle; rake gate: a charge, or the foe held since the round began (Sprint 7; corrected 2026-09-25).",
    "grizzly-bear": "Animal chassis with bite and two claws; claw grab on the shared summon grapple lifecycle (Sprint 6).",
    "dire-bear": "Animal chassis with bite and two claws; claw grab on the shared summon grapple lifecycle (Sprint 6).",
    "giant-spider": "Vermin chassis with bite and native poison; native 60-foot blindsight for tremorsense; bounded 50-foot Web resolved as a ranged touch attack with no save (a foe up to one size larger, the native web-grappled state, a Constitution-based break-free, two uses per summoning) with its own brain, delivered by one pinned native thrown-object projectile because the library carries no web projectile; immune to webs (Sprint 6; corrected 2026-09-25 and 2026-09-26).",
    "shambling-mound": "Plant chassis with two slams; slam grab and constrict on the shared summon grapple lifecycle; fire resistance 10 and electricity immunity; the native poison aura is not carried.",
    "giant-flytrap": "Huge plant chassis with four bites; bite grab on the shared summon grapple lifecycle with one link per bite, four at most, and a mouth that holds or has engulfed a foe attacks no one else (an active hold is session-scoped and releases cleanly on a reload); engulf of a Medium or smaller foe held since the round began for 1d8+7 crushing and 2d6 acid each round; acid resistance 20; tremorsense as native blindsight (Sprint 4; corrected 2026-09-25 and 2026-09-26).",
    "purple-worm": "Gargantuan magical-beast chassis with bite and sting; the bite grab holds, and a later turn's successful maintain check swallows a foe up to one size smaller whole through the native swallow-whole part; native Constitution-scaled sting poison; burrow omitted (Sprint 4; corrected 2026-09-25).",
    "dust-mephit": "Air mephit chassis with its own rim light colour, air subtype; 15-foot enemy-only slashing breath that sickens; blur once per summoning; project wind wall once per summoning (a 15-foot shelter for 6 rounds for every creature inside that is not the mephit's enemy: arrows and bolts deflected, other ranged weapons 30% miss).",
    "ice-mephit": "Water mephit chassis with its own rim light colour, air subtype; 15-foot enemy-only cold breath that sickens; magic missile once per summoning; cold immunity, fire vulnerability; project chill metal once per summoning (Will negates; seven-round cold table against metal armor, minimal against a metal weapon only).",
    "magma-mephit": "Fire mephit chassis with its own rim light colour, earth and fire subtypes; 15-foot enemy-only 1d8 fire breath; fire immunity, cold vulnerability; project pyrotechnics once per summoning (enemies within 20 feet blinded 1d4+1 rounds, Will negates); project magma form once per summoning (5 rounds: DR 20/magic, speed 10, no attacks).",
    "ooze-mephit": "Water mephit chassis with its own rim light colour, water subtype; 15-foot enemy-only acid breath that sickens (Reflex negates both); acid arrow once per summoning; stinking cloud once per summoning on a project ally-safe clone of the native cloud area (enemies of the caster only).",
    "salt-mephit": "Air mephit chassis with its own rim light colour, earth subtype; 15-foot enemy-only slashing breath that sickens; glitterdust (enemies only) once per summoning; dehydrate as a project 20-foot burst (2d8, Fortitude half) once per summoning.",
    "steam-mephit": "Water mephit chassis with its own rim light colour, fire and water subtypes; 15-foot enemy-only fire breath that sickens; fire immunity, cold vulnerability; blur once per summoning; boiling rain as a project 20-foot burst (2d6 fire, Fortitude half) once per summoning.",
    "deinonychus": "Sprint 24 adult Medium animal: exactly two primary talons, one primary bite and one secondary foreclaws attack; native Pounce only on a legal charge; exact land ranks/racial skills; no quadruped trip defence, Rake, Rend, Grab or Powerful Charge.",
    "tyrannosaurus": "Sprint 24 adult Gargantuan animal: one 4d6 bite with signed twice-Strength Powerful Bite, 19-20 threat, bite-only Grab, later-turn Swallow Whole with a creature-owned 2d8+11 periodic payload, and confirmed-bite Bleeding Critical through the native 2d6 bleed buff.",
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


def identity_count(name):
    source = (ROOT / "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs").read_text(encoding="utf-8")
    match = re.search(r"internal const int " + re.escape(name) + r" = (\d+);", source)
    if match is None:
        raise SystemExit(f"Missing literal identity authority: {name}")
    return int(match.group(1))


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
    expected = identity_count("UnitCount")
    if len(values) != expected:
        raise SystemExit(f"Expected {expected} parsed creatures; observed {len(values)}")
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
        ("KMG.Summoning.Special.LanternArchon.LightRayFull", "BlueprintAbility"),
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
        ("KMG.Summoning.Natural.DireBat.Blindsense", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Bite1d3", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Tail1d12", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Tail3d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Bite2d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Talon2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Claw1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Subtype.Extraplanar", "BlueprintFeature"),
        ("KMG.Summoning.Natural.WaspSting1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.GiantWasp.Poison", "BlueprintFeature"),
        ("KMG.Summoning.Natural.GiantWasp.Venom", "BlueprintBuff"),
        ("KMG.Summoning.Natural.GiantWasp.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.StirgeTouch", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Stirge.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Stirge.Hold", "BlueprintBuff"),
        ("KMG.Summoning.Special.Rhinoceros.PowerfulCharge", "BlueprintFeature"),
        ("KMG.Summoning.Special.WoollyRhinoceros.PowerfulCharge", "BlueprintFeature"),
        ("KMG.Summoning.Special.Aurochs.Trample", "BlueprintAbility"),
        ("KMG.Summoning.Special.Bison.Trample", "BlueprintAbility"),
        ("KMG.Summoning.Special.WoollyRhinoceros.Trample", "BlueprintAbility"),
        ("KMG.Summoning.Special.Stirge.Remove", "BlueprintAbility"),
        ("KMG.Summoning.Natural.DireRat.Disease", "BlueprintFeature"),
        ("KMG.Summoning.Natural.GoblinDog.Traits", "BlueprintFeature"),
        ("KMG.Summoning.Natural.GoblinDog.AllergicReaction", "BlueprintBuff"),
        # Sprint 13
        ("KMG.Summoning.Natural.Bite1", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Wolverine.Rage", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Wolverine.RageOnset", "BlueprintBuff"),
        ("KMG.Summoning.Natural.Wolverine.RageState", "BlueprintBuff"),
        # Sprint 13 Shadow Mastiff
        ("KMG.Summoning.Natural.Tail1d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.ShadowMastiff.Traits", "BlueprintFeature"),
        ("KMG.Summoning.Special.ShadowMastiff.Bay", "BlueprintAbility"),
        ("KMG.Summoning.Special.ShadowMastiff.BayPanic", "BlueprintBuff"),
        ("KMG.Summoning.Special.ShadowMastiff.BayImmunity", "BlueprintBuff"),
        ("KMG.Summoning.Special.ShadowMastiff.ShadowBlend", "BlueprintActivatableAbility"),
        ("KMG.Summoning.Special.ShadowMastiff.ShadowBlendState", "BlueprintBuff"),
        # Sprint 14. The soldier's sting is its own weapon so the poison
        # trigger can gate on that weapon's type and never reach the bite,
        # and its grab rides the shared grapple lifecycle on the primary limb
        # alone. The beetle's luminescence is a display feature beside a
        # view-local light; Kingmaker has no mechanics-layer illumination
        # model, so it grants and denies nothing.
        ("KMG.Summoning.Natural.AntSting1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.GiantAnt.Poison", "BlueprintFeature"),
        ("KMG.Summoning.Natural.GiantAnt.Venom", "BlueprintBuff"),
        ("KMG.Summoning.Natural.FireBeetle.Luminescence", "BlueprintFeature"),
        ("KMG.Summoning.Special.GiantAntSoldier.Traits", "BlueprintBuff"),
        # Sprint 14 follow-up: the ants' printed racial Perception, and
        # project unit types so the three insects are not classified as
        # the Giant Spider they borrow. One ant type serves both castes.
        ("KMG.Summoning.Natural.GiantAnt.RacialSkills", "BlueprintFeature"),
        ("KMG.Summoning.Natural.FireBeetle.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.GiantAnt.UnitType", "BlueprintUnitType"),
        # Existing Sprint 15-16 identities; never allocate replacements.
        ("KMG.Summoning.Special.GiantAntDrone.Traits", "BlueprintBuff"),
        ("KMG.Summoning.Special.GiantStagBeetle.Trample", "BlueprintAbility"),
        ("KMG.Summoning.Natural.GiantStagBeetle.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Bite3d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Tail4d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Crocodile.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.DireCrocodile.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Crocodile.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.Crocodile.Sprint", "BlueprintAbility"),
        ("KMG.Summoning.Special.Crocodile.SprintAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Crocodile.SprintState", "BlueprintBuff"),
        ("KMG.Summoning.Special.Crocodile.SprintCooldown", "BlueprintBuff"),
        ("KMG.Summoning.Special.DireCrocodile.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.DireCrocodile.Sprint", "BlueprintAbility"),
        ("KMG.Summoning.Special.DireCrocodile.SprintAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.DireCrocodile.SprintState", "BlueprintBuff"),
        ("KMG.Summoning.Special.DireCrocodile.SprintCooldown", "BlueprintBuff"),
        ("KMG.Summoning.Special.DireCrocodile.Swallowed", "BlueprintBuff"),
        ("KMG.Summoning.Special.Crocodilian.SprintNotEngaged", "IsEngagedConsideration"),
        ("KMG.Summoning.Natural.Viper.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.ConstrictorSnake.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Viper.CombatProfile", "BlueprintFeature"),
        ("KMG.Summoning.Natural.ConstrictorSnake.CombatProfile", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Viper.Poison", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Viper.Venom", "BlueprintBuff"),
        ("KMG.Summoning.Special.ConstrictorSnake.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Salamander.TailType", "BlueprintWeaponType"),
        ("KMG.Summoning.Special.Salamander.UnitType", "BlueprintUnitType"),
        # Sprint 18. Nothing native carries a 1d6 slam, which is the Ape's
        # printed routine; the installed library carries no primate unit type
        # at all, so each ape owns its own rather than reading as its donor;
        # and the Dire Ape owns the feature that carries the engine's own rend
        # damage component beside its bounded sequencing gate.
        ("KMG.Summoning.Natural.Slam1d6", "BlueprintItemWeapon"),
        # The guarded review measured the Dire Ape biting for 1d8 and clawing
        # for 1d6: the shared native 1d6 bite and 1d4 claw do not override
        # their damage dice, so the engine scales them one step up for a Large
        # wielder. Its printed entry is 1d6 and 1d4 at Large, so it owns two
        # weapons that hold their dice; the shared ones are left alone.
        ("KMG.Summoning.Natural.DireApe.Bite1d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.DireApe.Claw1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Ape.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.DireApe.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Special.DireApe.Rend", "BlueprintFeature"),
        # Granted to exactly the two apes: a rulebook component is only
        # subscribed when a unit fact carries it.
        ("KMG.Summoning.Natural.Primate.FullStrengthLimbs", "BlueprintFeature"),
        # Sprint 19. Girallon is a Large magical beast printing a 1d6 bite
        # and 1d4 claws, so like the Dire Ape it owns its own pair rather than
        # letting the engine scale shared natives a step up. Xill is Medium
        # and would not be scaled, but owns its pair too: its claws carry grab
        # and its bite carries paralysis, and neither belongs on a weapon
        # other creatures share. Girallon takes the existing Sprint 18
        # full-Strength carrier rather than a renamed copy; Xill is built by
        # the special builder, which owns its own facts.
        ("KMG.Summoning.Natural.Girallon.Bite1d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Girallon.Claw1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Girallon.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Special.Girallon.Rend", "BlueprintFeature"),
        ("KMG.Summoning.Special.Xill.Claw1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Xill.Bite1d3", "BlueprintItemWeapon"),
        ("KMG.Summoning.Special.Xill.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Special.Xill.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Xill.Paralysis", "BlueprintBuff"),
        ("KMG.Summoning.Special.Xill.FullStrengthLimbs", "BlueprintFeature"),
        # Sprint 20. The scorpion owns its claw because a shared native
        # 1d6 is scaled a step up for a Large wielder - the exact defect
        # the guarded Sprint 18 review measured on the Dire Ape - and it
        # owns its sting because the poison trigger gates on that
        # weapon's own type and must never reach a claw, which is why the
        # Giant Ant has its own sting too. The poison graph is new: the
        # printed one runs six rounds where every shipped carrier runs
        # four. Its difficulty class is computed live from Constitution.
        # The printed immunity to mind-affecting effects rides the game's
        # own descriptor immunity, the same component the Goblin Dog uses
        # for disease; it is carried explicitly rather than inferred from
        # the Intelligence score the engine forces to 1.
        ("KMG.Summoning.Natural.GiantScorpion.Claw1d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.GiantScorpion.Sting1d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.GiantScorpion.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.GiantScorpion.Poison", "BlueprintFeature"),
        ("KMG.Summoning.Natural.GiantScorpion.Venom", "BlueprintBuff"),
        ("KMG.Summoning.Natural.GiantScorpion.MindlessImmunity", "BlueprintFeature"),
        # Grab: a printed claw rider and the whole of the difference
        # between the printed +8 manoeuvre bonus and the +12 grapple
        # figure. Keyed to this unit, with the primary claw and one
        # additional limb grabbing, which is what keeps it off the sting.
        ("KMG.Summoning.Special.GiantScorpion.Traits", "BlueprintBuff"),
        # Two carriers the first guarded review proved missing. The shared
        # native eight-leg trip defence delivers +8 - what a six-legged
        # insect prints - where this eight-legged creature prints +12, and
        # the printed racial +4 on Perception and Stealth existed only in
        # the arithmetic that derived those totals.
        ("KMG.Summoning.Natural.GiantScorpion.TripDefense", "BlueprintFeature"),
        ("KMG.Summoning.Natural.GiantScorpion.RacialSkills", "BlueprintFeature"),
        # Sprint 21's Giant Crab. Five of its own and no weapon: a Medium
        # creature takes the shared native 1d4 claw unscaled, which is why
        # the Large Sprint 18 and 20 creatures had to own theirs and this
        # one does not. Its anti-trip carrier is its own because the shared
        # native eight-leg fact delivers +8 and the Sprint 20 carrier that
        # delivers +12 is named and described for a scorpion.
        ("KMG.Summoning.Natural.GiantCrab.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.GiantCrab.MindlessImmunity", "BlueprintFeature"),
        ("KMG.Summoning.Natural.GiantCrab.TripDefense", "BlueprintFeature"),
        ("KMG.Summoning.Natural.GiantCrab.RacialSkills", "BlueprintFeature"),
        ("KMG.Summoning.Special.GiantCrab.Traits", "BlueprintBuff"),
        # Sprint 21's Bebelith overhaul: ten identities for what the
        # released build left out or got wrong. Its unit and its three
        # roots are released and are not reallocated.
        ("KMG.Summoning.Special.Bebelith.Rot", "BlueprintFeature"),
        ("KMG.Summoning.Special.Bebelith.RotState", "BlueprintBuff"),
        ("KMG.Summoning.Special.Bebelith.PenetratingStrike", "BlueprintFeature"),
        ("KMG.Summoning.Special.Bebelith.TripDefense", "BlueprintFeature"),
        ("KMG.Summoning.Special.Bebelith.RacialSkills", "BlueprintFeature"),
        ("KMG.Summoning.Special.Bebelith.Web", "BlueprintAbility"),
        ("KMG.Summoning.Special.Bebelith.WebResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Special.Bebelith.WebAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Bebelith.Brain", "BlueprintBrain"),
        # The tenth, allocated after the first guarded run measured three
        # printed numbers the native outsider chassis cannot produce: a
        # Fortitude and a Will assigned to each other's slots, and one hit
        # point per die more than the block prints.
        ("KMG.Summoning.Special.Bebelith.ChassisCorrection", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Ankylosaurus.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Special.Ankylosaurus.Stun", "BlueprintFeature"),
        ("KMG.Summoning.Special.Ankylosaurus.Dazed", "BlueprintBuff"),
        ("KMG.Summoning.Special.Ankylosaurus.Stunned", "BlueprintBuff"),
        ("KMG.Summoning.Natural.Ankylosaurus.Chassis", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Ankylosaurus.TailType", "BlueprintWeaponType"),
        # Sprint 23: the two armored dinosaurs own their printed weapons,
        # species/chassis carriers and exact mechanics. These append after
        # every released Sprint 22 identity.
        ("KMG.Summoning.Natural.Triceratops.Gore2d10", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Stegosaurus.Tail4d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Triceratops.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Stegosaurus.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Triceratops.Chassis", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Stegosaurus.Chassis", "BlueprintFeature"),
        ("KMG.Summoning.Special.Triceratops.PowerfulCharge", "BlueprintFeature"),
        ("KMG.Summoning.Special.Triceratops.Trample", "BlueprintAbility"),
        ("KMG.Summoning.Special.Stegosaurus.Trip", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Stegosaurus.TailType", "BlueprintWeaponType"),
        # Sprint 24: two exact natural weapons, a species type and chassis
        # per creature, and four Tyrannosaurus-owned mechanic carriers.
        ("KMG.Summoning.Natural.Deinonychus.Talon1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Deinonychus.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Deinonychus.Chassis", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Tyrannosaurus.Bite4d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Tyrannosaurus.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Tyrannosaurus.Chassis", "BlueprintFeature"),
        ("KMG.Summoning.Special.Tyrannosaurus.PowerfulBite", "BlueprintFeature"),
        ("KMG.Summoning.Special.Tyrannosaurus.CombatTraits", "BlueprintBuff"),
        ("KMG.Summoning.Special.Tyrannosaurus.Swallowed", "BlueprintBuff"),
        ("KMG.Summoning.Special.Tyrannosaurus.BleedingCritical", "BlueprintFeature"),
        # Sprint 25: the new Brachiosaurus owns its 4d6 tail and the tail type
        # that carries twenty-foot reach, a species type so it never reads as
        # its donor, a chassis carrier for the size and reach corrections a
        # reconstructed Gargantuan body needs, and its own trample. Elephant
        # is an existing released creature and appends nothing here until the
        # bounded native census proves which of its production carriers are
        # actually missing against the frozen adult entry.
        ("KMG.Summoning.Natural.Brachiosaurus.Tail4d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Brachiosaurus.TailType", "BlueprintWeaponType"),
        ("KMG.Summoning.Natural.Brachiosaurus.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Brachiosaurus.Chassis", "BlueprintFeature"),
        ("KMG.Summoning.Special.Brachiosaurus.Trample", "BlueprintAbility"),
        # Sprint 25, appended 2026-10-16 after the bounded observation of
        # the released Elephant's own carriers: it recorded six grants, no
        # trample ability and no trample component of any kind, so the
        # reserved name becomes a real identity. Reusing another
        # creature's trample was rejected because the shared carrier reads
        # its damage and DC from the creature key the ability belongs to.
        ("KMG.Summoning.Special.Elephant.Trample", "BlueprintAbility"),
        # The bounded ordinary Overrun command the owner
        # selected over a waiver. It is a separate action
        # from Trample and never replays it.
        ("KMG.Summoning.Special.Brachiosaurus.Overrun", "BlueprintAbility"),
        # The cloned Greater Overrun carrier for this one
        # creature. A BlueprintFeature, like the native feat
        # it clones.
        ("KMG.Summoning.Special.Brachiosaurus.GreaterOverrun", "BlueprintFeature"),
        # Native qualification proved the missing high-rank Skill Focus +3.
        # A standard private fact owns its native modifier through save/load;
        # the released Elephant unit/type/weapons and eight roots do not move.
        ("KMG.Summoning.Natural.Elephant.Chassis", "BlueprintFeature"),
        # Sprint 26 exact private surface.
        ("KMG.Summoning.Natural.Gorgon.Gore2d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Gorgon.Hoof1d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Bulette.Bite2d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Bulette.Claw2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Bulette.BiteType", "BlueprintWeaponType"),
        ("KMG.Summoning.Natural.Gorgon.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Gorgon.Chassis", "BlueprintFeature"),
        ("KMG.Summoning.Natural.Bulette.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Bulette.Chassis", "BlueprintFeature"),
        ("KMG.Summoning.Special.Gorgon.Breath", "BlueprintAbility"),
        ("KMG.Summoning.Special.Gorgon.Petrification", "BlueprintBuff"),
        ("KMG.Summoning.Special.Gorgon.Cooldown", "BlueprintBuff"),
        ("KMG.Summoning.Special.Gorgon.Trample", "BlueprintAbility"),
        ("KMG.Summoning.Special.Gorgon.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.Gorgon.BreathAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Bulette.Leap", "BlueprintAbility"),
        ("KMG.Summoning.Special.Bulette.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Special.Bulette.LeapAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Special.Gorgon.BreathProjectile", "BlueprintProjectile"),
        # Sprint 27: adult Griffon, no automatic Grab.
        ("KMG.Summoning.Natural.Griffon.Bite1d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Griffon.Talon1d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Griffon.Rake1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.Natural.Griffon.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Natural.Griffon.Chassis", "BlueprintFeature"),
        ("KMG.Summoning.Special.Griffon.CombatTraits", "BlueprintBuff"),
        # Sprint 28: project-owned adult giant equipment and traits.
        ("KMG.Summoning.Giant.Greatclub2d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Greatsword3d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Slam1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.HillRock1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.StoneRock1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.FireRock1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.HillRockType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.StoneRockType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.FireRockType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.HillHideArmor", "BlueprintItemArmor"),
        ("KMG.Summoning.Giant.FireHalfPlate", "BlueprintItemArmor"),
        ("KMG.Summoning.Giant.HillUnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Giant.StoneUnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Giant.FireUnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Giant.StoneNaturalArmor11", "BlueprintFeature"),
        ("KMG.Summoning.Giant.HillTraits", "BlueprintFeature"),
        ("KMG.Summoning.Giant.StoneTraits", "BlueprintFeature"),
        ("KMG.Summoning.Giant.FireTraits", "BlueprintFeature"),
        ("KMG.Summoning.Giant.FireImprovedOverrun", "BlueprintFeature"),
        ("KMG.Summoning.Giant.CleaveAction", "BlueprintAbility"),
        ("KMG.Summoning.Giant.CleaveFeature", "BlueprintFeature"),
        ("KMG.Summoning.Giant.GreatclubReachType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.GreatswordReachType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.SlamReachType", "BlueprintWeaponType"),
        # Sprint 29 private builder dependencies, append-only.
        ("KMG.Summoning.Giant.Sprint29.AwesomeBlowAction", "BlueprintAbility"),
        ("KMG.Summoning.Giant.Sprint29.AwesomeBlowFeature", "BlueprintFeature"),
        ("KMG.Summoning.Giant.Sprint29.EttinFlailType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.Sprint29.EttinMainFlail2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Sprint29.EttinOffFlail2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Sprint29.EttinJavelinType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.Sprint29.EttinMainJavelin1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Sprint29.EttinOffJavelin1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Sprint29.EttinLeather", "BlueprintItemArmor"),
        ("KMG.Summoning.Giant.Sprint29.EttinNaturalArmor8", "BlueprintFeature"),
        ("KMG.Summoning.Giant.Sprint29.EttinUnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Giant.Sprint29.EttinTraits", "BlueprintFeature"),
        ("KMG.Summoning.Giant.Sprint29.StormGreatsword4d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Sprint29.StormGreatswordType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.Sprint29.StormSlam2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Sprint29.StormSlamType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.Sprint29.StormCompositeBow3d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Sprint29.StormCompositeBowType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.Sprint29.StormBreastplate", "BlueprintItemArmor"),
        ("KMG.Summoning.Giant.Sprint29.StormNaturalArmor12", "BlueprintFeature"),
        ("KMG.Summoning.Giant.Sprint29.StormUnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Giant.Sprint29.StormTraits", "BlueprintFeature"),
        ("KMG.Summoning.Giant.Sprint29.StormImprovedVitalStrike", "BlueprintFeature"),
        ("KMG.Summoning.Giant.Sprint29.StormBrain", "BlueprintBrain"),
        ("KMG.Summoning.Giant.Sprint29.StormCallAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Giant.Sprint29.StormBoltAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Giant.Sprint29.StormChainAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Giant.Sprint29.StormLightningSafety", "Sprint29LightningAiSafety"),
        ("KMG.Summoning.Giant.Sprint29.StormCallLightning", "BlueprintAbility"),
        ("KMG.Summoning.Giant.Sprint29.StormLightningBolt", "BlueprintAbility"),
        ("KMG.Summoning.Giant.Sprint29.StormLightningState", "BlueprintBuff"),
        ("KMG.Summoning.Giant.Sprint29.StormCallResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Giant.Sprint29.StormChainLightning", "BlueprintAbility"),
        ("KMG.Summoning.Giant.Sprint29.StormChainResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Giant.Sprint29.CloudMorningstar4d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Sprint29.CloudMorningstarType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.CloudSlam2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.Sprint29.CloudSlamType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.CloudRock2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.Giant.CloudRockType", "BlueprintWeaponType"),
        ("KMG.Summoning.Giant.Sprint29.CloudChainshirt", "BlueprintItemArmor"),
        ("KMG.Summoning.Giant.Sprint29.CloudNaturalArmor12", "BlueprintFeature"),
        ("KMG.Summoning.Giant.Sprint29.CloudUnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Giant.Sprint29.CloudTraits", "BlueprintFeature"),
        ("KMG.Summoning.Giant.Sprint29.CloudObscuringMist", "BlueprintAbility"),
        ("KMG.Summoning.Giant.Sprint29.CloudFogCloud", "BlueprintAbility"),
        ("KMG.Summoning.Giant.Sprint29.CloudFogResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.Giant.Sprint29.CloudBrain", "BlueprintBrain"),
        ("KMG.Summoning.Giant.Sprint29.CloudMistAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Giant.Sprint29.CloudFogAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.Giant.Sprint29.CloudMistSafety", "Sprint29FogAiSafety"),
        ("KMG.Summoning.Giant.Sprint29.CloudFogSafety", "Sprint29FogAiSafety"),
        ("KMG.Summoning.OutsiderCelestial.Lemure.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.OutsiderCelestial.Lemure.Traits", "BlueprintFeature"),
        ("KMG.Summoning.OutsiderCelestial.Lemure.ClawType", "BlueprintWeaponType"),
        ("KMG.Summoning.OutsiderCelestial.Lemure.Claw1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.Traits", "BlueprintFeature"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.ClawType", "BlueprintWeaponType"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.Claw1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.BiteType", "BlueprintWeaponType"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.Bite1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.CauseFear", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.CauseFearResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.StinkingCloud", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.StinkingCloudResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.Brain", "BlueprintBrain"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.CauseFearAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.StinkingCloudAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.CloudSafety", "Sprint29FogAiSafety"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.CauseFearEligibility", "DretchSpellAiEligibility"),
        ("KMG.Summoning.OutsiderCelestial.Dretch.CloudEligibility", "DretchSpellAiEligibility"),
        ("KMG.Summoning.OutsiderCelestial.Archon.MenaceCarrier", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.Archon.MenaceArea", "BlueprintAbilityAreaEffect"),
        ("KMG.Summoning.OutsiderCelestial.Archon.MenacePenalty", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.Archon.MenaceImmunity", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.Archon.LanternAid", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.Archon.LanternAidAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.Archon.LanternAidEligibility", "ArchonAidEligibility"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.Traits", "BlueprintFeature"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.GreatswordType", "BlueprintWeaponType"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.Greatsword2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.BiteType", "BlueprintWeaponType"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.Bite1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.SlamType", "BlueprintWeaponType"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.Slam1d4", "BlueprintItemWeapon"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.Aid", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.AidAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.AidEligibility", "ArchonAidEligibility"),
        ("KMG.Summoning.OutsiderCelestial.HoundArchon.Brain", "BlueprintBrain"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.Traits", "BlueprintFeature"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.GreatswordType", "BlueprintWeaponType"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.Greatsword2d6", "BlueprintItemWeapon"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.Spellbook", "BlueprintSpellbook"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.SpellList", "BlueprintSpellList"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.Brain", "BlueprintBrain"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.HealAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.HealEligibility", "TrumpetPreparedHealingEligibility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.InvisibilityPurge", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.PurgeCarrier", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.PurgeArea", "BlueprintAbilityAreaEffect"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.PurgeRecipient", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.BlindFight", "BlueprintFeature"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.Persuasive", "BlueprintFeature"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.Aid", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.AidAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.AidEligibility", "ArchonAidEligibility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.BanishmentAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.BanishmentEligibility", "TrumpetBanishmentEligibility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.Traits", "BlueprintFeature"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.WarhammerType", "BlueprintWeaponType"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.Warhammer1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.SlamType", "BlueprintWeaponType"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.Slam1d8", "BlueprintItemWeapon"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.Brain", "BlueprintBrain"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.ProtectiveCarrier", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.ProtectiveArea", "BlueprintAbilityAreaEffect"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.ProtectiveRecipient", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.Heal", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.HealResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.HealAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.HealEligibility", "AstralHealingEligibility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.BladeBarrier", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.BladeBarrierResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.CureLightWounds", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.CureLightWoundsResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.CureLightWoundsAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.CureLightWoundsEligibility", "AstralHealingEligibility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.SeeInvisibility", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.SeeInvisibilityResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.StunFeature", "BlueprintFeature"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.Stunned", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.Aid", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.DispelMagic", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.HolyAura", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.HolyWord", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.Invisibility", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.RemoveCurse", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.RemoveDisease", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.RemoveFear", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.HolySmite", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.HolySmiteBlinded", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.DispelEvil", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.DispelEvilDismissal", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.DispelEvilDismissalTouch", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.DispelEvilDispel", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.DispelEvilDispelTouch", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.DispelEvilWard", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.DispelEvil", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.DispelEvilDismissal", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.DispelEvilDismissalTouch", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.DispelEvilDispel", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.DispelEvilDispelTouch", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.DispelEvilWard", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.AstralDeva.DispelEvilHeld", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.DispelEvilHeld", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.TrumpetBlast", "BlueprintAbility"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.TrumpetParalyzed", "BlueprintBuff"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.TrumpetBlastAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.OutsiderCelestial.TrumpetArchon.TrumpetBlastEligibility", "TrumpetBlastEligibility"),

        ("KMG.Summoning.FeyCelestial.Satyr.Traits", "BlueprintFeature"),
        ("KMG.Summoning.FeyCelestial.Satyr.Fear", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.Satyr.FearResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.FeyCelestial.Satyr.FearAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.Satyr.FearEligibility", "SatyrControlEligibility"),
        ("KMG.Summoning.FeyCelestial.Satyr.Brain", "BlueprintBrain"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Traits", "BlueprintFeature"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.SpellList", "BlueprintSpellList"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Spellbook", "BlueprintSpellbook"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Longsword", "BlueprintItemWeapon"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.LongswordType", "BlueprintWeaponType"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.PerformanceResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Competence", "BlueprintActivatableAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CompetenceCarrier", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CompetenceArea", "BlueprintAbilityAreaEffect"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CompetenceRecipient", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.NoncombatDispatcher", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Fascinate", "BlueprintActivatableAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.FascinateCarrier", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Fascinated", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.FascinateResistance", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.FascinateAssistance", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.FascinateShakeFree", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.FascinateArea", "BlueprintAbilityAreaEffect"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.BardicSuggestion", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CourageRecipient", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CourageArea", "BlueprintAbilityAreaEffect"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CourageCarrier", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Courage", "BlueprintActivatableAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.HealAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.HealEligibility", "LillendHealingEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CourageAi", "LillendCourageAi32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Brain", "BlueprintBrain"),
        ("KMG.Summoning.FeyCelestial.Satyr.Dagger", "BlueprintItemWeapon"),
        ("KMG.Summoning.FeyCelestial.Satyr.DaggerType", "BlueprintWeaponType"),
        ("KMG.Summoning.FeyCelestial.Satyr.Horns", "BlueprintItemWeapon"),
        ("KMG.Summoning.FeyCelestial.Satyr.HornsType", "BlueprintWeaponType"),
        ("KMG.Summoning.FeyCelestial.Satyr.CharmPerson", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.Satyr.CharmPersonAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.Satyr.CharmPersonEligibility", "SatyrControlEligibility"),
        ("KMG.Summoning.FeyCelestial.Satyr.Sleep", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.Satyr.SleepAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.Satyr.SleepEligibility", "SatyrControlEligibility"),
        ("KMG.Summoning.FeyCelestial.Satyr.MeleeAi", "BlueprintAiSwitchWeapon"),
        ("KMG.Summoning.FeyCelestial.Satyr.MeleeEligibility", "SatyrWeaponChoice32"),
        ("KMG.Summoning.FeyCelestial.Satyr.RangedAi", "BlueprintAiSwitchWeapon"),
        ("KMG.Summoning.FeyCelestial.Satyr.RangedEligibility", "SatyrWeaponChoice32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Tail", "BlueprintItemWeapon"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.TailType", "BlueprintWeaponType"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.GrabTraits", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmMonster", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmPersonSla", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmPersonSlaResource", "BlueprintAbilityResource"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.DazeAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.DazeEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmPersonAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmPersonEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CureLightAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CureLightEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.SleepAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.SleepEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.HoldPersonAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.HoldPersonEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.InvisibilityAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.InvisibilityEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.SoundBurstAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.SoundBurstEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmMonsterAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmMonsterEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmPersonSlaAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmPersonSlaEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.SuggestionBuff", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.Satyr.Suggestion", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.Satyr.SuggestionAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.Satyr.SuggestionEligibility", "SatyrControlEligibility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Suggestion", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.SuggestionAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.SuggestionEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Lullaby", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.LullabyBuff", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.LullabyAi", "BlueprintAiCastSpell"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.LullabyEligibility", "LillendSpellEligibility32"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Daze", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.DazeBuff", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.DazeImmunity", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.CharmBuff", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.SleepBuff", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.CharmPerson", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.Sleep", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.HoldPerson", "BlueprintAbility"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.HoldPersonBuff", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.Satyr.FearShakenBuff", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.Satyr.FearFrightenedBuff", "BlueprintBuff"),
        ("KMG.Summoning.FeyCelestial.LillendAzata.SoundBurst", "BlueprintAbility"),
        ("KMG.Summoning.Infernal.BeardedDevil.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Infernal.BeardedDevil.Traits", "BlueprintFeature"),
        ("KMG.Summoning.Infernal.BeardedDevil.Glaive", "BlueprintItemWeapon"),
        ("KMG.Summoning.Infernal.BeardedDevil.GlaiveType", "BlueprintWeaponType"),
        ("KMG.Summoning.Infernal.BeardedDevil.Claw", "BlueprintItemWeapon"),
        ("KMG.Summoning.Infernal.BeardedDevil.ClawType", "BlueprintWeaponType"),
        ("KMG.Summoning.Infernal.BeardedDevil.DevilChills", "BlueprintBuff"),
        ("KMG.Summoning.Infernal.BeardedDevil.InfernalWound", "BlueprintBuff"),
        ("KMG.Summoning.Infernal.BeardedDevil.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Infernal.BeardedDevil.WeaponModeAi", "BarbazuWeaponModeAi34"),
        ("KMG.Summoning.Infernal.KytonChainDevil.UnitType", "BlueprintUnitType"),
        ("KMG.Summoning.Infernal.KytonChainDevil.Traits", "BlueprintFeature"),
        ("KMG.Summoning.Infernal.KytonChainDevil.Chain", "BlueprintItemWeapon"),
        ("KMG.Summoning.Infernal.KytonChainDevil.ChainType", "BlueprintWeaponType"),
        ("KMG.Summoning.Infernal.KytonChainDevil.ControlledChains", "BlueprintBuff"),
        ("KMG.Summoning.Infernal.KytonChainDevil.DancingChains", "BlueprintAbility"),
        ("KMG.Summoning.Infernal.KytonChainDevil.UnnervingGaze", "BlueprintAbility"),
        ("KMG.Summoning.Infernal.KytonChainDevil.Shaken", "BlueprintBuff"),
        ("KMG.Summoning.Infernal.KytonChainDevil.Regeneration", "BlueprintBuff"),
        ("KMG.Summoning.Infernal.KytonChainDevil.Brain", "BlueprintBrain"),
        ("KMG.Summoning.Infernal.KytonChainDevil.ControlAi", "KytonControlAi34"),
        ("KMG.Summoning.Infernal.KytonChainDevil.GazeAi", "KytonGazeAi34"),
    ))
    expected = sum(identity_count(name) for name in (
        "UnitCount", "LogicalAbilityCount", "TemplateBuffCount", "SpecialIdentityCount",
        "NativePreservationIdentityCount", "AlignmentModeIdentityCount", "NativeExpandedOptionIdentityCount")) + 2 * identity_count("TemplatedPlacementCount")
    if len(rows) != expected or len({symbol for symbol, _ in rows}) != expected:
        raise SystemExit(f"Foundation plan invariant failed: {len(rows)} rows")
    return rows


def adapter_families(text=None):
    """The alternate-summoning callers, read from their C# identity rows."""
    if text is None:
        text = ADAPTERS.read_text(encoding="utf-8")
    rows = []
    for key, family, tiers, preservation in ADAPTER_ROW.findall(text):
        rows.append({
            "key": key,
            "family": "SM" if family == "Monster" else "SNA",
            "tiers": [int(value) for value in tiers.split(",")],
            "preservation": preservation,
        })
    if not rows or len({row["key"] for row in rows}) != len(rows):
        raise SystemExit("Cannot read the alternate summoning identity rows")
    return rows


def quantities(tier):
    return ["One"] + (["OneD3"] if tier >= 2 else []) + (
        ["OneD4PlusOne"] if tier >= 3 else [])


def adapter_planned(creatures=None, families=None):
    """Every caller-specific identity, derived from the one central catalog.

    A creature added to the catalog reaches every caller whose summon family
    and tiers admit it, through exactly the ordinary spell's quantity rule.
    No caller keeps a creature list, so nothing here is edited per sprint.
    """
    creatures = parsed_creatures() if creatures is None else creatures
    families = adapter_families() if families is None else families
    rows = []
    for family in families:
        field = "monster" if family["family"] == "SM" else "ally"
        stem = ADAPTER_PREFIX + family["key"]
        for tier in family["tiers"]:
            for creature in creatures:
                source = creature[field]
                if source is None or source > tier:
                    continue
                count = "One" if source == tier else (
                    "OneD3" if source == tier - 1 else "OneD4PlusOne")
                rows.append((f"{stem}.Tier{tier}.{token(creature['key'])}.{count}",
                             "BlueprintAbility"))
            for native_family, native_tier, creature, count in NATIVE_EXPANDED_OPTIONS:
                if native_family == family["family"] and native_tier == tier:
                    rows.append((f"{stem}.NativeOption.Tier{tier}.{creature}.{count}",
                                 "BlueprintAbility"))
            preserved = quantities(tier) if family["preservation"] == "PerQuantity" \
                else (["One"] if tier == 1 else [])
            for count in preserved:
                rows.append((f"{stem}.Preserved.Tier{tier}.{count}",
                             "BlueprintAbility"))
    if len({symbol for symbol, _ in rows}) != len(rows):
        raise SystemExit("Alternate summoning adapter plan repeats a symbol")
    return rows


def adapter_guid(symbol):
    return uuid.uuid5(ADAPTER_NAMESPACE, symbol).hex


def allocate_adapters(manifest, plan):
    """Append each missing adapter identity once, in plan order."""
    existing = {entry["symbol"] for entry in manifest["entries"]}
    used = {entry["guid"] for entry in manifest["entries"]}
    added = 0
    for symbol, planned_type in plan:
        if symbol in existing:
            continue
        guid = adapter_guid(symbol)
        if guid in used:
            raise SystemExit(f"Adapter GUID collides with an existing identity: {symbol}")
        used.add(guid)
        manifest["entries"].append({
            "symbol": symbol,
            "guid": guid,
            "plannedType": planned_type,
            "status": "active",
            "milestone": ADAPTER_MILESTONE,
            "notes": "Caller-specific summon child projected from the central catalog.",
        })
        added += 1
    return added


def validate_adapters(manifest, plan):
    by_symbol = {entry["symbol"]: entry for entry in manifest["entries"]}
    planned_symbols = {symbol for symbol, _ in plan}
    for symbol, planned_type in plan:
        entry = by_symbol.get(symbol)
        if entry is None:
            raise SystemExit(f"Missing adapter identity: {symbol}; run with --allocate")
        if entry["plannedType"] != planned_type or entry["status"] != "active":
            raise SystemExit(f"Wrong adapter type/status for {symbol}")
        if entry["guid"] != adapter_guid(symbol):
            raise SystemExit(f"Adapter GUID is not the deterministic allocation: {symbol}")
    stray = [entry["symbol"] for entry in manifest["entries"]
             if entry["symbol"].startswith(ADAPTER_PREFIX) and
             entry["symbol"] not in planned_symbols and entry["status"] == "active"]
    if stray:
        raise SystemExit(f"Active adapter identity has no catalog source: {stray[0]}")
    print(f"Alternate summoning adapter manifest PASS: adapters={len(plan)}")


def generated_roster(manifest):
    withheld = suppressed_keys()
    by_symbol = {entry["symbol"]: entry for entry in manifest["entries"]}
    donors = {
        key: {"guid": guid, "dedicated": dedicated == "1"}
        for key, guid, dedicated in DONOR.findall(DONORS.read_text(encoding="utf-8"))
    }
    creatures = parsed_creatures()
    if len(donors) != len(creatures) or set(donors) != {value["key"] for value in creatures}:
        raise SystemExit("Roster generation requires one exact donor per creature")
    registered = sum(10-c[field] for c in creatures for field in ("monster", "ally") if c[field] is not None)
    hidden = suppressed_keys()
    withheld_count = sum(10-c[field] for c in creatures for field in ("monster", "ally") if c[field] is not None and c["key"] in hidden)
    lines = [
        "# Expanded Summoning roster and identity ledger",
        "",
        "Generated deterministically by `tools/expanded_summoning_manifest.py`; do not edit by hand.",
        "",
        f"Registered totals: {sum(c['monster'] is not None for c in creatures)} Summon Monster entries / {sum(10-c['monster'] for c in creatures if c['monster'] is not None)} placements; {sum(c['ally'] is not None for c in creatures)} Summon Nature's Ally entries / {sum(10-c['ally'] for c in creatures if c['ally'] is not None)} placements; {len(creatures)} unique units; {registered} registered, {registered-withheld_count} published and {withheld_count} withheld placements. Sprints 9-23 are technically qualified and published. Viper and Constrictor independently published after complete hidden PASS on 47e8c121 and all 32 public roots PASS on 996c5fe7. Salamander's observation-only reach correction db1da016 profile/view 73/73 PASS closed Sprint 17; its five existing public roots remain preserved and PASS. Sprint 18's Ape and Dire Ape published in v0.0.147 after their complete hidden candidate passed. Sprint 19 published the Girallon and the Xill in v0.0.148 after 39/39 mechanics and 12/12 body review. Sprint 20 published the Giant Scorpion in v0.0.149, Sprint 21 published the Giant Crab in v0.0.150, and Sprint 22 published the Ankylosaurus in v0.0.151. Sprint 23 published Triceratops and Stegosaurus in v0.0.152 after their complete hidden gameplay, presentation, ownership, persistence and compatibility qualification; publication removed only their two suppression keys and moved no identity. The v0.0.153 follow-up changes only Triceratops Powerful Charge odd-Strength rounding. Sprint 24 published Deinonychus and Tyrannosaurus in v0.0.155 after their complete hidden gameplay, presentation, ownership, persistence and shared-control qualification; publication removed only their two suppression keys together and moved no identity. Sprint 25 registers the Brachiosaurus and withholds all three of its roots. Like the Sprint 21 Giant Crab and the Sprint 23 Stegosaurus it is untemplated and is not on the Summon Monster table at all, so three suppressed placements hold back three roots and no execution children; the owner settled that placement on 2026-10-15 against a recorded source discrepancy, choosing the reversible direction because identities are append-only. Sprint 25 also completes the existing released Elephant rather than re-registering it: its identity, its eight existing roots, their children and their order do not move, and actual Mastodon units and assets are unchanged. With {len(NATIVE_EXPANDED_OPTIONS)} retained wrappers there are {registered-withheld_count+len(NATIVE_EXPANDED_OPTIONS)} visible choices. Owner visual review remains pending. The Frost Giant remains a retained native unit under Summon Monster VIII-IX and Summon Nature's Ally VII-IX wrappers. Current Sprint 25 authority and evidence are in `docs/SPRINT25-GIANT-BODIES-MISSION.md`.",
        "",
        "Phase 1 baseline qualification source: `5205805eab3fe0115d6888c53bce73c80474d1b7`. Structural run `20260812T1327062696968Z-bd09acfba08942df8f7c42e5c70252f4`; native cast run `20260812T1330147883834Z-ec8896f1d65b43e0913a6bea7cba4405`; visual run `20260812T1151394827201Z-add45a04f5de44c1a39e3251f7ff0778`; enabled/disabled persistence runs `20260812T1155220523013Z-6d2a18f9b33344d08d3127ffce7e5cb6` through `20260812T1208449380302Z-65c9b7056d97483fb48a4a9b76c22ea6`; all eight required compatibility transactions passed and restored their profiles. Phase 2 Sprint 10 and 11 mechanics, visual, quantity and lifecycle evidence is indexed in `EXPANDED-SUMMONING-PHASE2-EVIDENCE-INDEX.md`.",
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
            qualification = (
                "Registered and withheld; mechanics, original visual, both "
                "combat modes and lifecycle qualification pending"
                if creature["key"] in withheld else
                QUALIFICATION_NOTES.get(creature["key"],
                    "Structural PASS; native cast PASS; visual contract PASS; required profiles PASS"))
            lines.append(
                f"| {source} / {creature['name']} | {policy} | {donor_text} | "
                f"`{unit_guid}` | {';<br>'.join(abilities)} | {removed}{adaptation} "
                f"See `planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md`. | "
                + qualification + " |")
        lines.append("")
    lines.extend((
        "## Explicit exclusions",
        "",
        "No aquatic-only entries, unapproved ants, unapproved apes, dinosaurs beyond the two authorized Sprint 24 theropods, campaign spawns, companions, pets, vendors, loot, or external assets are added. Sprint 24 does not begin Brachiosaurus, Elephant changes, animal-companion or polymorph work. The two Sprint 18 apes are authorized by that sprint and no other primate is; Girallon belongs to a later sprint and is not registered here. The four authorized Sprint 12 canines and rat remain hidden until their mechanics, distinct visuals and lifecycle qualify. The four authorized Sprint 11 ungulates are published only after their mechanics, original visuals and lifecycle qualified. The Pony, Horse, Owlbear and Cyclops joined in Phase 1 Sprint 3, the Shambling Mound, Giant Flytrap and Purple Worm in Sprint 4, the six new mephits in Sprint 5 (no Lightning Mephit), the Tiger in Sprint 8, and the Frost Giant is reused, never duplicated, as a retained native unit under creature-named wrappers. Existing vanilla and third-party entries are preserved by reference and order.",
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


def write_lf(path, text):
    """Write with LF endings, which is what .gitattributes declares.

    Path.write_text translates to the platform's newline, so on Windows
    this silently produced CRLF in files the repository stores as LF. The
    file then hashes differently in the working tree than in a fresh
    checkout, and the icon catalog pins the ledger's hash, so the
    mismatch would surface as an icon validation failure on a clean clone
    rather than where it was caused.
    """
    with open(path, "w", encoding="utf-8", newline="\n") as handle:
        handle.write(text)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--allocate", action="store_true")
    parser.add_argument("--activate", action="store_true")
    parser.add_argument("--emit-roster", action="store_true")
    args = parser.parse_args()
    plan = planned()
    adapters = adapter_planned()
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
        allocate_adapters(manifest, adapters)
        write_lf(MANIFEST, json.dumps(manifest, indent=2, ensure_ascii=False) + "\n")
    if args.activate:
        planned_symbols = {symbol for symbol, _ in plan}
        for entry in manifest["entries"]:
            if entry["symbol"] in planned_symbols and entry["status"] != "active":
                entry["status"] = "active"
                entry["notes"] = "Registered in every feature-module state; live parent publication remains independently gated."
        write_lf(MANIFEST, json.dumps(manifest, indent=2, ensure_ascii=False) + "\n")
    validate(manifest, plan)
    validate_adapters(manifest, adapters)
    roster = generated_roster(manifest)
    if args.emit_roster:
        write_lf(ROSTER, roster)
    elif not ROSTER.is_file() or ROSTER.read_text(encoding="utf-8") != roster:
        raise SystemExit(
            "Expanded Summoning roster ledger is stale; run with --emit-roster")


if __name__ == "__main__":
    main()
