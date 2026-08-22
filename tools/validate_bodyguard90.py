#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_weapon_presentation89 as baseline

VERSION = "0.0.90"
INFORMATIONAL_VERSION = "0.0.90-bodyguard-in-harms-way"
PACKAGE = "KingmakerGunslinger-0.0.90-local-runtime.zip"
STATIC_KEY = "bodyguard90"
DETERMINISTIC_TEST_COUNT = 1190

EXPECTED_IDENTITIES = {
    "KMG.Feats.Bodyguard":
        ("b2baa3384b4d4328848cc07933b513be", "BlueprintFeature"),
    "KMG.Feats.UseBodyguard":
        ("ac31a9d5d34140978b7e778dc8d1e226", "BlueprintActivatableAbility"),
    "KMG.Feats.BodyguardModeMarker":
        ("a78147a3655f429883ad88e761ff9438", "BlueprintBuff"),
    "KMG.Feats.InHarmsWay":
        ("e481f30c8b6940e1b596e121443aa01e", "BlueprintFeature"),
    "KMG.Feats.UseInHarmsWay":
        ("ca1e74f0e60747209a8b7cf3737243ea", "BlueprintActivatableAbility"),
    "KMG.Feats.InHarmsWayModeMarker":
        ("57603d0b215e4ac6862bcdf9b5583568", "BlueprintBuff"),
}


def require_tokens(path: Path, *tokens: str) -> None:
    if not path.is_file():
        raise AssertionError(f"Bodyguard gate file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks Bodyguard contract token(s): {missing}")


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.validate(root)

    required = (
        "docs/investigations/bodyguard-in-harms-way.md",
        "src/KingmakerGunslinger/Blueprints/BodyguardFeatBlueprints.cs",
        "src/KingmakerGunslinger/Blueprints/BodyguardModeBlueprints.cs",
        "src/KingmakerGunslinger/Blueprints/BodyguardFeatCatalogPublication.cs",
        "src/KingmakerGunslinger/BodyguardFeats/BodyguardRuntime.cs",
        "src/KingmakerGunslinger/BodyguardFeats/InHarmsWayDeliveryAccess.cs",
        "src/KingmakerGunslinger/RuntimeTesting/BodyguardNativeContractObserver.cs",
        "src/KingmakerGunslinger/RuntimeTesting/BodyguardCombatScenario.cs",
        "scripts/Invoke-BodyguardRuntimeQualification.ps1",
        "tests/KingmakerGunslinger.DomainTests/BodyguardBlueprintContractTests.cs",
        "tests/KingmakerGunslinger.DomainTests/BodyguardPolicyTests.cs",
        "tests/KingmakerGunslinger.DomainTests/BodyguardRuntimeContractTests.cs",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Bodyguard gate file missing: {relative}")

    manifest = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))
    entries = manifest["entries"]
    if len(entries) != 1622:
        raise AssertionError(f"{VERSION} blueprint ledger must contain 1622 identities")
    if sum(entry.get("status") == "active" for entry in entries) != 1621:
        raise AssertionError(f"{VERSION} blueprint ledger must contain 1621 active identities")
    by_symbol = {entry["symbol"]: entry for entry in entries}
    if len(by_symbol) != len(entries):
        raise AssertionError("Blueprint manifest contains duplicate symbols")
    guids = [entry["guid"] for entry in entries]
    if len(set(guids)) != len(guids):
        raise AssertionError("Blueprint manifest contains duplicate GUIDs")
    for symbol, (guid, planned_type) in EXPECTED_IDENTITIES.items():
        entry = by_symbol.get(symbol)
        if entry is None or entry.get("guid") != guid or \
                entry.get("plannedType") != planned_type or \
                entry.get("status") != "active" or \
                re.fullmatch(r"[0-9a-f]{32}", guid) is None:
            raise AssertionError(f"Bodyguard blueprint identity mismatch: {symbol}")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    bodyguard = static.get(STATIC_KEY, {})
    expected_static = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "featureModuleCount": 9,
        "featureModuleSchemaVersion": 8,
        "featureModuleExhaustiveCount": 512,
        "featureModuleBoundaryCount": 20,
        "projectBlueprintCount": 6,
        "nativeCombatReflexesGuid": "0f8939ae6f220984e8fb568abbdfba95",
        "fullDeliveryRedirectionRequired": True,
    }
    for key, value in expected_static.items():
        if bodyguard.get(key) != value:
            raise AssertionError(f"Bodyguard static validation mismatch: {key}")

    require_tokens(root / "src/KingmakerGunslinger/FeatureModules/FeatureModuleSettingsStore.cs",
        "CurrentSchemaVersion = 8", "BodyguardFeatsId")
    require_tokens(root / "src/KingmakerGunslinger/FeatureModules/FeatureModuleConfiguration.cs",
        'BodyguardFeatsId = "bodyguard-feats"', "BodyguardFeats ? 256 : 0",
        '";bodyguard-feats=" + BodyguardFeats')
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/BodyguardFeatBlueprints.cs",
        '"0f8939ae6f220984e8fb568abbdfba95"',
        "FeatureGroup.Feat", "FeatureGroup.CombatFeat")
    require_tokens(root / "src/KingmakerGunslinger/BodyguardFeats/BodyguardActionEconomyAccess.cs",
        "AttackOfOpportunityCount", "AttackOfOpportunity(attacker, true)",
        "HasSwiftAction", "Cooldown.SwiftAction")
    require_tokens(root / "src/KingmakerGunslinger/BodyguardFeats/BodyguardThreatAccess.cs",
        "UnitEngagementExtension.IsReach", "RuleCalculateAttackBonus")
    require_tokens(root / "src/KingmakerGunslinger/BodyguardFeats/InHarmsWayDeliveryAccess.cs",
        "RulebookTargetEvent", "AbilityDeliveryTarget", "ApplyEffect",
        "ContractAvailable")
    require_tokens(root / "src/KingmakerGunslinger/BodyguardFeats/BodyguardRuntime.cs",
        "BeforeAttackRoll", "AfterAttackRoll", "AfterCalculateArmorClass",
        "RuleEventCompleted", "AbilityAttackContextDisposed", "RuleRollD20")
    require_tokens(root / "scripts/RuntimeAutomation.Common.ps1",
        "observe-bodyguard-native-contracts", "disposable-bodyguard-feats",
        "disposable-bodyguard-feats-disabled", f"active version {VERSION}")
    require_tokens(root / "scripts/package.ps1",
        "$($info.Id)-$($info.Version)-bodyguard-in-harms-way.zip")

    main_project = (root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj") \
        .read_text(encoding="utf-8")
    test_project = (root / "tests/KingmakerGunslinger.DomainTests/"
        "KingmakerGunslinger.DomainTests.csproj").read_text(encoding="utf-8")
    program = (root / "tests/KingmakerGunslinger.DomainTests/Program.cs") \
        .read_text(encoding="utf-8")
    for source in ("BodyguardRuntime.cs", "InHarmsWayDeliveryAccess.cs",
                   "BodyguardCombatScenario.cs", "BodyguardNativeContractObserver.cs"):
        if source not in main_project:
            raise AssertionError(f"Main project compile list lacks {source}")
    for source in ("BodyguardBlueprintContractTests.cs", "BodyguardPolicyTests.cs",
                   "BodyguardRuntimeContractTests.cs"):
        if source not in test_project or source[:-3] not in program:
            raise AssertionError(f"Bodyguard test is not compiled and registered: {source}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Bodyguard {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Bodyguard {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
