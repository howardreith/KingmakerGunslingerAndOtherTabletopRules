"""v0.0.182: exact reviewed master product, version-only metadata, 39 appended Spider Summoner identities, Stirge withheld."""
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

# Reviewed product: master after the Races Unleashed Spider Summoner integration
# (#125) and the summon menu order / Sunder / Lillend / Dismantle Armor
# corrections (#126), both owner-accepted. RELEASED is the v0.0.181 tag target.
PRODUCT = "22eea359b677b481801a956767299eb9ce1f3255"
RELEASED = "daf87b545693fc5000e7897b3835ccb80037797f"
VERSION = "0.0.182"
INFORMATIONAL = VERSION + "-summon-menu-order-spider-summoner"
OLD_INFORMATIONAL = "0.0.181-cleric-summon-alignment"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "summonMenuOrderRelease182"
RELEASED_IDENTITIES = 7894
APPENDED_IDENTITIES = 39
APPENDED_PREFIXES = ("KMG.Summoning.Spider.",)
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208
RECORD = dict(
    releaseVersion=VERSION,
    releaseInformationalVersion=INFORMATIONAL,
    baseStableVersion="0.0.181",
    reviewedProductSource=PRODUCT,
    appendedIdentities=APPENDED_IDENTITIES,
    releasedIdentityChanges=0,
    visibleChoiceTotal=PUBLIC_CHOICES,
    change="Races Unleashed Spider Summoner integration (repaired original rows, 38 generated higher-tier placements, the +2 poison/web DC benefit); quantity-first summon menus; no Sunder on supported summons (Storm Giant, Greater and Elder Earth Elemental) with save fixers; Azata, Lillend label; the Bebelith's Dismantle Armor withdrawn and its legacy penalty inert.",
    nativeEvidence="Native results stay attributed to their own development candidates; none was re-executed on this version-stamped build. Spider Summoner: candidate 0908e3415fa4, every RU-01..RU-20 row closed. Menu order and Sunder: candidate 3f170d4a9cea remains 44 PASS / 1 FAIL (the sunder-census FAIL is an observer defect; a separate reader-only re-evaluation passes with 0 carriers); candidate c6e861a98a93 remains an initialization FAIL.",
    deferredCoverage="Not observed natively: the Bebelith Dismantle Armor withdrawal and inert legacy penalty, the corrected census observer, an old-save cold load, Herald and alternate-source menus as opened popups, the Elder Earth Elemental live, autonomous AI decisions with a saved elemental's retained attack action. Call of the Wild metamagic-conversion rows remain an open design choice.",
    ownerPublicationAuthority="Howie Reith, 2026-11-11: 'Accepted. Please merge, push, and cut a new release with a new version number.' Published as the next stable patch, as v0.0.181 was.")


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
        raise AssertionError("Exactly the reviewed identities may be appended")
    if new_entries[:len(old_entries)] != old_entries:
        raise AssertionError("Every released identity must be preserved exactly and in order")
    if any(not entry["symbol"].startswith(APPENDED_PREFIXES) for entry in new_entries[len(old_entries):]):
        raise AssertionError("Only Spider Summoner identities may be appended")
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
    print("Summon menu order release182 PASS: exact reviewed master product and assets, all 7894 released identities preserved, exactly 39 Spider Summoner identities appended, 1208 public choices with Stirge withheld, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Summon menu order release182 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
