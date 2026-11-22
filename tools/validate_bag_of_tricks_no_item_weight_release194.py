"""v0.0.194: Bag of Tricks No Item Weight compatibility (PR #199); exactly its thirteen reviewed product files change from v0.0.193, version-only metadata, no identity change, Stirge withheld."""
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

# Reviewed product: master after PR #199. RELEASED is the v0.0.193 tag target.
PRODUCT = "8f36604fb6e9ef7902eff6694eb6c21f92ce61f9"
RELEASED = "527f7e2fba36d92ecc80ebd98be3267c287e6d58"
VERSION = "0.0.194"
INFORMATIONAL = VERSION + "-bag-of-tricks-no-item-weight"
OLD_INFORMATIONAL = "0.0.193-spear-bootstrap-tooltip-repair"
MANAGER = "0.31.1"
PINS = "scripts/loader-reference-pins.json"
PINNED = {"UnityModManager/UnityModManager.dll": "2e038ff23ab01d538dfef792096e1f5c04802d1edac3660db20d0d54c167c2c4",
          "UnityModManager/0Harmony12.dll": "aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292"}
# The reviewed build path: these must equal PRODUCT exactly.
BUILD_INPUTS = (PINS, "scripts/ReferenceProvenance.Common.ps1", "scripts/Build-Local.ps1",
                "scripts/validate-package.ps1", "scripts/DevelopmentCandidate.Common.ps1",
                "tools/build_mod_from_private_references.py")
# The only product files PR #199 changes from v0.0.193. The list is exact in both directions.
SOURCES = (
    "src/KingmakerGunslinger/Blueprints/AuthoredItemWeight.cs",
    "src/KingmakerGunslinger/Blueprints/BasicAmmunitionBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/EasternWeaponBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/FirearmRepairKitBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/MagicCircleScrollBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/MagicFirearmBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ProgressionWeaponBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/TeleportationScrollBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/WeaponTypeMechanicalAccess.cs",
    "src/KingmakerGunslinger/ElvenBranchedSpear/ElvenBranchedSpearProfilePolicy.cs",
    "src/KingmakerGunslinger/KingmakerGunslinger.csproj",
)
# The product tree reviewed in PR #199 and natively tested as development candidate c4e1259457e0.
PRODUCT_TREE = "d9167aadf5771140f400296f45f8b95ebff56c80"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "bagOfTricksNoItemWeightRelease194"
RELEASED_IDENTITIES = 7935
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208


def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source: " +
                             ", ".join(sorted(set(changed) - {ASSEMBLY}) + sorted(untracked)))


def check_release_delta(root, released_info, product_info, released_pins, product_pins, current_pins):
    """The reviewed product differs from v0.0.193 exactly in SOURCES; Info,
    props, loader pins and build inputs are unchanged."""
    if product_info != released_info or released_info.get("ManagerVersion") != MANAGER:
        raise AssertionError("Info.json changed between v0.0.193 and the reviewed product")
    delta = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", RELEASED, PRODUCT, "--",
         *ROOTS, "Directory.Build.props"], cwd=root).decode().splitlines()
    if sorted(delta) != sorted(SOURCES):
        raise AssertionError("Product change since v0.0.193 is not exactly the reviewed repair sources: +" +
                             ", ".join(sorted(set(delta) - set(SOURCES))) + " -" +
                             ", ".join(sorted(set(SOURCES) - set(delta))))
    tree = subprocess.check_output(["git", "rev-parse", PRODUCT + ":src/KingmakerGunslinger"],
                                   cwd=root).decode().strip()
    if tree != PRODUCT_TREE:
        raise AssertionError("The reviewed product tree differs from the natively tested source " + PRODUCT_TREE)
    pinned = {r.get("relativePath"): r.get("sha256") for r in product_pins.get("references", [])}
    if product_pins != released_pins or product_pins.get("minimumManagerVersion") != MANAGER or pinned != PINNED:
        raise AssertionError("The loader pins differ from the verified UMM 0.31.1 references of v0.0.193")
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
    """All v0.0.193 identities kept exactly and in order; none appended."""
    if current != product:
        raise AssertionError("Identities differ from exact reviewed product")
    old_entries = released["entries"]
    new_entries = current["entries"]
    if len(old_entries) != RELEASED_IDENTITIES:
        raise AssertionError("Released identity baseline differs")
    if new_entries != old_entries:
        raise AssertionError("Every released identity must be preserved exactly and in order, and none appended")
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


def check_inherited_template_policy(root):
    """v0.0.192's DEF-05 policy stays: no twelve-name list returns."""
    path = "src/KingmakerGunslinger/Blueprints/ExpandedSummoningAbilityBuilder.cs"
    if "InheritedTemplateRemovalCreatures" in (root / path).read_text(encoding="utf-8-sig"):
        raise AssertionError("The inherited-template name list must stay replaced by the DEF-05 policy")
    if not (root / "src/KingmakerGunslinger/Summoning/InheritedTemplatePolicy.cs").is_file():
        raise AssertionError("The DEF-05 inherited-template policy is missing")


def check_static(product, current):
    record = current.get(RECORD_KEY)
    expected = dict(product, version=VERSION, milestone=INFORMATIONAL)
    expected[RECORD_KEY] = record
    if current != expected or not isinstance(record, dict) or \
            record.get("releaseVersion") != VERSION or \
            record.get("releaseInformationalVersion") != INFORMATIONAL or \
            record.get("baseStableVersion") != "0.0.193" or \
            record.get("reviewedProductSource") != PRODUCT or \
            record.get("appendedIdentities") != 0 or \
            record.get("releasedIdentityChanges") != 0 or \
            record.get("visibleChoiceTotal") != PUBLIC_CHOICES:
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
    # Version metadata is checked against v0.0.193, whose values the reviewed product still carries.
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
    check_inherited_template_policy(root)
    result = subprocess.run([sys.executable, "-B",
        str(root / "tools/expanded_summoning_manifest.py")], cwd=root,
        capture_output=True, text=True)
    if result.returncode:
        raise AssertionError(result.stdout + result.stderr)
    print("Bag of Tricks No Item Weight release194 PASS: exactly the thirteen reviewed PR #199 product files changed from v0.0.193, the natively tested product tree, Info, loader pins and build inputs unchanged, exact reviewed master product and assets, all 7935 released identities preserved, none appended, 1208 public choices with Stirge withheld, the DEF-05 inherited-template policy kept, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Bag of Tricks No Item Weight release194 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
