#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_craft_magic_items98 as baseline

VERSION = "0.0.99"
INFORMATIONAL_VERSION = (
    "0.0.99-craft-magic-items-ammunition-ui-repair")
PACKAGE = "KingmakerGunslinger-0.0.99-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1241
STATIC_KEY = "craftMagicItems99"
FOCUSED_TEST_COUNT = 13
PACKAGE_SUFFIX = "craft-magic-items-ammunition-ui-repair"
REJECTED_BASELINE = "d7178d6ae77b79624917f955658231ae67894c51"
INNER_SEAM = (
    "post-selected-crafting-data:ordinary=IL_014d;"
    "new-item-bases=IL_0186;footer=IL_0774;"
    "locals=crafter:1,selected:4,recipe:5")


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"CMI ammunition UI repair file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks CMI ammunition UI repair token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.FOCUSED_TEST_COUNT = FOCUSED_TEST_COUNT
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.validate(root)

    contract_path = root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsMundaneUiContract.cs")
    transpiler_path = root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsMundaneUiTranspiler.cs")
    bridge_path = root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsReflectionBridge.cs")
    coordinator_path = root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsOptionalExtensionCoordinator.cs")
    adapter_path = root / (
        "src/KingmakerGunslinger/CraftMagicItemsCompatibility/"
        "CraftMagicItemsAmmunitionUiRuntimeAdapter.cs")
    observer_path = root / (
        "src/KingmakerGunslinger/RuntimeTesting/"
        "CraftMagicItemsAmmunitionUiObserver.cs")
    tests_path = root / (
        "tests/KingmakerGunslinger.DomainTests/"
        "CraftMagicItemsCompatibilityTests.cs")

    require_tokens(contract_path,
        "Mundane Crafting: ", "get_NewItemBaseIDs", "Current Money: {0}",
        "mundane-ui-parent-finalization", "mundane-ui-common-footer",
        "RenderLabelRow", "LabelRenderer",
        "TargetInvocationException", "CraftMagicItemsUiFailurePolicy",
        "Capture(", "CraftMagicItemsMundaneUiEventPolicy",
        "StringComparison.OrdinalIgnoreCase", "ShouldApplyPendingPhase")
    transpiler = require_tokens(transpiler_path,
        "CraftMagicItemsMundaneUiTranspiler", "Transpile(",
        "selected-data-cast", "ordinary-body-target",
        "new-item-base-access", "common-footer", "callback",
        "OpCodes.Brfalse", "OpCodes.Br")
    bridge = require_tokens(bridge_path,
        "TryRenderSelectedAmmunition", "AmmunitionLowerPanel",
        "RenderLabel.Invoke",
        "ReportUiBoundaryFailure", "ProcessDeferredUiFailure",
        "DeferredUiFailure", "ExceptionDispatchInfo.Capture",
        "BridgeFaulted")
    coordinator = require_tokens(coordinator_path,
        "ammunition-ui-inner-seam", "CraftMagicItemsMundaneUiTranspiler",
        "TryRenderSelectedAmmunition", "ProcessDeferredUiFailure")
    require_tokens(adapter_path,
        "CraftMagicItemsAmmunitionCraftObservation", "ArmCraft",
        "CompleteTimedProject", "CraftButtonPostfix", "WorkOnProjects",
        "CraftingTakesNoTime", "CancelCraftingProject")
    require_tokens(observer_path,
        "CraftMagicItemsAmmunitionUiProbeHost", "RouteObservedOn",
        "ConfigureAtNextLayout", "ShouldApplyPendingPhase", "ObserveCraft",
        "CompleteTimedProject", "FirearmReloadResult", "PaperCartridge",
        "StatType.SkillKnowledgeWorld", "knowledge.BaseValue = 100",
        "DescribeCleanup")
    require_tokens(tests_path,
        "MundaneUiPatchShapeIsExact", "MundaneUiRouteIsStable",
        "MundaneUiFailureIsDeferred", "sameDisplayButDifferentObject",
        "string[] events = { \"Layout\", \"MouseDown\", \"Repaint\" }",
        "TargetInvocationException")

    if "RenderMundanePrefix" in coordinator or \
            "TryRenderAmmunition" in coordinator or \
            "TryRenderAmmunition" in bridge:
        raise AssertionError(
            "Rejected conditional whole-method mundane UI replacement remains")
    if "SelectedIndexField" in bridge:
        raise AssertionError(
            "Production ammunition renderer still derives ownership from selection index")
    route_start = bridge.index("internal static bool TryRenderSelectedAmmunition")
    route_end = bridge.index("internal static BlueprintItemWeapon BuildQualificationClone",
        route_start)
    if "ImmediateModeGui.Label" in bridge[route_start:route_end]:
        raise AssertionError(
            "Ammunition lower panel mixes KMG's lazy GUI renderer with CMI rows")
    report_start = bridge.index("private static void ReportUiBoundaryFailure")
    report_end = bridge.index("private static void ObserveUiEventNoLock",
        report_start)
    if "RollbackCompatibilityGraph();" in bridge[report_start:report_end]:
        raise AssertionError(
            "OnGUI failure reporting still performs synchronous graph rollback")
    if "values.InsertRange(ordinaryIndex, injected)" not in transpiler:
        raise AssertionError("Inner-seam injection is not anchored at ordinary body")

    project = (root / "src/KingmakerGunslinger/"
        "KingmakerGunslinger.csproj").read_text(encoding="utf-8")
    for name in (contract_path.name, transpiler_path.name, adapter_path.name,
                 observer_path.name):
        if name not in project:
            raise AssertionError(f"Main project compile list lacks {name}")

    scenario_catalog = require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs"),
        "observe-craft-magic-items-ammunition-ui")
    runtime_common = require_tokens(root / "scripts/RuntimeAutomation.Common.ps1",
        "observe-craft-magic-items-ammunition-ui", "active version 0.0.99")
    if not scenario_catalog or not runtime_common:
        raise AssertionError("Guarded ammunition UI scenario is unavailable")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    contract = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "rejectedBaselineCommit": REJECTED_BASELINE,
        "rejectedBaselineVersion": "0.0.98",
        "outerMundaneSelectorOwner": "CraftMagicItems",
        "innerSeamIdentity": INNER_SEAM,
        "wholeMethodPrefixRemoved": True,
        "synchronousGuiRollbackProhibited": True,
        "targetInvocationUnwrapRequired": True,
    }
    for key, value in expected.items():
        if contract.get(key) != value:
            raise AssertionError(f"CMI UI repair static validation mismatch: {key}")

    pending = contract.get("repairRuntimeQualificationPending")
    qualified = (
        contract.get("repairRealCmiObserverQualified"),
        contract.get("ammunitionUiObserverQualified"),
        contract.get("workingSaveRepairQualified"),
    )
    assertion_count = contract.get("ammunitionUiRuntimeAssertionCount")
    if pending is True:
        if qualified != (False, False, False) or assertion_count != 0:
            raise AssertionError(
                "Pending repair qualification records contradictory PASS evidence")
    elif pending is False:
        if qualified != (True, True, True) or not isinstance(
                assertion_count, int) or assertion_count <= 0:
            raise AssertionError(
                "Completed repair qualification lacks exact observer evidence")
    else:
        raise AssertionError("Repair runtime qualification state is missing")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Craft Magic Items Ammunition UI {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Craft Magic Items Ammunition UI {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
