"""v0.0.190: the missing Gunslinger class repair (PR #161); only its sixteen reviewed product files change from v0.0.189, version-only metadata, no identity change, Stirge withheld."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import re
import subprocess
import sys
sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import NATIVE_EXPANDED_OPTIONS, parsed_creatures, suppressed_keys
import expanded_summoning_manifest as manifest

# Reviewed product: master after PR #161. RELEASED is the v0.0.189 tag target.
PRODUCT = "268a934192ecb4d10a1191c63600462b21c9fde8"
RELEASED = "90bbfbb08d7b56decf60c5f3f68a315d20c4e0e0"
VERSION = "0.0.190"
INFORMATIONAL = VERSION + "-missing-gunslinger-class-repair"
OLD_INFORMATIONAL = "0.0.189-elementals-launcher-repair"
MANAGER = "0.31.1"
PINS = "scripts/loader-reference-pins.json"
PINNED = {"UnityModManager/UnityModManager.dll": "2e038ff23ab01d538dfef792096e1f5c04802d1edac3660db20d0d54c167c2c4",
          "UnityModManager/0Harmony12.dll": "aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292"}
# The reviewed build path: these must equal PRODUCT exactly.
BUILD_INPUTS = (PINS, "scripts/ReferenceProvenance.Common.ps1", "scripts/Build-Local.ps1",
                "scripts/validate-package.ps1", "scripts/DevelopmentCandidate.Common.ps1",
                "tools/build_mod_from_private_references.py")
# The only product files PR #161 changes from v0.0.189.
SOURCES = (
    "src/KingmakerGunslinger/Blueprints/BleedingWoundBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/BlueprintUnitFactAccess.cs",
    "src/KingmakerGunslinger/Blueprints/DeathsShotBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ExpertLoadingBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/LightningReloadBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/LocalizationService.cs",
    "src/KingmakerGunslinger/Blueprints/OwnedMutationRollback.cs",
    "src/KingmakerGunslinger/Blueprints/PlayerFacingPresentation.cs",
    "src/KingmakerGunslinger/Blueprints/SlingersLuckBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/StunningShotBlueprints.cs",
    "src/KingmakerGunslinger/Development/DevelopmentUi.cs",
    "src/KingmakerGunslinger/FeatureModules/ContentInitializationStatus.cs",
    "src/KingmakerGunslinger/FeatureModules/FeatureModuleUi.cs",
    "src/KingmakerGunslinger/KingmakerGunslinger.csproj",
    "src/KingmakerGunslinger/Presentation/PlayerFacingTextAssignment.cs",
    "src/KingmakerGunslinger/Spells/ProtectionFromAlignment/ProtectionFromAlignmentPublication.cs",
)
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "missingGunslingerClassRelease190"
RELEASED_IDENTITIES = 7933
APPENDED_IDENTITIES = 0
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208
RECORD = {
    "releaseVersion": "0.0.190",
    "releaseInformationalVersion": "0.0.190-missing-gunslinger-class-repair",
    "baseStableVersion": "0.0.189",
    "reviewedProductSource": "268a934192ecb4d10a1191c63600462b21c9fde8",
    "appendedIdentities": 0,
    "releasedIdentityChanges": 0,
    "visibleChoiceTotal": 1208,
    "change": "Missing Gunslinger class repair (PR #161, reviewed at 42d8230b4c; review follow-ups at cb3d383178). A v0.0.189 report showed blueprint initialization aborting with 'Player-facing fact has incomplete localization: KMG_BleedingWound_HitPoints_Armed', which removed the Gunslinger class and the rest of the content for that process, followed by a Protection from Alignment rollback failure. Eleven buffs reached by the Gunslinger presentation traversal had no name or description; Kingmaker resolves unstored text to the non-empty placeholder '<null>', so the non-empty check accepted them on a stock game and failed where the same gap resolved to nothing. Ten visible buffs now carry names and descriptions (Bleeding Wound armed for Hit Points, Strength, Dexterity and Constitution; Expert Loading Armed; Lightning Reload Used; Slinger's Luck armed for saving throw and skill check; Death's Shot Armed; Stunning Shot Armed); the Death's Shot death carrier, which nothing applies to a unit, is hidden with Kingmaker's HiddenInUi buff flag. The presentation check requires assigned text and honours a buff's native visibility flag. The eleven buffs also get empty effect links: built without them, Kingmaker's buff activation threw after the buff was active and the applying rule swallowed it, so the buffs worked but never appeared in the party bar and expiry scheduling was skipped (Lightning Reload Used could linger past its round). Protection from Alignment publication journals each mutation before writing it and restores the exact captured objects entry by entry, leaving and reporting a later foreign change. The settings panel shows the loaded name and version and the content initialization state in place of a stale 0.0.154 label. Sixteen product files change; identities, grants, grit costs, stacking and marker consumption are unchanged and no art changes. Repository-only additions: 8 domain tests, the loader-compat harness conditions and probes with three sessions, and docs/GUNSLINGER-MISSING-CLASS-REPAIR.md.",
    "nativeEvidence": "Development candidate cb3d383178c3 (package c5d3913ad9e68219f2c65ca600ff65302ffbd6719f220214bb6f3bc2a0c7febc, DLL 39a6ca1d0d314dd6a3cc1af6124c1f4730ccf3355f2f96959b8f7875a7c8d77f; the same product source as the reviewed master) ran in the disposable loader-compat game copy on Unity Mod Manager 0.33.0, with the published v0.0.189 package as the failing baseline for the same launches. Passed on the candidate, failed on v0.0.189: sessions gunslinger-missing-class (20261115T062258Z; six launches: presentation census, validator controls, settings panel, initialization and Gunslinger creation with unstored text resolving blank, rollback after an injected failure with a blank original and two foreign changes, rollback on native state, zhCN locale) and gunslinger-missing-class-screens (20261115T061857Z; the New Character class screen listing the Gunslinger once, selected by its own toggle, Next continuing; the ten visible buffs in the party bar and on the character sheet with names, tooltip text and icons, Lightning Reload Used ending after one round; exact rollback after a failure injected once a description was stored). Session gunslinger-missing-class-shared-mods (20261115T062915Z; twelve of the reported mods at the reported versions) passed on the candidate. The published release bytes themselves were not run in the game: the release build differs from the candidate by version metadata and build stamps. Offline at cb3d383178: complete domain suite 2991/2991.",
    "deferredCoverage": "Not reproduced: the reporting installation, its translation and its full mod list; the failing condition was reproduced with a controlled test-only patch that makes unstored text resolve blank, and what produced that condition for the reporter is unknown. The lab loader was Unity Mod Manager 0.33.0, not the reported 0.33.3. The ten buffs were applied directly to a level 1 Fighter in a fixture save; arming them through their abilities as a Gunslinger and spending them on an attack was not played. Other project buffs built without effect links are unchanged and untested and are disclosed in KNOWN-ISSUES.md: Deadeye Armed, Gunslinger's Dodge, the Bleeding Wound bleeds on a target, Startling Shot, Targeting (Head), the Pistolero and Musket Master armed buffs, and one buff each in the Eastern Weapons, Elven Branched Spear and Expanded Summoning Suggestion builders. The new names and descriptions were reviewed for wording once (one copy edit applied) and have no separate owner sign-off. GS-REP-0002 and the two pre-existing feature-module publication gates carry over unchanged.",
    "ownerPublicationAuthority": "Howie Reith, 2026-11-15, with the review of PR #161: 'Could you do the thing mentioned in this review, then merge and cut a new release please?' (PR #161 merged as 268a934192). Published as the next stable patch, as v0.0.189 was."
}


def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source: " +
                             ", ".join(sorted(set(changed) - {ASSEMBLY}) + sorted(untracked)))


def check_release_delta(root, released_info, product_info, released_pins, product_pins, current_pins):
    """The reviewed product differs from v0.0.189 only in the sixteen repair
    sources; Info, props, loader pins and build inputs are unchanged."""
    if product_info != released_info or released_info.get("ManagerVersion") != MANAGER:
        raise AssertionError("Info.json changed between v0.0.189 and the reviewed product")
    delta = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", RELEASED, PRODUCT, "--",
         *ROOTS, "Directory.Build.props"], cwd=root).decode().splitlines()
    if sorted(delta) != sorted(SOURCES):
        raise AssertionError("Product change since v0.0.189 is not exactly the reviewed repair sources: " +
                             ", ".join(sorted(delta)))
    pinned = {r.get("relativePath"): r.get("sha256") for r in product_pins.get("references", [])}
    if product_pins != released_pins or product_pins.get("minimumManagerVersion") != MANAGER or pinned != PINNED:
        raise AssertionError("The loader pins differ from the verified UMM 0.31.1 references of v0.0.189")
    if current_pins != product_pins:
        raise AssertionError("Loader pins differ from the reviewed product")
    drift = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", PRODUCT, "--", *BUILD_INPUTS],
        cwd=root).decode().splitlines()
    if drift:
        raise AssertionError("Reviewed build inputs changed: " + ", ".join(drift))


def check_metadata(old_info, info, old_props, props, old_assembly, assembly):
    if info != dict(old_info, Version=VERSION):
        raise AssertionError("Only Info version may change")
    if info.get("ManagerVersion") != MANAGER:
        raise AssertionError("Info ManagerVersion must stay " + MANAGER)
    expected_props = old_props.replace(
        "<KmgVersion>" + old_info["Version"] + "</KmgVersion>",
        "<KmgVersion>" + VERSION + "</KmgVersion>").replace(
            "<KmgInformationalVersion>" + OLD_INFORMATIONAL + "</KmgInformationalVersion>",
            "<KmgInformationalVersion>" + INFORMATIONAL + "</KmgInformationalVersion>")
    expected_assembly = old_assembly
    for attribute, before, after in (
            ("AssemblyVersion", old_info["Version"], VERSION),
            ("AssemblyFileVersion", old_info["Version"], VERSION),
            ("AssemblyInformationalVersion", OLD_INFORMATIONAL, INFORMATIONAL)):
        token = attribute + '("' + before + '")'
        if expected_assembly.count(token) != 1:
            raise AssertionError("Ambiguous version metadata")
        expected_assembly = expected_assembly.replace(
            token, attribute + '("' + after + '")')
    if (props.splitlines() != expected_props.splitlines() or
            assembly.splitlines() != expected_assembly.splitlines()):
        raise AssertionError("Non-version build metadata changed")


def check_ledger(released, product, current):
    if current != product:
        raise AssertionError("Identities differ from exact reviewed product")
    old_entries = released["entries"]
    new_entries = current["entries"]
    if len(old_entries) != RELEASED_IDENTITIES:
        raise AssertionError("Released identity baseline differs")
    if len(new_entries) != RELEASED_IDENTITIES + APPENDED_IDENTITIES:
        raise AssertionError("No identity may be appended")
    if new_entries[:len(old_entries)] != old_entries:
        raise AssertionError("Every released identity must be preserved exactly and in order")
    if dict(released, entries=None) != dict(current, entries=None):
        raise AssertionError("Ledger metadata changed")


def released_creatures(root):
    text = common.blob(root, RELEASED,
        "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs").decode("utf-8-sig")
    return [dict(key=k, name=n, monster=None if m == "null" else int(m),
        templated=t == "true", ally=None if a == "null" else int(a), visual=v or n)
        for k, n, m, t, a, v in manifest.ENTRY.findall(text)]


def check_census(creatures, hidden, baseline):
    if set(hidden) != WITHHELD:
        raise AssertionError("Exactly the owner-withdrawn Stirge may be withheld")
    if creatures != baseline:
        raise AssertionError("Released creature identities, ordering or routes changed")
    monster = [c for c in creatures if c["monster"] is not None]
    ally = [c for c in creatures if c["ally"] is not None]
    totals = (len(creatures), len(monster), sum(10 - c["monster"] for c in monster),
              len(ally), sum(10 - c["ally"] for c in ally))
    withheld = sum(10 - c["ally"] for c in ally if c["key"] in WITHHELD) + \
        sum(10 - c["monster"] for c in monster if c["key"] in WITHHELD)
    if totals != (134, 112, 600, 105, 588) or \
            totals[2] + totals[4] + len(NATIVE_EXPANDED_OPTIONS) - withheld != PUBLIC_CHOICES:
        raise AssertionError("Released roster/route census differs")


def inherited_template_keys(text):
    block = re.search(r"InheritedTemplateRemovalCreatures = \{(.*?)\};", text, re.S)
    if block is None:
        raise AssertionError("The inherited-template removal set is no longer a named array")
    return re.findall(r'"([a-z\-]+)"', block.group(1))


def check_inherited_template_set(root):
    """GS-REP-0002 stays disclosed as pre-existing and unchanged: the membership
    must still equal v0.0.189's twelve keys."""
    path = "src/KingmakerGunslinger/Blueprints/ExpandedSummoningAbilityBuilder.cs"
    released_keys = inherited_template_keys(
        common.blob(root, RELEASED, path).decode("utf-8-sig"))
    current_keys = inherited_template_keys((root / path).read_text(encoding="utf-8-sig"))
    if len(released_keys) != 12 or current_keys != released_keys:
        raise AssertionError(
            "The disclosed inherited-template membership changed: released " +
            ",".join(released_keys) + " vs current " + ",".join(current_keys))


def check_static(product, current):
    expected = dict(product, version=VERSION, milestone=INFORMATIONAL)
    expected[RECORD_KEY] = RECORD
    if current != expected:
        raise AssertionError("Static evidence history or selected release record differs")


def validate(root):
    def blob(commit, path):
        return common.blob(root, commit, path).decode("utf-8-sig")
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", PRODUCT, "--", *ROOTS],
        cwd=root).decode().splitlines()
    untracked = subprocess.check_output(
        ["git", "ls-files", "--others", "--exclude-standard", "--", *ROOTS],
        cwd=root).decode().splitlines()
    check_product(changed, untracked)
    check_release_delta(root, json.loads(blob(RELEASED, "Info.json")),
        json.loads(blob(PRODUCT, "Info.json")), json.loads(blob(RELEASED, PINS)),
        json.loads(blob(PRODUCT, PINS)), common.document(root, PINS))
    # Version metadata is checked against v0.0.189, whose values the reviewed
    # product still carries.
    check_metadata(json.loads(blob(RELEASED, "Info.json")),
        common.document(root, "Info.json"), blob(RELEASED, "Directory.Build.props"),
        (root / "Directory.Build.props").read_text(encoding="utf-8-sig"),
        blob(RELEASED, ASSEMBLY), (root / ASSEMBLY).read_text(encoding="utf-8-sig"))
    path = "blueprints/blueprints.json"
    check_ledger(json.loads(blob(RELEASED, path)), json.loads(blob(PRODUCT, path)),
        common.document(root, path))
    check_static(json.loads(blob(PRODUCT, "validation/static-validation.json")),
        common.document(root, "validation/static-validation.json"))
    check_census(parsed_creatures(), suppressed_keys(), released_creatures(root))
    check_inherited_template_set(root)
    result = subprocess.run([sys.executable, "-B",
        str(root / "tools/expanded_summoning_manifest.py")], cwd=root,
        capture_output=True, text=True)
    if result.returncode:
        raise AssertionError(result.stdout + result.stderr)
    print("Missing Gunslinger class release190 PASS: the sixteen reviewed repair sources are the only product change from v0.0.189, Info, loader pins and build inputs unchanged, exact reviewed master product and assets, all 7933 released identities preserved, none appended, 1208 public choices with Stirge withheld, disclosed inherited-template membership unchanged, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Missing Gunslinger class release190 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
