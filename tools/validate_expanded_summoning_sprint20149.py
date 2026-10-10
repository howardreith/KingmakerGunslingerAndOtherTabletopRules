#!/usr/bin/env python3
"""Validate the Expanded Summoning Sprint 20 candidate: the Giant Scorpion.

Sprint 20 branches from the exact released v0.0.148 master and is allowed to
change one surface and no other. This validator states that surface by name:
every other byte of the released production tree, of the accepted summoning
tree and of every historical release record must be identical to the commit it
came from. Sprint 19 has shipped, so the Girallon, the Xill and their four body
files join the protected tree and this sprint may not move them.

This sprint differs from Sprint 19 in one structural way worth stating up
front: its creature IS templated. Its six Summon Monster roots therefore also
hold back twelve celestial and fiendish execution children, allocated and
inert, which is why the identity arithmetic here is 31 appended entries
against Sprint 19's 22 for half as many creatures. Summon Nature's Ally never
templates, so its six roots own no children.

Runtime qualification is deliberately separate and is not claimed here.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_favored_class140 as inherited
import inspect_elemental_character_trait_icons as trait_icons

VERSION = "0.0.149"
INFORMATIONAL_VERSION = VERSION + "-expanded-summoning-sprint20"
MASTER = "37eb4ee5656ec1f59def1ad2dc2b0892edd22c83"
KEY = "expandedSummoningSprint20149"

# The released production surfaces Sprint 20 may not touch at all.
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
# the released master except the exact files Sprint 20 declares below, so a
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
    "assets/sprint19-fourarmed/",
)

# Exactly what Sprint 20 adds to or changes in that tree. A file not listed
# here and not new is compared byte for byte.
SPRINT20_CHANGED = (
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs",
    "src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs",
    "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
    # The exact-rank component, which Sprint 19 had already reduced to plain
    # fields and which this sprint renames from primate to exact because a
    # scorpion now uses it. Behaviour is unchanged.
    "src/KingmakerGunslinger/Summoning/PrimateCombatComponents.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCreatureReview.cs",
    "assets-source/original-icons/expanded-summoning/icon-manifest.json",
    "assets-source/original-icons/expanded-summoning/prompts/icon-prompts.json",
    "assets-source/original-icons/expanded-summoning/tools/render_creature_icon.py",
    "assets/game/icons/expanded-summoning/icon-manifest.json",
)

SPRINT20_NEW = (
    "src/KingmakerGunslinger/Summoning/GiantScorpionRulesPolicy.cs",
    "src/KingmakerGunslinger/Summoning/GiantScorpionPoison.cs",
    "src/KingmakerGunslinger/RuntimeTesting/ArachnidRigSurveyPolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint20Survey.cs",
    "tests/KingmakerGunslinger.DomainTests/Sprint20RulesTests.cs",
    "planning/EXPANDED-SUMMONING-SPRINT20-CONTRACT.json",
    "docs/RELEASE-NOTES-0.0.149.md",
    "tools/validate_expanded_summoning_sprint20149.py",
    "tools/test_expanded_summoning_sprint20149.py",
)

SPRINT20_KEY = "giant-scorpion"
SPRINT20_KEYS = (SPRINT20_KEY,)

# What Sprint 20 allocates, and what each identity must be. Pinned here so the
# creature cannot quietly acquire a blueprint nobody reviewed. The released
# Sprint 18 full-Strength carrier is NOT in this list: the scorpion is granted
# the existing KMG.Summoning.Natural.Primate.FullStrengthLimbs rather than a
# renamed copy, because that symbol shipped in v0.0.147 and renaming it would
# retire a released GUID and mint a new one for the same fact.
SPRINT20_IDENTITY_TAIL = {
    "KMG.Summoning.Natural.GiantScorpion.Claw1d6": "BlueprintItemWeapon",
    "KMG.Summoning.Natural.GiantScorpion.Sting1d6": "BlueprintItemWeapon",
    "KMG.Summoning.Natural.GiantScorpion.UnitType": "BlueprintUnitType",
    "KMG.Summoning.Natural.GiantScorpion.Poison": "BlueprintFeature",
    "KMG.Summoning.Natural.GiantScorpion.Venom": "BlueprintBuff",
    "KMG.Summoning.Natural.GiantScorpion.MindlessImmunity": "BlueprintFeature",
}
# One unit, twelve roots, twelve execution children, six tail entries.
# Twelve children rather than twenty-four: only the six Summon Monster roots
# are templated, and each owns one celestial and one fiendish child. Summon
# Nature's Ally never templates, so its six roots own none.
SPRINT20_ROOTS = 12
SPRINT20_MONSTER_ROOTS = 6
SPRINT20_EXECUTION_CHILDREN = SPRINT20_MONSTER_ROOTS * 2
SPRINT20_IDENTITIES = (1 + SPRINT20_ROOTS + SPRINT20_EXECUTION_CHILDREN +
                       len(SPRINT20_IDENTITY_TAIL))
MASTER_LEDGER_ENTRIES = 3005

REGISTERED_PLACEMENTS = 1056
SUPPRESSED_PLACEMENTS = 12
PUBLISHED_WHILE_WITHHELD = 1044
NATIVE_WRAPPERS = 29
VISIBLE_WHILE_WITHHELD = PUBLISHED_WHILE_WITHHELD + NATIVE_WRAPPERS
VISIBLE_AFTER_PUBLICATION = REGISTERED_PLACEMENTS + NATIVE_WRAPPERS
UNIQUE_CREATURES = 102
ICON_CONCEPTS = 114


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


def validate_identity_append(root: Path) -> int:
    """The ledger is append-only: the released v0.0.148 entries keep their
    exact order, symbol, GUID and metadata, and Sprint 20 identities follow.

    Thirty-one entries for one creature, where Sprint 19 spent twenty-two on
    two. The difference is templating, not waste: this creature appears on the
    Summon Monster table, so each of its six Monster roots owns a celestial
    and a fiendish execution child. Twelve roots, twenty-four children.
    """
    accepted = json.loads(blob(root, MASTER, "blueprints/blueprints.json"))["entries"]
    current = document(root, "blueprints/blueprints.json")["entries"]
    if current[:len(accepted)] != accepted:
        raise AssertionError("Sprint 20 moved or rewrote a historical identity")
    appended = current[len(accepted):]
    if len(accepted) != MASTER_LEDGER_ENTRIES or \
            len(appended) != SPRINT20_IDENTITIES:
        raise AssertionError(
            f"Sprint 20 must append exactly {SPRINT20_IDENTITIES} identities "
            f"to {MASTER_LEDGER_ENTRIES}; observed {len(accepted)} + "
            f"{len(appended)}")
    if len({e["guid"] for e in current}) != len(current) or \
            len({e["symbol"] for e in current}) != len(current):
        raise AssertionError("Duplicate stable identity")
    if not all(re.fullmatch(r"[0-9a-f]{32}", e["guid"]) for e in appended):
        raise AssertionError("Appended identity GUIDs are malformed")
    for entry in appended:
        if "GiantScorpion" not in entry["symbol"]:
            raise AssertionError(
                "Sprint 20 allocated an identity outside its one creature: "
                + entry["symbol"])
    by_symbol = {e["symbol"]: e for e in appended}
    for symbol, planned in SPRINT20_IDENTITY_TAIL.items():
        entry = by_symbol.get(symbol)
        if entry is None or entry["plannedType"] != planned:
            raise AssertionError("Missing or mistyped Sprint 20 identity: " + symbol)
    units = [e for e in appended if e["plannedType"] == "BlueprintUnit"]
    abilities = [e for e in appended if e["plannedType"] == "BlueprintAbility"]
    children = [e for e in abilities
                if e["symbol"].endswith(".Celestial") or
                e["symbol"].endswith(".Fiendish")]
    if len(units) != 1:
        raise AssertionError("Sprint 20 appends exactly one unit identity")
    if len(abilities) != SPRINT20_ROOTS + SPRINT20_EXECUTION_CHILDREN:
        raise AssertionError(
            f"Sprint 20 appends {SPRINT20_ROOTS} roots and "
            f"{SPRINT20_EXECUTION_CHILDREN} execution children")
    if len(children) != SPRINT20_EXECUTION_CHILDREN:
        raise AssertionError(
            "A templated creature's Summon Monster roots each own a celestial "
            "and a fiendish execution child; observed " + str(len(children)))
    roots = [e for e in abilities if e not in children]
    monster_roots = [e for e in roots if ".SM." in e["symbol"]]
    ally_roots = [e for e in roots if ".SNA." in e["symbol"]]
    if len(monster_roots) != SPRINT20_MONSTER_ROOTS or             len(ally_roots) != SPRINT20_ROOTS - SPRINT20_MONSTER_ROOTS:
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
    # Withheld. What the suppression set contains is checked before what the
    # counts claim, so a file that withholds the wrong thing names the creature
    # rather than only reporting that a number moved.
    # The DECLARATION, not the last mention: the name appears again in the
    # IsPublished lookup, and that occurrence carries no creature keys at all,
    # so taking the last one would read an empty suppression set as correct
    # however the real one was written.
    declaration = visibility.split("SuppressedCreatureKeys =")
    if len(declaration) != 2:
        raise AssertionError(
            "The suppression set must be declared exactly once")
    suppressed = declaration[1].split(";")[0]
    if '"' + SPRINT20_KEY + '"' not in suppressed:
        raise AssertionError(
            "Sprint 20 must register its creature withheld: " + SPRINT20_KEY)
    # Everything v0.0.148 published stays published. Sprint 18's apes and
    # Sprint 19's Girallon and Xill are the ones a careless edit would reach.
    for key in ("ape", "dire-ape", "girallon", "xill"):
        if '"' + key + '"' in suppressed:
            raise AssertionError(
                "Sprint 20 withheld a creature v0.0.148 published: " + key)
    for token in ("RegisteredLogicalPlacementCount = "
                  + str(REGISTERED_PLACEMENTS),
                  "SuppressedLogicalPlacementCount = "
                  + str(SUPPRESSED_PLACEMENTS)):
        if token not in visibility:
            raise AssertionError("Sprint 20 candidate surface differs: " + token)
    catalog = (root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
               ).read_text(encoding="utf-8")
    for token in ('C("giant-scorpion","Giant Scorpion",4,true,4',
                  'C("girallon","Girallon",null,false,5',
                  'C("xill","Xill",5,false,null',
                  'C("ape","Ape",3,true,3', 'C("dire-ape","Dire Ape",4,true,4',
                  "ValidateFamily(SummonFamily.Monster, 92, 530)",
                  "ValidateFamily(SummonFamily.NaturesAlly, 90, 526)"):
        if token not in catalog:
            raise AssertionError("Frozen Sprint 20 placement contract differs: " + token)
    # The scorpion is templated and both of its tiers are 4. Getting either
    # wrong silently changes how many identities the sprint owes.
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
    for key in SPRINT20_KEYS:
        if key not in rows:
            raise AssertionError("Missing Sprint 20 icon concept: " + key)
    digests = [value["outputSha256"] for value in manifest["icons"]]
    if len(set(digests)) != len(digests):
        raise AssertionError("Two creatures share one shipped icon")
    sources = [value["sourceSha256"] for value in manifest["icons"]]
    if len(set(sources)) != len(sources):
        raise AssertionError("Two creatures share one icon painting")
    for key in SPRINT20_KEYS:
        row = rows[key]
        if row["width"] != 128 or row["height"] != 128 or row["format"] != "RGBA PNG":
            raise AssertionError(
                "Sprint 20 icon concept is not the house format: " + key)
        shipped = root / "assets/game/icons/expanded-summoning" / (key + ".png")
        if not shipped.is_file():
            raise AssertionError("Sprint 20 icon is not shipped: " + key)
        if hashlib.sha256(shipped.read_bytes()).hexdigest() != row["outputSha256"]:
            raise AssertionError(
                "Sprint 20 shipped icon is not the reviewed one: " + key)


def validate_contract(root: Path) -> None:
    contract = document(root, "planning/EXPANDED-SUMMONING-SPRINT20-CONTRACT.json")
    if contract.get("sprint") != 20 or contract.get("authority", {}).get(
            "masterBase") != MASTER:
        raise AssertionError(
            "The frozen Sprint 20 contract does not match this branch")
    placement = contract["placement"]
    if placement["newRoots"]["total"] != SPRINT20_ROOTS or \
            placement["executionChildren"] != SPRINT20_EXECUTION_CHILDREN or \
            placement["templated"] is not True or \
            placement["surfaceAfter"]["registeredGeneratedPlacements"] != \
            REGISTERED_PLACEMENTS or \
            placement["surfaceAfter"]["visibleChoicesAfterPublication"] != \
            VISIBLE_AFTER_PUBLICATION:
        raise AssertionError("The frozen contract arithmetic drifted")
    # The printed lines this sprint is most likely to get wrong: three limbs
    # at the plain Strength modifier rather than one and a half times it, a
    # six-round poison where every shipped carrier runs four, and the eight
    # legs that produce the printed anti-trip defence.
    printed = contract["printedProfile"]
    if printed["abilityScores"]["strength"] != 19 or \
            printed["abilityScores"]["intelligence"] is not None or \
            printed["hitPoints"]["total"] != 37 or \
            printed["armorClass"]["total"] != 16:
        raise AssertionError("The frozen printed Giant Scorpion profile changed")
    if "1d6+4" not in printed["melee"] or "plus grab" not in printed["melee"] \
            or "plus poison" not in printed["melee"]:
        raise AssertionError("The frozen printed melee line changed")
    if printed["poison"]["frequency"] != "1/round for 6 rounds" or \
            printed["poison"]["effect"] != "1d2 Strength damage":
        raise AssertionError("The frozen printed poison graph changed")
    if printed["combatManeuverDefense"]["versusTrip"] - \
            printed["combatManeuverDefense"]["base"] != 12:
        raise AssertionError(
            "The frozen printed eight-legged trip defence changed")
    # Every honest omission this sprint owes a reader must stay named in the
    # contract, so one cannot quietly become an implementation.
    recorded = {row["id"] for row in contract["honestOmissions"]}
    for required in ("PASSIVE_CREATURE_SENSES_UNMODELED",
                     "ORDINARY_MAP_LAND_USE_SCOPE",
                     "PER_LIMB_REACH_UNREPRESENTED"):
        if required not in recorded:
            raise AssertionError(
                "The frozen contract stopped recording an omission: " + required)


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
        root, MASTER, SUMMONING_PROTECTED, allowed=SPRINT20_CHANGED)
    for path in SPRINT20_NEW:
        if not (root / path).is_file():
            raise AssertionError("Sprint 20 source or asset missing: " + path)
        if path in tracked(root, MASTER):
            raise AssertionError(
                "Sprint 20 claims a file the release already had: " + path)

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
        "newRoots": SPRINT20_ROOTS,
        "executionChildren": SPRINT20_EXECUTION_CHILDREN,
        "templated": True,
    }
    for key, value in expected.items():
        if record.get(key) != value:
            raise AssertionError("Sprint 20 candidate metadata differs: " + key)
    # The suppression arithmetic has to agree with itself whichever state the
    # sprint is in, so this is a relation rather than a pinned pair.
    suppressed = record.get("suppressedGeneratedPlacements")
    published = record.get("publishedGeneratedPlacements")
    if suppressed not in (0, SUPPRESSED_PLACEMENTS):
        raise AssertionError(
            "Sprint 20 withholds all twelve of its roots or none of them")
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
                "Sprint 20 adds exactly one icon concept")
        validate_icons(root)
    # Publication is earned, not asserted. The same four rules that kept the
    # Sprint 18 apes and the Sprint 19 pair withheld through their reviews.
    if record.get("publicReleaseAuthorized") and not record.get("runtimeQualified"):
        raise AssertionError(
            "Sprint 20 cannot publish without a passing runtime review")
    if record.get("candidateOnly") and record.get("publicReleaseAuthorized"):
        raise AssertionError("A candidate cannot also be an authorized release")
    if record.get("runtimeQualified") and not record.get("runtimeEvidence"):
        raise AssertionError("Runtime qualification requires exact closure evidence")
    if not record.get("candidateOnly") and len(record.get("runtimeEvidence") or []) < 2:
        raise AssertionError(
            "A published Sprint 20 names its batched review and its "
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
            "Sprint 20 body must name the census that re-measured the rig")

    # The full inherited chain stays active.
    inherited.VERSION = VERSION
    inherited.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    inherited.PACKAGE = "KingmakerGunslinger-" + VERSION + "-local-runtime.zip"
    inherited.PACKAGE_SUFFIX = "expanded-summoning-sprint20"
    inherited.DETERMINISTIC_TEST_COUNT = count
    inherited.validate(root)

    intake = trait_icons.inspect(root)
    if intake["AssetReadiness"] != "READY" or not intake["existingIconContractPass"]:
        raise AssertionError(
            "All four original master trait assets must remain qualified")

    notes_path = root / ("docs/RELEASE-NOTES-" + VERSION + ".md")
    if notes_path.is_file():
        notes = notes_path.read_text(encoding="utf-8")
        for token in (INFORMATIONAL_VERSION, "Giant Scorpion", "poison",
                      "PASSIVE_CREATURE_SENSES_UNMODELED",
                      "ORDINARY_MAP_LAND_USE_SCOPE",
                      # The honest disposition this sprint owes a reader.
                      "PER_LIMB_REACH_UNREPRESENTED"):
            if token not in notes:
                raise AssertionError(
                    "Sprint 20 release notes omit: " + token)

    for key in ("expandedSummoningPhase2A141", "dataContentTraits142",
                "weaponFindability143", "weaponFindabilityRelease144",
                "heirloomNodachiIconRelease145", "expandedSummoningCheckpoint146",
                "expandedSummoningCheckpoint143", "expandedSummoningSprint18147",
                "expandedSummoningSprint19148"):
        if state[key] != json.loads(
                blob(root, MASTER, "validation/static-validation.json"))[key]:
            raise AssertionError("Historical release record changed: " + key)

    print(f"Sprint20 {VERSION} source validation PASS: tests={count}; "
          f"identities={identities}; registered={REGISTERED_PLACEMENTS}; "
          f"withheld={suppressed}; published={published}; "
          f"protected master={protected_master}; "
          f"protected summons={protected_summons}.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", default=".")
    arguments = parser.parse_args()
    try:
        validate(Path(arguments.root).resolve())
    except AssertionError as error:
        print("Sprint20 validation failed: " + str(error))
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
