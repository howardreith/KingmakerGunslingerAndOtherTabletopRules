"""v0.0.188: the declared Unity Mod Manager minimum drops to 0.31.1 and the build pins the UMM 0.31.1 loader references; no product source, asset or identity change, version-only metadata, Stirge withheld."""
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

# Reviewed product: master after PR #153. RELEASED is the v0.0.187 tag target.
PRODUCT = "99fd8fa04a91330c4dcd32014f8cabec3fd79778"
RELEASED = "bfaac01f522080aa43a8f89b9c119de1e1a744ae"
VERSION = "0.0.188"
INFORMATIONAL = VERSION + "-umm-0311-minimum"
OLD_INFORMATIONAL = "0.0.187-umm-display-name"
OLD_MANAGER = "0.32.4"
MANAGER = "0.31.1"
PINS = "scripts/loader-reference-pins.json"
PINNED = {"UnityModManager/UnityModManager.dll": "2e038ff23ab01d538dfef792096e1f5c04802d1edac3660db20d0d54c167c2c4",
          "UnityModManager/0Harmony12.dll": "aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292"}
# The reviewed build path: these must equal PRODUCT exactly.
BUILD_INPUTS = (PINS, "scripts/ReferenceProvenance.Common.ps1", "scripts/Build-Local.ps1",
                "scripts/validate-package.ps1", "scripts/DevelopmentCandidate.Common.ps1",
                "tools/build_mod_from_private_references.py")
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "ummMinimumRelease188"
RELEASED_IDENTITIES = 7933
APPENDED_IDENTITIES = 0
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208
RECORD = dict(
    releaseVersion="0.0.188",
    releaseInformationalVersion="0.0.188-umm-0311-minimum",
    baseStableVersion="0.0.187",
    reviewedProductSource=PRODUCT,
    appendedIdentities=0,
    releasedIdentityChanges=0,
    visibleChoiceTotal=1208,
    change="Info.json ManagerVersion drops from 0.32.4 to 0.31.1, so Unity Mod Manager 0.31.1 loads the mod. The DLL is now compiled against the pinned UMM 0.31.1 UnityModManager.dll and 0Harmony12.dll (scripts/loader-reference-pins.json) instead of the locally installed loader; package validation requires ManagerVersion to equal that floor. No product source, asset, blueprint identity, setting key or gameplay change: the bound UMM/Harmony member set is identical to v0.0.187 and present in UMM 0.31.1, 0.32.4 and 0.33.0. Repository-only additions since v0.0.187: loader-pin provenance tests, the test-only loader-compatibility harness and session runner, and docs/UMM-LOADER-COMPATIBILITY.md.",
    nativeEvidence="Development candidate umm-0311-compat at b9400333e5 (ZIP c82c73abf3efb62e740ee42687127a93355a711a7646e39863a846879c44867a, DLL 7fa421d7a783512e60d8efe56d65c8da758b3529d18cebed6b833c5e7266ab1b), whose product source equals this release's except the version attributes: eight native sessions PASS in a disposable game copy on UMM 0.31.1 and 0.32.4 (component-assembled) and 0.33.0 (owner Nexus install) with exact in-process loader/Harmony identities, alone, with Craft Magic Items 2.1.0 and Kingmaker Bugfixes 1.3.3, and with Call of the Wild and Favored Class on 0.31.1 and 0.33.0 (docs/UMM-LOADER-COMPATIBILITY.md). The released DLL differs from that candidate by its version attributes and commit stamp and was not part of that matrix.",
    deferredCoverage="Not run: UMM 0.27.x, other 0.31.x, 0.32.0-0.32.3, 0.33.1 and later, the original Nexus installer ZIPs for 0.31.1 and 0.32.4, Harmony 2.3.5, Call of the Wild/Favored Class on 0.32.4, combat with a Craft Magic Items-crafted firearm and the owner's full mod stack. GS-REP-0002 (the inherited-template removal set, membership unchanged) and the two pre-existing feature-module publication gates carry over unchanged and stay disclosed in KNOWN-ISSUES.md.",
    ownerPublicationAuthority="Howie Reith, 2026-11-13: 'Thank you. Please merge and cut a new release.' (PR #153 merged as 99fd8fa04a). Published as the next stable patch, as v0.0.187 was.")


def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source: " +
                             ", ".join(sorted(set(changed) - {ASSEMBLY}) + sorted(untracked)))


def check_release_delta(root, released_info, product_info, product_pins, current_pins):
    """The reviewed product differs from v0.0.187 only in Info ManagerVersion,
    which equals the pinned UMM 0.31.1 build floor."""
    if released_info.get("ManagerVersion") != OLD_MANAGER:
        raise AssertionError("v0.0.187 ManagerVersion baseline differs")
    if product_info != dict(released_info, ManagerVersion=MANAGER):
        raise AssertionError("Only the Info ManagerVersion may change from v0.0.187")
    delta = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", RELEASED, PRODUCT, "--",
         *ROOTS, "Directory.Build.props"], cwd=root).decode().splitlines()
    if delta:
        raise AssertionError("Product source changed since v0.0.187: " + ", ".join(delta))
    pinned = {r.get("relativePath"): r.get("sha256") for r in product_pins.get("references", [])}
    if product_pins.get("minimumManagerVersion") != MANAGER or pinned != PINNED:
        raise AssertionError("The reviewed loader pins are not the verified UMM 0.31.1 references")
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
    must still equal v0.0.187's twelve keys."""
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
        json.loads(blob(PRODUCT, "Info.json")), json.loads(blob(PRODUCT, PINS)),
        common.document(root, PINS))
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
    print("UMM minimum release188 PASS: Info ManagerVersion 0.32.4 -> 0.31.1 is the only product change from v0.0.187, equal to the verified UMM 0.31.1 loader pins and unchanged build inputs, exact reviewed master product and assets, all 7933 released identities preserved, none appended, 1208 public choices with Stirge withheld, disclosed inherited-template membership unchanged, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("UMM minimum release188 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
