"""Sprint37 exact reviewed product, append-only identities and public scope."""
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

# The Sprint 37 product branch head and the previous release's records merge.
# The head is after the version bump, as Sprint 36's was, so the metadata check
# degenerates to "product metadata is unchanged from the reviewed head" and
# OLD_INFORMATIONAL equals INFORMATIONAL.
PRODUCT = "1dec3c22adb4ac6163abcda99e1d85e5c4412d55"
RELEASED = "b1cf3a3dd97a4df7d4838961bdea97a254a576d8"
VERSION = "0.0.175"
INFORMATIONAL = VERSION + "-expanded-summoning-vrock"
OLD_INFORMATIONAL = INFORMATIONAL
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "expandedSummoningSprint37Release175"
HARNESS = {"scripts/Deploy-Local.ps1": "713cceb319c2850d5f440139ebedc5468501d5b9c458e4d418d944cce98d9353", "scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1": "df662a36a3d8b1f83292767cae51f42beb3d30c5971a9354874633fe301e66f6", "scripts/Invoke-KingmakerRuntimeTest.ps1": "64d4643834519f5c77170c6689bbd71fd05e63ddc9129aafc796105dc9219fee", "scripts/Invoke-Sprint22InstalledStack.ps1": "38fb00b2ef7ea3d1b857623c3e84d1eda80d85b5ddc3070b88535f94a3ff26b3", "scripts/Sprint37QualifiedRelease.Common.ps1": "3976cbe53bc8d9843b63212e245437016cd6c3cd9af8758fb7c2f92c7563e9c7", "scripts/Test-Sprint22InstalledStack.ps1": "e7c6c4a401e6881c02516d184c8d0e377c02df24af75cc491a9c7178fca08f2c", "scripts/Test-Sprint37Release.ps1": "3d93b1df01081d7ad4928ff806c20db5379b683000fa019590cc238261e54275"}
RECORD = dict(
    releaseVersion=VERSION,
    releaseInformationalVersion=INFORMATIONAL,
    baseStableVersion="0.0.174",
    reviewedProductSource=PRODUCT,
    appendedIdentities=41,
    releasedIdentityChanges=0,
    change="Vrock, three untemplated Monster VII-IX routes, source-owned Stunning Screech and Spores, two once-per-summoning native self-buffs and a distinct original body/icon",
    nativeEvidence="Final exact package requires fresh grouped proof for every mandatory row: no Sprint37 row may be reused, so no development-candidate receipt can qualify this release",
    ownerPublicationAuthority="Sprint37 Vrock Fresh Codex Mission, its two corrections, and the owner decisions of 2026-11-02 approving the implemented Spores with its three working counters while explicitly accepting omission of the printed holy-water interaction, and retaining the conservative automatic Screech policy with manual use preserved through the supported player interface; telekinesis, Dance of Ruin, greater teleport and the printed 30-foot land speed remain disclosed omissions",
    harnessIdentity=HARNESS,
    harnessIdentityNote="Tool and fixture identity for the orchestration that produced the native rows, recorded separately from the compiled product identity. These paths lie outside the validator's guarded product roots, so they are pinned here rather than by the product diff. The validator file itself cannot carry its own digest; its provenance is the reviewed product commit.",
    installedStackFixtureChecks=52)


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
    if len(current["entries"]) - len(released["entries"]) != 41:
        raise AssertionError("Sprint37 identity append count differs")


def released_creatures(root):
    text = common.blob(root, RELEASED,
        "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs").decode("utf-8-sig")
    return [dict(key=k, name=n, monster=None if m == "null" else int(m),
        templated=t == "true", ally=None if a == "null" else int(a), visual=v or n)
        for k, n, m, t, a, v in manifest.ENTRY.findall(text)]


def check_census(creatures, hidden, baseline):
    # One creature, three Monster roots from SM VII, no Nature's Ally route,
    # no template child, and nothing withheld: Sprint 37 published after its
    # six guarded mechanics phases.
    expected = {"vrock": 7}
    selected = [c for c in creatures if c["key"] in expected]
    if (hidden or len(selected) != 1 or {c["key"] for c in selected} != set(expected) or
            any(c["monster"] != expected[c["key"]] or c["ally"] is not None or
                c["templated"] for c in selected)):
        raise AssertionError("Exactly Vrock SM VII must be public and untemplated with nothing withheld")
    if [c for c in creatures if c["key"] not in expected] != baseline:
        raise AssertionError("Released creature identities, ordering or routes changed")
    monster = sum(10 - c["monster"] for c in creatures if c["monster"] is not None)
    ally = sum(10 - c["ally"] for c in creatures if c["ally"] is not None)
    if (len(creatures), sum(c["monster"] is not None for c in creatures), monster,
            sum(c["ally"] is not None for c in creatures), ally) != (131, 109, 597, 105, 588):
        raise AssertionError("Sprint37 source-derived roster totals differ")


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
    print("Sprint37 release175 PASS: exact reviewed Vrock product and assets, append-only identities, three public routes and version-only product metadata. Native release qualification is separate.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Sprint37 release175 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
