#!/usr/bin/env python3
"""Release gate for the locally qualified 0.0.116 Elemental Feats candidate."""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_elemental_heritages115 as baseline
import validate_immediate_action95 as immediate_action

VERSION = "0.0.116"
INFORMATIONAL_VERSION = "0.0.116-elemental-feats"
PACKAGE = "KingmakerGunslinger-0.0.116-local-runtime.zip"
PACKAGE_SUFFIX = "elemental-feats"
DETERMINISTIC_TEST_COUNT = 1408
STATIC_KEY = "elementalFeats116"
FEAT_GUID_PREFIX = "e116e1e0a17a4aceb001"
MANIFEST_TOTAL = 1784
MANIFEST_ACTIVE = 1782
MANIFEST_RESERVED = 2
ELEMENTAL_TOTAL = 147
ELEMENTAL_ACTIVE = 146


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.116 release file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks 0.0.116 token(s): {missing}")
    return text


def validate(root: Path) -> None:
    immediate_action.EXPECTED_LEDGER_ENTRIES_BY_VERSION[VERSION] = \
        MANIFEST_TOTAL
    immediate_action.EXPECTED_ACTIVE_BLUEPRINTS_BY_VERSION[VERSION] = \
        MANIFEST_ACTIVE
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
    baseline.ELEMENTAL_RACE_COUNT = 4
    baseline.validate(root)

    manifest = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))
    entries = manifest.get("entries", [])
    feats = [entry for entry in entries
        if entry.get("guid", "").startswith(FEAT_GUID_PREFIX)]
    if len(feats) != 25 or any(entry.get("status") != "active"
            for entry in feats):
        raise AssertionError("Elemental feat identity inventory drifted")
    type_counts = {}
    for entry in feats:
        key = entry.get("plannedType")
        type_counts[key] = type_counts.get(key, 0) + 1
    expected_types = {
        "BlueprintFeature": 11,
        "BlueprintAbility": 9,
        "BlueprintBuff": 4,
        "BlueprintWeaponEnchantment": 1,
    }
    if type_counts != expected_types:
        raise AssertionError(f"Elemental feat identity types drifted: {type_counts}")
    expected_guids = {
        FEAT_GUID_PREFIX + f"{index:012d}" for index in range(1, 26)}
    if {entry.get("guid") for entry in feats} != expected_guids:
        raise AssertionError("Stable elemental feat GUID namespace drifted")
    if any(re.fullmatch(r"[0-9a-f]{32}", entry.get("guid", "")) is None
            for entry in feats):
        raise AssertionError("Elemental feat GUID format drifted")

    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalRaceIdentityCatalog.cs", "FeatIdentityCount = 25",
        "RaceBlueprintIdentityCount", "FeatSymbols()",
        "ElementalStrikeFeat", "TritonPortalAbility")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalFeatBlueprintSet.cs", "ElementalFeatPolicy.FeatCount",
        "FeatIdentityCount", "AllFeats()", "CombatFeats()")
    factory = require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalFeatBlueprintFactory.cs", "ElementalFeatPolicy.Ordered()",
        "FeatureGroup.Feat", "FeatureGroup.CombatFeat",
        "PrerequisiteCharacterLevel", "races.Undine.SlaFeature",
        "SetIsFullRoundAction(true)")
    if "RaceId.Aasimar" in factory or "Guid.NewGuid" in factory:
        raise AssertionError("Elemental feat factory uses an unsafe identity predicate")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalFeatPublication.cs", "set.AllFeats()",
        "set.CombatFeats()", 'role + ".Features"', "selection.Features",
        'role + ".AllFeatures"', "selection.AllFeatures",
        "basicTx.Rollback()", "if (!moduleActive)")
    require_tokens(root / "src/KingmakerGunslinger/FeatureModules/"
        "FeatureModulePublicationPlan.cs",
        "ElementalRaceFeats = active.ElementalRaces")
    require_tokens(root / "src/KingmakerGunslinger/Bootstrap/"
        "BlueprintBootstrap.cs", "ElementalFeatBlueprintFactory.Register(",
        "publicationPlan.ElementalRaceFeats",
        "elementalFeatPublication.Rollback()")
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "ElementalRaceProductionTests.cs", "FeatManifestInventoryIsExact",
        "FeatRegistrationAndPublicationAreSaveSafe")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.116.md",
        "Kingmaker Gunslinger 0.0.116", "elemental-feats",
        "Release B is a local PASS")
    require_tokens(root / "docs/ELEMENTAL-RACES-0.0.116-QUALIFICATION.md",
        "Status: **LOCAL PASS**", "1,408/1,408", "73/73", "31/31",
        "359/359", "12 ON/OFF transactions",
        "e5b8f77e77fe9d6bf56c43a2371304b631b8fd65e410c7a931abe27adf8ba032")
    require_tokens(root / "README.md", INFORMATIONAL_VERSION,
        "Elemental Strike", "Hydraulic Maneuver", "Triton Portal",
        "Release B passes locally")
    require_tokens(root / "INSTALLATION-COMPATIBILITY.md",
        "KingmakerGunslinger-0.0.116-elemental-feats.zip",
        "The Release B matrix passed 31 guarded Steam processes")
    require_tokens(root / "ELEMENTAL-RACES-DEVIATION-MATRIX.md",
        "Dirty Trick (dazzle)", "Triton Portal", "Wings of Air",
        "RELEASE B LOCAL PASS")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "featIdentityCount": 25,
        "activeElementalIdentityCount": 146,
        "elementalFeatCount": 11,
        "combatFeatCount": 4,
        "moduleSchemaChanged": False,
        "unconditionalIdentityRegistration": True,
        "moduleControlledPublication": True,
        "exactRacePrerequisites": True,
        "dynamicSaveBearingGuidGeneration": False,
        "mechanicsImplementationPending": False,
        "featRuntimeQualificationPending": False,
        "featPersistenceQualificationPending": False,
        "featCompatibilityQualificationPending": False,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.116 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Elemental Feats {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Elemental Feats {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
