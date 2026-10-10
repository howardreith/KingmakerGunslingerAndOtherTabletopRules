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

# The released Bebelith, named file by file. This sprint owns its body and
# nothing else, and the easiest way for it to go wrong is to improve a
# creature that already published: its rules are qualified, its recorded
# deviations were accepted then, and its chassis differs from the printed
# Bestiary block on purpose. Every one of these is already inside
# SUMMONING_PROTECTED; they are named again here so a failure says Bebelith
# rather than only naming a file.
BEBELITH_RULES = (
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialProfiles.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialCombatComponents.cs",
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
    # the released Bebelith's own traits are configured in the same file.
    # Admitted with two checks rather than on trust: see
    # validate_the_grab_is_the_only_addition, which requires every added code
    # line to belong to the Giant Crab, and
    # validate_the_released_bebelith_is_untouched, which requires every line
    # mentioning the Bebelith to be the released one.
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCreatureReview.cs",
    "assets-source/original-icons/expanded-summoning/icon-manifest.json",
    "assets-source/original-icons/expanded-summoning/prompts/icon-prompts.json",
    "assets/game/icons/expanded-summoning/icon-manifest.json",
)

SPRINT21_NEW = (
    "src/KingmakerGunslinger/Summoning/GiantCrabRulesPolicy.cs",
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
                       len(SPRINT21_IDENTITY_TAIL))
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


def validate_the_grab_is_the_only_addition(root: Path) -> int:
    """The special builder gains this creature's grab and nothing else.

    The file is admitted into the boundary because every grabber in the
    project lives in it, which makes a blanket byte comparison impossible and
    a blanket waiver dangerous: this one file configures the grab, constrict,
    swallow, rake and death roll of two dozen creatures that have already
    qualified. So the waiver is narrow. Nothing may be removed, and every
    added line must belong to the Giant Crab - which is checked by
    rebuilding the file from the released one plus the additions and requiring
    the result to be byte-identical to what is on disk.
    """
    accepted = blob(root, MASTER,
                    "src/KingmakerGunslinger/Blueprints/"
                    "ExpandedSummoningSpecialBuilder.cs").decode("utf-8")
    current = (root / "src/KingmakerGunslinger/Blueprints/"
               "ExpandedSummoningSpecialBuilder.cs").read_text(encoding="utf-8")
    was = accepted.replace("\r\n", "\n").split("\n")
    now = current.replace("\r\n", "\n").split("\n")
    matcher = difflib.SequenceMatcher(None, was, now, autojunk=False)
    added = []
    for tag, i1, i2, j1, j2 in matcher.get_opcodes():
        if tag == "equal":
            continue
        if tag in ("delete", "replace"):
            raise AssertionError(
                "Sprint 21 removed or rewrote a released line of the special "
                "builder: " + "; ".join(was[i1:i2])[:200])
        added.extend(now[j1:j2])
    if not added:
        raise AssertionError(
            "The crab's grab is not wired in the special builder at all, "
            "which is how the printed +12 grapple figure went two points "
            "light while three places claimed it worked.")
    # No added CODE line may name another creature. Comments are exempt, and
    # deliberately so: the scorpion takes the multi-target hold Sprint 19
    # built for the Xill, and a comment forbidden from saying which creature a
    # shared buff came from is a worse comment rather than a safer file. What
    # this refuses is a line that configures, requires or renames another
    # creature's wiring.
    code = [line for line in added if not line.strip().startswith("//")]
    for other in ("GiantAnt", "Owlbear", "PurpleWorm", "Xill", "Tiger",
                  "Lion", "Leopard", "Crocodile", "Salamander", "Bear",
                  "ShamblingMound", "GiantFlytrap", "MonitorLizard",
                  "ConstrictorSnake", "Cheetah", "Stirge", "Cyclops"):
        if any(other in line for line in code):
            raise AssertionError(
                "Sprint 21 touched another creature's wiring in the special "
                "builder: " + other)
    if not any("ConfigureGrabber(library, bySymbol, GiantCrabUnitSymbol"
               in line for line in code):
        raise AssertionError("The addition does not configure a grabber")
    # Primary plus one additional limb is both claws and stops short of the
    # sting, which is the second additional limb.
    joined = "\n".join(code)
    for required in ("Primary = true",
                     "Additional = GiantCrabRulesPolicy.ClawCount - 1",
                     "MaxHeld = GiantCrabRulesPolicy.ClawCount",
                     "Hold = multiHold, Grappled = multiHeld",
                     '"KMG.Summoning.Special.GiantCrab.Traits"'):
        if required not in joined:
            raise AssertionError(
                "The crab's grab spec is missing: " + required)
    if "Rake" in joined:
        raise AssertionError("A crab rakes nothing")
    return len(added)


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
        if "GiantCrab" not in entry["symbol"]:
            raise AssertionError(
                "Sprint 21 allocated an identity outside its one creature: "
                + entry["symbol"])
    by_symbol = {e["symbol"]: e for e in appended}
    for symbol, planned in SPRINT21_IDENTITY_TAIL.items():
        entry = by_symbol.get(symbol)
        if entry is None or entry["plannedType"] != planned:
            raise AssertionError("Missing or mistyped Sprint 21 identity: " + symbol)
    units = [e for e in appended if e["plannedType"] == "BlueprintUnit"]
    abilities = [e for e in appended if e["plannedType"] == "BlueprintAbility"]
    children = [e for e in abilities
                if e["symbol"].endswith(".Celestial") or
                e["symbol"].endswith(".Fiendish")]
    if len(units) != 1:
        raise AssertionError("Sprint 21 appends exactly one unit identity")
    if len(abilities) != SPRINT21_ROOTS + SPRINT21_EXECUTION_CHILDREN:
        raise AssertionError(
            f"Sprint 21 appends {SPRINT21_ROOTS} roots and "
            f"{SPRINT21_EXECUTION_CHILDREN} execution children")
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
    if contract.get("sprint") != 21 or contract.get("masterBase") not in (
            MASTER, MASTER[:8]):
        raise AssertionError(
            "The frozen Sprint 21 contract does not match this branch")
    placement = contract["placement"]["giantCrab"]
    if placement["roots"] != SPRINT21_ROOTS or \
            placement["templated"] is not False or \
            placement["registeredPlacementsAfter"] != REGISTERED_PLACEMENTS or \
            placement["uniqueCreaturesAfter"] != UNIQUE_CREATURES:
        raise AssertionError("The frozen contract arithmetic drifted")
    # No execution children at all, which is the structural difference from
    # every templated creature in the series, and the contract has to say why
    # rather than leaving a zero to be read as an oversight.
    if SPRINT21_EXECUTION_CHILDREN != 0 or \
            placement.get("templatedReason") is None:
        raise AssertionError(
            "An untemplated Nature's Ally creature owns no execution children "
            "and the contract has to say why")
    # The released Bebelith's three roots, declared and unchanged.
    bebelith = contract["placement"]["bebelith"]
    if bebelith["roots"] != 3 or "untouched" not in bebelith["change"]:
        raise AssertionError(
            "The frozen contract stopped declaring the Bebelith unchanged")
    # The printed lines this sprint is most likely to get wrong: two claws at
    # the plain Strength modifier rather than one and a half times it, the
    # eight legs that produce the printed anti-trip defence, and the aquatic
    # half of the block that must stay recorded rather than implemented.
    printed = contract["printedProfile"]
    if printed["abilityScores"]["strength"] != 15 or \
            printed["abilityScores"]["intelligence"] is not None or \
            printed["hitPoints"]["total"] != 19 or \
            printed["armorClass"]["total"] != 15:
        raise AssertionError("The frozen printed Giant Crab profile changed")
    if "1d4+2" not in printed["melee"] or "plus grab" not in printed["melee"] \
            or "2 claws" not in printed["melee"]:
        raise AssertionError("The frozen printed melee line changed")
    if "plus poison" in printed["melee"]:
        raise AssertionError(
            "A crab prints no poison; a contract that claimed one would owe a "
            "carrier and a review case neither this sprint nor its review has")
    if printed["combatManeuverDefense"]["versusTrip"] - \
            printed["combatManeuverDefense"]["base"] != 12:
        raise AssertionError(
            "The frozen printed eight-legged trip defence changed")
    # The aquatic half stays in the contract as printed numbers, so the
    # omission cannot quietly become a substitution: a swim speed folded into
    # the ground speed, or a skill raised to stand in for Swim.
    if printed["swimSpeedFeet"] != 20 or printed["speedFeet"] != 30 or \
            printed["racialModifiers"]["Swim"] != 8 or \
            printed["skills"]["Swim"] != 10:
        raise AssertionError("The frozen printed aquatic line changed")
    if printed["swimSpeedFeet"] == printed["speedFeet"]:
        raise AssertionError(
            "The printed swim speed must stay distinct from the ground speed "
            "it is not substituted into")
    # Every honest omission this sprint owes a reader must stay named in the
    # contract, so one cannot quietly become an implementation.
    recorded = {row["id"] for row in contract["honestOmissions"]}
    for required in ("PASSIVE_CREATURE_SENSES_UNMODELED",
                     "ORDINARY_MAP_LAND_USE_SCOPE"):
        if required not in recorded:
            raise AssertionError(
                "The frozen contract stopped recording an omission: " + required)
    # And the boundary this creature exists to test. An aquatic creature is
    # exactly the thing that would tempt a subsystem into being.
    if not any("aquatic" in row.lower() for row in contract["hardBoundaries"]):
        raise AssertionError(
            "The frozen contract stopped forbidding an aquatic subsystem")


def validate_the_released_bebelith_is_untouched(root: Path) -> int:
    """The Bebelith's rules are the release's, line for line.

    This sprint owns its body and nothing else. Its mechanics were qualified in
    an earlier phase, its recorded deviations - a bounded one-round armour-class
    penalty in place of permanent armour destruction, demon hunting keyed to
    exact chaotic-evil outsider facts, rot and climb omitted - were accepted
    then, and its chassis differs from the printed Bestiary block on purpose.
    The easiest way for this sprint to go wrong is to improve it, so every line
    of every file that mentions it has to be the released one.
    """
    checked = 0
    for path in BEBELITH_RULES:
        accepted = blob(root, MASTER, path).decode("utf-8").replace(
            "\r\n", "\n").split("\n")
        current = (root / path).read_text(encoding="utf-8-sig").replace(
            "\r\n", "\n").split("\n")
        was = [line for line in accepted if "ebelith" in line]
        now = [line for line in current if "ebelith" in line]
        if was != now:
            raise AssertionError(
                "Sprint 21 changed a released Bebelith line in " + path)
        if not was:
            raise AssertionError(
                "No Bebelith line found in " + path + ", so this check has "
                "stopped checking anything")
        checked += len(was)
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

    grabLines = validate_the_grab_is_the_only_addition(root)
    carriers = validate_the_measured_carriers_are_wired(root)
    bebelith = validate_the_released_bebelith_is_untouched(root)
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
          f"grab wiring lines={grabLines}; "
          f"measured carriers={carriers}; "
          f"released Bebelith lines={bebelith}.")


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
