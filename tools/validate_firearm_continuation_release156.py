"""v0.0.156 firearm RTWP continuation patch on released v0.0.155.

The patch changes no blueprint identity, route, asset or save data. Relative to
the released v0.0.155 master it may change only the five firearm-continuation
production files and the version metadata, and the static record may change
only its version, milestone and this patch's own pinned entry. Every released
Sprint 24 public-boundary check still holds; the Sprint 24 validator itself
stays the v0.0.155 release text.
"""
from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
import validate_expanded_summoning_sprint24154 as released155

RELEASED = "97c881ea847a5d3703982ed6b0aebc0e0537c7c5"
VERSION = "0.0.156"
INFORMATIONAL = "0.0.156-firearm-rtwp-kill-continuation"
RECORD_KEY = "firearmRtwpKillContinuation156"

FIREARM_PRODUCTION = {
    "src/KingmakerGunslinger/Firing/EmptyFirearmAttackCommandPatch.cs",
    "src/KingmakerGunslinger/Firing/FirearmAttackOrderLedger.cs",
    "src/KingmakerGunslinger/Firing/NativeFirearmAttackOrder.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.FirearmMaintenance.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.FirearmReviewKillChain.cs",
}
VERSION_PRODUCTION = {"src/KingmakerGunslinger/Properties/AssemblyInfo.cs"}
# The project may differ from v0.0.155 only by compiling the new fixture file.
PROJECT = "src/KingmakerGunslinger/KingmakerGunslinger.csproj"
PROJECT_LINE = '    <Compile Include="RuntimeTesting\\RuntimeTestRunner.FirearmReviewKillChain.cs" />'

RECORD = {
    "releaseVersion": VERSION,
    "releaseInformationalVersion": INFORMATIONAL,
    "baseStableVersion": "0.0.155",
    "correction": "A finished explicit firearm order whose target is dead "
                  "completes without suppression when the equipped firearm "
                  "is currently Normal",
    "identityChanges": 0,
    "routeChanges": 0,
    "assetChanges": 0,
    "serializationChanges": 0,
    "registeredLogicalPlacements": 1093,
    "retainedNativeWrappers": 29,
    "visibleChoiceTotal": 1122,
    "guardedNativeFixtureQualified": True,
    "originalInFullAttackSymptomReproduced": False,
    "ownerLiveRtwpCheckPerformed": False,
}


def check_identities(released_entries, current_entries):
    if current_entries != released_entries:
        raise AssertionError("The firearm patch must not change blueprint identities")


def check_project(released_text, current_text):
    current = current_text.splitlines()
    if current.count(PROJECT_LINE) != 1 or \
            released_text.splitlines().count(PROJECT_LINE) != 0:
        raise AssertionError("The project must add exactly the kill-chain fixture compile item")
    current.remove(PROJECT_LINE)
    if current != released_text.splitlines():
        raise AssertionError("The project changed beyond the kill-chain fixture compile item")


def check_production_scope(changed):
    unexpected = sorted(set(changed) - FIREARM_PRODUCTION - VERSION_PRODUCTION - {PROJECT})
    if unexpected:
        raise AssertionError("Production source outside the firearm patch changed: " +
                             ", ".join(unexpected))


def check_static(released, current):
    for key in set(released) | set(current):
        if key in ("version", "milestone", RECORD_KEY):
            continue
        if released.get(key) != current.get(key):
            raise AssertionError("Released static evidence changed: " + key)
    if current.get("version") != VERSION or current.get("milestone") != INFORMATIONAL:
        raise AssertionError("Static record version metadata mismatch")
    if current.get(RECORD_KEY) != RECORD:
        raise AssertionError("Firearm patch static record mismatch")


def check_version(info_version, props, assembly):
    if info_version != VERSION:
        raise AssertionError("Unsupported firearm patch version: " + repr(info_version))
    if "<KmgVersion>" + VERSION + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + INFORMATIONAL + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + VERSION + '")' not in assembly or \
            'AssemblyFileVersion("' + VERSION + '")' not in assembly or \
            'AssemblyInformationalVersion("' + INFORMATIONAL + '")' not in assembly:
        raise AssertionError("Active firearm patch version metadata mismatch")


def changed_production(root: Path):
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", RELEASED, "--", "src/KingmakerGunslinger"],
        cwd=root, text=True).splitlines()
    changed += subprocess.check_output(
        ["git", "ls-files", "--others", "--exclude-standard", "--",
         "src/KingmakerGunslinger"], cwd=root, text=True).splitlines()
    return changed


def validate(root: Path):
    current = common.document(root, "blueprints/blueprints.json")["entries"]
    check_identities(json.loads(common.blob(
        root, RELEASED, "blueprints/blueprints.json"))["entries"], current)

    # The released Sprint 24 public boundary, unchanged.
    accepted = json.loads(common.blob(
        root, released155.BASELINE, "blueprints/blueprints.json"))["entries"]
    visibility_source = (root / "src/KingmakerGunslinger/Summoning/"
                         "SummonVisibilityCatalog.cs").read_text(encoding="utf-8-sig")
    stage = released155.validate_visibility(
        visibility_source, released155.validate_append(accepted, current))
    if stage != "public":
        raise AssertionError("Released Sprint 24 routes are not public: " + stage)
    released155.validate_static(root, stage)
    released155.validate_art_and_package_owners(root, stage)

    check_production_scope(changed_production(root))
    check_project(common.blob(root, RELEASED, PROJECT).decode("utf-8-sig"),
                  (root / PROJECT).read_text(encoding="utf-8-sig"))
    check_static(json.loads(common.blob(root, RELEASED, "validation/static-validation.json")),
                 common.document(root, "validation/static-validation.json"))
    check_version(common.document(root, "Info.json").get("Version"),
                  (root / "Directory.Build.props").read_text(encoding="utf-8-sig"),
                  (root / "src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
                  .read_text(encoding="utf-8-sig"))

    manifest = subprocess.run(
        [sys.executable, str(root / "tools/expanded_summoning_manifest.py")],
        cwd=root, text=True, capture_output=True, check=False)
    if manifest.returncode:
        raise AssertionError("Expanded Summoning manifest validation failed: " +
                             (manifest.stdout + manifest.stderr)[-1000:])
    print("Firearm continuation patch PASS: identities=" + str(len(current)) +
          "; version=" + VERSION + "; milestone=" + INFORMATIONAL + ".")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except (AssertionError, ValueError, KeyError,
            subprocess.CalledProcessError) as error:
        print("Firearm continuation patch FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
