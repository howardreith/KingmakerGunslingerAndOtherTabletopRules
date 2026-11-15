"""v0.0.191: the grit UI recipient repair (PR #166) and the named-spear buff repairs (PR #164, PR #165); only their six reviewed product files change from v0.0.190, version-only metadata, no identity change, Stirge withheld."""
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

# Reviewed product: master after PR #166. RELEASED is the v0.0.190 tag target.
PRODUCT = "b8ba915d36c282cb096196381dbc77590d429562"
RELEASED = "27244257b1ba6f823f999611bd28cecbb1d329fb"
VERSION = "0.0.191"
INFORMATIONAL = VERSION + "-grit-ui-recipient-repair"
OLD_INFORMATIONAL = "0.0.190-missing-gunslinger-class-repair"
MANAGER = "0.31.1"
PINS = "scripts/loader-reference-pins.json"
PINNED = {"UnityModManager/UnityModManager.dll": "2e038ff23ab01d538dfef792096e1f5c04802d1edac3660db20d0d54c167c2c4",
          "UnityModManager/0Harmony12.dll": "aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292"}
# The reviewed build path: these must equal PRODUCT exactly.
BUILD_INPUTS = (PINS, "scripts/ReferenceProvenance.Common.ps1", "scripts/Build-Local.ps1",
                "scripts/validate-package.ps1", "scripts/DevelopmentCandidate.Common.ps1",
                "tools/build_mod_from_private_references.py")
# The only product files PR #164, PR #165 and PR #166 change from v0.0.190.
SOURCES = (
    "assets-source/original-icons/icon-catalog.json",
    "src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearNamedBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/MysteriousStrangerBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs",
    "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs",
    "src/KingmakerGunslinger/Grit/GritAbilityUiIntegration.cs",
)
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "gritUiRecipientRelease191"
RELEASED_IDENTITIES = 7933
APPENDED_IDENTITIES = 0
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208
RECORD = {
    "releaseVersion": "0.0.191",
    "releaseInformationalVersion": "0.0.191-grit-ui-recipient-repair",
    "baseStableVersion": "0.0.190",
    "reviewedProductSource": "b8ba915d36c282cb096196381dbc77590d429562",
    "appendedIdentities": 0,
    "releasedIdentityChanges": 0,
    "visibleChoiceTotal": 1208,
    "change": "Second missing Gunslinger class repair (PR #166, reviewed at 21387ac767; review follow-ups at e99888c4ad) and the named Elven Branched Spear buff repairs (PR #164 and PR #165). A v0.0.190 report showed blueprint initialization aborting with a NullReferenceException in Kingmaker's UIUtilityTexts.GetLongOrShortText, called from BlueprintAbility.get_Description, called from GritAbilityUiIntegration.DescribesGritSpend, which removed the Gunslinger class and the rest of the content for that process. The shared grit UI pass scanned every KMG_ ability in the library and read its displayed description to decide which abilities show grit on the action bar; Kingmaker's description getter dereferences the description, so one that resolves to a null string throws inside the getter. The pass now takes the constructed Gunslinger blueprint set and binds an explicit list of 24 typed ability references: 22 receive the shared no-spend indicator (Quick Clear as a move action, Pistol Whip, Dead Shot, the four Targeting deeds, Menacing Shot, the four Bleeding Wound arming abilities, Expert Loading, both Slinger's Luck abilities, Death's Shot, Stunning Shot, Focused Aim, Clipping Shot, Up Close and Deadly, Twin Shot Knockdown and Steady Aim) and Deadeye and Gunslinger's Dodge keep the grit resource logic they were built with. That is the set the released build binds on a stock English game. No description is read and the library is not scanned. Recipients are all checked before any is changed and the set is verified afterwards; a repeat changes nothing; a missing, duplicated or incompatible recipient fails with the deed, internal name and GUID. The indicator component and its no-op Spend are unchanged. MysteriousStrangerBlueprintSet exposes its Focused Aim and Clipping Shot abilities, and the bootstrap call site changes by one exact hunk recorded in the icon catalog. Named Elven Branched Spear buffs: all eight (Boughkeeper's armor-class buff, the Thornstep, Viper's Reach and First Branch's Reprisal penalties and four hidden round markers) get the empty effect links Kingmaker's buff activation needs, so they complete activation, appear beside the portrait when visible and end by themselves; and the four visible ones get the existing Elven Branched Spear icon in place of a blank white square, recorded as authorized icon-catalog hunks. Six product files change; identities, grants, grit costs, spending, durations and values are unchanged and no art file changes. Repository-only additions: 2 domain tests, loader-compat harness conditions, probes and sessions, docs/GUNSLINGER-GRIT-UI-RECIPIENT-REPAIR.md and the two native check records under reports/boughkeeper-buff-icon.",
    "nativeEvidence": "Grit UI recipients: development candidate 4acb7055c92c (package 9d59acd5267399829759cb9e676f82e2b8be80fef0eaa63d926be04cf883ba79, DLL 4611260e4e07db275d2685c5645570e244e5ca33e2d8650ab466f216dfc43d7d; the same product source as the reviewed master) ran in the disposable loader-compat game copy on Unity Mod Manager 0.33.0, with the published v0.0.190 package as the failing baseline for the same launches (baseline 20261115T170712Z; candidate 20261115T171454Z, its class-screen launch rerun as 20261115T172339Z after a harness screenshot wait was corrected, and its stock launch rerun as 20261115T183438Z with stricter refusal checks). With unassigned text resolving to null the published package fails with the reported stack frame for frame; the candidate completes initialization, publishes the Gunslinger once, lists it once on the New Character class screen where its own toggle selects it and Next continues, and native creation commits a Gunslinger. On the candidate exactly the 24 audited abilities carry the contract in every launch, the pass reads no ability description, a repeat returns 22 and replaces no component array, five malformed recipient lists are refused, an unrelated ability whose description getter throws is left alone and an unrelated ability whose description says 'spend grit' does not become a recipient. The product's own scenarios passed on the candidate for the indicator count and no-op Spend, Deadeye, Dead Shot, Expert Loading and both Slinger's Luck variants (20261115T172457Z, 20261115T173801Z); all recorded Pistolero grit assertions passed. Named-spear buffs: development candidates recorded in reports/boughkeeper-buff-icon/NATIVE-UI-CHECK.json and PENALTY-BUFF-ICONS-NATIVE-CHECK.json (sessions 20261115T153646Z, 20261115T153418Z and 20261115T160344Z): a real attack of opportunity with Boughkeeper shows its buff in the portrait at once with the spear icon and tooltip, all eight buffs complete activation and end after 6.0 s of game time with armor class, speed and Reflex restored, the four visible ones draw the spear icon, the four hidden markers stay hidden, and the product's spear combat scenario passes its 24 assertions. Offline at 4acb7055c9: complete domain suite 2993/2993. The published release bytes are run in the game after publication; that result is recorded in docs/BUILD-AND-RELEASE.md, not here.",
    "deferredCoverage": "Not reproduced: the reporting installation, its translation and its full mod list. Null resolution of unassigned text is a controlled test-only stand-in that matches the reported stack; which ability failed for the reporter and what makes text resolve to null there are unknown. The lab loader was Unity Mod Manager 0.33.0, not the reported 0.33.3. Three hidden Expanded Summoning abilities still have no stored description (Barbed Devil Order's Wrath and Pyrotechnics, Bone Devil Dimensional Anchor); the repaired pass does not read them, and other code that asked for their displayed description on such an installation would still throw. disposable-gunslinger-dodge errors before its assertions in the disposable copy and reproduces on the published v0.0.190 package, so Gunslinger's Dodge has no passing spend check. The disposable-pistolero-deeds damage assertion up-close-critical is intermittent on the repaired candidate (two of three runs failed); its cause is not established and one passing run of the published package does not show whether it is intermittent there. Not every recipient was spent in the game and the action bar was not looked at for each deed. Design question left as released: Steady Aim carries the indicator while Quick Clear as a standard action, Utility Shot, Startling Shot and Lightning Reload do not, and the indicator counts remaining grit in units of 1 whatever a deed costs. Named-spear buffs: the icon's legibility at the 19 px portrait slot has no separate owner visual sign-off beyond the owner's direction to use the spear icon; the timed-buff check applied the eight buffs directly, and the product's combat scenario, which grants the named effects by their triggers, removes them itself without letting time run. Other project buffs built without effect links remain as disclosed in KNOWN-ISSUES.md. GS-REP-0002 and the two pre-existing feature-module publication gates carry over unchanged.",
    "ownerPublicationAuthority": "Howie Reith, 2026-11-15, with the review of PR #166: 'Please address the minor issues identified, then merge this and cut a new release. Thank you.' (PR #166 merged as b8ba915d36). Published as the next stable patch, as v0.0.190 was."
}


def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source: " +
                             ", ".join(sorted(set(changed) - {ASSEMBLY}) + sorted(untracked)))


def check_release_delta(root, released_info, product_info, released_pins, product_pins, current_pins):
    """The reviewed product differs from v0.0.190 only in the six repair
    sources; Info, props, loader pins and build inputs are unchanged."""
    if product_info != released_info or released_info.get("ManagerVersion") != MANAGER:
        raise AssertionError("Info.json changed between v0.0.190 and the reviewed product")
    delta = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", RELEASED, PRODUCT, "--",
         *ROOTS, "Directory.Build.props"], cwd=root).decode().splitlines()
    if sorted(delta) != sorted(SOURCES):
        raise AssertionError("Product change since v0.0.190 is not exactly the reviewed repair sources: " +
                             ", ".join(sorted(delta)))
    pinned = {r.get("relativePath"): r.get("sha256") for r in product_pins.get("references", [])}
    if product_pins != released_pins or product_pins.get("minimumManagerVersion") != MANAGER or pinned != PINNED:
        raise AssertionError("The loader pins differ from the verified UMM 0.31.1 references of v0.0.190")
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
    must still equal v0.0.190's twelve keys."""
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
    # Version metadata is checked against v0.0.190, whose values the reviewed
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
    print("Grit UI recipient release191 PASS: the six reviewed repair sources are the only product change from v0.0.190, Info, loader pins and build inputs unchanged, exact reviewed master product and assets, all 7933 released identities preserved, none appended, 1208 public choices with Stirge withheld, disclosed inherited-template membership unchanged, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Grit UI recipient release191 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
