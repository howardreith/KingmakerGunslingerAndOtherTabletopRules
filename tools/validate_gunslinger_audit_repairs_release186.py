"""v0.0.186: exact reviewed master product, version-only metadata, no appended identities, Stirge withheld."""
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

# Reviewed product: master after the seven v0.0.182-audit repair PRs
# (#134, #136, #137, #139, #135, #138, #140), each reviewed at its own head and
# merged in that order. RELEASED is the v0.0.185 tag target.
PRODUCT = "2c7ff93be9ebd50b3548618eda88e9d1171238c3"
RELEASED = "c136527a999b9fa22227e630d4a8bee57e593b9a"
VERSION = "0.0.186"
INFORMATIONAL = VERSION + "-gunslinger-audit-repairs"
OLD_INFORMATIONAL = "0.0.185-regional-vendor-melee-withdrawal"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "gunslingerAuditRepairsRelease186"
RELEASED_IDENTITIES = 7933
APPENDED_IDENTITIES = 0
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208
RECORD = dict(
    releaseVersion="0.0.186",
    releaseInformationalVersion="0.0.186-gunslinger-audit-repairs",
    baseStableVersion="0.0.185",
    reviewedProductSource="2c7ff93be9ebd50b3548618eda88e9d1171238c3",
    appendedIdentities=0,
    releasedIdentityChanges=0,
    visibleChoiceTotal=1208,
    change="Repair tranche from the accepted v0.0.182 release audit (PR #132, report cd71d8973). Corrects an inverted attack-penalty sign convention in the scatter volley and the Dead Shot iterative probes, which realised BONUSES where penalties were intended; stops a failed feature-module settings migration write from quarantining a valid file and substituting all-ON defaults; puts the Cord of Stubborn Resolve on one live substitution policy and clamps its outcome at non-positive hit points; makes firearm crafting account for partially committed output and cap its compensation at what the transaction requested; preserves another publisher's level-up selection rows across both of this mod's selection postfixes; presents an unrankable teleport scroll row as visibly disabled with its reason instead of as an executable offer; and shares one inherited-template removal rule between the canonical and alternate summoning builders. Creature identities, the roster, prices, merchants, loot and crafting recipes are unchanged.",
    nativeEvidence="Guarded save-free native runs on development candidates built from this lineage, with the installation snapshot-restored and verified after each. disposable-gunslinger-scatter-dead-shot 50 assertions 0 non-PASS on 6c866f83a1dd (penaltyField=2 with a realised 32 against a matched control 34; probes 0/5/10 and 0/5/10/15 realising 16,11,6,1). observe-feature-module-settings staged twice on 468391fc06a3: the writable case persisted schemaVersion 12 and the controlled write failure left the file byte-identical at schemaVersion 10 with no quarantine artifact, both reporting recovered=False. disposable-firearm-dependent-feats 17/17 and disposable-paper-cartridge-crafting-vendors 13/13 on 1de8b5c7305d, the crafting set including an actual repeat refusal, two faults compensating exactly, a compensation failure retaining both causes, and an external surplus left in place and reported. The crafting budget cap was additionally demonstrated against a negative control a6fcd6e51d3b that reverts only the cap, where that observation FAILS.",
    deferredCoverage="Not observed natively: the Cord clamp at non-positive hit points, whose four existing assertions pass without it; the summoning composition repair's behaviour-neutrality across every possible caller, which remains a source inference for the measured route; and a third-party mod actually supplying a non-FeatureUIData selection row, which no installed mod does, so the selection-boundary evidence uses a synthetic foreign row and demonstrates preservation rather than the repair of an observed breakage. GS-REP-0002 stays open: the inherited-template removal set is a hand-maintained list of twelve creature keys whose membership is byte-identical to v0.0.185 and disagrees with the installed library in both directions. It is pre-existing, unchanged by this release and separately tracked. Two feature-module publication gates fail on this installation; a matched baseline on released master a3912f569ac5 reproduces both with identical measured values, so they are pre-existing and not caused by this tranche, and their mechanism is untraced.",
    ownerPublicationAuthority="Howie Reith, 2026-11-13: 'Approved. Please merge #140 as well and cut a release.' Published as the next stable patch, as v0.0.185 was.")


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


def check_inherited_template_set(root):
    """GS-REP-0002 is disclosed as pre-existing and UNCHANGED, so the release
    must not quietly alter the membership it discloses. The set moved from an
    inline disjunction to a named array during the repair tranche; what is
    pinned here is the membership, which must still equal v0.0.185's."""
    import re
    path = "src/KingmakerGunslinger/Blueprints/ExpandedSummoningAbilityBuilder.cs"
    old = common.blob(root, RELEASED, path).decode("utf-8-sig")
    inline = [line for line in old.splitlines()
              if 'variant.Creature.Key == "lemure"' in line and "||" in line]
    if len(inline) != 1:
        raise AssertionError("The v0.0.185 inherited-template disjunction is no longer identifiable")
    released_keys = re.findall(r'Key == "([a-z\-]+)"', inline[0])
    current = (root / path).read_text(encoding="utf-8-sig")
    block = re.search(r"InheritedTemplateRemovalCreatures = \{(.*?)\};", current, re.S)
    if block is None:
        raise AssertionError("The inherited-template removal set is no longer a named array")
    current_keys = re.findall(r'"([a-z\-]+)"', block.group(1))
    if current_keys != released_keys:
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
    print("Gunslinger audit repairs release186 PASS: exact reviewed master product and assets, all 7933 released identities preserved, none appended, 1208 public choices with Stirge withheld, disclosed inherited-template membership unchanged, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Gunslinger audit repairs release186 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
