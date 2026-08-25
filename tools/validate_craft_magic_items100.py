#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_craft_magic_items99 as baseline

VERSION = "0.0.100"
INFORMATIONAL_VERSION = (
    "0.0.100-craft-magic-items-post-human-refinement")
PACKAGE = "KingmakerGunslinger-0.0.100-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1243
STATIC_KEY = "craftMagicItems100"
FOCUSED_TEST_COUNT = 15
PACKAGE_SUFFIX = "craft-magic-items-post-human-refinement"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"CMI refinement file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks CMI refinement token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.FOCUSED_TEST_COUNT = FOCUSED_TEST_COUNT
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.baseline.HARMONY_PATCH_COUNT = 13
    baseline.validate(root)

    policy = require_tokens(root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsCompatibilityPolicy.cs"),
        "SupportedRecognitionOnly", "AmmunitionTimedProjectTarget = 5",
        "ValueDerivedTarget", "TimedProjectTarget",
        "NormalizeAmmunitionProjectTarget",
        "IsInternalEnchantmentPresentationMarker")
    catalog = require_tokens(root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsRegistrationCatalog.cs"),
        "value.Spec.AcquisitionRole", "FirearmCreationBases",
        "FirearmRecognitionBases", "CustomFamilyRecognitionBases")
    require_tokens(root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsContractProbe.cs"),
        "CraftingProjectData", "CraftingTimerComponent",
        "CraftingProjectConstructor", "GetCraftingTimer",
        "crafting-project-shape")
    bridge = require_tokens(root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsReflectionBridge.cs"),
        "NormalizeNewAmmunitionProject",
        "MigrateExistingAmmunitionProjects",
        "goldSpentPreserved=true", "progressPreserved=true",
        "FirearmRecognitionIdentities", "CustomFamilyMagicItemTypes",
        "advanced-firearms-recognition-only",
        "ordinary-arms-owned-custom-family-upgrades")
    coordinator = require_tokens(root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsOptionalExtensionCoordinator.cs"),
        "ammunition-project-target", "ammunition-project-migration",
        "CraftingProjectPostfix", "GetCraftingTimerPostfix", "patches=13")
    tooltip = require_tokens(root / (
        "src/KingmakerGunslinger/Firearms/"
        "FirearmInternalEnchantmentPresentation.cs"),
        "FillWeaponQualities", "GetQualities",
        "FirearmStateTokenComponent", "BatteredFirearmOriginComponent",
        "ShouldRender", "OpCodes.Brfalse")
    observer = require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/"
        "CraftMagicItemsCompatibilityObserver.cs"),
        "internal-tooltip-markers-hidden", "Capture(true)")
    ui_observer = require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/"
        "CraftMagicItemsAmmunitionUiObserver.cs"),
        "project target=5", "timed-ammunition-cancellation",
        "graphAfter.ItemTypes == 3")
    require_tokens(root / (
        "tests/KingmakerGunslinger.DomainTests/"
        "CraftMagicItemsCompatibilityTests.cs"),
        "AmmunitionProjectMigrationIsExact",
        "InternalTooltipMarkersAreExact",
        "advanced-rifle", "advanced-revolver")

    obsolete = ("KMGMagicEasternAndElvenWeapons",
        "KMG.CraftMagicItems.CustomWeapons.Name",
        "MagicCustomWeaponsIdentity", "_magicCustomWeapons",
        "CategoryScope.CustomWeapons")
    production = "\n".join((policy, catalog, bridge, coordinator, tooltip,
        observer, ui_observer))
    if any(token in production for token in obsolete):
        raise AssertionError(
            "Obsolete Eastern/Elven magic category remains in production")
    if "RenderMundanePrefix" in coordinator:
        raise AssertionError("Rejected 0.0.98 whole-method prefix returned")
    if "CraftMagicItems.dll" in (root / (
            "src/KingmakerGunslinger/KingmakerGunslinger.csproj")
            ).read_text(encoding="utf-8"):
        raise AssertionError("Production gained a static CMI dependency")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "addedItemTypes": 3,
        "firearmCreationBases": 3,
        "firearmRecognitionIdentities": 5,
        "martialAdditions": 1,
        "exoticAdditions": 3,
        "customFamilyMagicItemTypes": 0,
        "ammunitionTimedProjectTarget": 5,
        "ammunitionGoldAtScaleOne": [34, 4, 40],
        "legacyProjectMigrationIdempotent": True,
        "tooltipMarkerSuppressionOnly": True,
        "runtimeQualificationPending": False,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"CMI refinement static mismatch: {key}")

    pending = state.get("refinementRuntimeQualificationPending")
    qualified = (
        state.get("refinementRealCmiObserverQualified"),
        state.get("refinementAmmunitionUiObserverQualified"),
        state.get("refinementWorkingSaveSmokeQualified"),
        state.get("refinementPersistenceQualified"),
    )
    assertion_counts = (
        state.get("refinementRealCmiAssertionCount"),
        state.get("refinementAmmunitionUiAssertionCount"),
        state.get("refinementWorkingSaveAssertionCount"),
        state.get("refinementPersistencePrepareAssertionCount"),
        state.get("refinementPersistenceVerifyAssertionCount"),
    )
    if pending is True:
        if qualified != (False, False, False, False) or assertion_counts != (
                0, 0, 0, 0, 0):
            raise AssertionError(
                "Pending refinement records contradictory runtime PASS evidence")
    elif pending is False:
        if qualified != (True, True, True, True) or assertion_counts != (
                27, 13, 11, 6, 6):
            raise AssertionError(
                "Completed refinement lacks exact guarded runtime evidence")
    else:
        raise AssertionError("Refinement runtime qualification state is missing")
    if state.get("humanAcceptancePending") is not True:
        raise AssertionError("CMI refinement must not claim unperformed human acceptance")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Craft Magic Items Refinement {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Craft Magic Items Refinement {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
