"""v0.0.179: exact reviewed master product, version-only metadata, unchanged identities and routes."""
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

# Reviewed product: master after PRs #98, #101, #103, #105, #106, #107 and #108
# (and records #102, #104). RELEASED is the v0.0.178 tag target.
PRODUCT = "9e67c45f46227ad6dcda10187e6609ffa1f1cab6"
RELEASED = "839204cac36e2c2079377269b8024f8a76d4c4f6"
VERSION = "0.0.179"
INFORMATIONAL = VERSION + "-expanded-summoning-wall-reach-closeout"
OLD_INFORMATIONAL = "0.0.178-expanded-summoning-ice-devil"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "expandedSummoningCloseoutRelease179"
RECORD = dict(
    releaseVersion=VERSION,
    releaseInformationalVersion=INFORMATIONAL,
    baseStableVersion="0.0.178",
    reviewedProductSource=PRODUCT,
    appendedIdentities=0,
    releasedIdentityChanges=0,
    visibleChoiceTotal=1217,
    change="Wall of Ice durable navigation, steam and tail (PR #98, #101); Bone Devil Wall sections take no Infernal Wound and natural reach is correct under ProperFlanking2 (PR #103); 45 false ProperFlanking2 reach-weapon rows on published summons corrected with live reach preserved (PR #105); observation-only Sprint 41 repairs (PR #106) and visual-closeout observer (PR #107); no creature model, texture or scale change since v0.0.178",
    nativeEvidence="Native results stay attributed to their own development candidates (for example Wall 155/156, reach s4g 41/41, closeout 5, shadow motion 29); none is fresh execution of the version-stamped release DLL",
    deferredCoverage="Sprint 41 A-15 Wall review phase passes 1 run in 5 (verification reliability demonstrated, product defect undetermined); A-5, A-6, A-7, A-9, A-13 and A-16 open; Ice Devil visual acceptance pending; persistence matrix, roster-scale visual acceptance and performance not run for this release",
    ownerPublicationAuthority="Howie Reith, 2026-11-07: 'Alright. Please cut a release that I will test with. Thank you.' then, asked stable or prerelease: 'Stable 0.0.179. I also just merged the other PRs that were in the repo so make sure everything is incorporated.'")


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
    if current != released or len(current["entries"]) != 7868:
        raise AssertionError("All 7868 released identities must be preserved exactly; no additions")


def released_creatures(root):
    text = common.blob(root, RELEASED,
        "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs").decode("utf-8-sig")
    return [dict(key=k, name=n, monster=None if m == "null" else int(m),
        templated=t == "true", ally=None if a == "null" else int(a), visual=v or n)
        for k, n, m, t, a, v in manifest.ENTRY.findall(text)]


def check_census(creatures, hidden, baseline):
    if hidden:
        raise AssertionError("No creature may be withheld")
    if creatures != baseline:
        raise AssertionError("Released creature identities, ordering or routes changed")
    monster = [c for c in creatures if c["monster"] is not None]
    ally = [c for c in creatures if c["ally"] is not None]
    totals = (len(creatures), len(monster), sum(10 - c["monster"] for c in monster),
              len(ally), sum(10 - c["ally"] for c in ally))
    if totals != (134, 112, 600, 105, 588) or totals[2] + totals[4] + len(NATIVE_EXPANDED_OPTIONS) != 1217:
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
    print("Closeout release179 PASS: exact reviewed master product and assets, all 7868 identities unchanged, released routes and 1217 public choices unchanged, version-only product metadata.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Closeout release179 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
