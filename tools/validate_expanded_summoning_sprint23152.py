"""Sprint 23 source, identity, asset and package-owner boundary.

Native gameplay and presentation remain separate guarded observations. This
gate proves that the candidate changes only the declared armored-dinosaur
surface and that every released identity remains an exact prefix.
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
import validate_expanded_summoning_phase1 as phase1

VERSION = "0.0.152"
INFORMATIONAL_VERSION = VERSION + "-expanded-summoning-sprint23"
BASELINE = "bda157c6c402b329f47fb0531d96b52e01514114"
KEY = "expandedSummoningSprint23152"

ADDED = {
    "KMG.Summoning.Unit.Triceratops": ("ce540cd4b3b74861a4a91458d742a692", "BlueprintUnit"),
    "KMG.Summoning.Unit.Stegosaurus": ("39cab5be4dc148339f4ce8b0157bd716", "BlueprintUnit"),
    "KMG.Summoning.Ability.SM.Tier6.Triceratops.One": ("2c7a44ebb6d645ddaa30e8d241c91418", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier6.Triceratops.One.Celestial": ("682209c547bf4ff08ea2bad373199ebb", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier6.Triceratops.One.Fiendish": ("b7d7b5684caf4186a8102d1c7151086e", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier7.Triceratops.OneD3": ("088b7d9f07bd4631ad16520cc15c842e", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier7.Triceratops.OneD3.Celestial": ("1f91ee8c4bdc44cdb68c9c0c97e75fa2", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier7.Triceratops.OneD3.Fiendish": ("222af4efca3448f9b14474613118378b", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier8.Triceratops.OneD4PlusOne": ("6fbc68c391f84b45ad6f18c4ac6b103e", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier8.Triceratops.OneD4PlusOne.Celestial": ("019e064bbe594406976408ca08345554", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier8.Triceratops.OneD4PlusOne.Fiendish": ("9f52d7bb38d8419c8cd1810123cca345", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier9.Triceratops.OneD4PlusOne": ("8cc56e3b180f4232b463721268b65f62", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier9.Triceratops.OneD4PlusOne.Celestial": ("b21964bcead442f5b5897b789ee51109", "BlueprintAbility"),
    "KMG.Summoning.Ability.SM.Tier9.Triceratops.OneD4PlusOne.Fiendish": ("bd5e37f4953547e79cb34bf95afe4685", "BlueprintAbility"),
    "KMG.Summoning.Ability.SNA.Tier6.Triceratops.One": ("774fe43dbf164e82a024af00a383f077", "BlueprintAbility"),
    "KMG.Summoning.Ability.SNA.Tier6.Stegosaurus.One": ("bb1b546fe26943c8a4dc396e66e17b04", "BlueprintAbility"),
    "KMG.Summoning.Ability.SNA.Tier7.Triceratops.OneD3": ("290584eb8a644df98fa013dfb6ac8931", "BlueprintAbility"),
    "KMG.Summoning.Ability.SNA.Tier7.Stegosaurus.OneD3": ("d522a44c27fe41feafa3b16243fcdff2", "BlueprintAbility"),
    "KMG.Summoning.Ability.SNA.Tier8.Triceratops.OneD4PlusOne": ("104ef9483c2843fd8573f927dcb44760", "BlueprintAbility"),
    "KMG.Summoning.Ability.SNA.Tier8.Stegosaurus.OneD4PlusOne": ("6f8a6c3c6b614f21a1b084559b24c326", "BlueprintAbility"),
    "KMG.Summoning.Ability.SNA.Tier9.Triceratops.OneD4PlusOne": ("134873f5a4234e959a90ebb2a6bf48be", "BlueprintAbility"),
    "KMG.Summoning.Ability.SNA.Tier9.Stegosaurus.OneD4PlusOne": ("ada164beea7d45edab2aa33bc8b8718a", "BlueprintAbility"),
    "KMG.Summoning.Natural.Triceratops.Gore2d10": ("ffe6a2d9dbe4451abf1855addda1af15", "BlueprintItemWeapon"),
    "KMG.Summoning.Natural.Stegosaurus.Tail4d6": ("96b5e8692e3949bcba63b57507a5075f", "BlueprintItemWeapon"),
    "KMG.Summoning.Natural.Triceratops.UnitType": ("fccd9310f6ed4f70ba02594a81874489", "BlueprintUnitType"),
    "KMG.Summoning.Natural.Stegosaurus.UnitType": ("41029e7b408a45759507797885efb29c", "BlueprintUnitType"),
    "KMG.Summoning.Natural.Triceratops.Chassis": ("5e08305bfe3b4ab2bd65c74dd472c65c", "BlueprintFeature"),
    "KMG.Summoning.Natural.Stegosaurus.Chassis": ("95278500c12647adbf53f8b77b7d3955", "BlueprintFeature"),
    "KMG.Summoning.Special.Triceratops.PowerfulCharge": ("7168413e07084ac086f3eb409b1199fc", "BlueprintFeature"),
    "KMG.Summoning.Special.Triceratops.Trample": ("20c83d35a8234c9fb35e8f174d19e6ba", "BlueprintAbility"),
    "KMG.Summoning.Special.Stegosaurus.Trip": ("ce0af5b47a1a462095e9f308a5d1bba1", "BlueprintFeature"),
    "KMG.Summoning.Natural.Stegosaurus.TailType": ("deb29b6a26c049a39daf3ec6cb3f77f1", "BlueprintWeaponType"),
}

ART = {
    "assets/game/icons/expanded-summoning/triceratops.png": "b261aa99e47d40e55fd794799906ca53a92b17c37736f52da1a017eb0201dc66",
    "assets/game/icons/expanded-summoning/stegosaurus.png": "dfa37b0bdff97f5bda5a2772a64096ad9c39f283d7618fb99c9b8e34f15890e4",
    "assets/sprint23-armored-dinosaurs/triceratops-mesh.json": "ec2e38eae2cb3279a003ec2cf507f4eb30e4e3153905a60a697940833cd8c914",
    "assets/sprint23-armored-dinosaurs/triceratops-albedo.png": "15be7f5c733e770572513dc735cb85fa303ddaa8205de6edc722b558eb5e58e3",
    "assets/sprint23-armored-dinosaurs/stegosaurus-mesh.json": "02a37218c41c09ed9962ac39a8e01ad0e9d99d8f07d718e1e27d9b9b7573582f",
    "assets/sprint23-armored-dinosaurs/stegosaurus-albedo.png": "2305063faf8ad033c412bb5aaf1b749eb496a854b3b5c43b6e427b488a875a3f",
}

ALLOWED_PRODUCTION = {
    "src/KingmakerGunslinger/Assets/PteranodonAssetRuntime.cs",
    "src/KingmakerGunslinger/Assets/PteranodonAssetRuntime.ArmoredDinosaurs.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.ArmoredDinosaurs.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
    "src/KingmakerGunslinger/Development/DevelopmentUi.cs",
    "src/KingmakerGunslinger/KingmakerGunslinger.csproj",
    "src/KingmakerGunslinger/Properties/AssemblyInfo.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningResourceReceipts.cs",
    "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningInventoryObservationPolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCorrection.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeProfile.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint23Review.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ArmoredDinosaurRulesPolicy.cs",
    "src/KingmakerGunslinger/Summoning/ArmoredDinosaurVisualPolicy.cs",
    "src/KingmakerGunslinger/Summoning/ContextActionUngulateTrample.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningPteranodonViewPatch.cs",
    "src/KingmakerGunslinger/Summoning/StegosaurusTailTrip.cs",
    "src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs",
    "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/UngulatePowerfulCharge.cs",
    "src/KingmakerGunslinger/Summoning/UngulateRulesPolicy.cs",
}


def validate_append(accepted, current):
    if current[:len(accepted)] != accepted:
        raise AssertionError("A released identity or manifest row changed")
    tail = current[len(accepted):]
    observed = {row["symbol"]: (row["guid"], row["plannedType"]) for row in tail}
    if observed != ADDED or len(tail) != len(ADDED):
        raise AssertionError("Sprint 23 must append exactly its 32 pinned identities")
    if len({row["guid"] for row in current}) != len(current):
        raise AssertionError("Duplicate blueprint GUID")


def validate_art(root: Path):
    for relative, expected in ART.items():
        path = root / relative
        if not path.is_file():
            raise AssertionError("Missing Sprint 23 art: " + relative)
        actual = hashlib.sha256(path.read_bytes()).hexdigest()
        if actual != expected:
            raise AssertionError("Sprint 23 art hash changed: " + relative)


def validate_production_scope(root: Path):
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", BASELINE, "--", "src/KingmakerGunslinger"],
        cwd=root, text=True).splitlines()
    unexpected = sorted(set(changed) - ALLOWED_PRODUCTION)
    if unexpected:
        raise AssertionError("Unrelated production source changed: " + ", ".join(unexpected))


def validate(root: Path):
    old = json.loads(common.blob(root, BASELINE, "blueprints/blueprints.json"))["entries"]
    current = common.document(root, "blueprints/blueprints.json")["entries"]
    validate_append(old, current)
    validate_art(root)
    validate_production_scope(root)

    info = common.document(root, "Info.json")
    props = (root / "Directory.Build.props").read_text(encoding="utf-8-sig")
    assembly = (root / "src/KingmakerGunslinger/Properties/AssemblyInfo.cs").read_text(
        encoding="utf-8-sig")
    if info.get("Version") != VERSION or \
            "<KmgVersion>" + VERSION + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + INFORMATIONAL_VERSION + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + VERSION + '")' not in assembly or \
            'AssemblyFileVersion("' + VERSION + '")' not in assembly or \
            'AssemblyInformationalVersion("' + INFORMATIONAL_VERSION + '")' \
            not in assembly:
        raise AssertionError("Active version metadata mismatch")

    static = common.document(root, "validation/static-validation.json")
    historic = json.loads(common.blob(root, BASELINE,
                                      "validation/static-validation.json"))
    for key, value in historic.items():
        if key not in ("version", "milestone") and static.get(key) != value:
            raise AssertionError("Historical release evidence changed: " + key)
    if static.get("version") != VERSION or static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static version metadata mismatch")
    record = static.get(KEY, {})
    expected = {
        "newCreatures": 2, "newLogicalRoots": 12,
        "newTemplateExecutionChildren": 8, "newPrivateIdentities": 10,
        "registeredLogicalPlacements": 1075, "retainedNativeWrappers": 29,
        "projectIconConcepts": 117, "originalBodyFiles": 4,
        "sprint24Started": False,
    }
    for key, value in expected.items():
        if record.get(key) != value:
            raise AssertionError("Sprint 23 static metadata mismatch: " + key)
    if record.get("suppressedLogicalPlacements") not in (12, 0) or \
            record.get("publishedLogicalPlacements") + \
            record.get("suppressedLogicalPlacements") != 1075:
        raise AssertionError("Sprint 23 publication arithmetic mismatch")

    source = (root / "src/KingmakerGunslinger/Summoning/"
              "SummonVisibilityCatalog.cs").read_text(encoding="utf-8-sig")
    hidden = '"triceratops"' in source and '"stegosaurus"' in source
    if hidden != (record["suppressedLogicalPlacements"] == 12):
        raise AssertionError("Suppression source and static record disagree")

    manifest = subprocess.run(
        [sys.executable, str(root / "tools/expanded_summoning_manifest.py")],
        cwd=root, text=True, capture_output=True, check=False)
    if manifest.returncode:
        raise AssertionError("Expanded Summoning manifest validation failed: " +
                             (manifest.stdout + manifest.stderr)[-1000:])
    phase1.validate_package_count_owners(root, 363)
    for token in ("sprint23-armored-dinosaurs", "triceratops", "stegosaurus"):
        for script in ("scripts/Build-Local.ps1", "scripts/package.ps1",
                       "scripts/validate-build-output.ps1",
                       "scripts/validate-package.ps1"):
            if token not in (root / script).read_text(encoding="utf-8-sig"):
                raise AssertionError(script + " lacks package owner token: " + token)

    count = len(re.findall(r'\bCase\("([^"\n]+)"',
        (root / "tests/KingmakerGunslinger.DomainTests/Program.cs")
        .read_text(encoding="utf-8-sig")))
    if count < 2590:
        raise AssertionError("Current deterministic test registration count regressed")
    print("Sprint23 source PASS: 32 append-only identities; 6 pinned art exports; "
          f"{count} registered tests (execution separately required).")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except (AssertionError, ValueError, KeyError, subprocess.CalledProcessError) as error:
        print("Sprint23 source FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
