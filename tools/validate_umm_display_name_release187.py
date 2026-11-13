"""v0.0.187: the Unity Mod Manager display name is the only product change; version-only metadata, no appended identities, Stirge withheld."""
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

# Reviewed product: master after PRs #149, #148 and #150, merged in that order.
# RELEASED is the v0.0.186 tag target. Both are post-2026-11-13-rewrite ids.
PRODUCT = "ca6f4c0543498a50954d5624e7ebf95fb5647bac"
RELEASED = "98f42031f7bda70460d5af52b583d2d1aaea82e3"
VERSION = "0.0.187"
INFORMATIONAL = VERSION + "-umm-display-name"
OLD_INFORMATIONAL = "0.0.186-gunslinger-audit-repairs"
OLD_DISPLAY_NAME = "Kingmaker Gunslinger"
DISPLAY_NAME = "Mr. Bard's Tabletop Rules Expansion"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "ummDisplayNameRelease187"
RELEASED_IDENTITIES = 7933
APPENDED_IDENTITIES = 0
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208
RECORD = dict(
    releaseVersion="0.0.187",
    releaseInformationalVersion="0.0.187-umm-display-name",
    baseStableVersion="0.0.186",
    reviewedProductSource=PRODUCT,
    appendedIdentities=0,
    releasedIdentityChanges=0,
    visibleChoiceTotal=1208,
    change="Unity Mod Manager now lists the mod as \"Mr. Bard's Tabletop Rules Expansion\" instead of \"Kingmaker Gunslinger\". Only the Info.json DisplayName changes in the product. The mod Id, assembly, entry point, settings keys, blueprint identities, the roster, prices, merchants, loot and crafting recipes are unchanged, so existing installs, module settings and saves carry over. Repository-only changes since v0.0.186: project-level Claude Code settings that disable co-author attribution, and the 2026-11-13 commit-id map with the checks that resolve commits pointed at the rewritten history.",
    nativeEvidence="None. The compiled source differs from v0.0.186 only in its version attributes; the change is a Unity Mod Manager metadata string. Info.json parses in Windows PowerShell 5.1 and Python with the new DisplayName.",
    deferredCoverage="Not observed natively: the Unity Mod Manager mod list rendering the new name. GS-REP-0002 (the hand-maintained inherited-template removal set, membership unchanged from v0.0.186) and the two pre-existing feature-module publication gates carry over unchanged and stay disclosed in KNOWN-ISSUES.md.",
    ownerPublicationAuthority="Howie Reith, 2026-11-13: 'Please merge all 3 PRs and then cut a new release.' Published as the next stable patch, as v0.0.186 was.")


def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source: " +
                             ", ".join(sorted(set(changed) - {ASSEMBLY}) + sorted(untracked)))


def check_release_delta(root, released_info, product_info):
    """The reviewed product differs from v0.0.186 only in the UMM display name."""
    if released_info.get("DisplayName") != OLD_DISPLAY_NAME:
        raise AssertionError("v0.0.186 display name baseline differs")
    if product_info != dict(released_info, DisplayName=DISPLAY_NAME):
        raise AssertionError("Only the Info DisplayName may change from v0.0.186")
    delta = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", RELEASED, PRODUCT, "--",
         *ROOTS, "Directory.Build.props"], cwd=root).decode().splitlines()
    if delta:
        raise AssertionError("Product source changed since v0.0.186: " + ", ".join(delta))


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


def inherited_template_keys(text):
    block = re.search(r"InheritedTemplateRemovalCreatures = \{(.*?)\};", text, re.S)
    if block is None:
        raise AssertionError("The inherited-template removal set is no longer a named array")
    return re.findall(r'"([a-z\-]+)"', block.group(1))


def check_inherited_template_set(root):
    """GS-REP-0002 stays disclosed as pre-existing and unchanged: the membership
    must still equal v0.0.186's twelve keys."""
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
        json.loads(blob(PRODUCT, "Info.json")))
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
    check_inherited_template_set(root)
    result = subprocess.run([sys.executable, "-B",
        str(root / "tools/expanded_summoning_manifest.py")], cwd=root,
        capture_output=True, text=True)
    if result.returncode:
        raise AssertionError(result.stdout + result.stderr)
    print("UMM display name release187 PASS: the Info DisplayName is the only product change from v0.0.186, exact reviewed master product and assets, all 7933 released identities preserved, none appended, 1208 public choices with Stirge withheld, disclosed inherited-template membership unchanged, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("UMM display name release187 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
