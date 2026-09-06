#!/usr/bin/env python3
"""Release gate for the in-progress 0.0.117 Elemental Traits candidate."""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_elemental_feats116 as baseline

VERSION = "0.0.117"
INFORMATIONAL_VERSION = "0.0.117-elemental-traits"
PACKAGE = "KingmakerGunslinger-0.0.117-local-runtime.zip"
PACKAGE_SUFFIX = "elemental-traits"
DETERMINISTIC_TEST_COUNT = 1427
STATIC_KEY = "elementalTraits117"
TRAIT_GUID_PREFIX = "e117e1e0a17a4acec001"
MANIFEST_TOTAL = 1860
MANIFEST_ACTIVE = 1858
MANIFEST_RESERVED = 2
ELEMENTAL_TOTAL = 223
ELEMENTAL_ACTIVE = 222


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.117 release file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks 0.0.117 token(s): {missing}")
    return text


def validate(root: Path) -> None:
    # Retain every independently qualified Release B contract under the new
    # candidate identity before evaluating Release C's additive framework.
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    baseline.MANIFEST_RESERVED = MANIFEST_RESERVED
    baseline.ELEMENTAL_TOTAL = ELEMENTAL_TOTAL
    baseline.ELEMENTAL_ACTIVE = ELEMENTAL_ACTIVE
    baseline.validate(root)

    manifest = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))
    traits = [entry for entry in manifest.get("entries", [])
        if entry.get("guid", "").startswith(TRAIT_GUID_PREFIX)]
    if len(traits) != 76 or any(entry.get("status") != "active"
            for entry in traits):
        raise AssertionError("Elemental alternate-trait identity inventory drifted")
    type_counts = {}
    for entry in traits:
        key = entry.get("plannedType")
        type_counts[key] = type_counts.get(key, 0) + 1
    if type_counts != {
            "BlueprintFeatureSelection": 10,
            "BlueprintFeature": 52, "BlueprintBuff": 4,
            "BlueprintAbilityResource": 4, "BlueprintAbility": 5,
            "BlueprintActivatableAbility": 1}:
        raise AssertionError(
            f"Elemental alternate-trait identity types drifted: {type_counts}")
    expected_guids = {
        TRAIT_GUID_PREFIX + f"{index:012d}" for index in range(1, 77)
    }
    if {entry.get("guid") for entry in traits} != expected_guids:
        raise AssertionError("Stable alternate-trait GUID namespace drifted")
    if any(re.fullmatch(r"[0-9a-f]{32}", entry.get("guid", "")) is None
            for entry in traits):
        raise AssertionError("Alternate-trait GUID format drifted")

    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalRaceIdentityCatalog.cs", "TraitFrameworkIdentityCount = 62",
        "TraitSymbols()", "RaceBlueprintIdentityCount")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalAlternateTraitPolicy.cs", "TraitCount = 21",
        "SelectionCount = 10", "TransitionMarkers", "ResolveMarkers",
        "EnergyResistance", "ElementalAffinity", "RacialSpellLikeAbility")
    factory = require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalAlternateTraitBlueprintFactory.cs", "CreateProvider",
        "CreateMarker", "CreateRetainMarker", "CreateSelection",
        "PrerequisiteNoFeature", "Obligatory = true",
        "IgnorePrerequisites = false")
    if "Guid.NewGuid" in factory or "RemoveFeatureOnApply" in factory:
        raise AssertionError("Alternate-trait graph uses an unsafe identity or ordering replacement")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalAlternateTraitBlueprintSet.cs", "ReferenceEquals(Marker, Provider)",
        "OwnedProviders()", "RegisteredCount")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalHeritageRuntime.cs", "ElementalAlternateTraitPolicy.TransitionMarkers",
        "ElementalAlternateTraitPolicy.Resolve", "DesiredFactsArePresent",
        "ProviderFactsAreExact", "InactiveAbilitiesAreAbsent",
        "TryRemove(owner, blueprint.Resistance)",
        "TraitFrameworkIdentityCount")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalRaceBlueprintFactory.cs",
        "ElementalAlternateTraitBlueprintFactory.Register(",
        "features.AddRange(alternateTraits.Selections()")
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/Program.cs",
        'Case("elemental-traits.catalog-slots"',
        'Case("elemental-traits.provider-matrix"',
        'Case("elemental-traits.ordering-reconstruction"',
        'Case("elemental-traits.framework-identities"',
        'Case("elemental-traits.blueprint-architecture"')
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "ElementalAlternateTraitPolicyTests.cs", "expectedLegalCounts",
        "Permutations", "TransitionMarkers", "MarkerSymbols()")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "ElementalAlternateTraitFrameworkScenario.cs",
        "TraitFrameworkIdentityCount", "BlueprintsByAssetId.TryGetValue",
        "PrerequisiteNoFeature", "SaveStateTouched = false",
        "PreserveReferencesHandling.None", "ReferenceLoopHandling.Error")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs",
        "observe-elemental-alternate-trait-framework")
    require_tokens(root / "scripts/RuntimeAutomation.Common.ps1",
        "'observe-elemental-alternate-trait-framework' = [pscustomobject]")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.117.md",
        "Kingmaker Gunslinger 0.0.117", "elemental-traits",
        "Release C remains in progress")
    require_tokens(root / "README.md", INFORMATIONAL_VERSION,
        "alternate racial traits", "Release C remains in progress")
    require_tokens(root / "INSTALLATION-COMPATIBILITY.md",
        "KingmakerGunslinger-0.0.117-elemental-traits.zip")
    require_tokens(root / "ELEMENTAL-RACES-DEVIATION-MATRIX.md",
        "Replacement-slot framework", "RELEASE C IN PROGRESS")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "traitFrameworkIdentityCount": 62,
        "alternateTraitCount": 21,
        "traitSelectionCount": 10,
        "activeElementalIdentityCount": ELEMENTAL_ACTIVE,
        "replacementSlotCount": 3,
        "separateMarkerAndProviderFacts": True,
        "moduleSchemaChanged": False,
        "unconditionalIdentityRegistration": True,
        "dynamicSaveBearingGuidGeneration": False,
        "traitMechanicsImplementationPending": True,
        "traitRuntimeQualificationPending": True,
        "traitPersistenceQualificationPending": True,
        "traitCompatibilityQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.117 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Elemental Traits {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Elemental Traits {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
