"""v0.0.183: exact reviewed master product, version-only metadata, no appended identities, Stirge withheld."""
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

# Reviewed product: master after the Gunslinger playtest repairs (#129),
# owner-accepted. RELEASED is the v0.0.182 tag target.
PRODUCT = "57d86403239b94715c10adaca0187bc2ada63d49"
RELEASED = "c19d12e095f17cd01292b6616be9a2d87bfb19a7"
VERSION = "0.0.183"
INFORMATIONAL = VERSION + "-gunslinger-playtest-repairs"
OLD_INFORMATIONAL = "0.0.182-summon-menu-order-spider-summoner"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "gunslingerPlaytestRelease183"
RELEASED_IDENTITIES = 7933
APPENDED_IDENTITIES = 0
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208
RECORD = dict(
    releaseVersion="0.0.183",
    releaseInformationalVersion="0.0.183-gunslinger-playtest-repairs",
    baseStableVersion="0.0.182",
    reviewedProductSource="57d86403239b94715c10adaca0187bc2ada63d49",
    appendedIdentities=0,
    releasedIdentityChanges=0,
    visibleChoiceTotal=1208,
    change="Gunslinger playtest repairs: the spellbook 'Elementals - show/hide original sources' disclosure withdrawn on PC and controller (level-up learning and the action-bar source chooser unchanged); Deadeye Armed, Gunslinger's Dodge, Focused Aim and Dead Shot descriptions clarified; both cold iron branched spears removed from Oleg's stock; Thornstep retired from campaign acquisition (registry 28, discoverability 29 items / 27 exact areas) with provenance-based reconciliation of its former source in older saves.",
    nativeEvidence="None of the six repairs was observed natively and no development candidate was built or run. Evidence: focused gate 153 of 2,948 with 0 failures at e04609c2f1, compilation, read-only Harmony target preparation (spellbook 40/40, retired-loot 1/1) and inspection of the decompiled 2.1.7b engine contract.",
    deferredCoverage="Not observed natively: spellbook rows through refresh and reload on PC and controller, the Deadeye Armed, Dodge and Focused Aim tooltips, a Dodge activation, Oleg's fresh and saved stock, Thornstep's fresh and previously generated sources, legacy owned copies. Known migration limits: a cold iron spear sold to Oleg after his stocked copy was bought is decremented; an engine-added zero-time Thornstep in a container generated before its placement existed is removed.",
    ownerPublicationAuthority="Howie Reith, 2026-11-11: 'This is acceptable. Please merge this and cut a new release.' Published as the next stable patch, as v0.0.182 was.")


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
    print("Gunslinger playtest release183 PASS: exact reviewed master product and assets, all 7933 released identities preserved, none appended, 1208 public choices with Stirge withheld, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Gunslinger playtest release183 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
