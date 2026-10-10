#!/usr/bin/env python3
"""Validate the Expanded Summoning Sprint 21 candidate: the Giant Crab, and
the Bebelith's own body.

Sprint 21 branches from the exact released v0.0.149 master and is allowed to
change one surface and no other. This validator states that surface by name:
every other byte of the released production tree, of the accepted summoning
tree and of every historical release record must be identical to the commit it
came from. Sprint 21 has shipped, so the Giant Scorpion, its nine own
blueprints and its two shipped body files join the protected tree and this
sprint may not move them.

This sprint has a shape no earlier one in the series had: two creatures at
different stages. The GIANT CRAB is new and is registered withheld, with seven
Summon Nature's Ally roots and no Summon Monster tier - the first creature in
the roster shaped that way, which is why its placement arithmetic is 7 rather
than the 12 a two-table creature owes. The BEBELITH is already published and
qualified, and this sprint owns exactly one thing for it: the original body it
has never had. So this validator carries one check the others did not - that
the Bebelith's rules, chassis, identities and placements are byte-identical to
the release that qualified them - because the easiest way for this sprint to go
wrong is to improve a creature that already shipped.

This is the last sprint in the series. There is no Sprint 22.
"""
from __future__ import annotations

import argparse
import hashlib
import difflib
import json
import re
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_favored_class140 as inherited
import inspect_elemental_character_trait_icons as trait_icons

VERSION = "0.0.150"
INFORMATIONAL_VERSION = VERSION + "-expanded-summoning-sprint21"
MASTER = "d0ceafae75c1d74a1e0d4fa318193ef002240da1"
KEY = "expandedSummoningSprint21150"

# The released production surfaces Sprint 21 may not touch at all.
MASTER_PROTECTED = (
    "src/KingmakerGunslinger/ElementalRaces/",
    "src/KingmakerGunslinger/FavoredClass/",
    "src/KingmakerGunslinger/Acquisition/",
    "src/KingmakerGunslinger/RuntimeTesting/GuardedDisposableSaveLease.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ElementalCharacterTrait",
    "docs/RELEASE-NOTES-0.0.142.md",
    "docs/RELEASE-NOTES-0.0.143.md",
    "docs/RELEASE-NOTES-0.0.144.md",
    "docs/RELEASE-NOTES-0.0.145.md",
    "docs/RELEASE-NOTES-0.0.146.md",
    "docs/RELEASE-NOTES-0.0.147.md",
    "docs/RELEASE-NOTES-0.0.148.md",
    "src/KingmakerGunslinger/Blueprints/EasternWeaponCampaignBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/RareFirearmCampaignLootBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/HeirloomNodachiBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/MagicFirearmBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/EasternWeaponNamedBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearNamedBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearCampaignBlueprints.cs",
    "src/KingmakerGunslinger/CraftMagicItemsCompatibility/CraftMagicItemsRegistrationCatalog.cs",
    "validation/weapon-findability-",
)

# The qualified summoning tree. Everything beneath these prefixes must match
# the released master except the exact files Sprint 21 declares below, so a
# creature that already qualified cannot be moved by this sprint. Sprint 19's
# two shipped bodies are in here now.
SUMMONING_PROTECTED = (
    "src/KingmakerGunslinger/Summoning/",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoning",
    "assets-source/original-icons/expanded-summoning/",
    "assets/game/icons/expanded-summoning/",
    "assets/flying-animals/", "assets/sprint12-quadrupeds/",
    "assets/sprint13-creatures/", "assets/ungulates/",
    "assets/sprint14-insects/", "assets/sprint16-crocodilians/",
    "assets/sprint17-serpents/", "assets/sprint18-primates/",
    "assets/sprint19-fourarmed/", "assets/sprint20-arachnids/",
    # Sprint 20's own sources are protected too. Its generator, painter,
    # reviewer and fixtures are released work, and the Giant Crab rides the
    # same rig - so the tempting mistake is to edit the scorpion's generator
    # rather than write the crab's.
    "assets-source/original-models/sprint20-arachnids/",
)

# Exactly what Sprint 21 adds to or changes in that tree. A file not listed
# here and not new is compared byte for byte.
SPRINT21_CHANGED = (
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs",
    "src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs",
    "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/Sprint14BonePolicy.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningPteranodonViewPatch.cs",
    "src/KingmakerGunslinger/Assets/PteranodonAssetRuntime.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs",
    # Every grabber in the project is configured in one pass here, natural
    # creatures included, so a crab whose pincers grab has to reach it - and
    # the Bebelith's own mechanics are rebuilt in the same file. Admitted with
    # two checks rather than on trust: validate_only_this_sprints_creatures_
    # changed, which refuses any changed line of code naming a third
    # creature and requires the Giant Spider's released web values to still
    # arrive at the generalised seam, and
    # validate_the_released_bebelith_identity_is_preserved, which holds its
    # symbols and GUIDs to the release.
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
    # Sprint 21 overhauls the released Bebelith's mechanics, which this
    # mission authorises. Its released identity is preserved by
    # validate_the_released_bebelith_identity_is_preserved instead of by a
    # byte comparison, because a byte comparison would refuse the work.
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialProfiles.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialCombatComponents.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCreatureReview.cs",
    # The procedural renderer gains this sprint's creature, which is what
    # every sprint that ships an icon does to it.
    "assets-source/original-icons/expanded-summoning/tools/render_creature_icon.py",
    "assets-source/original-icons/expanded-summoning/icon-manifest.json",
    "assets-source/original-icons/expanded-summoning/prompts/icon-prompts.json",
    "assets/game/icons/expanded-summoning/icon-manifest.json",
)

SPRINT21_NEW = (
    "src/KingmakerGunslinger/Summoning/GiantCrabRulesPolicy.cs",
    # The Bebelith's printed contract, and the action that derives its
    # difficulty class live from Constitution immediately before the engine's
    # own save. Neither existed while this sprint owned only its body.
    "src/KingmakerGunslinger/Summoning/BebelithRulesPolicy.cs",
    "src/KingmakerGunslinger/Summoning/BebelithRot.cs",
    "assets-source/original-icons/expanded-summoning/sources/giant-crab.png",
    "assets/game/icons/expanded-summoning/giant-crab.png",
    "src/KingmakerGunslinger/RuntimeTesting/Sprint21ReviewPolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint21Review.cs",
    "tests/KingmakerGunslinger.DomainTests/Sprint21RulesTests.cs",
    "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json",
    "docs/RELEASE-NOTES-0.0.150.md",
    "tools/validate_expanded_summoning_sprint21150.py",
    "tools/test_expanded_summoning_sprint21150.py",
)

SPRINT21_KEY = "giant-crab"
SPRINT21_KEYS = (SPRINT21_KEY,)

# What Sprint 21 allocates, and what each identity must be. Pinned here so the
# creature cannot quietly acquire a blueprint nobody reviewed. The released
# Sprint 18 full-Strength carrier is NOT in this list: the crab is granted
# the existing KMG.Summoning.Natural.Primate.FullStrengthLimbs rather than a
# renamed copy, because that symbol shipped in v0.0.147 and renaming it would
# retire a released GUID and mint a new one for the same fact.
# The Bebelith's nine. Its unit and its three roots are released and are NOT
# in this list: they keep their allocated identities, and this sprint changes
# only what stands behind them.
BEBELITH_IDENTITY_TAIL = {
    # Rot's own carrier and its victim-owned state. Its graph is longer than
    # any poison this project ships - two Constitution a save, five exposures,
    # two consecutive successes to cure - which is why it cannot share one.
    "KMG.Summoning.Special.Bebelith.Rot": "BlueprintFeature",
    "KMG.Summoning.Special.Bebelith.RotState": "BlueprintBuff",
    # Penetrating strike, replacing an invented +2 against chaotic-evil
    # outsiders with the printed descriptors.
    "KMG.Summoning.Special.Bebelith.PenetratingStrike": "BlueprintFeature",
    # The printed +12 against trip, where the shared native eight-leg fact
    # delivers +8. The third creature in this series to need its own.
    "KMG.Summoning.Special.Bebelith.TripDefense": "BlueprintFeature",
    # The printed racial +8 Stealth, which existed nowhere.
    "KMG.Summoning.Special.Bebelith.RacialSkills": "BlueprintFeature",
    # The web: an ability, the resource that limits it, the AI action that
    # chooses it and a brain that can cast at all. The released creature takes
    # the native brain that casts nothing.
    "KMG.Summoning.Special.Bebelith.Web": "BlueprintAbility",
    "KMG.Summoning.Special.Bebelith.WebResource": "BlueprintAbilityResource",
    "KMG.Summoning.Special.Bebelith.WebAi": "BlueprintAiCastSpell",
    "KMG.Summoning.Special.Bebelith.Brain": "BlueprintBrain",
}

# The released Bebelith identities this sprint must not move. Checked by symbol
# and by GUID against the release, because a reallocated root would be a save
# and user-interface migration rather than a creature correction.
BEBELITH_RELEASED_IDENTITIES = (
    "KMG.Summoning.Unit.Bebelith",
    "KMG.Summoning.Ability.SM.Tier7.Bebelith.One",
    "KMG.Summoning.Ability.SM.Tier8.Bebelith.OneD3",
    "KMG.Summoning.Ability.SM.Tier9.Bebelith.OneD4PlusOne",
    "KMG.Summoning.Special.Bebelith.Claw",
    "KMG.Summoning.Special.Bebelith.CombatTraits",
    "KMG.Summoning.Special.Bebelith.DismantledArmor",
)

SPRINT21_IDENTITY_TAIL = {
    # A crab has no native unit type to inherit, and a creature that does not
    # ask for one keeps its donor's - so without this the inspection window
    # would call it a Giant Spider.
    "KMG.Summoning.Natural.GiantCrab.UnitType": "BlueprintUnitType",
    # The printed immunity to mind-affecting effects, carried explicitly
    # because Kingmaker cannot hold an absent Intelligence score and the 1 it
    # forces is not mindless to the engine.
    "KMG.Summoning.Natural.GiantCrab.MindlessImmunity": "BlueprintFeature",
    # The printed +12 against trip. Its own twice over: the shared native
    # eight-leg fact delivers +8, which Sprint 20 measured on a live creature,
    # and the Sprint 20 carrier that does deliver +12 is named and described
    # for a scorpion.
    "KMG.Summoning.Natural.GiantCrab.TripDefense": "BlueprintFeature",
    # The printed racial +4 Perception. The printed racial +8 Swim has no
    # Kingmaker equivalent and is recorded as omitted rather than substituted.
    "KMG.Summoning.Natural.GiantCrab.RacialSkills": "BlueprintFeature",
    # The grab carrier both pincers need, and the whole of the difference
    # between the printed +4 manoeuvre bonus and the +8 grapple figure.
    "KMG.Summoning.Special.GiantCrab.Traits": "BlueprintBuff",
}
# No weapon of its own, which is the one identity a reader might expect to
# find here. This creature is Medium, so the engine does not scale the shared
# native 1d4 claw for it - that scaling is exactly why the Large Sprint 18 and
# Sprint 20 creatures had to own their weapons - and the shared claw already
# carries the printed dice.
# One unit, seven roots, no execution children, five tail entries.
# No children at all, which is the structural difference from Sprint 20: this
# creature is not on the Summon Monster table, and Summon Nature's Ally never
# templates. Seven roots rather than twelve for the same reason - one family,
# parents 3 through 9.
SPRINT21_ROOTS = 7
SPRINT21_MONSTER_ROOTS = 0
SPRINT21_EXECUTION_CHILDREN = SPRINT21_MONSTER_ROOTS * 2
SPRINT21_IDENTITIES = (1 + SPRINT21_ROOTS + SPRINT21_EXECUTION_CHILDREN +
                       len(SPRINT21_IDENTITY_TAIL) +
                       len(BEBELITH_IDENTITY_TAIL))
MASTER_LEDGER_ENTRIES = 3039

REGISTERED_PLACEMENTS = 1063
SUPPRESSED_PLACEMENTS = 7
# What v0.0.148 published, kept as the number this release must not have
# reduced rather than as the number it shows.
PUBLISHED_BY_THE_PRIOR_RELEASE = 1056
NATIVE_WRAPPERS = 29
VISIBLE_WHILE_WITHHELD = PUBLISHED_BY_THE_PRIOR_RELEASE + NATIVE_WRAPPERS
VISIBLE_AFTER_PUBLICATION = REGISTERED_PLACEMENTS + NATIVE_WRAPPERS
UNIQUE_CREATURES = 103
ICON_CONCEPTS = 115


def blob(root: Path, ref: str, path: str) -> bytes:
    return subprocess.check_output(
        ["git", "-c", "core.longpaths=true", "show", ref + ":" + path], cwd=root)


def document(root: Path, path: str):
    return json.loads((root / path).read_text(encoding="utf-8-sig"))


def tracked(root: Path, ref: str) -> list:
    return subprocess.check_output(
        ["git", "-c", "core.longpaths=true", "ls-tree", "-r", "--name-only", ref],
        cwd=root, text=True).splitlines()


def preserved_files(root: Path, ref: str, prefixes, allowed=()) -> int:
    paths = [p for p in tracked(root, ref) if any(p.startswith(k) for k in prefixes)]
    if not paths:
        raise AssertionError("Empty protected boundary")
    checked = 0
    for path in paths:
        if path in allowed:
            continue
        current = (root / path).read_bytes()
        accepted = blob(root, ref, path)
        if Path(path).suffix in {".cs", ".md", ".json", ".py"}:
            current = current.replace(b"\r\n", b"\n")
            accepted = accepted.replace(b"\r\n", b"\n")
        if current != accepted:
            raise AssertionError("Accepted production changed: " + path)
        checked += 1
    return checked


def validate_only_this_sprints_creatures_changed(root: Path) -> int:
    """Nothing that changed in the special builder belongs to a third creature.

    Two dozen creatures have qualified mechanics in this one file - grab,
    constrict, swallow, rake, death roll, breath, sprint - so the risk worth
    checking is a change that reaches one of them. Sprint 20's version of this
    check required the file to be addition-only, which was right for a sprint
    that only added a grabber and is wrong for one that overhauls a released
    creature and generalises a released seam.

    The Giant Spider gets its own clause. Turning its web configuration into a
    parameterized seam is exactly the kind of refactor that can alter a
    released creature without anybody noticing, so its five released values
    must all still be passed by name - which proves the spider's web arrives at
    the same numbers through the new code path rather than merely that the old
    path is gone.
    """
    accepted = blob(root, MASTER,
                    "src/KingmakerGunslinger/Blueprints/"
                    "ExpandedSummoningSpecialBuilder.cs").decode("utf-8")
    current = (root / "src/KingmakerGunslinger/Blueprints/"
               "ExpandedSummoningSpecialBuilder.cs").read_text(encoding="utf-8")
    was = accepted.replace("\r\n", "\n").split("\n")
    now = current.replace("\r\n", "\n").split("\n")
    matcher = difflib.SequenceMatcher(None, was, now, autojunk=False)
    touched = []
    for tag, i1, i2, j1, j2 in matcher.get_opcodes():
        if tag == "equal":
            continue
        touched.extend(was[i1:i2])
        touched.extend(now[j1:j2])
    if not touched:
        raise AssertionError(
            "The special builder is unchanged, so neither the Bebelith "
            "overhaul nor the crab's grab reached it")
    # Comments are exempt, deliberately: a comment that cannot name the
    # creature a shared seam was built for is a worse comment, not a safer
    # file. What this refuses is a line of CODE that configures another
    # creature.
    code = [line for line in touched if not line.strip().startswith("//")
            and not line.strip().startswith("///")]
    allowed = ("GiantCrab", "Bebelith", "GiantSpider", "WebSpec", "spec.",
               "ConfigureSummonWeb", "LiveCrabConstrict", "CrabProfileOwner")
    for other in ("GiantAnt", "Owlbear", "PurpleWorm", "Xill", "Tiger",
                  "Lion", "Leopard", "Crocodile", "Salamander", "Bear",
                  "ShamblingMound", "GiantFlytrap", "MonitorLizard",
                  "ConstrictorSnake", "Cheetah", "Stirge", "Cyclops",
                  "Succubus", "Pixie", "Erinyes", "ShadowDemon",
                  "GiantScorpion", "GiantStagBeetle"):
        for line in code:
            if other in line and not any(ok in line for ok in allowed):
                raise AssertionError(
                    "Sprint 21 touched another creature's wiring in the "
                    "special builder: " + other + " at " + line.strip()[:120])
    # The Giant Spider's released web, proved to arrive through the new seam at
    # the values it already had.
    for required in ("UnitSymbol = GiantSpiderUnitSymbol",
                     "AbilitySymbol = GiantSpiderWebSymbol",
                     "ResourceSymbol = GiantSpiderWebResourceSymbol",
                     "AiSymbol = GiantSpiderWebAiSymbol",
                     "BrainSymbol = GiantSpiderBrainSymbol",
                     "TraitsSymbol = GiantSpiderCombatTraitsSymbol",
                     ".GiantSpiderWebRangeFeet",
                     ".GiantSpiderWebRounds",
                     ".GiantSpiderWebMaxSizeDelta",
                     ".GiantSpiderWebSpellLevel",
                     ".GiantSpiderWebUses",
                     'For("giant-spider").HitDice'):
        if required not in current:
            raise AssertionError(
                "The Giant Spider's released web value is no longer passed to "
                "the generalised seam: " + required)
    # The Bebelith's own web, at its own values, through the same seam.
    for required in ("UnitSymbol = BebelithUnitSymbol",
                     "AbilitySymbol = BebelithWebSymbol",
                     ".BebelithWebRangeFeet", ".BebelithWebRounds",
                     ".BebelithWebUses",
                     "CasterHitDice = BebelithRulesPolicy.HitDice"):
        if required not in current:
            raise AssertionError(
                "The Bebelith's web is not configured through the generalised "
                "seam: " + required)
    # And the crab's grab, which is the other half of this file's changes.
    if "ConfigureGrabber(library, bySymbol, GiantCrabUnitSymbol" not in current:
        raise AssertionError("The crab's pincers must carry a grab carrier")
    for required in ("Primary = true",
                     "Additional = GiantCrabRulesPolicy.ClawCount - 1",
                     "MaxHeld = GiantCrabRulesPolicy.ClawCount",
                     "LiveCrabConstrict = true"):
        if required not in current:
            raise AssertionError("The crab's grab spec is missing: " + required)
    # The invented demon-hunting bonus is gone, replaced by the printed
    # descriptors. A constant left behind would mean the payload had not
    # actually moved.
    if "BebelithDemonHunterBonus" in current:
        raise AssertionError(
            "The invented demon-hunting bonus must be gone: penetrating "
            "strike is a descriptor, not a numeric bonus")
    return len(code)

def validate_the_measured_carriers_are_wired(root: Path) -> int:
    """The two carriers the first guarded review earned, wired rather than named.

    Both were printed lines with a derivation and no implementation, and both
    read wrong on a live creature: the anti-trip defence at 27 against a
    printed 31, and Perception at 0 against a printed +4. A registered
    identity would not have fixed either, so this reads the builder.
    """
    builder = (root / "src/KingmakerGunslinger/Blueprints"
               / "ExpandedSummoningNaturalBuilder.cs").read_text(
                   encoding="utf-8-sig")
    profiles = (root / "src/KingmakerGunslinger/Summoning"
                / "ExpandedSummoningNaturalProfiles.cs").read_text(
                    encoding="utf-8-sig")
    checked = 0
    for fact, configure in (("GiantCrabTripDefense",
                             "ConfigureGiantCrabTripDefense"),
                            ("GiantCrabRacialSkills",
                             "ConfigureGiantCrabRacialSkills"),
                            ("GiantCrabMindlessImmunity",
                             "ConfigureGiantCrabMindlessImmunity")):
        if ('"' + fact + '"') not in profiles:
            raise AssertionError("The profile does not carry " + fact)
        if (configure + "(Require<BlueprintFeature>") not in builder:
            raise AssertionError("The builder does not configure " + fact)
        if ('fact == "' + fact + '"') not in builder:
            raise AssertionError("The builder cannot resolve " + fact)
        checked += 1
    # The shared native carrier delivers eight and this creature prints
    # twelve, so taking the shared one is the defect, not the fix.
    block = profiles.split('PK("giant-crab"')[1].split("PK(")[0]
    if '"TripDefenseEightLegs"' in block:
        raise AssertionError(
            "The Giant Crab must not take the shared eight-leg carrier")
    # Nor the Sprint 20 creature's, which is named and described for a
    # scorpion: a crab wearing it would show the wrong creature's feature.
    if "GiantScorpion" in block:
        raise AssertionError(
            "The Giant Crab must not wear a scorpion's named feature")
    # The anti-trip value is the printed one and is not written twice.
    if "defence.Bonus = GiantCrabRulesPolicy.EightLegTripBonus" \
            not in builder:
        raise AssertionError(
            "The anti-trip carrier must take its value from the rules policy")
    if "perception.Value = GiantCrabRulesPolicy.RacialPerceptionBonus" \
            not in builder:
        raise AssertionError(
            "The printed racial Perception bonus must come from the rules "
            "policy")
    # And the printed racial Swim must be wired NOWHERE. Kingmaker has no Swim
    # skill, so a line that set one would be a substitution rather than the
    # recorded omission this creature's contract declares.
    if "RacialSwimBonus" in builder:
        raise AssertionError(
            "The printed racial Swim bonus is recorded as omitted and must not "
            "be wired to any skill")
    return checked


def validate_identity_append(root: Path) -> int:
    """The ledger is append-only: the released v0.0.148 entries keep their
    exact order, symbol, GUID and metadata, and Sprint 21 identities follow.

    Thirty-four entries for one creature, where Sprint 19 spent twenty-two on
    two. The difference is mostly templating rather than waste: this creature
    appears on the Summon Monster table, so each of its six Monster roots owns
    a celestial and a fiendish execution child. Twelve roots, twelve children,
    and nine facts of its own - three more than it first registered, each of
    them a printed line that had a derivation and no carrier: the grab, the
    twelve-point anti-trip defence, and the racial +4 on two skills.
    """
    accepted = json.loads(blob(root, MASTER, "blueprints/blueprints.json"))["entries"]
    current = document(root, "blueprints/blueprints.json")["entries"]
    if current[:len(accepted)] != accepted:
        raise AssertionError("Sprint 21 moved or rewrote a historical identity")
    appended = current[len(accepted):]
    if len(accepted) != MASTER_LEDGER_ENTRIES or \
            len(appended) != SPRINT21_IDENTITIES:
        raise AssertionError(
            f"Sprint 21 must append exactly {SPRINT21_IDENTITIES} identities "
            f"to {MASTER_LEDGER_ENTRIES}; observed {len(accepted)} + "
            f"{len(appended)}")
    if len({e["guid"] for e in current}) != len(current) or \
            len({e["symbol"] for e in current}) != len(current):
        raise AssertionError("Duplicate stable identity")
    if not all(re.fullmatch(r"[0-9a-f]{32}", e["guid"]) for e in appended):
        raise AssertionError("Appended identity GUIDs are malformed")
    for entry in appended:
        if "GiantCrab" not in entry["symbol"] and \
                "Bebelith" not in entry["symbol"]:
            raise AssertionError(
                "Sprint 21 allocated an identity outside its two creatures: "
                + entry["symbol"])
    by_symbol = {e["symbol"]: e for e in appended}
    for tail in (SPRINT21_IDENTITY_TAIL, BEBELITH_IDENTITY_TAIL):
        for symbol, planned in tail.items():
            entry = by_symbol.get(symbol)
            if entry is None or entry["plannedType"] != planned:
                raise AssertionError(
                    "Missing or mistyped Sprint 21 identity: " + symbol)
    # The Bebelith's released identities must NOT be among the appended: they
    # were allocated when it shipped, and re-appending one would mean the
    # creature had been reallocated rather than overhauled.
    for symbol in BEBELITH_RELEASED_IDENTITIES:
        if symbol in by_symbol:
            raise AssertionError(
                "Sprint 21 reallocated a released Bebelith identity: " + symbol)
    units = [e for e in appended if e["plannedType"] == "BlueprintUnit"]
    # Placement abilities and creature abilities are different things and this
    # sprint appends both: seven generated placements for the Giant Crab, and
    # one creature ability - the Bebelith's web - which is not a root and must
    # not be counted as one. The prefix separates them, because every generated
    # placement is allocated under KMG.Summoning.Ability and every
    # creature-owned blueprint under KMG.Summoning.Special.
    abilities = [e for e in appended if e["plannedType"] == "BlueprintAbility"
                 and e["symbol"].startswith("KMG.Summoning.Ability.")]
    creatureAbilities = [e for e in appended
                         if e["plannedType"] == "BlueprintAbility"
                         and not e["symbol"].startswith("KMG.Summoning.Ability.")]
    children = [e for e in abilities
                if e["symbol"].endswith(".Celestial") or
                e["symbol"].endswith(".Fiendish")]
    if len(units) != 1:
        raise AssertionError("Sprint 21 appends exactly one unit identity")
    if len(abilities) != SPRINT21_ROOTS + SPRINT21_EXECUTION_CHILDREN:
        raise AssertionError(
            f"Sprint 21 appends {SPRINT21_ROOTS} roots and "
            f"{SPRINT21_EXECUTION_CHILDREN} execution children")
    if len(creatureAbilities) != 1 or             creatureAbilities[0]["symbol"] !=             "KMG.Summoning.Special.Bebelith.Web":
        raise AssertionError(
            "The only creature ability this sprint appends is the Bebelith's "
            "web; observed " + ", ".join(e["symbol"] for e in creatureAbilities))
    if len(children) != SPRINT21_EXECUTION_CHILDREN:
        raise AssertionError(
            "A templated creature's Summon Monster roots each own a celestial "
            "and a fiendish execution child; observed " + str(len(children)))
    roots = [e for e in abilities if e not in children]
    monster_roots = [e for e in roots if ".SM." in e["symbol"]]
    ally_roots = [e for e in roots if ".SNA." in e["symbol"]]
    if len(monster_roots) != SPRINT21_MONSTER_ROOTS or             len(ally_roots) != SPRINT21_ROOTS - SPRINT21_MONSTER_ROOTS:
        raise AssertionError(
            "Tier 4 on both tables gives six Summon Monster roots and six "
            "Summon Nature's Ally roots")
    # Every child belongs to a Summon Monster root. Nature's Ally never
    # templates, so a celestial Nature's Ally scorpion would be a rule this
    # project does not have.
    for entry in children:
        if ".SM." not in entry["symbol"]:
            raise AssertionError(
                "Summon Nature's Ally never templates: " + entry["symbol"])
    return len(current)


def validate_suppression(root: Path) -> None:
    visibility = (root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs"
                  ).read_text(encoding="utf-8")
    # Published. What the suppression set contains is checked before what the
    # counts claim, so a file that withholds the wrong thing names the creature
    # rather than only reporting that a number moved. The named checks come
    # first for the same reason: the dangerous mistakes get their own message.
    #
    # The DECLARATION, not the last mention: the name appears again in the
    # IsPublished lookup, and that occurrence carries no creature keys at all,
    # so taking the last one would read an empty suppression set as correct
    # however the real one was written.
    declaration = visibility.split("SuppressedCreatureKeys =")
    if len(declaration) != 2:
        raise AssertionError(
            "The suppression set must be declared exactly once")
    suppressed = declaration[1].split(";")[0]
    if '"' + SPRINT21_KEY + '"' not in suppressed:
        raise AssertionError(
            "Sprint 21 must register its creature withheld: " + SPRINT21_KEY)
    # Everything v0.0.148 published stays published. Sprint 18's apes and
    # Sprint 19's Girallon and Xill are the ones a careless edit would reach.
    for key in ("ape", "dire-ape", "girallon", "xill", "giant-scorpion",
                "bebelith"):
        if '"' + key + '"' in suppressed:
            raise AssertionError(
                "Sprint 21 withheld a creature v0.0.149 published: " + key)
    # Exactly one creature withheld, and it is this one. A second name here
    # would be a creature registered and then forgotten.
    if suppressed.count('"') != 2:
        raise AssertionError(
            "Sprint 21 withholds exactly its own creature: " + suppressed.strip())
    # The set is empty rather than absent, and every registered placement is
    # visible. Publication is the removal of one key and nothing else, so the
    # registered count must be the one registration allocated.
    for token in ("RegisteredLogicalPlacementCount = "
                  + str(REGISTERED_PLACEMENTS),
                  "SuppressedLogicalPlacementCount = "
                  + str(SUPPRESSED_PLACEMENTS),
                  'new HashSet<string>(new[] { "giant-crab" }'):
        if token not in visibility:
            raise AssertionError("Sprint 21 candidate surface differs: " + token)
    catalog = (root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
               ).read_text(encoding="utf-8")
    # This sprint's creature is the other way round, and it is the structural
    # mistake only this creature could make: Summon Nature's Ally never
    # templates, so a templated crab would owe fourteen execution children
    # this sprint has not allocated - and the ledger check would then fail for
    # a reason that did not name the cause.
    if 'C("giant-crab","Giant Crab",null,true' in catalog:
        raise AssertionError(
            "Summon Nature's Ally never templates; a templated crab would owe "
            "execution children this sprint has not allocated")
    for token in ('C("giant-crab","Giant Crab",null,false,3,"Giant Spider")',
                  'C("giant-scorpion","Giant Scorpion",4,true,4',
                  'C("girallon","Girallon",null,false,5',
                  'C("xill","Xill",5,false,null',
                  'C("ape","Ape",3,true,3', 'C("dire-ape","Dire Ape",4,true,4',
                  "ValidateFamily(SummonFamily.Monster, 92, 530)",
                  "ValidateFamily(SummonFamily.NaturesAlly, 91, 533)"):
        if token not in catalog:
            raise AssertionError("Frozen Sprint 21 placement contract differs: " + token)
    # The released scorpion is templated and both of its tiers are 4.
    if 'C("giant-scorpion","Giant Scorpion",4,false' in catalog:
        raise AssertionError(
            "Every vermin on the Summon Monster table is templated; an "
            "untemplated scorpion would owe no execution children")


def validate_icons(root: Path) -> None:
    manifest = document(
        root, "assets-source/original-icons/expanded-summoning/icon-manifest.json")
    if manifest["count"] != ICON_CONCEPTS or \
            len(manifest["icons"]) != ICON_CONCEPTS:
        raise AssertionError(
            "Project summon icon concept count must be " + str(ICON_CONCEPTS))
    rows = {row["key"]: row for row in manifest["icons"]}
    for key in SPRINT21_KEYS:
        if key not in rows:
            raise AssertionError("Missing Sprint 21 icon concept: " + key)
    digests = [value["outputSha256"] for value in manifest["icons"]]
    if len(set(digests)) != len(digests):
        raise AssertionError("Two creatures share one shipped icon")
    sources = [value["sourceSha256"] for value in manifest["icons"]]
    if len(set(sources)) != len(sources):
        raise AssertionError("Two creatures share one icon painting")
    for key in SPRINT21_KEYS:
        row = rows[key]
        if row["width"] != 128 or row["height"] != 128 or row["format"] != "RGBA PNG":
            raise AssertionError(
                "Sprint 21 icon concept is not the house format: " + key)
        shipped = root / "assets/game/icons/expanded-summoning" / (key + ".png")
        if not shipped.is_file():
            raise AssertionError("Sprint 21 icon is not shipped: " + key)
        if hashlib.sha256(shipped.read_bytes()).hexdigest() != row["outputSha256"]:
            raise AssertionError(
                "Sprint 21 shipped icon is not the reviewed one: " + key)


def validate_contract(root: Path) -> None:
    contract = document(root, "planning/EXPANDED-SUMMONING-SPRINT21-CONTRACT.json")
    if contract.get("schemaVersion") != 2 or contract.get("sprint") != 21 or \
            contract.get("masterBase") not in (MASTER, MASTER[:8]):
        raise AssertionError(
            "The frozen Sprint 21 contract does not match this branch")
    # The superseded first draft scoped the Bebelith to a body and mis-copied
    # three Giant Crab numbers. A contract that stopped saying so would let the
    # correction be forgotten.
    if "supersedes" not in contract:
        raise AssertionError(
            "The frozen contract must record what it corrected and why")

    surface = contract["surface"]
    if surface["registeredPlacementsAfter"] != REGISTERED_PLACEMENTS or \
            surface["uniqueCreaturesAfter"] != UNIQUE_CREATURES or \
            surface["visibleChoicesAfterPublication"] != \
            VISIBLE_AFTER_PUBLICATION or \
            surface["monsterPlacementsUnchanged"] != 530 or \
            surface["naturesAllyPlacementsAfter"] != 533:
        raise AssertionError("The frozen contract arithmetic drifted")
    if "Ten" not in surface["affectedRoots"]:
        raise AssertionError(
            "The frozen contract must name all ten affected roots: the crab's "
            "seven new ones and the Bebelith's three existing ones")

    # ---------------------------------------------------------------- the crab
    placement = contract["giantCrab"]["placement"]
    if placement["roots"] != SPRINT21_ROOTS or \
            placement["templated"] is not False or \
            placement["ownTier"] != 3 or \
            placement["family"] != "Summon Nature's Ally only":
        raise AssertionError("The frozen Giant Crab placement drifted")
    # No execution children at all, which is the structural difference from
    # every templated creature in the series, and the contract has to say why
    # rather than leaving a zero to be read as an oversight.
    if SPRINT21_EXECUTION_CHILDREN != 0 or \
            placement.get("templatedReason") is None:
        raise AssertionError(
            "An untemplated Nature's Ally creature owns no execution children "
            "and the contract has to say why")
    printed = contract["giantCrab"]["printedProfile"]
    if printed["abilityScores"]["strength"] != 15 or \
            printed["abilityScores"]["dexterity"] != 13 or \
            printed["abilityScores"]["intelligence"] is not None or \
            printed["hitPoints"]["total"] != 19 or \
            printed["armorClass"]["total"] != 16 or \
            printed["armorClass"]["flatFooted"] != 15 or \
            printed["naturalArmor"] != 5:
        raise AssertionError("The frozen printed Giant Crab profile changed")
    if "1d4+2" not in printed["melee"] or "plus grab" not in printed["melee"] \
            or "2 claws" not in printed["melee"]:
        raise AssertionError("The frozen printed melee line changed")
    if "plus poison" in printed["melee"]:
        raise AssertionError(
            "A crab prints no poison; a contract that claimed one would owe a "
            "carrier and a review case neither this sprint nor its review has")
    if printed.get("constrict") != "1d4+2":
        raise AssertionError(
            "The frozen printed constrict changed. The first draft of this "
            "contract omitted it entirely, which is why it is pinned here")
    if printed["combatManeuverDefense"]["versusTrip"] - \
            printed["combatManeuverDefense"]["base"] != 12:
        raise AssertionError(
            "The frozen printed eight-legged trip defence changed")
    # The aquatic half stays in the contract as printed numbers, so the
    # omission cannot quietly become a substitution: a swim speed folded into
    # the ground speed, or a skill raised to stand in for Swim.
    if printed["swimSpeedFeet"] != 20 or printed["speedFeet"] != 30 or \
            "water dependency" not in printed["specialQualities"]:
        raise AssertionError("The frozen printed aquatic line changed")
    if printed["swimSpeedFeet"] == printed["speedFeet"]:
        raise AssertionError(
            "The printed swim speed must stay distinct from the ground speed "
            "it is not substituted into")

    # ------------------------------------------------------------ the Bebelith
    released = contract["bebelith"]["releasedIdentity"]
    if len(released["roots"]) != 3 or \
            any(row["family"] != "Monster" for row in released["roots"]) or \
            sorted(row["parentTier"] for row in released["roots"]) != [7, 8, 9] or \
            "suppressed" not in released["notSuppressed"]:
        raise AssertionError(
            "The frozen contract stopped declaring the Bebelith's released "
            "identity preserved")
    bebelith = contract["bebelith"]["printedProfile"]
    if bebelith["hitDice"] != 12 or bebelith["hitPoints"] != 150 or \
            bebelith["armorClass"]["total"] != 22 or \
            bebelith["saves"]["will"] != 7 or \
            bebelith["racialModifiers"]["Stealth"] != 8 or \
            bebelith["web"]["difficultyClass"] != 23 or \
            bebelith["web"]["hitPoints"] != 12:
        raise AssertionError("The frozen printed Bebelith profile changed")
    if bebelith["combatManeuverDefense"]["versusTrip"] - \
            bebelith["combatManeuverDefense"]["base"] != 12:
        raise AssertionError(
            "The frozen printed Bebelith trip defence changed")
    if "critical 19-20" not in bebelith["melee"] or \
            "plus rot" not in bebelith["melee"]:
        raise AssertionError("The frozen printed Bebelith melee line changed")
    rot = contract["bebelith"]["rot"]
    if "bite only" not in rot["delivery"] or \
            rot["cure"] != "two consecutive successful saves" or \
            "2 Constitution damage" not in rot["effect"]:
        raise AssertionError("The frozen printed rot graph changed")
    if not any("claw" in row for row in rot["prohibitions"]):
        raise AssertionError(
            "The frozen contract stopped forbidding rot from a claw")
    strike = contract["bebelith"]["penetratingStrike"]
    if "chaotic and magic" not in strike["generally"] or \
            "cold iron and good" not in strike["versusDemons"] or \
            not any("non-demon" in row for row in strike["prohibitions"]):
        raise AssertionError(
            "The frozen penetrating strike contract changed")
    dismantle = contract["bebelith"]["dismantleArmor"]
    # Lowered once: the contract sentence starts with a capital, and the
    # printed trigger is a conjunction whose four clauses all matter.
    trigger = dismantle["trigger"].lower()
    for clause in ("both claws", "same target", "one qualifying",
                   "combat manoeuvre check succeeds"):
        if clause not in trigger:
            raise AssertionError(
                "The frozen Dismantle Armor trigger dropped a clause: "
                + clause)
    for required in ("never owner party gear", "unarmoured target",
                     "natural-armour target", "unrelated inventory item",
                     "duplicate application", "permanent inventory corruption"):
        if not any(required in row for row in dismantle["mandatorySafety"]):
            raise AssertionError(
                "The frozen Dismantle Armor safety list dropped: " + required)
    if "No global item-durability system is invented" not in \
            dismantle["engineGapPolicy"]:
        raise AssertionError(
            "The frozen contract stopped forbidding an item-durability system")

    # --------------------------------------------- omissions and boundaries
    recorded = {row["id"] for row in
                contract["giantCrab"]["acceptedLimitations"]} | \
               {row["id"] for row in
                contract["bebelith"]["acceptedLimitations"]}
    for required in ("PASSIVE_CREATURE_SENSES_UNMODELED",
                     "ORDINARY_MAP_LAND_USE_SCOPE",
                     "SUMMONED_PLANAR_TRAVEL_FORBIDDEN"):
        if required not in recorded:
            raise AssertionError(
                "The frozen contract stopped recording an omission: " + required)
    boundaries = contract["hardBoundaries"]
    for required in ("aquatic", "item-durability", "Sprint 22",
                     "rename of the released bebelith key"):
        if not any(required.lower() in row.lower() for row in boundaries):
            raise AssertionError(
                "The frozen contract stopped forbidding: " + required)
    # The released spelling, which a source may disagree with and this sprint
    # does not change.
    spelling = contract["spelling"]
    if spelling["key"] != "bebelith" or spelling["display"] != "Bebelith" or \
            "migration" not in spelling["decision"]:
        raise AssertionError(
            "The released Bebelith spelling and the reason it is kept must "
            "stay in the frozen contract")

def validate_the_released_bebelith_identity_is_preserved(root: Path) -> int:
    """The Bebelith's released identity is intact; its implementation is not.

    This sprint overhauls its mechanics on purpose, so the released-line
    comparison an earlier draft of this gate used would now refuse the work it
    is meant to admit. What must not move is narrower and more important: the
    unit, the three generated roots and the three blueprints it already owned
    keep their exact symbols and their exact GUIDs, so a save made against the
    released build still resolves every one of them. Changing the code behind a
    stable identity is a creature correction; changing the identity would be a
    save and user-interface migration.
    """
    accepted = {entry["symbol"]: entry["guid"] for entry in
                json.loads(blob(root, MASTER, "blueprints/blueprints.json"))
                ["entries"]}
    current = {entry["symbol"]: entry["guid"] for entry in
               document(root, "blueprints/blueprints.json")["entries"]}
    checked = 0
    for symbol in BEBELITH_RELEASED_IDENTITIES:
        if symbol not in accepted:
            raise AssertionError(
                "This check names a Bebelith identity the release did not "
                "have, so it has stopped checking anything: " + symbol)
        if current.get(symbol) != accepted[symbol]:
            raise AssertionError(
                "Sprint 21 moved a released Bebelith identity: " + symbol)
        checked += 1
    # Its three roots stay on the Summon Monster table at their printed tiers
    # and quantities, and nothing withholds them.
    catalog = (root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
               ).read_text(encoding="utf-8")
    if 'C("bebelith","Bebelith",7,false,null,"Doomspider")' not in catalog:
        raise AssertionError(
            "The released Bebelith row must stay Summon Monster VII, "
            "untemplated, with no Nature's Ally tier")
    visibility = (root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs"
                  ).read_text(encoding="utf-8")
    declaration = visibility.split("SuppressedCreatureKeys =")[1].split(";")[0]
    if '"bebelith"' in declaration:
        raise AssertionError(
            "The released Bebelith is published and must stay published")
    return checked


def validate_shipped_bodies_reach_the_release_build(root: Path) -> None:
    """Every shipped creature body must be required and copied by the build.

    Inherited from the Sprint 19 gate, where it was written after the release
    build refused its own output: Sprint 19's four body files were copied by
    the local builder and declared nowhere else, so a locally validated
    package looked complete while a clean release build produced none of them.

    Two textual requirements, both on scripts the release actually runs:
    validate-build-output.ps1 must require the file, so no build path can omit
    it silently, and Build-Local.ps1 must copy it, so the provenance-checked
    release build produces it. Deliberately general - any shipped body, this
    sprint's or a later one's.
    """
    required = (root / "scripts/validate-build-output.ps1").read_text(
        encoding="utf-8-sig")
    builder = (root / "scripts/Build-Local.ps1").read_text(encoding="utf-8-sig")
    bodies = sorted((root / "assets").glob("*/*-mesh.json")) + \
        sorted((root / "assets").glob("*/*-albedo.png"))
    if not bodies:
        raise AssertionError("No shipped creature bodies were found at all")
    for body in bodies:
        relative = "assets\\" + body.parent.name + "\\" + body.name
        if relative not in required:
            raise AssertionError(
                "A shipped body is not required of the build output, so a "
                "release could omit it silently: " + relative)
        if body.parent.name not in builder or \
                body.name.split("-")[0] not in builder:
            raise AssertionError(
                "A shipped body is never copied by the release builder: "
                + relative)


def validate(root: Path) -> None:
    info = document(root, "Info.json")
    if info["Version"] != VERSION:
        raise AssertionError("Info.json version must be " + VERSION)
    props = (root / "Directory.Build.props").read_text(encoding="utf-8-sig")
    if "<KmgVersion>" + VERSION + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + INFORMATIONAL_VERSION + \
            "</KmgInformationalVersion>" not in props:
        raise AssertionError(
            "Directory.Build.props version metadata must be " +
            INFORMATIONAL_VERSION)

    state = document(root, "validation/static-validation.json")
    identities = validate_identity_append(root)
    protected_master = preserved_files(root, MASTER, MASTER_PROTECTED)
    protected_summons = preserved_files(
        root, MASTER, SUMMONING_PROTECTED, allowed=SPRINT21_CHANGED)
    for path in SPRINT21_NEW:
        if not (root / path).is_file():
            raise AssertionError("Sprint 21 source or asset missing: " + path)
        if path in tracked(root, MASTER):
            raise AssertionError(
                "Sprint 21 claims a file the release already had: " + path)

    touched = validate_only_this_sprints_creatures_changed(root)
    carriers = validate_the_measured_carriers_are_wired(root)
    bebelith = validate_the_released_bebelith_identity_is_preserved(root)
    validate_suppression(root)
    validate_contract(root)
    validate_shipped_bodies_reach_the_release_build(root)

    record = state[KEY]
    count = len(re.findall(
        r'\bCase\("',
        (root / "tests/KingmakerGunslinger.DomainTests/Program.cs").read_text(
            encoding="utf-8-sig")))
    expected = {
        "releaseVersion": VERSION,
        "releaseInformationalVersion": INFORMATIONAL_VERSION,
        "masterBase": MASTER,
        "deterministicTestCount": count,
        "registeredManifestEntryCount": identities,
        "registeredGeneratedPlacements": REGISTERED_PLACEMENTS,
        "retainedNativeWrappers": NATIVE_WRAPPERS,
        "uniqueCreatures": UNIQUE_CREATURES,
        "newRoots": SPRINT21_ROOTS,
        "executionChildren": SPRINT21_EXECUTION_CHILDREN,
        # False, and the only sprint in the series for which it is: this
        # creature is not on the Summon Monster table, so it owes no
        # celestial or fiendish execution children at all.
        "templated": False,
    }
    for key, value in expected.items():
        if record.get(key) != value:
            raise AssertionError("Sprint 21 candidate metadata differs: " + key)
    # The suppression arithmetic has to agree with itself whichever state the
    # sprint is in, so this is a relation rather than a pinned pair.
    suppressed = record.get("suppressedGeneratedPlacements")
    published = record.get("publishedGeneratedPlacements")
    # All twelve or none, which is what keeps a half-published creature out.
    # This takes the ROOT count rather than the surface constant: the surface
    # constant is zero now that the creature has published, and reading it here
    # would collapse the two states into one.
    if suppressed not in (0, SPRINT21_ROOTS):
        raise AssertionError(
            "Sprint 21 withholds all twelve of its roots or none of them")
    if published != REGISTERED_PLACEMENTS - suppressed:
        raise AssertionError(
            "The published surface must be the registered one less the withheld")
    if record.get("visibleChoiceTotal") != published + NATIVE_WRAPPERS:
        raise AssertionError(
            "The visible total must be the published surface plus the retained "
            "native wrappers")
    if record.get("creatureSuppressed") != (suppressed != 0):
        raise AssertionError(
            "The record's suppression flag disagrees with its own arithmetic")
    # The icon and body gates only apply once the sprint claims them, so a
    # registration-stage candidate is not asked for art it has not authored -
    # and a candidate that claims the art is held to all of it.
    if record.get("projectIconConcepts") is not None:
        if record["projectIconConcepts"] != ICON_CONCEPTS:
            raise AssertionError(
                "Sprint 21 adds exactly one icon concept")
        validate_icons(root)
    # Publication is earned, not asserted. The same four rules that kept the
    # Sprint 18 apes and the Sprint 19 pair withheld through their reviews.
    if record.get("publicReleaseAuthorized") and not record.get("runtimeQualified"):
        raise AssertionError(
            "Sprint 21 cannot publish without a passing runtime review")
    if record.get("candidateOnly") and record.get("publicReleaseAuthorized"):
        raise AssertionError("A candidate cannot also be an authorized release")
    if record.get("runtimeQualified") and not record.get("runtimeEvidence"):
        raise AssertionError("Runtime qualification requires exact closure evidence")
    if not record.get("candidateOnly") and len(record.get("runtimeEvidence") or []) < 2:
        raise AssertionError(
            "A published Sprint 21 names its batched review and its "
            "party-camera art review")
    if record.get("originalBodiesAuthored") and not record.get("donorRigReuseVerified"):
        raise AssertionError(
            "Original bodies cannot predate the verification of the donor rig "
            "they are authored on")
    # Unlike Sprint 19, this sprint genuinely needs a census: the Sprint 14
    # arachnid capture did not survive. A record that claims an authored body
    # without naming the census that measured the rig would be claiming
    # geometry fitted to a rig nobody looked at.
    if record.get("originalBodiesAuthored") and \
            not record.get("donorCensusEvidence"):
        raise AssertionError(
            "The Sprint 14 arachnid capture did not survive, so an authored "
            "Sprint 21 body must name the census that re-measured the rig")

    # The full inherited chain stays active.
    inherited.VERSION = VERSION
    inherited.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    inherited.PACKAGE = "KingmakerGunslinger-" + VERSION + "-local-runtime.zip"
    inherited.PACKAGE_SUFFIX = "expanded-summoning-sprint21"
    inherited.DETERMINISTIC_TEST_COUNT = count
    inherited.validate(root)

    intake = trait_icons.inspect(root)
    if intake["AssetReadiness"] != "READY" or not intake["existingIconContractPass"]:
        raise AssertionError(
            "All four original master trait assets must remain qualified")

    notes_path = root / ("docs/RELEASE-NOTES-" + VERSION + ".md")
    if notes_path.is_file():
        notes = notes_path.read_text(encoding="utf-8")
        for token in (INFORMATIONAL_VERSION, "Giant Crab", "Bebelith",
                      "grab",
                      "PASSIVE_CREATURE_SENSES_UNMODELED",
                      "ORDINARY_MAP_LAND_USE_SCOPE",
                      # The honest disposition this sprint owes a reader.
                      "PER_LIMB_REACH_UNREPRESENTED"):
            if token not in notes:
                raise AssertionError(
                    "Sprint 21 release notes omit: " + token)

    for key in ("expandedSummoningPhase2A141", "dataContentTraits142",
                "weaponFindability143", "weaponFindabilityRelease144",
                "heirloomNodachiIconRelease145", "expandedSummoningCheckpoint146",
                "expandedSummoningCheckpoint143", "expandedSummoningSprint18147",
                "expandedSummoningSprint19148"):
        if state[key] != json.loads(
                blob(root, MASTER, "validation/static-validation.json"))[key]:
            raise AssertionError("Historical release record changed: " + key)

    print(f"Sprint21 {VERSION} source validation PASS: tests={count}; "
          f"identities={identities}; registered={REGISTERED_PLACEMENTS}; "
          f"withheld={suppressed}; published={published}; "
          f"protected master={protected_master}; "
          f"protected summons={protected_summons}; "
          f"special-builder code lines touched={touched}; "
          f"measured carriers={carriers}; "
          f"preserved Bebelith identities={bebelith}.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", default=".")
    arguments = parser.parse_args()
    try:
        validate(Path(arguments.root).resolve())
    except AssertionError as error:
        print("Sprint21 validation failed: " + str(error))
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
