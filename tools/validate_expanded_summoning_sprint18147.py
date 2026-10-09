#!/usr/bin/env python3
"""Validate the Expanded Summoning Sprint 18 candidate: Ape and Dire Ape.

Sprint 18 branches from the exact released v0.0.146 master and is allowed to
change one surface and no other. This validator states that surface by name:
every other byte of the released production tree, of the accepted summoning
tree and of every historical release record must be identical to the commit it
came from.

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

VERSION = "0.0.147"
INFORMATIONAL_VERSION = VERSION + "-expanded-summoning-sprint18"
MASTER = "da782d2297d6cf361ac30a5e0ea07348d3b7f7d7"
KEY = "expandedSummoningSprint18147"

# The released production surfaces Sprint 18 may not touch at all.
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
# the released master except the exact files Sprint 18 declares below, so a
# creature that already qualified cannot be moved by this sprint.
SUMMONING_PROTECTED = (
    "src/KingmakerGunslinger/Summoning/",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoning",
    "assets-source/original-icons/expanded-summoning/",
    "assets/game/icons/expanded-summoning/",
    "assets/flying-animals/", "assets/sprint12-quadrupeds/",
    "assets/sprint13-creatures/", "assets/ungulates/",
    "assets/sprint14-insects/", "assets/sprint16-crocodilians/",
    "assets/sprint17-serpents/",
)

# Exactly what Sprint 18 adds to or changes in that tree. A file not listed
# here and not new is compared byte for byte.
SPRINT18_CHANGED = (
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs",
    "src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs",
    "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "scripts/RuntimeAutomation.Common.ps1",
    "assets-source/original-icons/expanded-summoning/icon-manifest.json",
    "assets-source/original-icons/expanded-summoning/prompts/icon-prompts.json",
    "assets-source/original-icons/expanded-summoning/tools/render_creature_icon.py",
    "assets/game/icons/expanded-summoning/icon-manifest.json",
)

SPRINT18_NEW = (
    "src/KingmakerGunslinger/Summoning/PrimateRulesPolicy.cs",
    "src/KingmakerGunslinger/Summoning/PrimateCombatComponents.cs",
    "src/KingmakerGunslinger/Summoning/PrimateVisualPolicy.cs",
    "src/KingmakerGunslinger/Summoning/PrimateVisualAttachment.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningPrimateViewPatch.cs",
    "src/KingmakerGunslinger/RuntimeTesting/PrimateRigSurveyPolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/PrimateReviewPolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint18Review.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint18Routine.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint18Surface.cs",
    "tests/KingmakerGunslinger.DomainTests/PrimateVisualTests.cs",
    "assets-source/original-icons/expanded-summoning/sources/ape.png",
    "assets-source/original-icons/expanded-summoning/sources/dire-ape.png",
    "assets/game/icons/expanded-summoning/ape.png",
    "assets/game/icons/expanded-summoning/dire-ape.png",
    "assets/sprint18-primates/ape-mesh.json",
    "assets/sprint18-primates/ape-albedo.png",
    "assets/sprint18-primates/dire-ape-mesh.json",
    "assets/sprint18-primates/dire-ape-albedo.png",
    "assets-source/original-models/sprint18-primates/primate_capture.py",
    "assets-source/original-models/sprint18-primates/primate_regions.py",
    "assets-source/original-models/sprint18-primates/generate_primates.py",
    "assets-source/original-models/sprint18-primates/paint_primate_albedo.py",
    "assets-source/original-models/sprint18-primates/render_primate_review.py",
    "assets-source/original-models/sprint18-primates/test_primate_prototype.py",
    "assets-source/original-models/sprint18-primates/SOURCE.md",
)

# The four shipped Sprint 18 asset files, and what each one must say about
# itself before the runtime is allowed to show it.
SPRINT18_BODIES = {
    "ape": dict(clawedHands=False, bones=56),
    "dire-ape": dict(clawedHands=True, bones=56),
}

APE_KEYS = ("ape", "dire-ape")


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
    """The ledger is append-only: master's entries keep their exact order,
    symbol, GUID and metadata, and Sprint 18's 61 identities follow them.

    Sixty rather than fifty-eight because the guarded review measured the
    Dire Ape biting for 1d8 and clawing for 1d6: the shared native weapons
    do not override their damage dice, so the engine scales them one step up
    for a Large wielder. Its printed entry is 1d6 and 1d4, so it owns two
    weapons of its own and the shared ones are left exactly as they were.
    """
    accepted = json.loads(blob(root, MASTER, "blueprints/blueprints.json"))["entries"]
    current = document(root, "blueprints/blueprints.json")["entries"]
    if current[:len(accepted)] != accepted:
        raise AssertionError("Sprint 18 moved or rewrote a historical identity")
    appended = current[len(accepted):]
    if len(accepted) != 2922 or len(appended) != 61:
        raise AssertionError(
            f"Sprint 18 must append exactly 61 identities to 2922; "
            f"observed {len(accepted)} + {len(appended)}")
    if len({e["guid"] for e in current}) != len(current) or \
            len({e["symbol"] for e in current}) != len(current):
        raise AssertionError("Duplicate stable identity")
    if not all(re.fullmatch(r"[0-9a-f]{32}", e["guid"]) for e in appended):
        raise AssertionError("Appended identity GUIDs are malformed")
    expected_tail = {
        "KMG.Summoning.Natural.Slam1d6": "BlueprintItemWeapon",
        "KMG.Summoning.Natural.DireApe.Bite1d6": "BlueprintItemWeapon",
        "KMG.Summoning.Natural.DireApe.Claw1d4": "BlueprintItemWeapon",
        "KMG.Summoning.Natural.Ape.UnitType": "BlueprintUnitType",
        "KMG.Summoning.Natural.DireApe.UnitType": "BlueprintUnitType",
        "KMG.Summoning.Special.DireApe.Rend": "BlueprintFeature",
        "KMG.Summoning.Natural.Primate.FullStrengthLimbs": "BlueprintFeature",
    }
    by_symbol = {e["symbol"]: e for e in appended}
    for symbol, planned in expected_tail.items():
        entry = by_symbol.get(symbol)
        if entry is None or entry["plannedType"] != planned:
            raise AssertionError("Missing or mistyped Sprint 18 identity: " + symbol)
    units = [e for e in appended if e["plannedType"] == "BlueprintUnit"]
    abilities = [e for e in appended if e["plannedType"] == "BlueprintAbility"]
    if len(units) != 2 or len(abilities) != 52:
        raise AssertionError(
            "Sprint 18 appends two units and 52 ability identities "
            "(26 roots plus 26 template children)")
    return len(current)


def validate_suppression(root: Path) -> None:
    visibility = (root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs"
                  ).read_text(encoding="utf-8")
    # Published. Every registered placement is visible, nothing is
    # withheld, and the suppression set is empty rather than missing: the
    # two names came out and nothing else in the file moved.
    for token in ("RegisteredLogicalPlacementCount = 1034",
                  "SuppressedLogicalPlacementCount = 0",
                  "new HashSet<string>(new string[0], StringComparer.Ordinal)"):
        if token not in visibility:
            raise AssertionError("Sprint 18 publication surface differs: " + token)
    if '"ape", "dire-ape"' in visibility.split(
            "SuppressedCreatureKeys")[-1].split(";")[0]:
        raise AssertionError("Sprint 18 publication did not remove its two keys")
    catalog = (root / "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
               ).read_text(encoding="utf-8")
    for token in ('C("ape","Ape",3,true,3', 'C("dire-ape","Dire Ape",4,true,4',
                  "ValidateFamily(SummonFamily.Monster, 90, 519)",
                  "ValidateFamily(SummonFamily.NaturesAlly, 88, 515)"):
        if token not in catalog:
            raise AssertionError("Frozen Sprint 18 placement contract differs: " + token)


def validate_icons(root: Path) -> None:
    manifest = document(
        root, "assets-source/original-icons/expanded-summoning/icon-manifest.json")
    if manifest["count"] != 111 or len(manifest["icons"]) != 111:
        raise AssertionError("Project summon icon concept count must be 111")
    rows = {row["key"]: row for row in manifest["icons"]}
    for key in APE_KEYS:
        row = rows.get(key)
        if row is None:
            raise AssertionError("Missing Sprint 18 icon concept: " + key)
        if row["width"] != 128 or row["height"] != 128 or row["format"] != "RGBA PNG":
            raise AssertionError("Sprint 18 icon export contract differs: " + key)
        if not (root / row["productionFile"]).is_file() or \
                not (root / row["sourceFile"]).is_file():
            raise AssertionError("Sprint 18 icon file missing: " + key)
        # A withheld creature still owns its own face, so every registered
        # consumer has to be named before publication rather than after it.
        if not any(s.startswith("KMG.Summoning.Unit.") for s in row["blueprintSymbols"]):
            raise AssertionError("Sprint 18 icon has no unit consumer: " + key)
    if rows["ape"]["outputSha256"] == rows["dire-ape"]["outputSha256"]:
        raise AssertionError("The two apes must not share one painting")


def validate_contract(root: Path) -> None:
    contract = document(root, "planning/EXPANDED-SUMMONING-SPRINT18-CONTRACT.json")
    if contract.get("sprint") != 18 or contract.get("authority", {}).get(
            "masterBase") != MASTER:
        raise AssertionError("The frozen Sprint 18 contract does not match this branch")
    placement = contract["placement"]
    if placement["newRootTotal"] != 26 or \
            placement["surfaceAfter"]["registeredGeneratedPlacements"] != 1034 or \
            placement["surfaceAfter"]["visibleChoices"] != 1063:
        raise AssertionError("The frozen contract arithmetic drifted")
    if contract["direApe"]["printedProfile"]["specialAttacks"][0]["damage"] != "1d4+6":
        raise AssertionError("The frozen printed rend line changed")


def validate(root: Path) -> None:
    identities = validate_identity_append(root)
    protected_master = preserved_files(root, MASTER, MASTER_PROTECTED)
    protected_summons = preserved_files(
        root, MASTER, SUMMONING_PROTECTED, allowed=SPRINT18_CHANGED)
    for path in SPRINT18_NEW:
        if not (root / path).is_file():
            raise AssertionError("Sprint 18 source or asset missing: " + path)
        if path in tracked(root, MASTER):
            raise AssertionError("Sprint 18 claims a file the release already had: " + path)

    for key, expected in SPRINT18_BODIES.items():
        body = document(root, "assets/sprint18-primates/" + key + "-mesh.json")
        if body.get("creature") != key:
            raise AssertionError("Sprint 18 body is not its own creature: " + key)
        if body.get("triangleWinding") != "shared-exporter-sprint18":
            raise AssertionError("Sprint 18 body does not declare its winding: " + key)
        if body.get("donorPrefab") != "0bc98460fca38964aae3af6ad5c655ee" or \
                body.get("donorRenderer") != "Troll_base":
            raise AssertionError("Sprint 18 body names the wrong donor: " + key)
        # An ape has no tail and no tongue geometry, has a separate jaw and
        # four limbs, and only the Dire Ape has claws. These are the printed
        # entries, not preferences, so they are checked on the shipped file.
        if body.get("tailGeometry") is not False or \
                body.get("tongueGeometry") is not False or \
                body.get("jawSeparated") is not True or \
                body.get("visibleLimbs") != 4 or \
                body.get("opposableThumbs") is not True or \
                body.get("knuckleWalkAuthored") is not False or \
                body.get("clawedHands") is not expected["clawedHands"]:
            raise AssertionError("Sprint 18 body anatomy contract broken: " + key)
        bones = body.get("bones")
        if not isinstance(bones, list) or len(bones) != expected["bones"] or \
                len(set(bones)) != expected["bones"]:
            raise AssertionError("Sprint 18 body driver set changed: " + key)
        if {"Tail_01", "Tail_02", "Tongue_01", "Tongue_02", "Tongue_03"} & set(bones):
            raise AssertionError(
                "Sprint 18 body weights a deliberately empty branch: " + key)
        albedo = body.get("albedo") or {}
        painting = root / "assets/sprint18-primates" / str(albedo.get("file"))
        if not painting.is_file():
            raise AssertionError("Sprint 18 body has no painting beside it: " + key)
        if hashlib.sha256(painting.read_bytes()).hexdigest() != albedo.get("sha256"):
            raise AssertionError("Sprint 18 painting is not the reviewed one: " + key)

    state = document(root, "validation/static-validation.json")
    old = json.loads(blob(root, MASTER, "validation/static-validation.json"))
    for key in ("expandedSummoningPhase2A141", "dataContentTraits142",
                "weaponFindability143", "weaponFindabilityRelease144",
                "heirloomNodachiIconRelease145", "expandedSummoningCheckpoint146",
                "expandedSummoningCheckpoint143"):
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
        "registeredManifestEntryCount": identities,
        "registeredGeneratedPlacements": 1034,
        "suppressedGeneratedPlacements": 0,
        "publishedGeneratedPlacements": 1034,
        "retainedNativeWrappers": 29,
        "visibleChoiceTotal": 1063,
        "uniqueCreatures": 99,
        "projectIconConcepts": 111,
        "newRoots": 26,
        "apeRoots": 14,
        "direApeRoots": 12,
        "bothApesSuppressed": False,
    }
    for key, value in expected.items():
        if record.get(key) != value:
            raise AssertionError("Sprint 18 candidate metadata differs: " + key)
    # Publication is earned, not asserted: a sprint may only leave candidate
    # state with a runtime claim, and a runtime claim needs exact evidence
    # behind it. These two rules are what kept the apes withheld for four
    # runs, and they still refuse a publication without them.
    if record.get("publicReleaseAuthorized") and not record.get("runtimeQualified"):
        raise AssertionError(
            "Sprint 18 cannot publish without a passing runtime review")
    if record.get("candidateOnly") and record.get("publicReleaseAuthorized"):
        raise AssertionError("A candidate cannot also be an authorized release")
    if record.get("runtimeQualified") and not record.get("runtimeEvidence"):
        raise AssertionError("Runtime qualification requires exact closure evidence")
    if not record.get("candidateOnly") and len(record.get("runtimeEvidence") or []) < 3:
        raise AssertionError(
            "A published Sprint 18 names its donor census, its batched review and "
            "its party-camera art review")
    if record.get("originalBodiesAuthored") and not record.get("primateDonorCensusRun"):
        raise AssertionError(
            "Original ape bodies cannot predate the donor census they are authored on")

    # The full inherited chain stays active.
    inherited.VERSION = VERSION
    inherited.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    inherited.PACKAGE = "KingmakerGunslinger-" + VERSION + "-local-runtime.zip"
    inherited.PACKAGE_SUFFIX = "expanded-summoning-sprint18"
    inherited.DETERMINISTIC_TEST_COUNT = count
    inherited.validate(root)

    intake = trait_icons.inspect(root)
    if intake["AssetReadiness"] != "READY" or not intake["existingIconContractPass"]:
        raise AssertionError(
            "All four original master trait assets must remain qualified")

    notes = (root / "docs/RELEASE-NOTES-0.0.147.md").read_text(encoding="utf-8")
    for token in (INFORMATIONAL_VERSION, "Ape", "Dire Ape", "rend",
                  "PASSIVE_CREATURE_SENSES_UNMODELED",
                  "ORDINARY_MAP_LAND_USE_SCOPE",
                  # Published, and the notes have to say what that cost: the
                  # creatures were withheld until their review passed, the
                  # bodies are authored, the gait is not, and the donor rig
                  # the census chose is named.
                  "withheld", "runtime qualified and published",
                  "NOT authored", "Troll", "census", "uninstall"):
        if token not in notes:
            raise AssertionError("Release notes lack honest disposition: " + token)

    print(f"Sprint18 {VERSION} source validation PASS: tests={count}; "
          f"identities={identities}; registered=1034; withheld=0; published=1034; "
          f"protected master={protected_master}; protected summons={protected_summons}. "
          f"Runtime qualified on {len(record['runtimeEvidence'])} guarded runs.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print("Sprint18 validation failed: " + str(exc), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
