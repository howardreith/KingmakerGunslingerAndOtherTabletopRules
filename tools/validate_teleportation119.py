#!/usr/bin/env python3
"""Release gate for owner-authorized contextual world-map teleportation."""
from __future__ import annotations

import argparse
import hashlib
import importlib
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_elemental_traits117 as baseline

VERSION = "0.0.119"
INFORMATIONAL_VERSION = "0.0.119-contextual-world-map-teleportation"
PACKAGE = "KingmakerGunslinger-0.0.119-local-runtime.zip"
PACKAGE_SUFFIX = "contextual-world-map-teleportation"
DETERMINISTIC_TEST_COUNT = 1550
STATIC_KEY = "teleportation118InheritedContracts"
MANIFEST_TOTAL = 1872
MANIFEST_ACTIVE = 1870
PUBLISHED_117_PREFIX_SHA256 = "3e4f39cba12da23d142ed3e49a3b56e319c7ad894887038ba0a4522c94512713"
SPELL_IDS = {
    "KMG.Spells.Teleport.Ability": "82e3fb1dce1647b58d3b7169c8520af0",
    "KMG.Spells.GreaterTeleport.Ability": "73d19adfe18743e0a2a3a21abf4af5f3",
    "KMG.Spells.WordOfRecall.Ability": "596d85a666204d6ea5c0188e53f4b4de",
}


def require_tokens(path: Path, *tokens: str) -> str:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks release contract(s): {missing}")
    return text


def validate(root: Path) -> None:
    # Re-run the retained published content contracts with current build metadata.
    # Historical evidence objects keep their original counts and acceptance scope.
    for name in ("VERSION", "INFORMATIONAL_VERSION", "PACKAGE", "PACKAGE_SUFFIX",
                 "DETERMINISTIC_TEST_COUNT"):
        setattr(baseline, name, globals()[name])
    for module in ("validate_player_presentation105", "validate_fatigue_authority106",
                   "validate_icon_overhaul107", "validate_icon_polish108",
                   "validate_martial_repair_notifications109",
                   "validate_protection_from_alignment110", "validate_gunslinger_outfit_kitbash111"):
        importlib.import_module(module).STATIC_KEY = STATIC_KEY
    baseline.STATIC_KEY = STATIC_KEY
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    baseline.validate(root)

    entries = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))["entries"]
    digest = hashlib.sha256(json.dumps(entries[:1869], sort_keys=True,
        separators=(",", ":")).encode()).hexdigest()
    if digest != PUBLISHED_117_PREFIX_SHA256:
        raise AssertionError("Published 0.0.117 manifest prefix changed")
    if hashlib.sha256(json.dumps(entries[:1872], sort_keys=True, separators=(",", ":")).encode()).hexdigest() != "af821f3920497c228bfdaa98c226578163a1d6d163eeddf05ca3bee4f4924dee":
        raise AssertionError("Published 0.0.118 blueprint manifest changed")
    spells = entries[1869:1872]
    if {entry["symbol"]: entry["guid"] for entry in spells} != SPELL_IDS or any(
            entry["status"] != "active" or entry["plannedType"] != "BlueprintAbility"
            for entry in spells):
        raise AssertionError("Stable strategic spell identities drifted")

    src = root / "src/KingmakerGunslinger"
    require_tokens(src / "FeatureModules/FeatureModuleConfiguration.cs",
        "ModuleCount = 12", 'TeleportationSpellsId = "teleportation-spells"',
        "TeleportationSpells == other.TeleportationSpells")
    require_tokens(src / "FeatureModules/FeatureModuleSettingsStore.cs",
        "CurrentSchemaVersion = 11")
    require_tokens(src / "Blueprints/TeleportationSpellBlueprints.cs",
        "SpellSchool.Conjuration", "CommandType.Standard",
        "ability.MaterialComponent = new BlueprintAbility.MaterialComponentData()",
        "ability.ActionBarAutoFillIgnored = true", "ability.AvailableMetamagic = 0",
        "ability.CanTargetSelf = false", "Teleport Without Error")
    require_tokens(src / "Blueprints/TeleportationSpellListPublication.cs",
        "WizardListId, 5, spells.Teleport", "WizardListId, 7, spells.GreaterTeleport",
        "ClericListId, 6, spells.WordOfRecall", "DruidListId, 8, spells.WordOfRecall",
        "TravelListId, 5, spells.Teleport", "TravelListId, 7, spells.GreaterTeleport")
    require_tokens(src / "Spells/Teleportation/TeleportCastTransaction.cs",
        "Interlocked.CompareExchange", "RevalidateAndCapture",
        "ObserveExpenditure", "RestoreAndVerifyExactResource", "MaterialEffectStarted")
    require_tokens(src / "Spells/Teleportation/UnitPartTeleportFamiliarity.cs",
        "[JsonProperty]", "private string _state", "private string _explorationBoundary")
    # All production adapters must be explicitly compiled; the domain project
    # separately includes only policies and bounded source-contract fixtures.
    project = (src / "KingmakerGunslinger.csproj").read_text(encoding="utf-8")
    for path in (src / "Spells/Teleportation").glob("*.cs"):
        if path.name not in project:
            raise AssertionError(f"Uncompiled Teleportation source: {path.name}")
    require_tokens(root / "scripts/Publish-Release.ps1",
        rf"docs\RELEASE-NOTES-{VERSION}.md", "AllowNonDefaultReleaseBranch",
        "ConfirmReleaseReady")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.119.md",
        "0.0.119-contextual-world-map-teleportation", "real spellbook", "owner authorized",
        "remain unqualified", "1,550", "0.0.118")
    require_tokens(src / "Spells/Teleportation/TeleportDestinationPolicy.cs",
        "EvaluateSafety", "p.OrdinaryArrivals < 0 || (requireArrival && p.OrdinaryArrivals == 0)")
    require_tokens(src / "Spells/Teleportation/WorldMapPointSpellActionComposer.cs",
        "TeleportDestinationPolicy.Evaluate(destination, originId, catalog).Eligible",
        "WordOfRecallDestinationPolicy.Matches")
    for name in ("TeleportPersistencePlan", "GuardedDisposableSaveLease", "GuardedReadOnlySave",
                 "RuntimeTestRunner.TeleportationPersistence", "RuntimeTestRunner.TeleportationCoexistence",
                 "TeleportCoexistenceForeignAction"):
        if name + ".cs" not in project:
            raise AssertionError("Missing compiled hardening contract: " + name)
    require_tokens(root / "scripts/Invoke-TeleportationHardeningQualification.ps1",
        "Get-KmgFeatureModuleConfigurations -Boundary", "Open-KmgProtectedSaveCatalog",
        "Restore-PersistenceSidecars", "ReuseInstalledArtifact")
    require_tokens(root / "scripts/Invoke-TeleportationPersistenceQualification.ps1",
        "foreach ($phase in @('A', 'B', 'C', 'D'))", "Open-KmgProtectedSaveCatalog",
        "Register-PersistenceOwnedSave", "Restore-PersistenceSettings")
    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))["teleportation119"]
    expected = {
        "deterministicTestCount": 1550,
        "featureModuleCount": 12, "featureModuleSchemaVersion": 11,
        "defaultOn": True, "strategicSpellCount": 3, "nativePointContextOnly": True,
        "realSpellbookExpenditure": True, "ownerReleaseAuthorized": True,
        "ledgerAuthoritativeAfterMigration": True,
        "legacyFormatAndCountsPreserved": True,
        "movementObserverUnchanged": True,
        "duplicateArrivalDefectReproduced": False,
        "freshProcessPersistencePhases": 4,
        "coexistenceControllerModuleConfigurations": 4,
        "moduleBoundaryConfigurations": 26,
        "mandatoryInstalledStackQualificationRequired": False,
        "remainingFinalArtifactQualificationWaivedByOwner": True,
        "ownerHighLevelCampaignPlaytestPending": True,
        "optionalModAndOtherUmmQualificationComplete": False,
        "retainedPublishedMaster": "8e5eeae7973c71ca4b78dc8216d00d815af7ea26",
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.119 release scope mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Teleportation {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Teleportation {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
