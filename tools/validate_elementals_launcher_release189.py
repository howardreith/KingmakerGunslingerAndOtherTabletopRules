"""v0.0.189: the Expanded Summoning action-bar Elementals launcher repair (PR #156); only the three chooser sources change from v0.0.188, version-only metadata, no identity change, Stirge withheld."""
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

# Reviewed product: master after PR #156 plus the warning-helper repair
# fabe5d211b. RELEASED is the v0.0.188 tag target.
PRODUCT = "fabe5d211b35b81d25c5db4dad1953d6728d48dd"
RELEASED = "097e522f9fa0a8fdc8a773fb51fc3cfbf91d115c"
VERSION = "0.0.189"
INFORMATIONAL = VERSION + "-elementals-launcher-repair"
OLD_INFORMATIONAL = "0.0.188-umm-0311-minimum"
MANAGER = "0.31.1"
PINS = "scripts/loader-reference-pins.json"
PINNED = {"UnityModManager/UnityModManager.dll": "2e038ff23ab01d538dfef792096e1f5c04802d1edac3660db20d0d54c167c2c4",
          "UnityModManager/0Harmony12.dll": "aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292"}
# The reviewed build path: these must equal PRODUCT exactly.
BUILD_INPUTS = (PINS, "scripts/ReferenceProvenance.Common.ps1", "scripts/Build-Local.ps1",
                "scripts/validate-package.ps1", "scripts/DevelopmentCandidate.Common.ps1",
                "tools/build_mod_from_private_references.py")
# The only product files PR #156 and fabe5d211b change from v0.0.188.
SOURCES = ("src/KingmakerGunslinger/Summoning/ElementalSourceChooserPolicy.cs",
           "src/KingmakerGunslinger/Summoning/ElementalSourceChooserRuntime.cs",
           "src/KingmakerGunslinger/Summoning/ElementalSourceChooserView.cs")
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "elementalsLauncherRelease189"
RELEASED_IDENTITIES = 7933
APPENDED_IDENTITIES = 0
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208
RECORD = {
    "releaseVersion": "0.0.189",
    "releaseInformationalVersion": "0.0.189-elementals-launcher-repair",
    "baseStableVersion": "0.0.188",
    "reviewedProductSource": "fabe5d211b35b81d25c5db4dad1953d6728d48dd",
    "appendedIdentities": 0,
    "releasedIdentityChanges": 0,
    "visibleChoiceTotal": 1208,
    "change": "Expanded Summoning action-bar Elementals launcher repair (PR #156, reviewed at 7298937558, b7029441cf and d548b150d8). On PC the action-bar spell popup no longer shows a dead extra Elementals icon: the launcher is built only when an original standalone Summon Elemental tile was consolidated into it. A retained launcher opens in the Default and Pause game modes, so it works while the game is paused. When it cannot open (another game mode, controller input, keyboard unavailable) the original Summon Elemental spells return to the row with a warning instead of leaving an unclickable tile. Keyboard suppression is one owned count: it is released on close, recovered after a fault without resetting or decrementing another UI's count, and released when the action-bar owner is destroyed while the chooser is open. Only ElementalSourceChooserPolicy.cs, ElementalSourceChooserRuntime.cs and ElementalSourceChooserView.cs change; identities, settings keys, the roster and gameplay rules are unchanged. The fallback warning goes through UIUtility.SendWarning (fabe5d211b), as the native-warning boundary requires. Repository-only additions: 12 domain tests.",
    "nativeEvidence": "None. No development candidate or released build of this change ran in the game before publication; the owner directed publication after the offline review. Offline only: the complete domain suite passed 2983/2983 at fabe5d211b; at d548b150d8 the focused gate compiled production and passed 44/44 related tests, and the patch-target check passed 50/50 against the installed game and Harmony12. The first publisher run on 784b5e27ec refused in its complete suite (mission.native-warning-route) before any build or publication; fabe5d211b repairs that.",
    "deferredCoverage": "Not observed in the game: the launcher being omitted from a PC spell popup with no consolidated original; paused clicks and casts and the spend of the prepared spell; the native-rows fallback, its refilled level tab and warning text; keyboard recovery after a fault; the chooser closing after its action-bar owner is destroyed. Controller parity is open: the controller chooser can still insert an Elementals entry when nothing is consolidated. GS-REP-0002 (inherited-template removal set, membership unchanged) and the two pre-existing feature-module publication gates carry over unchanged and stay disclosed in KNOWN-ISSUES.md.",
    "ownerPublicationAuthority": "Howie Reith, 2026-11-14: 'Please merge this and cut a new release.' (PR #156 merged as 784b5e27ec). Published as the next stable patch, as v0.0.188 was."
}


def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source: " +
                             ", ".join(sorted(set(changed) - {ASSEMBLY}) + sorted(untracked)))


def check_release_delta(root, released_info, product_info, released_pins, product_pins, current_pins):
    """The reviewed product differs from v0.0.188 only in the three chooser
    sources and the version metadata (checked against v0.0.188 by
    check_metadata); loader pins and build inputs are unchanged."""
    if product_info != dict(released_info, Version=VERSION) or released_info.get("ManagerVersion") != MANAGER:
        raise AssertionError("Info.json changed beyond its version between v0.0.188 and the reviewed product")
    delta = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", RELEASED, PRODUCT, "--",
         *ROOTS, "Directory.Build.props"], cwd=root).decode().splitlines()
    if sorted(delta) != sorted(SOURCES + (ASSEMBLY, "Directory.Build.props")):
        raise AssertionError("Product change since v0.0.188 is not exactly the reviewed chooser sources: " +
                             ", ".join(sorted(delta)))
    pinned = {r.get("relativePath"): r.get("sha256") for r in product_pins.get("references", [])}
    if product_pins != released_pins or product_pins.get("minimumManagerVersion") != MANAGER or pinned != PINNED:
        raise AssertionError("The loader pins differ from the verified UMM 0.31.1 references of v0.0.188")
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
    must still equal v0.0.188's twelve keys."""
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
    # Version metadata is checked against v0.0.188 itself: the reviewed
    # product already carries the 0.0.189 values.
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
    print("Elementals launcher release189 PASS: the three reviewed chooser sources are the only product change from v0.0.188, Info, loader pins and build inputs unchanged, exact reviewed master product and assets, all 7933 released identities preserved, none appended, 1208 public choices with Stirge withheld, disclosed inherited-template membership unchanged, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Elementals launcher release189 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
