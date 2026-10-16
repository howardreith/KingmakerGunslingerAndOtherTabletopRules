"""v0.0.157 alternate summoning sources release on released v0.0.156.

Relative to released v0.0.156 master the release may append exactly the
3,746 alternate summoning adapter identities (the released ledger stays an
exact, unchanged prefix), change only the pinned alternate summoning
production files and the version metadata, add only the pinned compile items
to the project, and add only its own record to the static evidence. Every
released Sprint 24 public-boundary check still holds, and the Expanded
Summoning manifest tool re-derives every adapter identity.
"""
from __future__ import annotations

import argparse
import json
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_development_candidate as identities
import validate_expanded_summoning_sprint21150 as common
import validate_expanded_summoning_sprint24154 as released155

# Released v0.0.156 master, after its release record (PR #51).
RELEASED = "968b35495c11d87a7e00b96f3d33bea3ce05929b"
VERSION = "0.0.157"
INFORMATIONAL = "0.0.157-alternate-summoning-sources"
RECORD_KEY = "alternateSummoningSources157"

ADAPTER_PREFIX = "KMG.Summoning.Adapter."
APPENDED_IDENTITIES = 3746

ALTERNATE_SUMMONING_PRODUCTION = {
    "src/KingmakerGunslinger/Blueprints/AlternateSummoningAdapters.cs",
    "src/KingmakerGunslinger/Blueprints/AlternateSummoningCompositionReader.cs",
    "src/KingmakerGunslinger/Blueprints/BlueprintUnitFactAccess.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningAbilityBuilder.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/SummonSignatureReader.cs",
    "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs",
    "src/KingmakerGunslinger/Main.cs",
    "src/KingmakerGunslinger/RuntimeTesting/AlternateSummoningFaultDrillControl.cs",
    "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.AlternateSummoning.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.AlternateSummoningComposition.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.AlternateSummoningContracts.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.AlternateSummoningNatural.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.AlternateSummoningPlayerPaths.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.AlternateSummoningPlayerPathsHost.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeProfile.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningAvailability.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningComposition.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningContracts.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningCoordinator.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningLifecyclePolicy.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningMenuPolicy.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningMutationLog.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningNaturalVerdict.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningProjection.cs",
    "src/KingmakerGunslinger/Summoning/AlternateSummoningSourceCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningVariantMenuPatch.cs",
    "src/KingmakerGunslinger/Summoning/SummonSignature.cs",
}
VERSION_PRODUCTION = {"src/KingmakerGunslinger/Properties/AssemblyInfo.cs"}
PROJECT = "src/KingmakerGunslinger/KingmakerGunslinger.csproj"
PROJECT_LINES = [
    '    <Compile Include="Summoning\\AlternateSummoningSourceCatalog.cs" />',
    '    <Compile Include="Summoning\\AlternateSummoningProjection.cs" />',
    '    <Compile Include="Summoning\\AlternateSummoningMenuPolicy.cs" />',
    '    <Compile Include="Summoning\\AlternateSummoningLifecyclePolicy.cs" />',
    '    <Compile Include="Summoning\\AlternateSummoningMutationLog.cs" />',
    '    <Compile Include="Summoning\\AlternateSummoningAvailability.cs" />',
    '    <Compile Include="Summoning\\AlternateSummoningCoordinator.cs" />',
    '    <Compile Include="Blueprints\\AlternateSummoningAdapters.cs" />',
    '    <Compile Include="RuntimeTesting\\RuntimeTestRunner.AlternateSummoning.cs" />',
    '    <Compile Include="RuntimeTesting\\AlternateSummoningFaultDrillControl.cs" />',
    '    <Compile Include="RuntimeTesting\\RuntimeTestRunner.AlternateSummoningContracts.cs" />',
    '    <Compile Include="RuntimeTesting\\RuntimeTestRunner.AlternateSummoningPlayerPaths.cs" />',
    '    <Compile Include="RuntimeTesting\\RuntimeTestRunner.AlternateSummoningPlayerPathsHost.cs" />',
    '    <Compile Include="RuntimeTesting\\RuntimeTestRunner.AlternateSummoningNatural.cs" />',
    '    <Compile Include="RuntimeTesting\\RuntimeTestRunner.AlternateSummoningComposition.cs" />',
    '    <Compile Include="Blueprints\\SummonSignatureReader.cs" />',
    '    <Compile Include="Blueprints\\AlternateSummoningCompositionReader.cs" />',
    '    <Compile Include="Summoning\\SummonSignature.cs" />',
    '    <Compile Include="Summoning\\AlternateSummoningContracts.cs" />',
    '    <Compile Include="Summoning\\AlternateSummoningComposition.cs" />',
    '    <Compile Include="Summoning\\AlternateSummoningNaturalVerdict.cs" />',
]

RECORD = {
    "releaseVersion": VERSION,
    "releaseInformationalVersion": INFORMATIONAL,
    "baseStableVersion": "0.0.156",
    "change": "Alternate summoning sources: the published Expanded Summoning "
              "roster projected onto the direct class summon callers (native "
              "Monster Tactician; with Call of the Wild 1.14.4c Summoner, "
              "Master Summoner, Occultist, Naturalist, Feral Hunter and "
              "Preservationist)",
    "appendedIdentities": APPENDED_IDENTITIES,
    "appendedNamespace": ADAPTER_PREFIX,
    "releasedIdentityChanges": 0,
    "adapterFamilies": 7,
    "registeredLogicalPlacements": 1093,
    "retainedNativeWrappers": 29,
    "visibleChoiceTotal": 1122,
    "guardedNativeQualified": True,
    "ownerManualChecksOpen": ["hotbar-binding", "saved-shortcut-cold-load",
                              "turn-based-round", "scroll-required-menu"],
    "maintainerFixturesOpen": ["blocked-family-shortcut-load",
                               "grapple-swallow-reset-on-reload"],
}


def check_identities(released_entries, current_entries):
    """The released ledger is an exact prefix; only adapter rows follow."""
    appended = identities.check_append_only(released_entries, current_entries)
    if appended != APPENDED_IDENTITIES:
        raise AssertionError("Expected exactly " + str(APPENDED_IDENTITIES) +
                             " appended adapter identities, observed " + str(appended))
    foreign = [row["symbol"] for row in current_entries[len(released_entries):]
               if not row["symbol"].startswith(ADAPTER_PREFIX)]
    if foreign:
        raise AssertionError("Appended identities outside the adapter namespace: " +
                             ", ".join(foreign[:5]))


def check_project(released_text, current_text):
    current = current_text.splitlines()
    released = released_text.splitlines()
    for line in PROJECT_LINES:
        if current.count(line) != 1 or released.count(line) != 0:
            raise AssertionError("The project must add exactly one compile item: " + line)
        current.remove(line)
    if current != released:
        raise AssertionError("The project changed beyond the alternate summoning compile items")


def check_production_scope(changed):
    unexpected = sorted(set(changed) - ALTERNATE_SUMMONING_PRODUCTION -
                        VERSION_PRODUCTION - {PROJECT})
    if unexpected:
        raise AssertionError("Production source outside the alternate summoning release changed: " +
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
        raise AssertionError("Alternate summoning release static record mismatch")


def check_version(info_version, props, assembly):
    if info_version != VERSION:
        raise AssertionError("Unsupported alternate summoning release version: " +
                             repr(info_version))
    if "<KmgVersion>" + VERSION + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + INFORMATIONAL + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + VERSION + '")' not in assembly or \
            'AssemblyFileVersion("' + VERSION + '")' not in assembly or \
            'AssemblyInformationalVersion("' + INFORMATIONAL + '")' not in assembly:
        raise AssertionError("Active alternate summoning release version metadata mismatch")


def changed_production(root: Path):
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", RELEASED, "--", "src/KingmakerGunslinger"],
        cwd=root, text=True).splitlines()
    changed += subprocess.check_output(
        ["git", "ls-files", "--others", "--exclude-standard", "--",
         "src/KingmakerGunslinger"], cwd=root, text=True).splitlines()
    return changed


def validate(root: Path):
    released_ledger = json.loads(common.blob(root, RELEASED, "blueprints/blueprints.json"))
    current_ledger = common.document(root, "blueprints/blueprints.json")
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current_ledger.get(key) != released_ledger.get(key):
            raise AssertionError("The identity ledger header changed: " + key)
    current = current_ledger["entries"]
    check_identities(released_ledger["entries"], current)

    # The released Sprint 24 public boundary, unchanged. Its 48 rows sit in
    # the released v0.0.156 prefix, which check_identities pins exactly, so
    # Sprint 24's own exact-append check runs on that prefix.
    accepted = json.loads(common.blob(
        root, released155.BASELINE, "blueprints/blueprints.json"))["entries"]
    visibility_source = (root / "src/KingmakerGunslinger/Summoning/"
                         "SummonVisibilityCatalog.cs").read_text(encoding="utf-8-sig")
    stage = released155.validate_visibility(
        visibility_source, released155.validate_append(accepted, released_ledger["entries"]))
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
    print("Alternate summoning release PASS: identities=" + str(len(current)) +
          "; appended=" + str(len(current) - len(released_ledger["entries"])) +
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
        print("Alternate summoning release FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
