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

VERSION = "0.0.118"
INFORMATIONAL_VERSION = "0.0.118-contextual-world-map-teleportation"
PACKAGE = "KingmakerGunslinger-0.0.118-local-runtime.zip"
PACKAGE_SUFFIX = "contextual-world-map-teleportation"
DETERMINISTIC_TEST_COUNT = 1550
PUBLISHED_117_PREFIX_SHA256 = "c648ebdad613e50c36b97f3337e8247a870d5ec24abc8df55b4f9e9a6a33519b"
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
        importlib.import_module(module).STATIC_KEY = "teleportation118InheritedContracts"
    baseline.STATIC_KEY = "teleportation118InheritedContracts"
    baseline.MANIFEST_TOTAL = 1872
    baseline.MANIFEST_ACTIVE = 1870
    baseline.validate(root)

    entries = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))["entries"]
    digest = hashlib.sha256(json.dumps(entries[:1869], sort_keys=True,
        separators=(",", ":")).encode()).hexdigest()
    if digest != PUBLISHED_117_PREFIX_SHA256:
        raise AssertionError("Published 0.0.117 manifest prefix changed")
    spells = entries[1869:]
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
        r"docs\RELEASE-NOTES-0.0.118.md", "AllowNonDefaultReleaseBranch",
        "ConfirmReleaseReady")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.118.md",
        INFORMATIONAL_VERSION, "real spellbook", "owner authorized",
        "remain unqualified", "1,550", "0.0.117")
    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))["teleportation118"]
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "featureModuleCount": 12, "featureModuleSchemaVersion": 11,
        "defaultOn": True, "strategicSpellCount": 3, "nativePointContextOnly": True,
        "realSpellbookExpenditure": True, "ownerReleaseAuthorized": True,
        "allOriginalRuntimeGatesPassed": False,
        "remainingQualificationAssignedToOwnerTesting": True,
        "retainedPublishedMaster": "8e5eeae7973c71ca4b78dc8216d00d815af7ea26",
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.118 release scope mismatch: {key}")


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
