"""Sprint 24 theropod source and append-only identity boundary.

The validator deliberately understands three reviewable states: the released
Sprint 23 prefix plus the bounded Sprint 24 census, a registered hidden
candidate, and the final public candidate. Registration allocates all IDs at
once; publication may remove only the two suppression keys.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
import validate_expanded_summoning_sprint23152 as prior

BASELINE = "e825411a8f920fdb9447f7ad48d3f7d0267e6e32"
SPRINT24_KEY = "expandedSummoningSprint24154"
THEROPODS = {"deinonychus", "tyrannosaurus"}

GUIDS = {
    "KMG.Summoning.Unit.Deinonychus": "10a13f87e3664e30b642a3c59cc8c700",
    "KMG.Summoning.Unit.Tyrannosaurus": "04b1a75a76544a9581ee832045f8227a",
    "KMG.Summoning.Ability.SM.Tier4.Deinonychus.One": "46a5cfe1ffae4b57acb94077ceb0ffaa",
    "KMG.Summoning.Ability.SM.Tier4.Deinonychus.One.Celestial": "44933dbcd05c43a5b242567712d86531",
    "KMG.Summoning.Ability.SM.Tier4.Deinonychus.One.Fiendish": "7b0080e107fe4529bbd9fc5e0fcbcfaf",
    "KMG.Summoning.Ability.SM.Tier5.Deinonychus.OneD3": "f310d0f0154a4b5586c5cfed86ef925a",
    "KMG.Summoning.Ability.SM.Tier5.Deinonychus.OneD3.Celestial": "46b57bb9d9af4782a421ef9b3434166b",
    "KMG.Summoning.Ability.SM.Tier5.Deinonychus.OneD3.Fiendish": "f50867059c0442178c7132ce9957e4a7",
    "KMG.Summoning.Ability.SM.Tier6.Deinonychus.OneD4PlusOne": "a5b34561d75648479e69942627eca878",
    "KMG.Summoning.Ability.SM.Tier6.Deinonychus.OneD4PlusOne.Celestial": "1f9c98d2815e46d3b934125fe2d07e99",
    "KMG.Summoning.Ability.SM.Tier6.Deinonychus.OneD4PlusOne.Fiendish": "233f45a798194a0bb57a97583b780f56",
    "KMG.Summoning.Ability.SM.Tier7.Deinonychus.OneD4PlusOne": "e6cc6b38b68942448b9739044bc1ff90",
    "KMG.Summoning.Ability.SM.Tier7.Deinonychus.OneD4PlusOne.Celestial": "21f266bedce44089a941c5eb53f755cd",
    "KMG.Summoning.Ability.SM.Tier7.Deinonychus.OneD4PlusOne.Fiendish": "2e90e59c7ea04c35a62c3eb308569b50",
    "KMG.Summoning.Ability.SM.Tier7.Tyrannosaurus.One": "e1f591950fc94750b2876c5698f0fbe9",
    "KMG.Summoning.Ability.SM.Tier7.Tyrannosaurus.One.Celestial": "60e1c7b2628a4eea87c5c4131cd96d4c",
    "KMG.Summoning.Ability.SM.Tier7.Tyrannosaurus.One.Fiendish": "07fb9af037944f3485079dd7d9baccdb",
    "KMG.Summoning.Ability.SM.Tier8.Deinonychus.OneD4PlusOne": "335c4f37b9c64ce699f15645312991ca",
    "KMG.Summoning.Ability.SM.Tier8.Deinonychus.OneD4PlusOne.Celestial": "2215a7a20e0e4419add1c660b701259f",
    "KMG.Summoning.Ability.SM.Tier8.Deinonychus.OneD4PlusOne.Fiendish": "9d2016893ae44e53b98cb8b695bcc857",
    "KMG.Summoning.Ability.SM.Tier8.Tyrannosaurus.OneD3": "f98d4354c6de491f804c5605c58ce982",
    "KMG.Summoning.Ability.SM.Tier8.Tyrannosaurus.OneD3.Celestial": "191f3ef571cb4c6db2a4397edac70a95",
    "KMG.Summoning.Ability.SM.Tier8.Tyrannosaurus.OneD3.Fiendish": "4b480f09291442b3ac3226a12f69fe91",
    "KMG.Summoning.Ability.SM.Tier9.Deinonychus.OneD4PlusOne": "8546ddfbed574070920dca37c39f2da9",
    "KMG.Summoning.Ability.SM.Tier9.Deinonychus.OneD4PlusOne.Celestial": "5dd271af3df541ccb21469b66bb685b4",
    "KMG.Summoning.Ability.SM.Tier9.Deinonychus.OneD4PlusOne.Fiendish": "e575eb70d12248febdc72438733f2264",
    "KMG.Summoning.Ability.SM.Tier9.Tyrannosaurus.OneD4PlusOne": "f5a745af800142aabdff6c656848445b",
    "KMG.Summoning.Ability.SM.Tier9.Tyrannosaurus.OneD4PlusOne.Celestial": "98aa7b4b4ff94c06acdbbc42e4192100",
    "KMG.Summoning.Ability.SM.Tier9.Tyrannosaurus.OneD4PlusOne.Fiendish": "1c55ba50b6404db381aedcddc91f6c5c",
    "KMG.Summoning.Ability.SNA.Tier4.Deinonychus.One": "9f24f72736a549f8b1dac6d3241e0f00",
    "KMG.Summoning.Ability.SNA.Tier5.Deinonychus.OneD3": "0fc9fa82f3b947ffad3e25824495cb4a",
    "KMG.Summoning.Ability.SNA.Tier6.Deinonychus.OneD4PlusOne": "ba60bfa64d17429da0b3a4ef175d49bb",
    "KMG.Summoning.Ability.SNA.Tier7.Deinonychus.OneD4PlusOne": "129e1ccfb23c483d845f8ac1400c00ec",
    "KMG.Summoning.Ability.SNA.Tier7.Tyrannosaurus.One": "5a3e64964c814377ae14362af529584a",
    "KMG.Summoning.Ability.SNA.Tier8.Deinonychus.OneD4PlusOne": "4c18a58e32c14a47b67e8d21e73f18dc",
    "KMG.Summoning.Ability.SNA.Tier8.Tyrannosaurus.OneD3": "5fb610228a8043b88f9ebd11e0f1ccdb",
    "KMG.Summoning.Ability.SNA.Tier9.Deinonychus.OneD4PlusOne": "52dc5c5341794ce48c7363cf69639625",
    "KMG.Summoning.Ability.SNA.Tier9.Tyrannosaurus.OneD4PlusOne": "cb1921bee3e94b8d974e45fb636ea770",
    "KMG.Summoning.Natural.Deinonychus.Talon1d8": "7bb056a081a4444b804e52508d91f6c8",
    "KMG.Summoning.Natural.Deinonychus.UnitType": "cf0919eae4304a148666742cebff1887",
    "KMG.Summoning.Natural.Deinonychus.Chassis": "fed90fe9166348a8b7ae8bff5f03566a",
    "KMG.Summoning.Natural.Tyrannosaurus.Bite4d6": "cfb6ea9c9c3b4405bb001f00159e8370",
    "KMG.Summoning.Natural.Tyrannosaurus.UnitType": "b3ae2d5ad26c4df9aea68260ad0a4228",
    "KMG.Summoning.Natural.Tyrannosaurus.Chassis": "7c6bb677353045189045129c097e4c2f",
    "KMG.Summoning.Special.Tyrannosaurus.PowerfulBite": "c1a9b9554bb744a9a22f7ee1557dd678",
    "KMG.Summoning.Special.Tyrannosaurus.CombatTraits": "e22d935930eb4a9398e1160699f87b09",
    "KMG.Summoning.Special.Tyrannosaurus.Swallowed": "b53c756df5b04546a87049d6f24e6f37",
    "KMG.Summoning.Special.Tyrannosaurus.BleedingCritical": "e45b109a0e96496d9efd42edd11f95c8",
}


def planned_type(symbol: str) -> str:
    if symbol.startswith("KMG.Summoning.Unit."):
        return "BlueprintUnit"
    if symbol.startswith("KMG.Summoning.Ability."):
        return "BlueprintAbility"
    if symbol.endswith("Talon1d8") or symbol.endswith("Bite4d6"):
        return "BlueprintItemWeapon"
    if symbol.endswith("UnitType"):
        return "BlueprintUnitType"
    if symbol.endswith("CombatTraits") or symbol.endswith("Swallowed"):
        return "BlueprintBuff"
    return "BlueprintFeature"


ADDED = {symbol: (guid, planned_type(symbol)) for symbol, guid in GUIDS.items()}

# This is a source-ownership boundary, not a claim that listed files already
# qualify. Additions must be exact Sprint 24 seams and remain reviewable here.
ALLOWED_PRODUCTION = {
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.Theropods.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Theropods.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
    "src/KingmakerGunslinger/KingmakerGunslinger.csproj",
    "src/KingmakerGunslinger/Assets/PteranodonAssetRuntime.cs",
    "src/KingmakerGunslinger/Assets/PteranodonAssetRuntime.Theropods.cs",
    "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningRosterPersistencePolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeProfile.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Census.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Review.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Persistence.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "src/KingmakerGunslinger/RuntimeTesting/Sprint24TheropodCensusPolicy.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningPteranodonViewPatch.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialCombatComponents.cs",
    # Admitted 2026-10-15 for the swallowed-state reload scope only: the shared
    # swallow/engulf constants already live here, and the narrowed opt-in policy
    # belongs beside them rather than in a new file.
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialProfiles.cs",
    "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/SummonViewScaleCatalog.cs",
    "src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs",
    "src/KingmakerGunslinger/Summoning/TheropodCombatComponents.cs",
    "src/KingmakerGunslinger/Summoning/TheropodRulesPolicy.cs",
    "src/KingmakerGunslinger/Summoning/TheropodVisualPolicy.cs",
}

ART = {
    "assets/game/icons/expanded-summoning/deinonychus.png":
        "d7c15ef813f4920efac88a5b1bcd194e1f872af98c5d619dd60758e96aa1dd36",
    "assets/game/icons/expanded-summoning/tyrannosaurus.png":
        "e80aeaa47e293e74fd6bd589670ff864f4c3375d01f1a8c6c7ca3eb25eb50698",
    "assets/sprint24-theropods/deinonychus-mesh.json":
        "eb1a06fb8933d603bfb89655df82d6e240bbf861bbe98d4134c9e25df4ed16ab",
    "assets/sprint24-theropods/deinonychus-albedo.png":
        "4e0414763bc70122a4645402724c27c6dadd72fd9341ff1b25fad4bd57c236a7",
    "assets/sprint24-theropods/tyrannosaurus-mesh.json":
        "308a9a9950227b538ce9e7c068e090bf9e81a95fb6a573e8e8a0a4ea62a9ae87",
    "assets/sprint24-theropods/tyrannosaurus-albedo.png":
        "3a4f09eabf7095c6f79fc4a7f0d230c458bdc2b7838fec6faaac86884b5ef7e8",
}


def validate_append(accepted, current):
    if current[:len(accepted)] != accepted:
        raise AssertionError("A released identity or manifest row changed")
    tail = current[len(accepted):]
    if not tail:
        return "census"
    observed = {row["symbol"]: (row["guid"], row["plannedType"])
                for row in tail}
    if observed != ADDED or len(tail) != len(ADDED):
        raise AssertionError("Sprint 24 must append exactly its 48 pinned identities")
    if any(row.get("status") != "active" for row in tail):
        raise AssertionError("Every Sprint 24 identity must be active")
    if len({row["guid"] for row in current}) != len(current):
        raise AssertionError("Duplicate blueprint GUID")
    return "registered"


def suppressed_keys(source: str):
    match = re.search(
        r"SuppressedCreatureKeys\s*=\s*\n?\s*new HashSet<string>\((.*?)\);",
        source, re.S)
    if match is None:
        raise AssertionError("Cannot read summon suppression source")
    return set(re.findall(r'"([a-z0-9-]+)"', match.group(1)))


def validate_visibility(source: str, identity_stage: str):
    keys = suppressed_keys(source)
    if identity_stage == "census":
        if keys & THEROPODS:
            raise AssertionError("Unregistered Sprint 24 creature is suppressed")
        return "census"
    if keys == THEROPODS:
        return "hidden"
    if not keys:
        return "public"
    raise AssertionError("Publication must retain both theropods or remove both")


def validate_production_scope(root: Path):
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", BASELINE, "--", "src/KingmakerGunslinger"],
        cwd=root, text=True).splitlines()
    changed += subprocess.check_output(
        ["git", "ls-files", "--others", "--exclude-standard", "--",
         "src/KingmakerGunslinger"], cwd=root, text=True).splitlines()
    unexpected = sorted(set(changed) - ALLOWED_PRODUCTION)
    if unexpected:
        raise AssertionError("Unrelated production source changed: " +
                             ", ".join(unexpected))


def validate_static(root: Path, stage: str):
    static = common.document(root, "validation/static-validation.json")
    released = json.loads(common.blob(
        root, BASELINE, "validation/static-validation.json"))
    for key, value in released.items():
        if key not in ("version", "milestone") and static.get(key) != value:
            raise AssertionError("Historical release evidence changed: " + key)
    record = static.get(SPRINT24_KEY)
    if stage == "census":
        if record is not None:
            raise AssertionError("Census state cannot claim a registered Sprint 24 candidate")
        return
    if not isinstance(record, dict):
        raise AssertionError("Registered Sprint 24 candidate lacks static metadata")
    expected = {
        "newCreatures": 2,
        "newLogicalRoots": 18,
        "newTemplateExecutionChildren": 18,
        "newPrivateIdentities": 10,
        "registeredLogicalPlacements": 1093,
        "suppressedLogicalPlacements": 18 if stage == "hidden" else 0,
        "publishedLogicalPlacements": 1075 if stage == "hidden" else 1093,
        "retainedNativeWrappers": 29,
        "visibleChoiceTotal": 1104 if stage == "hidden" else 1122,
        "projectIconConcepts": 119,
        "originalBodyFiles": 4,
        "sprint25Started": False,
    }
    for key, value in expected.items():
        if record.get(key) != value:
            raise AssertionError("Sprint 24 static metadata mismatch: " + key)


def validate_art_and_package_owners(root: Path, stage: str):
    if stage == "census":
        return
    for relative, expected in ART.items():
        path = root / relative
        if not path.is_file():
            raise AssertionError("Missing Sprint 24 original asset: " + relative)
        observed = hashlib.sha256(path.read_bytes()).hexdigest()
        if observed != expected:
            raise AssertionError("Sprint 24 original asset hash changed: " + relative)
    for token in ("sprint24-theropods", "deinonychus", "tyrannosaurus"):
        for script in ("scripts/Build-Local.ps1", "scripts/package.ps1",
                       "scripts/validate-build-output.ps1",
                       "scripts/validate-package.ps1"):
            if token not in (root / script).read_text(encoding="utf-8-sig"):
                raise AssertionError(script +
                                     " lacks package owner token: " + token)


def validate_version(root: Path):
    info = common.document(root, "Info.json")
    version = info.get("Version")
    if version not in ("0.0.153", "0.0.154"):
        raise AssertionError("Unsupported Sprint 24 lineage version: " + repr(version))
    informational = ("0.0.153-expanded-summoning-sprint23-charge-rounding"
                     if version == "0.0.153" else
                     "0.0.154-expanded-summoning-sprint24-theropods")
    props = (root / "Directory.Build.props").read_text(encoding="utf-8-sig")
    assembly = (root / "src/KingmakerGunslinger/Properties/AssemblyInfo.cs").read_text(
        encoding="utf-8-sig")
    if "<KmgVersion>" + version + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + informational + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + version + '")' not in assembly or \
            'AssemblyFileVersion("' + version + '")' not in assembly or \
            'AssemblyInformationalVersion("' + informational + '")' not in assembly:
        raise AssertionError("Active Sprint 24 version metadata mismatch")
    return version, informational


def validate(root: Path):
    accepted = json.loads(common.blob(
        root, BASELINE, "blueprints/blueprints.json"))["entries"]
    current = common.document(root, "blueprints/blueprints.json")["entries"]
    identity_stage = validate_append(accepted, current)
    visibility_source = (root / "src/KingmakerGunslinger/Summoning/"
                         "SummonVisibilityCatalog.cs").read_text(encoding="utf-8-sig")
    stage = validate_visibility(visibility_source, identity_stage)
    validate_production_scope(root)
    validate_static(root, stage)
    validate_art_and_package_owners(root, stage)
    version, informational = validate_version(root)

    if stage == "census":
        # Retain every released Sprint 23 check while the manifest is still
        # exactly that release. Its allowlist admits only the bounded census.
        prior.validate(root)

    manifest = subprocess.run(
        [sys.executable, str(root / "tools/expanded_summoning_manifest.py")],
        cwd=root, text=True, capture_output=True, check=False)
    if manifest.returncode:
        raise AssertionError("Expanded Summoning manifest validation failed: " +
                             (manifest.stdout + manifest.stderr)[-1000:])
    print("Sprint24 source PASS: stage=" + stage + "; identities=" +
          str(len(current)) + "; version=" + version + "; milestone=" +
          informational + ".")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except (AssertionError, ValueError, KeyError,
            subprocess.CalledProcessError) as error:
        print("Sprint24 source FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
