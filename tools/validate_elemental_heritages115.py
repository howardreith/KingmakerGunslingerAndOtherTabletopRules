#!/usr/bin/env python3
"""Release gate for the 0.0.115 Elemental Heritages candidate."""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_save_load_hotfix113 as baseline

VERSION = "0.0.115"
INFORMATIONAL_VERSION = "0.0.115-elemental-heritages"
PACKAGE = "KingmakerGunslinger-0.0.115-local-runtime.zip"
PACKAGE_SUFFIX = "elemental-heritages"
DETERMINISTIC_TEST_COUNT = 1408
STATIC_KEY = "elementalHeritages115"
HERITAGE_GUID_PREFIX = "e115e1e0a17a4aceb001"
MANIFEST_TOTAL = 1759
MANIFEST_ACTIVE = 1757
MANIFEST_RESERVED = 2
ELEMENTAL_TOTAL = 122
ELEMENTAL_ACTIVE = 121
ELEMENTAL_RACE_COUNT = 4


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.115 release file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks 0.0.115 token(s): {missing}")
    return text


def validate(root: Path) -> None:
    # Retain the accepted 0.0.113 safety contract under the new release identity.
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.validate(root)

    manifest = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))
    entries = manifest.get("entries", [])
    active = [entry for entry in entries if entry.get("status") == "active"]
    reserved = [entry for entry in entries if entry.get("status") == "reserved"]
    elemental = [entry for entry in entries
        if entry.get("symbol", "").startswith("KMG.ElementalRaces.")]
    elemental_active = [entry for entry in elemental
        if entry.get("status") == "active"]
    races = [entry for entry in elemental_active
        if entry.get("plannedType") == "BlueprintRace"]
    heritage = [entry for entry in entries
        if entry.get("guid", "").startswith(HERITAGE_GUID_PREFIX)]
    if (len(entries), len(active), len(reserved)) != (
            MANIFEST_TOTAL, MANIFEST_ACTIVE, MANIFEST_RESERVED):
        raise AssertionError("Authoritative blueprint manifest arithmetic drifted")
    if (len(elemental), len(elemental_active), len(races)) != (
            ELEMENTAL_TOTAL, ELEMENTAL_ACTIVE, ELEMENTAL_RACE_COUNT):
        raise AssertionError("Elemental identity inventory drifted")
    if len(heritage) != 53 or any(entry.get("status") != "active"
            for entry in heritage):
        raise AssertionError("Heritage identity inventory drifted")
    type_counts = {}
    for entry in heritage:
        key = entry.get("plannedType")
        type_counts[key] = type_counts.get(key, 0) + 1
    expected_types = {
        "BlueprintFeatureSelection": 4,
        "BlueprintFeature": 28,
        "BlueprintAbilityResource": 8,
        "BlueprintAbility": 12,
        "BlueprintWeaponEnchantment": 1,
    }
    if type_counts != expected_types:
        raise AssertionError(f"Heritage identity types drifted: {type_counts}")
    expected_guids = {
        HERITAGE_GUID_PREFIX + f"{index:012d}" for index in range(1, 54)}
    if {entry.get("guid") for entry in heritage} != expected_guids:
        raise AssertionError("Stable heritage GUID namespace drifted")
    all_guids = [entry.get("guid", "") for entry in entries]
    if len(set(all_guids)) != len(all_guids) or any(
            re.fullmatch(r"[0-9a-f]{32}", guid) is None for guid in all_guids):
        raise AssertionError("Blueprint manifest GUIDs are not unique lower hex")

    identity = require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalRaceIdentityCatalog.cs", "LegacyMechanicIdentityCount = 24",
        "HeritageIdentityCount = 53", "HeritageSymbols",
        "SelectionSymbol", "MarkerSymbol", "AffinityFeatureSymbol",
        "SlaFeatureSymbol", "SlaResourceSymbol", "SlaAbilitySymbol",
        "UnerringWeaponEnchantment", "ChillTouchDeliveryAbility",
        "ShockingGraspDeliveryAbility")
    if "Guid.NewGuid" in identity:
        raise AssertionError("Identity catalog dynamically generates GUIDs")

    policy = require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalHeritagePolicy.cs", "HeritageCount = 12",
        "ChoicesPerRace = 3", "General Ifrit", "Lavasoul", "Sunsoul",
        "General Oread", "Gemsoul", "Ironsoul", "General Sylph",
        "Smokesoul", "Stormsoul", "General Undine", "Mistsoul", "Rimesoul")
    for token in ("Firebelly", "Flare Burst", "Color Spray",
                  "Unerring Weapon", "Expeditious Retreat",
                  "Shocking Grasp", "Blur", "Chill Touch"):
        if token not in policy:
            raise AssertionError(f"Heritage policy lacks SLA: {token}")

    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalHeritageBlueprintFactory.cs", "CreateSelection",
        "FeatureGroup.None", "Obligatory = true", "IgnorePrerequisites = false",
        "CreateMarker", "definition.IsGeneral", "CreateAffinity")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalHeritageAbilityFactory.cs", "AbilityType.SpellLike",
        "CloneNativeSpell", "RegisterUnerring", "RegisterChillTouch",
        "NativeShockingGraspDeliveryGuid", "fallbackIcon",
        "delivery.Parent = ability")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalHeritageRuntime.cs", "Resolve(",
        "Reconcile", "RememberCurrent", "SetAmount",
        "owner.Resources.Restore",
        "AddFact", "RemoveFact", "ReferenceEquals",
        "ElementalHeritageSelectionController")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalHeritageRuleComponents.cs", "RuleAttackRoll",
        "WeaponEnchantmentLogic", "RuleDealDamage", "RuleSavingThrow",
        "UnerringWeaponEnchantment", "ChillTouch",
        "ChillTouchUndeadPanicRounds")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalHeritageSlaPolicy.cs", "UnerringConfirmationBonus",
        "ChillTouchCount", "ChillTouchUndeadPanicRounds")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalRaceBlueprintFactory.cs", "ElementalHeritageBlueprintFactory.Register",
        "keen, resistance, affinity, sla, heritageSelection",
        "race.Features = features.ToArray", "ElementalHeritageRaceController")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "ElementalHeritageBlueprintScenario.cs", "HeritageIdentityCount",
        "BlueprintsByAssetId", "CharacterRaces", "SaveStateTouched = false")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "ElementalHeritageMechanicsScenario.cs",
        "ElementalHeritageRuntime.Reconcile", "FeatureSelectionState",
        "PersistantResources", "AbilityExecutionContext",
        "SaveStateTouched = false",
        "ContractResolver = new DefaultContractResolver()",
        "PreserveReferencesHandling.None", "ReferenceLoopHandling.Error")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "ElementalHeritageSlaScenario.cs",
        "UnitUseAbility", "AbilityExecutionProcess", "ItemEnchantment",
        "RuleAttackRoll", "TouchSpellsController",
        "UnitPartElementalChillTouch", "SaveStateTouched = false",
        "ContractResolver = new DefaultContractResolver()",
        "PreserveReferencesHandling.None", "ReferenceLoopHandling.Error")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs",
        "disposable-elemental-heritage-mechanics")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs",
        "disposable-elemental-heritage-slas")
    require_tokens(root / "scripts/RuntimeAutomation.Common.ps1",
        "'disposable-elemental-heritage-mechanics' = [pscustomobject]")
    require_tokens(root / "scripts/RuntimeAutomation.Common.ps1",
        "'disposable-elemental-heritage-slas' = [pscustomobject]")

    program = require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "Program.cs", 'Case("elemental-heritages.catalog"',
        'Case("elemental-heritages.stats"',
        'Case("elemental-heritages.legacy-resolution"',
        'Case("elemental-heritages.sla-adaptations"',
        'Case("elemental-heritages.identities"',
        'Case("elemental-heritages.architecture"')
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic test cases")
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "ElementalHeritagePolicyTests.cs",
        "CatalogHasExactChoiceAndProviderInventory",
        "AbilityModifiersAndOverlayDeltasAreExact",
        "LegacyAbsenceResolvesToGeneralAndInvalidStatesFailClosed",
        "NativeDonorsAndProjectOwnedImplementationsAreExact")

    profiles = json.loads((root / "compatibility/profiles.json").read_text(
        encoding="utf-8")).get("profiles", [])
    if not profiles or any(profile.get("requiredGunslingerPackage") != PACKAGE
            for profile in profiles):
        raise AssertionError("Compatibility profiles do not target 0.0.115")
    required_profiles = {
        "gunslinger-only", "gunslinger-call-of-the-wild",
        "gunslinger-races-unleashed",
        "gunslinger-tweak-or-treat",
        "gunslinger-call-of-the-wild-favored-class",
        "gunslinger-high-risk-combined-favored-class",
    }
    by_id = {profile.get("id"): profile for profile in profiles}
    if not required_profiles.issubset(by_id):
        raise AssertionError("Required Release A compatibility profiles are absent")
    compatibility_pending = any(by_id[key].get("disposition") !=
        "RUNTIME-QUALIFIED-EXACT" for key in required_profiles)
    schema = (root / "compatibility/profiles.schema.json").read_text(
        encoding="utf-8")
    if PACKAGE not in schema:
        raise AssertionError("Compatibility profile schema package drifted")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.115.md",
        "Kingmaker Gunslinger 0.0.115", "elemental-heritages",
        "Legacy 0.0.114", "Release A qualification PASS")
    require_tokens(root / "README.md", INFORMATIONAL_VERSION,
        "Lavasoul", "Ironsoul", "Stormsoul", "Rimesoul",
        "historical evidence")
    require_tokens(root / "INSTALLATION-COMPATIBILITY.md",
        "KingmakerGunslinger-0.0.115-elemental-heritages.zip",
        "RaceId.Aasimar", "Visual Adjustments", "uninstall")
    require_tokens(root / "ELEMENTAL-RACES-DEVIATION-MATRIX.md",
        "Lavasoul Burning Sands", "Sunsoul Sun Metal",
        "Smokesoul Blurred Movement", "Mistsoul Obscuring Mist",
        "Ironsoul Unerring Weapon", "Rimesoul Chill Touch")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "legacyMechanicIdentityCount": 24,
        "heritageIdentityCount": 53,
        "activeElementalIdentityCount": ELEMENTAL_ACTIVE,
        "reservedDiagnosticIdentityCount": 1,
        "raceCount": 4,
        "heritageCount": 12,
        "heritageSelectionCount": 4,
        "choicesPerRace": 3,
        "alternateSlaResourceCount": 8,
        "generalProviderGuidsPreserved": True,
        "legacyMarkerAbsenceMeansGeneral": True,
        "newTopLevelRaceCount": 0,
        "nativeRaceEnumAdded": False,
        "moduleSchemaChanged": False,
        "unconditionalIdentityRegistration": True,
        "moduleControlledPublication": True,
        "resourceAmountReconciliation": True,
        "dynamicSaveBearingGuidGeneration": False,
        "runtimeQualificationPending": False,
        "compatibilityRuntimeQualificationPending": compatibility_pending,
        "persistenceQualificationPending": False,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"{VERSION} static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Elemental Heritages {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Elemental Heritages {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
