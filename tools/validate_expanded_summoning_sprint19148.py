#!/usr/bin/env python3
"""Validate the Expanded Summoning Sprint 19 candidate: Girallon and Xill.

Sprint 19 branches from the exact released v0.0.147 master and is allowed to
change one surface and no other. This validator states that surface by name:
every other byte of the released production tree, of the accepted summoning
tree and of every historical release record must be identical to the commit it
came from. Sprint 18 has shipped, so its two creatures and its four body files
join the protected tree and this sprint may not move them.

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

VERSION = "0.0.148"
INFORMATIONAL_VERSION = VERSION + "-expanded-summoning-sprint19"
MASTER = "95d610348d7d2322ddc00ef04fa06d2cd352dbdd"
KEY = "expandedSummoningSprint19148"

# The released production surfaces Sprint 19 may not touch at all.
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
# the released master except the exact files Sprint 19 declares below, so a
# creature that already qualified cannot be moved by this sprint. Sprint 18's
# shipped bodies are in here now.
SUMMONING_PROTECTED = (
    "src/KingmakerGunslinger/Summoning/",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoning",
    "assets-source/original-icons/expanded-summoning/",
    "assets/game/icons/expanded-summoning/",
    "assets/flying-animals/", "assets/sprint12-quadrupeds/",
    "assets/sprint13-creatures/", "assets/ungulates/",
    "assets/sprint14-insects/", "assets/sprint16-crocodilians/",
    "assets/sprint17-serpents/", "assets/sprint18-primates/",
)

# Exactly what Sprint 19 adds to or changes in that tree. A file not listed
# here and not new is compared byte for byte.
SPRINT19_CHANGED = (
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialProfiles.cs",
    "src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs",
    "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/PrimateRulesPolicy.cs",
    "src/KingmakerGunslinger/Summoning/PrimateCombatComponents.cs",
    "src/KingmakerGunslinger/Summoning/PrimateVisualPolicy.cs",
    "src/KingmakerGunslinger/Summoning/PrimateVisualAttachment.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningPrimateViewPatch.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCreatureReview.cs",
    "scripts/RuntimeAutomation.Common.ps1",
    "assets-source/original-icons/expanded-summoning/icon-manifest.json",
    "assets-source/original-icons/expanded-summoning/prompts/icon-prompts.json",
    "assets-source/original-icons/expanded-summoning/tools/render_creature_icon.py",
    "assets/game/icons/expanded-summoning/icon-manifest.json",
    "assets-source/original-models/sprint18-primates/generate_primates.py",
    "assets-source/original-models/sprint18-primates/primate_regions.py",
    "assets-source/original-models/sprint18-primates/paint_primate_albedo.py",
    "assets-source/original-models/sprint18-primates/render_primate_review.py",
    "assets-source/original-models/sprint18-primates/test_primate_prototype.py",
    "assets-source/original-models/sprint18-primates/SOURCE.md",
)

SPRINT19_NEW = (
    "src/KingmakerGunslinger/Summoning/GirallonRulesPolicy.cs",
    "src/KingmakerGunslinger/Summoning/XillRulesPolicy.cs",
    "src/KingmakerGunslinger/Summoning/XillCombatComponents.cs",
    "src/KingmakerGunslinger/RuntimeTesting/Sprint19ReviewPolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint19Review.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint19Routine.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint19Surface.cs",
    "tests/KingmakerGunslinger.DomainTests/Sprint19RulesTests.cs",
    "assets-source/original-icons/expanded-summoning/sources/girallon.png",
    "assets-source/original-icons/expanded-summoning/sources/xill.png",
    "assets/game/icons/expanded-summoning/girallon.png",
    "assets/game/icons/expanded-summoning/xill.png",
    "assets/sprint19-fourarmed/girallon-mesh.json",
    "assets/sprint19-fourarmed/girallon-albedo.png",
    "assets/sprint19-fourarmed/xill-mesh.json",
    "assets/sprint19-fourarmed/xill-albedo.png",
    "assets-source/original-models/sprint19-fourarmed/generate_fourarmed.py",
    "assets-source/original-models/sprint19-fourarmed/paint_fourarmed_albedo.py",
    "assets-source/original-models/sprint19-fourarmed/render_fourarmed_review.py",
    "assets-source/original-models/sprint19-fourarmed/test_fourarmed_prototype.py",
    "assets-source/original-models/sprint19-fourarmed/SOURCE.md",
    "planning/EXPANDED-SUMMONING-SPRINT19-CONTRACT.json",
    "docs/RELEASE-NOTES-0.0.148.md",
    "EXPANDED-SUMMONING-SPRINT19-STATE.md",
    "tools/validate_expanded_summoning_sprint19148.py",
    "tools/test_expanded_summoning_sprint19148.py",
)

# The four shipped Sprint 19 asset files, and what each one must say about
# itself before the runtime is allowed to show it. Both creatures print four
# arms on a two-armed donor rig, so both declare the shared driver chains
# rather than pretending to four.
SPRINT19_BODIES = {
    "girallon": dict(bones=56, size="Large", clawedHands=True),
    "xill": dict(bones=56, size="Medium", clawedHands=True),
}

SPRINT19_KEYS = ("girallon", "xill")

# What Sprint 19 allocates, and what each identity must be. Pinned here so a
# creature cannot quietly acquire a blueprint nobody reviewed. The released
# Sprint 18 carrier is NOT in this list: Girallon is granted the existing
# KMG.Summoning.Natural.Primate.FullStrengthLimbs rather than a renamed copy,
# because that symbol shipped in v0.0.147 and renaming it would retire a
# released GUID and mint a new one for the same fact.
SPRINT19_IDENTITY_TAIL = {
    "KMG.Summoning.Natural.Girallon.Bite1d6": "BlueprintItemWeapon",
    "KMG.Summoning.Natural.Girallon.Claw1d4": "BlueprintItemWeapon",
    "KMG.Summoning.Natural.Girallon.UnitType": "BlueprintUnitType",
    "KMG.Summoning.Special.Girallon.Rend": "BlueprintFeature",
    "KMG.Summoning.Special.Xill.Claw1d4": "BlueprintItemWeapon",
    "KMG.Summoning.Special.Xill.Bite1d3": "BlueprintItemWeapon",
    "KMG.Summoning.Special.Xill.UnitType": "BlueprintUnitType",
    "KMG.Summoning.Special.Xill.CombatTraits": "BlueprintBuff",
    "KMG.Summoning.Special.Xill.Paralysis": "BlueprintBuff",
    "KMG.Summoning.Special.Xill.FullStrengthLimbs": "BlueprintFeature",
}
SPRINT19_IDENTITIES = 22


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
    """The ledger is append-only: the released v0.0.147 entries keep their
    exact order, symbol, GUID and metadata, and Sprint 19 identities follow.

    Ten ability identities rather than thirty, because neither creature is
    templated: a magical beast with no Summon Monster entry and an outsider
    that is already evil take no celestial or fiendish execution children.
    """
    accepted = json.loads(blob(root, MASTER, "blueprints/blueprints.json"))["entries"]
    current = document(root, "blueprints/blueprints.json")["entries"]
    if current[:len(accepted)] != accepted:
        raise AssertionError("Sprint 19 moved or rewrote a historical identity")
    appended = current[len(accepted):]
    if len(accepted) != 2983 or len(appended) != SPRINT19_IDENTITIES:
        raise AssertionError(
            f"Sprint 19 must append exactly {SPRINT19_IDENTITIES} identities "
            f"to 2983; observed {len(accepted)} + {len(appended)}")
    if len({e["guid"] for e in current}) != len(current) or \
            len({e["symbol"] for e in current}) != len(current):
        raise AssertionError("Duplicate stable identity")
    if not all(re.fullmatch(r"[0-9a-f]{32}", e["guid"]) for e in appended):
        raise AssertionError("Appended identity GUIDs are malformed")
    for entry in appended:
        if not any(token in entry["symbol"] for token in ("Girallon", "Xill")):
            raise AssertionError(
                "Sprint 19 allocated an identity outside its two creatures: "
                + entry["symbol"])
    by_symbol = {e["symbol"]: e for e in appended}
    for symbol, planned in SPRINT19_IDENTITY_TAIL.items():
        entry = by_symbol.get(symbol)
        if entry is None or entry["plannedType"] != planned:
            raise AssertionError("Missing or mistyped Sprint 19 identity: " + symbol)
    units = [e for e in appended if e["plannedType"] == "BlueprintUnit"]
    abilities = [e for e in appended if e["plannedType"] == "BlueprintAbility"]
    if len(units) != 2 or len(abilities) != 10:
        raise AssertionError(
            "Sprint 19 appends two units and ten ability identities "
            "(ten untemplated roots, no celestial or fiendish children)")
    return len(current)


def validate_suppression(root: Path) -> None:
    visibility = (root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs"
                  ).read_text(encoding="utf-8")
    # Published. What the suppression set contains is checked before what the
    # counts claim, so a file that withholds something still says which
    # creature rather than only that a number moved. The named checks come
    # first for the same reason: the dangerous mistakes get their own message.
    suppressed = visibility.split("SuppressedCreatureKeys")[-1].split(";")[0]
    # Sprint 18 shipped. Neither ape may be withheld to make room.
    for key in ("dire-ape", "ape"):
        if '"' + key + '"' in suppressed:
            raise AssertionError(
                "Sprint 19 withheld a creature v0.0.147 published: " + key)
    for key in ("girallon", "xill"):
        if '"' + key + '"' in suppressed:
            raise AssertionError(
                "Sprint 19 publication did not remove its two keys: " + key)
    if '"' in suppressed:
        raise AssertionError("Sprint 19 publishes everything; nothing may be "
                             "withheld: " + suppressed.strip())
    # Every registered placement is visible, and the suppression set is empty
    # rather than missing: the two names came out and nothing else moved.
    for token in ("RegisteredLogicalPlacementCount = 1044",
                  "SuppressedLogicalPlacementCount = 0",
                  "new HashSet<string>(new string[0], StringComparer.Ordinal)"):
        if token not in visibility:
            raise AssertionError("Sprint 19 publication surface differs: " + token)
    catalog = (root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
               ).read_text(encoding="utf-8")
    for token in ('C("girallon","Girallon",null,false,5',
                  'C("xill","Xill",5,false,null',
                  'C("ape","Ape",3,true,3', 'C("dire-ape","Dire Ape",4,true,4',
                  "ValidateFamily(SummonFamily.Monster, 91, 524)",
                  "ValidateFamily(SummonFamily.NaturesAlly, 89, 520)"):
        if token not in catalog:
            raise AssertionError("Frozen Sprint 19 placement contract differs: " + token)


def validate_icons(root: Path) -> None:
    manifest = document(
        root, "assets-source/original-icons/expanded-summoning/icon-manifest.json")
    if manifest["count"] != 113 or len(manifest["icons"]) != 113:
        raise AssertionError("Project summon icon concept count must be 113")
    rows = {row["key"]: row for row in manifest["icons"]}
    for key in SPRINT19_KEYS:
        if key not in rows:
            raise AssertionError("Missing Sprint 19 icon concept: " + key)
    # Two creatures, two icons. A player picking between them in the menu
    # has the icon and the name, so sharing one painting would leave only
    # the name - and these two are not alike enough for that to be an
    # accident worth tolerating. Checked before the per-key hashes, because
    # a shared painting necessarily fails one of those first and "the xill
    # icon is not the reviewed one" is the wrong thing to say about it.
    if rows["girallon"]["outputSha256"] == rows["xill"]["outputSha256"] or             rows["girallon"]["sourceSha256"] == rows["xill"]["sourceSha256"]:
        raise AssertionError(
            "The Girallon and the Xill must not share one painting")
    digests = [value["outputSha256"] for value in manifest["icons"]]
    if len(set(digests)) != len(digests):
        raise AssertionError("Two creatures share one shipped icon")
    for key in SPRINT19_KEYS:
        row = rows[key]
        if row["width"] != 128 or row["height"] != 128 or row["format"] != "RGBA PNG":
            raise AssertionError("Sprint 19 icon concept is not the house format: " + key)
        shipped = root / "assets/game/icons/expanded-summoning" / (key + ".png")
        if not shipped.is_file():
            raise AssertionError("Sprint 19 icon is not shipped: " + key)
        if hashlib.sha256(shipped.read_bytes()).hexdigest() != row["outputSha256"]:
            raise AssertionError("Sprint 19 shipped icon is not the reviewed one: " + key)


def validate_contract(root: Path) -> None:
    contract = document(root, "planning/EXPANDED-SUMMONING-SPRINT19-CONTRACT.json")
    if contract.get("sprint") != 19 or contract.get("authority", {}).get(
            "masterBase") != MASTER:
        raise AssertionError("The frozen Sprint 19 contract does not match this branch")
    placement = contract["placement"]
    if placement["newRootTotal"] != 10 or \
            placement["surfaceAfter"]["registeredGeneratedPlacements"] != 1044 or \
            placement["surfaceAfter"]["visibleChoicesAfterPublication"] != 1073:
        raise AssertionError("The frozen contract arithmetic drifted")
    # The two printed lines this sprint is most likely to get wrong: a rend
    # that needs all four claws, and a bite that is primary in the natural
    # routine and therefore adds the whole Strength modifier.
    rend = contract["girallon"]["printedProfile"]["specialAttacks"][0]
    if rend["damage"] != "1d4+6" or "four" not in rend["trigger"]:
        raise AssertionError("The frozen printed Girallon rend line changed")
    if contract["xill"]["printedProfile"]["attributes"]["strength"] != 17 or \
            contract["xill"]["printedProfile"]["armorClass"] != 21:
        raise AssertionError("The frozen printed Xill profile changed")


def validate_shipped_bodies_reach_the_release_build(root: Path) -> None:
    """Every shipped creature body must be required and copied by the build.

    Sprint 19 shipped four body files that a clean release build produced none
    of: the builder copied them in the local path and the project file never
    declared them, so a locally validated 346-member package looked complete.
    The build-output validator caught it, and this check is the offline
    version of that catch.

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
        if body.parent.name not in builder or body.name.split("-")[0] not in builder:
            raise AssertionError(
                "A shipped body is never copied by the release builder: "
                + relative)


def validate(root: Path) -> None:
    identities = validate_identity_append(root)
    protected_master = preserved_files(root, MASTER, MASTER_PROTECTED)
    protected_summons = preserved_files(
        root, MASTER, SUMMONING_PROTECTED, allowed=SPRINT19_CHANGED)
    for path in SPRINT19_NEW:
        if not (root / path).is_file():
            raise AssertionError("Sprint 19 source or asset missing: " + path)
        if path in tracked(root, MASTER):
            raise AssertionError("Sprint 19 claims a file the release already had: " + path)

    for key, expected in SPRINT19_BODIES.items():
        body = document(root, "assets/sprint19-fourarmed/" + key + "-mesh.json")
        if body.get("creature") != key:
            raise AssertionError("Sprint 19 body is not its own creature: " + key)
        if body.get("triangleWinding") != "shared-exporter-sprint18":
            raise AssertionError("Sprint 19 body does not declare its winding: " + key)
        if body.get("donorPrefab") != "0bc98460fca38964aae3af6ad5c655ee" or \
                body.get("donorRenderer") != "Troll_base":
            raise AssertionError("Sprint 19 body names the wrong donor: " + key)
        # Four arms on a two-armed rig. The body must say so rather than
        # claim four independent limbs it does not have.
        if body.get("visibleLimbs") != 6 or \
                body.get("visibleArms") != 4 or \
                body.get("armDriverChains") != 2 or \
                body.get("lowerArmsShareUpperArmDrivers") is not True or \
                body.get("clawedHands") is not expected["clawedHands"] or \
                body.get("printedSize") != expected["size"]:
            raise AssertionError("Sprint 19 body anatomy contract broken: " + key)
        bones = body.get("bones")
        if not isinstance(bones, list) or len(bones) != expected["bones"] or \
                len(set(bones)) != expected["bones"]:
            raise AssertionError("Sprint 19 body driver set changed: " + key)
        if {"Tail_01", "Tail_02", "Tongue_01", "Tongue_02", "Tongue_03"} & set(bones):
            raise AssertionError(
                "Sprint 19 body weights a deliberately empty branch: " + key)
        albedo = body.get("albedo") or {}
        painting = root / "assets/sprint19-fourarmed" / str(albedo.get("file"))
        if not painting.is_file():
            raise AssertionError("Sprint 19 body has no painting beside it: " + key)
        if hashlib.sha256(painting.read_bytes()).hexdigest() != albedo.get("sha256"):
            raise AssertionError("Sprint 19 painting is not the reviewed one: " + key)

    state = document(root, "validation/static-validation.json")
    old = json.loads(blob(root, MASTER, "validation/static-validation.json"))
    for key in ("expandedSummoningPhase2A141", "dataContentTraits142",
                "weaponFindability143", "weaponFindabilityRelease144",
                "heirloomNodachiIconRelease145", "expandedSummoningCheckpoint146",
                "expandedSummoningCheckpoint143", "expandedSummoningSprint18147"):
        if state[key] != old[key]:
            raise AssertionError("Historical release record changed: " + key)

    from weapon_findability_native_reference import production_targets
    targets = production_targets(root)
    master_targets = production_targets(
        root, lambda path: blob(root, MASTER, path).decode("utf-8-sig"))
    references = document(
        root, "validation/weapon-findability-native-reference.json")["weapons"]
    if targets != master_targets or \
            [dict((k, row[k]) for k in targets[0]) for row in references] != targets:
        raise AssertionError("Released findability targets/native contracts changed")

    validate_suppression(root)
    validate_icons(root)
    validate_contract(root)
    validate_shipped_bodies_reach_the_release_build(root)

    count = len(re.findall(
        r'\bCase\("',
        (root / "tests/KingmakerGunslinger.DomainTests/Program.cs").read_text(
            encoding="utf-8-sig")))
    record = state[KEY]
    expected = {
        "releaseVersion": VERSION,
        "releaseInformationalVersion": INFORMATIONAL_VERSION,
        "masterBase": MASTER,
        "deterministicTestCount": count,
        # Sprint 18's 340 plus this sprint's four body files and two icons.
        # Pinned because the record first carried Sprint 18's 340 unchanged.
        "packageMemberCount": 346,
        "registeredManifestEntryCount": identities,
        "registeredGeneratedPlacements": 1044,
        "suppressedGeneratedPlacements": 0,
        "publishedGeneratedPlacements": 1044,
        "retainedNativeWrappers": 29,
        "visibleChoiceTotal": 1073,
        "uniqueCreatures": 101,
        "projectIconConcepts": 113,
        "newRoots": 10,
        "girallonRoots": 5,
        "xillRoots": 5,
        "bothSuppressed": False,
    }
    for key, value in expected.items():
        if record.get(key) != value:
            raise AssertionError("Sprint 19 candidate metadata differs: " + key)
    # Publication is earned, not asserted. These are the same four rules that
    # kept the Sprint 18 apes withheld for four guarded runs.
    if record.get("publicReleaseAuthorized") and not record.get("runtimeQualified"):
        raise AssertionError(
            "Sprint 19 cannot publish without a passing runtime review")
    if record.get("candidateOnly") and record.get("publicReleaseAuthorized"):
        raise AssertionError("A candidate cannot also be an authorized release")
    if record.get("runtimeQualified") and not record.get("runtimeEvidence"):
        raise AssertionError("Runtime qualification requires exact closure evidence")
    if not record.get("candidateOnly") and len(record.get("runtimeEvidence") or []) < 2:
        raise AssertionError(
            "A published Sprint 19 names its batched review and its "
            "party-camera art review")
    if record.get("originalBodiesAuthored") and not record.get("donorRigReuseVerified"):
        raise AssertionError(
            "Original bodies cannot predate the verification of the donor rig "
            "they are authored on")

    # The full inherited chain stays active.
    inherited.VERSION = VERSION
    inherited.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    inherited.PACKAGE = "KingmakerGunslinger-" + VERSION + "-local-runtime.zip"
    inherited.PACKAGE_SUFFIX = "expanded-summoning-sprint19"
    inherited.DETERMINISTIC_TEST_COUNT = count
    inherited.validate(root)

    intake = trait_icons.inspect(root)
    if intake["AssetReadiness"] != "READY" or not intake["existingIconContractPass"]:
        raise AssertionError(
            "All four original master trait assets must remain qualified")

    notes = (root / "docs/RELEASE-NOTES-0.0.148.md").read_text(encoding="utf-8")
    for token in (INFORMATIONAL_VERSION, "Girallon", "Xill", "rend",
                  "PASSIVE_CREATURE_SENSES_UNMODELED",
                  "ORDINARY_MAP_LAND_USE_SCOPE",
                  # The honest disposition this sprint owes a reader: what is
                  # withheld, which printed abilities are not represented, and
                  # that four arms share two animation chains.
                  "withheld", "FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS",
                  "MULTIWEAPON_ARMED_ROUTINE_UNREPRESENTED",
                  "IMPLANT_UNREPRESENTED", "Troll", "uninstall"):
        if token not in notes:
            raise AssertionError("Release notes lack honest disposition: " + token)

    print(f"Sprint19 {VERSION} source validation PASS: tests={count}; "
          f"identities={identities}; registered=1044; withheld=0; published=1044; "
          f"protected master={protected_master}; protected summons={protected_summons}.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print("Sprint19 validation failed: " + str(exc), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
