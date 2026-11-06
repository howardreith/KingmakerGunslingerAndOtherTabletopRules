"""Sprint40 exact reviewed product, append-only identities and public scope."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import subprocess
import sys
sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import parsed_creatures, suppressed_keys
import validate_development_candidate as identities
import expanded_summoning_manifest as manifest

# Exact reviewed product after the focused full-gate contract repair, and the
# tested product source of the latest reviewed stable v0.0.177. The reviewed
# product already carries final version metadata, so metadata must remain exact.
PRODUCT = "3e67e91772f2fd68e5d1441d080de47318488c8c"
RELEASED = "d1365d621295cf63c7b646247d6bf604edb208da"
VERSION = "0.0.178"
INFORMATIONAL = VERSION + "-expanded-summoning-ice-devil"
OLD_INFORMATIONAL = INFORMATIONAL
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "expandedSummoningSprint40Release178"
HARNESS = {
    "scripts/Deploy-Local.ps1": "713cceb319c2850d5f440139ebedc5468501d5b9c458e4d418d944cce98d9353",
    "scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1": "9d17b2a451534d2dbcbc9e453b9c8bed40460d10bdec14cc8c3e30447ad685d1",
    "scripts/Invoke-KingmakerRuntimeTest.ps1": "406db39f0e0a262548de62845d603c123c1babcb2e96cabb3586185bdd250146",
    "scripts/Invoke-Sprint22InstalledStack.ps1": "67521fab76cb4752e9de14c7830ebd4dba59dbda3badcf39ce7acf4e283f35ff",
    "scripts/RuntimeAutomation.Common.ps1": "8e208622b7b767b0c484fab11be46406cac568020c4a66c9f2973d9e3efc16b1",
    "scripts/RuntimeHarness.Common.ps1": "f8c67bb58e84b677e377d4a8a93472ecfefffe38fa2b5b778186209d6f7aadb2",
    "scripts/Sprint40QualifiedRelease.Common.ps1": "d6f0310ba4c3466f62186f7c169919e8103e241ba7b4590d5a53ab2287fb08f4",
    "scripts/Test-Sprint22InstalledStack.ps1": "222f8dba9ac74f8006a29cd0e0da9c4c2a576121e2ee647b5b5a198989a099b3",
    "scripts/Test-Sprint32NativePersistenceRequest.ps1": "d841bf7a970af04dfb5db8344354e72cad2c3582ef8a48368dbff5ea3c8ff34",
    "scripts/Test-Sprint40Release.ps1": "4390e1bc2fa427cf4db2cdf7b9383336857f93c306ccfb55a867a9203c2543e3",
}
RECORD = dict(
    releaseVersion=VERSION,
    releaseInformationalVersion=INFORMATIONAL,
    baseStableVersion="0.0.177",
    reviewedProductSource=PRODUCT,
    appendedIdentities=33,
    releasedIdentityChanges=0,
    change="Ice Devil, one untemplated Summon Monster IX route, its armed iterative frost-spear plus bite and tail routine, exact-owner supernatural Slow, at-will Cone of Cold, Ice Storm and bounded Wall of Ice, fear, regeneration, area-control AI and a distinct original insectile body/icon; also corrects only the Nalfeshnee-owned Slow range and its Nimbus release wording",
    nativeEvidence="Final exact package requires fresh grouped proof for every mandatory Ice Devil row, the affected Nalfeshnee range boundary and six unique mixed ON/OFF persistence processes; development-candidate receipts cannot qualify this release",
    ownerPublicationAuthority="Sprint40 Ice Devil Fresh Codex Completion Mission, its continuity amendment and factory recovery/completion authorization, together with the owner decision approving omission of Persistent Image with disclosure and no substitute; the selected Wall of Ice is the S35 bounded single grounded 10-foot movement-obstruction adaptation with no line-of-effect claim, greater teleport and nested summoning remain excluded, effective movement is the printed 60-foot fly with the separate 40-foot land mode unmodeled, Nimbus is documented as Daze rather than Blindness, and the Nalfeshnee-owned Slow is corrected to its printed CL12 close range of 55 feet",
    harnessIdentity=HARNESS,
    harnessIdentityNote="Tool and fixture identity for the orchestration that produces the native rows, recorded separately from the compiled product identity. These paths lie outside the validator's guarded product roots, so they are pinned here rather than by the product diff. The validator file itself cannot carry its own digest; its provenance is the reviewed release commit.",
    installedStackFixtureChecks=57,
    compiledRequestAdmissionChecks=33)


def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source")


def check_metadata(old_info, info, old_props, props, old_assembly, assembly):
    if info != dict(old_info, Version=VERSION):
        raise AssertionError("Only Info version may change")
    expected_props = old_props.replace(
        "<KmgVersion>" + old_info["Version"] + "</KmgVersion>",
        "<KmgVersion>" + VERSION + "</KmgVersion>").replace(
            OLD_INFORMATIONAL, INFORMATIONAL)
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
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current.get(key) != released.get(key):
            raise AssertionError("Released identity header changed: " + key)
    identities.check_append_only(released["entries"], current["entries"])
    if len(current["entries"]) - len(released["entries"]) != 33:
        raise AssertionError("Sprint40 identity append count differs")


def released_creatures(root):
    text = common.blob(root, RELEASED,
        "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs").decode("utf-8-sig")
    return [dict(key=k, name=n, monster=None if m == "null" else int(m),
        templated=t == "true", ally=None if a == "null" else int(a), visual=v or n)
        for k, n, m, t, a, v in manifest.ENTRY.findall(text)]


def check_census(creatures, hidden, baseline):
    expected = {"ice-devil": 9}
    selected = [c for c in creatures if c["key"] in expected]
    if (hidden or len(selected) != 1 or {c["key"] for c in selected} != set(expected) or
            any(c["monster"] != expected[c["key"]] or c["ally"] is not None or
                c["templated"] for c in selected)):
        raise AssertionError("Exactly Ice Devil SM IX must be public and untemplated with nothing withheld")
    if [c for c in creatures if c["key"] not in expected] != baseline:
        raise AssertionError("Released creature identities, ordering or routes changed")
    monster = sum(10 - c["monster"] for c in creatures if c["monster"] is not None)
    ally = sum(10 - c["ally"] for c in creatures if c["ally"] is not None)
    if (len(creatures), sum(c["monster"] is not None for c in creatures), monster,
            sum(c["ally"] is not None for c in creatures), ally) != (134, 112, 600, 105, 588):
        raise AssertionError("Sprint40 source-derived roster totals differ")


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
    print("Sprint40 release178 PASS: exact reviewed Ice Devil product and assets, append-only identities, one public route and version-only product metadata. Native release qualification is separate.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Sprint40 release178 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
