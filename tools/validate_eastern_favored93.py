#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_helpful92 as baseline

VERSION = "0.0.93"
INFORMATIONAL_VERSION = "0.0.93-eastern-favored-compatibility"
PACKAGE = "KingmakerGunslinger-0.0.93-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1211
STATIC_KEY = "easternFavored93"

HEIRLOOM_IDENTITIES = {
    "KMG.Traits.HeirloomWeapon.Nodachi.Selection": (
        "5ae9f898e45846d19d3802caf91e06b6", "BlueprintFeatureSelection"),
    "KMG.Traits.HeirloomWeapon.Nodachi.Proficiency": (
        "af205733f7fe49838edb37cdf1b90cbb", "BlueprintFeature"),
    "KMG.Traits.HeirloomWeapon.Nodachi.AttackOfOpportunity": (
        "4caf60ed8b264701a3965288a65eebc2", "BlueprintFeature"),
    "KMG.Traits.HeirloomWeapon.Nodachi.CombatManeuver": (
        "e17fafa6f75641f8a2e3fe4b6f71da78", "BlueprintFeature"),
    "KMG.Traits.HeirloomWeapon.Nodachi.CombatManeuverBonus": (
        "1a7a5d985fe740cc8442f04f0fe814d8", "BlueprintFeature"),
}


def require_tokens(path: Path, *tokens: str) -> None:
    if not path.is_file():
        raise AssertionError(f"Eastern/Favored gate file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks Eastern/Favored contract token(s): {missing}")


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.EXPECTED_LEDGER_ENTRIES = 1628
    baseline.EXPECTED_ACTIVE_BLUEPRINTS = 1627
    baseline.PROJECT_BLUEPRINT_COUNT = 12
    baseline.EXPECTED_IDENTITIES.update(HEIRLOOM_IDENTITIES)
    baseline.FAVORED_AVAILABILITY_DISPOSITION = (
        "NOT-TESTED", "RUNTIME-QUALIFIED-EXACT")
    baseline.FAVORED_PROFILE_DISPOSITION = (
        "NOT-TESTED", "RUNTIME-QUALIFIED-EXACT")
    baseline.FAVORED_PROFILE_RUNTIME_LOADABLE = True
    baseline.validate(root)

    required = (
        "docs/investigations/eastern-weapons-favored-class-tweak-or-treat.md",
        "src/KingmakerGunslinger/EasternWeapons/EasternWeaponMartialPublicationPolicy.cs",
        "src/KingmakerGunslinger/EasternWeapons/EasternWeaponMartialPublication.cs",
        "src/KingmakerGunslinger/EasternWeapons/EasternWeaponLatePublicationCoordinator.cs",
        "src/KingmakerGunslinger/EasternWeapons/TweakOrTreatHeirloomResolver.cs",
        "src/KingmakerGunslinger/EasternWeapons/HeirloomNodachiEffects.cs",
        "src/KingmakerGunslinger/Blueprints/HeirloomNodachiBlueprints.cs",
        "tests/KingmakerGunslinger.DomainTests/EasternFavoredCompatibilityTests.cs",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Eastern/Favored gate file missing: {relative}")

    require_tokens(root / required[0], "4934986", "0x004b4d4a",
        "dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c",
        "a518324e15632aba46d6c467b156a31e9afd282e9827dee3e79ad14673852b92",
        "first UMM update", "Heirloom Weapon: Nodachi")
    require_tokens(root / required[1], "NodachiCategoryValue = 4934986",
        "AppendNodachiExactlyOnce", "IsBroadGrant")
    require_tokens(root / required[2],
        "UnityEngine.Object.Instantiate(grant)",
        "EasternWeaponProficiencyRuntime.Configure(facts)",
        "entry.Key.ComponentsArray = entry.Value", "Rollback()")
    require_tokens(root / required[3], "FirstUpdate",
        "first-update-after-load-dictionary", "early != 0",
        "favored-equipment-traits-heirloom-nodachi")
    require_tokens(root / required[4], "TweakOrTreat.HeirloomWeapon",
        "ZFavoredClass.NewMechanics.PrerequisiteRace",
        "typeof(PrerequisiteFeature)")
    require_tokens(root / required[5],
        "evt.RuleAttackWithWeapon.IsAttackOfOpportunity",
        "Owner.AddFact(Feature", "Owner.RemoveFact")
    require_tokens(root / required[6], "FeatureGroup.Trait",
        "AddStartingEquipment", "PrerequisiteNotProficient",
        "AllFeatures.Length != 3")

    early = (root / "src/KingmakerGunslinger/EasternWeapons/"
        "EasternWeaponSelectorPublication.cs").read_text(encoding="utf-8")
    for prohibited in ("PublishMartial", "EasternWeaponProficiencyRuntime.Configure"):
        if prohibited in early:
            raise AssertionError(
                f"Early Eastern selector still contains {prohibited}")

    project = (root / "src/KingmakerGunslinger/"
        "KingmakerGunslinger.csproj").read_text(encoding="utf-8")
    tests = (root / "tests/KingmakerGunslinger.DomainTests/"
        "KingmakerGunslinger.DomainTests.csproj").read_text(encoding="utf-8")
    runner = (root / "tests/KingmakerGunslinger.DomainTests/"
        "Program.cs").read_text(encoding="utf-8")
    for path in required[1:7]:
        if Path(path).name not in project:
            raise AssertionError(f"Production file is not compiled: {path}")
    if Path(required[7]).name not in tests:
        raise AssertionError("Eastern/Favored tests are not compiled")
    if "eastern-favored." not in runner:
        raise AssertionError("Eastern/Favored tests are not registered")

    profiles = json.loads((root / "compatibility/profiles.json")
        .read_text(encoding="utf-8"))["profiles"]
    by_id = {entry["id"]: entry for entry in profiles}
    expected = {
        "gunslinger-call-of-the-wild-favored-class":
            ["call-of-the-wild", "favored-class"],
        "gunslinger-call-of-the-wild-favored-class-traits-disabled":
            ["call-of-the-wild", "favored-class"],
        "gunslinger-high-risk-combined-favored-class":
            ["call-of-the-wild", "favored-class", "tweak-or-treat",
             "races-unleashed"],
    }
    for profile_id, mod_keys in expected.items():
        profile = by_id.get(profile_id)
        if not profile or profile["modKeys"] != mod_keys or \
                not profile["runtimeLoadableRequired"] or \
                profile["requiredGunslingerPackage"] != PACKAGE:
            raise AssertionError(f"Compatibility profile changed: {profile_id}")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    contract = static.get(STATIC_KEY, {})
    expected_static = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "projectBlueprintCount": 12,
        "lateMartialPublicationRequired": True,
        "favoredClassTraitsMustComplete": True,
        "tweakOrTreatHeirloomMustComplete": True,
        "runtimeQualificationPending": False,
    }
    for key, value in expected_static.items():
        if contract.get(key) != value:
            raise AssertionError(
                f"Eastern/Favored static validation mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Eastern/Favored Compatibility {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Eastern/Favored Compatibility {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
