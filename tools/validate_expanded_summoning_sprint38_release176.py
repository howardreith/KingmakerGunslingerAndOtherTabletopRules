"""Sprint38 exact reviewed product, append-only identities and public scope."""
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

# The Sprint 38 product branch head and the previous release's records merge.
# The head is after the version bump, as Sprints 36 and 37's were, so the
# metadata check degenerates to "product metadata is unchanged from the
# reviewed head" and OLD_INFORMATIONAL equals INFORMATIONAL.
PRODUCT = "cb7d8e9dd493f49c961db03fd1fac4eec9cc0544"
RELEASED = "2af6f60858b23fef58e981da78725c5e6d8cde73"
VERSION = "0.0.176"
INFORMATIONAL = VERSION + "-expanded-summoning-glabrezu"
OLD_INFORMATIONAL = INFORMATIONAL
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "expandedSummoningSprint38Release176"
HARNESS = {"scripts/Deploy-Local.ps1": "713cceb319c2850d5f440139ebedc5468501d5b9c458e4d418d944cce98d9353", "scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1": "e1d798f8f228b6b9cd7444e20697f2e06de40343b92d8a5b8bf6681b0dec6c6a", "scripts/Invoke-KingmakerRuntimeTest.ps1": "388e130c32947ce34afc4e51598b70ac50d23c7e9f5abc83e94cf217ec37f220", "scripts/Invoke-Sprint22InstalledStack.ps1": "64ee3bc0e803f461e2801341bc3bcea65c7b65ade4dc2bc6f450edfe2562c7f0", "scripts/RuntimeAutomation.Common.ps1": "8dbcf5a2ddabba81917bdc5ef3e2fcca430eab4173dfc8c079f7877defd8ffa3", "scripts/RuntimeHarness.Common.ps1": "f8c67bb58e84b677e377d4a8a93472ecfefffe38fa2b5b778186209d6f7aadb2", "scripts/Sprint38QualifiedRelease.Common.ps1": "19012b7a39a947df463d4e1555bed1e181a50f09bc995e8d38c8cb08e67dc05f", "scripts/Test-Sprint22InstalledStack.ps1": "e7c6c4a401e6881c02516d184c8d0e377c02df24af75cc491a9c7178fca08f2c", "scripts/Test-Sprint32NativePersistenceRequest.ps1": "e6e69985607782c5c1c4ee476a66e8f8a68c295f8603732fbd774c84b8d9dcb2", "scripts/Test-Sprint38Release.ps1": "99388e60f898414e92911898de579154f20a77753d32264c6b21cc1685e44b33"}
RECORD = dict(
    releaseVersion=VERSION,
    releaseInformationalVersion=INFORMATIONAL,
    baseStableVersion="0.0.175",
    reviewedProductSource=PRODUCT,
    appendedIdentities=32,
    releasedIdentityChanges=0,
    change="Glabrezu, one untemplated Summon Monster IX route, a source-owned round-scoped rend and a source-owned Power Word Stun at its printed close range, five at-will and constant native spell-likes, and a distinct original four-armed body/icon",
    nativeEvidence="Final exact package requires fresh grouped proof for every mandatory row: no Sprint38 row may be reused, so no development-candidate receipt can qualify this release",
    ownerPublicationAuthority="Sprint38 Glabrezu Fresh Codex Mission, its correction continuation and its completion instruction, together with the owner decisions of 2026-11-03 omitting Reverse Gravity with disclosure and no substitute, and accepting the self-only Veil omission with accurately scoped disclosure; greater teleport, nested summoning and wish remain excluded for every summon, and the live Perception of 28 against the printed 26 is a measured deviation caused by the installed Persuasive feat rather than an omission",
    harnessIdentity=HARNESS,
    harnessIdentityNote="Tool and fixture identity for the orchestration that produced the native rows, recorded separately from the compiled product identity. These paths lie outside the validator's guarded product roots, so they are pinned here rather than by the product diff. The validator file itself cannot carry its own digest; its provenance is the reviewed product commit.",
    installedStackFixtureChecks=52,
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
    if len(current["entries"]) - len(released["entries"]) != 32:
        raise AssertionError("Sprint38 identity append count differs")


def released_creatures(root):
    text = common.blob(root, RELEASED,
        "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs").decode("utf-8-sig")
    return [dict(key=k, name=n, monster=None if m == "null" else int(m),
        templated=t == "true", ally=None if a == "null" else int(a), visual=v or n)
        for k, n, m, t, a, v in manifest.ENTRY.findall(text)]


def check_census(creatures, hidden, baseline):
    # One creature, one Monster root from SM IX, no Nature's Ally route, no
    # template child, and nothing withheld: Sprint38 published after its five
    # guarded mechanics phases.
    expected = {"glabrezu": 9}
    selected = [c for c in creatures if c["key"] in expected]
    if (hidden or len(selected) != 1 or {c["key"] for c in selected} != set(expected) or
            any(c["monster"] != expected[c["key"]] or c["ally"] is not None or
                c["templated"] for c in selected)):
        raise AssertionError("Exactly Glabrezu SM IX must be public and untemplated with nothing withheld")
    if [c for c in creatures if c["key"] not in expected] != baseline:
        raise AssertionError("Released creature identities, ordering or routes changed")
    monster = sum(10 - c["monster"] for c in creatures if c["monster"] is not None)
    ally = sum(10 - c["ally"] for c in creatures if c["ally"] is not None)
    if (len(creatures), sum(c["monster"] is not None for c in creatures), monster,
            sum(c["ally"] is not None for c in creatures), ally) != (132, 110, 598, 105, 588):
        raise AssertionError("Sprint38 source-derived roster totals differ")


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
    print("Sprint38 release176 PASS: exact reviewed Glabrezu product and assets, append-only identities, one public route and version-only product metadata. Native release qualification is separate.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Sprint38 release176 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
