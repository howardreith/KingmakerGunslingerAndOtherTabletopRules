#!/usr/bin/env python3
"""Release gate for the final 0.0.114 Elemental Races release."""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_save_load_hotfix113 as baseline

VERSION = "0.0.114"
INFORMATIONAL_VERSION = "0.0.114-elemental-races"
PACKAGE = "KingmakerGunslinger-0.0.114-local-runtime.zip"
PACKAGE_SUFFIX = "elemental-races"
DETERMINISTIC_TEST_COUNT = 1390
STATIC_KEY = "elementalRaces114"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.114 release file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.114 release token(s): {missing}")
    return text


def validate(root: Path) -> None:
    # Retain every accepted 0.0.113 source invariant under the new identity.
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
    elemental = [entry for entry in entries
        if entry.get("symbol", "").startswith("KMG.ElementalRaces.")]
    active = [entry for entry in entries if entry.get("status") == "active"]
    reserved = [entry for entry in entries if entry.get("status") == "reserved"]
    elemental_active = [entry for entry in elemental
        if entry.get("status") == "active"]
    elemental_reserved = [entry for entry in elemental
        if entry.get("status") == "reserved"]
    elemental_races = [entry for entry in elemental_active
        if entry.get("plannedType") == "BlueprintRace"]
    if (len(entries), len(active), len(reserved)) != (1706, 1704, 2):
        raise AssertionError("Authoritative blueprint manifest arithmetic drifted")
    if (len(elemental), len(elemental_active), len(elemental_reserved),
            len(elemental_races)) != (69, 68, 1, 4):
        raise AssertionError("Elemental identity inventory drifted")
    guids = [entry.get("guid", "") for entry in entries]
    if len(set(guids)) != len(guids) or any(
            re.fullmatch(r"[0-9a-f]{32}", guid) is None for guid in guids):
        raise AssertionError("Blueprint manifest GUIDs are not unique lower hex")
    expected_races = {
        "KMG.ElementalRaces.Ifrit.Race",
        "KMG.ElementalRaces.Oread.Race",
        "KMG.ElementalRaces.Sylph.Race",
        "KMG.ElementalRaces.Undine.Race",
    }
    if {entry["symbol"] for entry in elemental_races} != expected_races:
        raise AssertionError("The four stable elemental race identities drifted")

    require_tokens(root / "src/KingmakerGunslinger/FeatureModules/"
        "FeatureModuleConfiguration.cs", "internal const int ModuleCount = 11",
        'ElementalRacesId = "elemental-races"', "(ElementalRaces ? 1024 : 0)",
        "true, true, true, true, true); } }")
    require_tokens(root / "src/KingmakerGunslinger/FeatureModules/"
        "FeatureModuleSettingsStore.cs", "CurrentSchemaVersion = 10",
        "ReadDefaultOn", "recovered defaults (all modules ON)")
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "FeatureModuleSettingsTests.cs", "ExhaustiveCount(11)",
        "BoundaryCount(11)", "24 states for eleven modules")

    catalog = require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalRaceCatalog.cs", "RaceCount = 4",
        "ElementalRaceKind.Ifrit", "ElementalRaceKind.Oread",
        "ElementalRaceKind.Sylph", "ElementalRaceKind.Undine",
        "Burning Hands", "Stone Fist", "Feather Step", "Hydraulic Push",
        "DamageEnergyType.Fire", "DamageEnergyType.Acid",
        "DamageEnergyType.Electricity", "DamageEnergyType.Cold")
    if catalog.index("ElementalRaceKind.Ifrit") > catalog.index(
            "ElementalRaceKind.Oread") or catalog.index(
            "ElementalRaceKind.Oread") > catalog.index(
            "ElementalRaceKind.Sylph") or catalog.index(
            "ElementalRaceKind.Sylph") > catalog.index(
            "ElementalRaceKind.Undine"):
        raise AssertionError("Elemental race definition order drifted")

    require_tokens(root / "src/KingmakerGunslinger/Bootstrap/"
        "BlueprintBootstrap.cs", "ElementalRaceBlueprintFactory.Register",
        "ElementalRacePublication.Apply",
        "publicationPlan.ElementalRaceSelectors")
    publication = require_tokens(root / "src/KingmakerGunslinger/"
        "ElementalRaces/ElementalRacePublication.cs",
        "BlueprintRace[] previous = root.Progression.CharacterRaces",
        "previous.Concat(missing).ToArray()", "root.Progression.CharacterRaces = previous",
        "All four distinct elemental race identities are required")
    if "OrderBy" in publication or "display name" in publication.lower():
        raise AssertionError("Elemental race publication may not reorder or name-merge")

    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalRaceIdentityCatalog.cs", "MechanicIdentityCount = 24",
        "ManifestIdentityCount", "AasimarRaceGuid", "SlowAndSteadyGuid")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalRaceRuleComponents.cs", "RuleCalculateAbilityParams",
        "evt.AddBonusDC(1)", "Owner.Progression.CharacterLevel",
        "evt.ReplaceCasterLevel")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/"
        "ElementalRaceAbilityFactory.cs", "AbilityType.SpellLike",
        "ContextActionCombatManeuver", "CombatManeuver.BullRush")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/Visuals/"
        "ElementalRaceVisualCatalog.cs", "RaceVisualBlueprintCount = 4",
        "ResourceIdentityCount = 28", "SkinRampCount = 7")
    require_tokens(root / "src/KingmakerGunslinger/ElementalRaces/Visuals/"
        "ElementalRaceVisualFactory.cs", "TextureFormat.RGB24",
        "FilterMode.Bilinear", "TextureWrapMode.Clamp")

    scenarios = require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs", "observe-elemental-race-blueprints",
        "disposable-elemental-race-mechanics",
        "disposable-elemental-race-slas", "disposable-hydraulic-push",
        "elemental-race-visual-audit", "elemental-race-class-equipment",
        "elemental-race-motion", "elemental-race-persistence-prepare",
        "elemental-race-module-disabled-persistence",
        "elemental-races-races-unleashed-compatibility")
    if scenarios.count("ElementalRace") < 20:
        raise AssertionError("Guarded elemental runtime scenario inventory drifted")
    program = require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "Program.cs", 'Case("elemental-races.identities"',
        'Case("elemental-races.registration-publication"',
        'Case("elemental-races.runtime-hydraulic-push"',
        'Case("elemental-races.persistence"')
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic test cases")

    profiles = json.loads((root / "compatibility/profiles.json").read_text(
        encoding="utf-8")).get("profiles", [])
    if not profiles or any(profile.get("requiredGunslingerPackage") != PACKAGE
            for profile in profiles):
        raise AssertionError("Compatibility profiles do not target 0.0.114")
    required_profile_ids = {
        "gunslinger-only",
        "gunslinger-call-of-the-wild",
        "gunslinger-races-unleashed",
        "gunslinger-call-of-the-wild-races-unleashed",
        "gunslinger-high-risk-combined-favored-class",
    }
    profiles_by_id = {profile.get("id"): profile for profile in profiles}
    if not required_profile_ids.issubset(profiles_by_id):
        raise AssertionError("Required Elemental Races profiles are absent")
    compatibility_pending = any(
        profiles_by_id[profile_id].get("disposition") !=
            "RUNTIME-QUALIFIED-EXACT"
        for profile_id in required_profile_ids)
    schema = (root / "compatibility/profiles.schema.json").read_text(
        encoding="utf-8")
    if PACKAGE not in schema:
        raise AssertionError("Compatibility profile schema package drifted")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.114.md",
        "Kingmaker Gunslinger 0.0.114", PACKAGE_SUFFIX,
        "Elemental Races defaults ON", "explicitly authorized finalization")
    require_tokens(root / "README.md", INFORMATIONAL_VERSION,
        "Elemental Races", "Keen Senses", "Feather Step",
        "Hydraulic Push", "restart")
    require_tokens(root / "INSTALLATION-COMPATIBILITY.md",
        "KingmakerGunslinger-0.0.114-elemental-races.zip",
        "RaceId.Aasimar", "Visual Adjustments", "uninstall")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "paperToggleAuthoritative": "activatable-is-on",
        "paperModeReadOnly": True,
        "paperModePostLoadMutation": False,
        "paperSetterPersistentFactMutation": False,
        "staleMarkerIgnoredMechanically": True,
        "toggleOffLooseReloadPreserved": True,
        "cmiSyntheticToggleProhibited": True,
        "cmiTargetedBridgeIdempotent": True,
        "saveCompatibilityPreserved": True,
        "featureModuleSchema": 10,
        "featureModuleCount": 11,
        "boundaryConfigurationCount": 24,
        "raceCount": 4,
        "elementalManifestEntryCount": 69,
        "activeElementalIdentityCount": 68,
        "reservedDiagnosticIdentityCount": 1,
        "visualBlueprintCount": 16,
        "visualEquipmentProxyCount": 28,
        "defaultEnabled": True,
        "unconditionalRegistration": True,
        "atomicPublication": True,
        "donorRaceId": "Aasimar",
        "saveLoadRepairPreserved": True,
        "compatibilityRuntimeQualificationPending": compatibility_pending,
        "humanVisualReviewRequired": False,
        "ownerReleaseAuthorized": True,
        "individualHumanChecklistEvidenceProvided": False,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.114 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Elemental Races {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Elemental Races {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
