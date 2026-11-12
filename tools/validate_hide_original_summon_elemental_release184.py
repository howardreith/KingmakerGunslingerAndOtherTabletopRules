"""v0.0.184: exact reviewed master product, version-only metadata, no appended identities, Stirge withheld."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import subprocess
import sys
sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import NATIVE_EXPANDED_OPTIONS, parsed_creatures, suppressed_keys
import expanded_summoning_manifest as manifest

# Reviewed product: master after hiding the original Summon Elemental spells
# (#133), owner-approved. RELEASED is the v0.0.183 tag target.
PRODUCT = "9469105336d4a3d6ad0da153bc2016f51bf98739"
RELEASED = "6365ddb2616c8056805a2333b92ea4c7b1b1f687"
VERSION = "0.0.184"
INFORMATIONAL = VERSION + "-hide-original-summon-elemental"
OLD_INFORMATIONAL = "0.0.183-gunslinger-playtest-repairs"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "hideOriginalSummonElementalRelease184"
RELEASED_IDENTITIES = 7933
APPENDED_IDENTITIES = 0
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208
RECORD = dict(
    releaseVersion="0.0.184",
    releaseInformationalVersion="0.0.184-hide-original-summon-elemental",
    baseStableVersion="0.0.183",
    reviewedProductSource="9469105336d4a3d6ad0da153bc2016f51bf98739",
    appendedIdentities=0,
    releasedIdentityChanges=0,
    visibleChoiceTotal=1208,
    change="Original Summon Elemental spells hidden by default while Expanded Summoning is active: level-up spell learning (PC and controller), spellbook and memorize lists (PC and controller) and the PC metamagic tab; the level-up 'Elementals (n) - show original sources' disclosure withdrawn on PC and controller; new UMM option 'Show original Summon Elemental spells' (default off; no restart; close and reopen the spell list to apply) reveals ordinary rows. Presentation only: spells, spell lists, known and custom spells, preparations, casting, saves, scroll copying and the action-bar elemental chooser unchanged; domain/special grants kept.",
    nativeEvidence="Desktop development candidate 60d2d1b618 (DLL cd62c823..., MVID dd2fbe1c-84a8-49f1-bddf-9b1770814748) guarded native run PASS 22/22: mixed spellbook and PC metamagic tab hidden and revealed (backing lists and rendered rows), preparation/undo, fixture restoration, revealed level-up row, native allocation, live-state native replacement and normal cancellation. Candidate 0d944a262f (FAIL 12/1 on an observer defect) observed the default-hidden Wizard 'Choose Spells' list and the ordinary row after reopening with the option on. Earlier candidates b0d235c33a (7/3) and 312bb462e8 (8/2) keep their FAIL results. None was re-executed on this version-stamped build.",
    deferredCoverage="Not observed natively: controller learning and spellbook (compile, Harmony target and shared-policy tests only); domain/special elemental grants (policy tests); pending-choice filtering under a rebuilt hidden list (focused tests); an existing campaign save. Scroll copying is unrestricted and unclassified learning contexts fall back to native rows.",
    ownerPublicationAuthority="Howie Reith, 2026-11-12: 'Approved. Please merge and cut a release.' Published as the next stable patch, as v0.0.183 was.")


def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source: " +
                             ", ".join(sorted(set(changed) - {ASSEMBLY}) + sorted(untracked)))


def check_metadata(old_info, info, old_props, props, old_assembly, assembly):
    if info != dict(old_info, Version=VERSION):
        raise AssertionError("Only Info version may change")
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
    check_metadata(json.loads(blob(PRODUCT, "Info.json")),
        common.document(root, "Info.json"), blob(PRODUCT, "Directory.Build.props"),
        (root / "Directory.Build.props").read_text(encoding="utf-8-sig"),
        blob(PRODUCT, ASSEMBLY), (root / ASSEMBLY).read_text(encoding="utf-8-sig"))
    path = "blueprints/blueprints.json"
    check_ledger(json.loads(blob(RELEASED, path)), json.loads(blob(PRODUCT, path)),
        common.document(root, path))
    check_static(json.loads(blob(PRODUCT, "validation/static-validation.json")),
        common.document(root, "validation/static-validation.json"))
    check_census(parsed_creatures(), suppressed_keys(), released_creatures(root))
    result = subprocess.run([sys.executable, "-B",
        str(root / "tools/expanded_summoning_manifest.py")], cwd=root,
        capture_output=True, text=True)
    if result.returncode:
        raise AssertionError(result.stdout + result.stderr)
    print("Hide original Summon Elemental release184 PASS: exact reviewed master product and assets, all 7933 released identities preserved, none appended, 1208 public choices with Stirge withheld, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Hide original Summon Elemental release184 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
