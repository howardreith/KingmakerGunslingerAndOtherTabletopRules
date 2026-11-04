"""Sprint39 exact reviewed product, append-only identities and public scope."""
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

# The Sprint 39 product branch head and the previous release's records merge.
# The head is after the version bump, as Sprints 36 to 38's were, so the
# metadata check degenerates to "product metadata is unchanged from the
# reviewed head" and OLD_INFORMATIONAL equals INFORMATIONAL.
PRODUCT = "fd9f0b77004daebb63adf2ce70f7558acab9748a"
RELEASED = "c69212d6c7871c8f3ebf043bfffa53ee3851455c"
VERSION = "0.0.177"
INFORMATIONAL = VERSION + "-expanded-summoning-nalfeshnee"
OLD_INFORMATIONAL = INFORMATIONAL
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "expandedSummoningSprint39Release177"
HARNESS = {"scripts/Deploy-Local.ps1": "713cceb319c2850d5f440139ebedc5468501d5b9c458e4d418d944cce98d9353", "scripts/Invoke-ExpandedSummoningRuntimeScenario.ps1": "e1d798f8f228b6b9cd7444e20697f2e06de40343b92d8a5b8bf6681b0dec6c6a", "scripts/Invoke-KingmakerRuntimeTest.ps1": "6bab6582bfdcf3913dab2ab296a7a206c5f047503647dc66c7df4fc371609164", "scripts/Invoke-Sprint22InstalledStack.ps1": "70c2734fc81ee6c9f47d7060c974e03268c664aac02bc2ff4dbdcc0e9d63b398", "scripts/RuntimeAutomation.Common.ps1": "43ddee00e71601d248356b81bc6912835bb7db51fb31d9158dce6d273fb058be", "scripts/RuntimeHarness.Common.ps1": "f8c67bb58e84b677e377d4a8a93472ecfefffe38fa2b5b778186209d6f7aadb2", "scripts/Sprint39QualifiedRelease.Common.ps1": "4016f4e6fc767f493a1f5e5665b94ff1b8b1d72828098c89fabed1deb692684b", "scripts/Test-Sprint22InstalledStack.ps1": "e7c6c4a401e6881c02516d184c8d0e377c02df24af75cc491a9c7178fca08f2c", "scripts/Test-Sprint32NativePersistenceRequest.ps1": "173001949007ba2821be83adaaf6abc40257077b63786f6eb873feae33057f48", "scripts/Test-Sprint39Release.ps1": "694e41410fa7605bb9ab8a79a36bb486145281425a4d5e07197233dc7c29d67e"}
RECORD = dict(
    releaseVersion=VERSION,
    releaseInformationalVersion=INFORMATIONAL,
    baseStableVersion="0.0.176",
    reviewedProductSource=PRODUCT,
    appendedIdentities=31,
    releasedIdentityChanges=0,
    change="Nalfeshnee, one untemplated Summon Monster IX route, a source-owned Unholy Nimbus with a one-round delayed burst and at most one pending activation per creature, an exact-source constant Unholy Aura, feeblemind with a real spellcasting prohibition and the native heal cure, greater dispel magic carrying one decisive check to its printed cap, two further at-will spell-likes at their own derived reach, and a distinct original corpulent boar-headed winged body/icon",
    nativeEvidence="Final exact package requires fresh grouped proof for every mandatory row: no Sprint39 row may be reused, so no development-candidate receipt can qualify this release. Unholy Nimbus carries two rows because its two combat modes are separate phases and game time advances differently in each",
    ownerPublicationAuthority="Sprint39 Nalfeshnee Completion Mission, its implementation/release continuation, its native-behaviour repair instruction and its finish instruction, together with the owner decisions of 2026-11-04 limiting Unholy Nimbus to at most one pending activation per creature and scoping the constant Unholy Aura to an exact-source self effect with no ally grant; greater teleport, nested summoning and wish remain excluded for every summon, demon reinforcement summoning is a disclosed omission, the effective movement is the printed 40-foot fly with the 30-foot land mode recorded as omitted, feeblemind deliberately forbids spellcasting and not magic items, and Slow carries the project-recorded medium reach rather than the printed close derivation",
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
    if len(current["entries"]) - len(released["entries"]) != 31:
        raise AssertionError("Sprint39 identity append count differs")


def released_creatures(root):
    text = common.blob(root, RELEASED,
        "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs").decode("utf-8-sig")
    return [dict(key=k, name=n, monster=None if m == "null" else int(m),
        templated=t == "true", ally=None if a == "null" else int(a), visual=v or n)
        for k, n, m, t, a, v in manifest.ENTRY.findall(text)]


def check_census(creatures, hidden, baseline):
    # One creature, one Monster root from SM IX, no Nature's Ally route, no
    # template child, and nothing withheld: Sprint39 published after its four
    # guarded mechanics phases, two of them the same ability in the two combat
    # modes.
    expected = {"nalfeshnee": 9}
    selected = [c for c in creatures if c["key"] in expected]
    if (hidden or len(selected) != 1 or {c["key"] for c in selected} != set(expected) or
            any(c["monster"] != expected[c["key"]] or c["ally"] is not None or
                c["templated"] for c in selected)):
        raise AssertionError("Exactly Nalfeshnee SM IX must be public and untemplated with nothing withheld")
    if [c for c in creatures if c["key"] not in expected] != baseline:
        raise AssertionError("Released creature identities, ordering or routes changed")
    monster = sum(10 - c["monster"] for c in creatures if c["monster"] is not None)
    ally = sum(10 - c["ally"] for c in creatures if c["ally"] is not None)
    if (len(creatures), sum(c["monster"] is not None for c in creatures), monster,
            sum(c["ally"] is not None for c in creatures), ally) != (133, 111, 599, 105, 588):
        raise AssertionError("Sprint39 source-derived roster totals differ")


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
    print("Sprint39 release177 PASS: exact reviewed Nalfeshnee product and assets, append-only identities, one public route and version-only product metadata. Native release qualification is separate.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Sprint39 release177 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
