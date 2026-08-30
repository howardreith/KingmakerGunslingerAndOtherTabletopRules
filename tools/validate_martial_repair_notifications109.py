#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_icon_polish108 as baseline

VERSION = "0.0.109"
INFORMATIONAL_VERSION = (
    "0.0.109-martial-performance-repair-notifications")
PACKAGE = "KingmakerGunslinger-0.0.109-local-runtime.zip"
PACKAGE_SUFFIX = "martial-performance-repair-notifications"
DETERMINISTIC_TEST_COUNT = 1348
STATIC_KEY = "martialPerformanceRepairNotifications109"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.109 mission file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.109 mission token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    martial = require_tokens(root / (
        "src/KingmakerGunslinger/Compatibility/"
        "CustomWeaponMartialPerformanceCompatibility.cs"),
        "19d1ff4cf70845d094b0ec231473e97f",
        "MartialPerformanceFeatureSelection",
        "BlueprintFeatureSelection",
        "b7786666fe5b4694b8c4560efa6053c3",
        "DaggerMartialPerformanceFeature",
        "1e1f627d26ad36f43bbd26cc2bf8ac7e",
        "PrerequisiteCustomWeaponMartialPerformanceProficiency",
        "selection.AllFeatures = policy.Publish",
        "selection.AllFeatures = _allFeaturesBefore")
    if martial.count("internal const string ") < 12:
        raise AssertionError(
            "Martial Performance stable identity set is incomplete")

    policy = require_tokens(root / (
        "src/KingmakerGunslinger/Compatibility/"
        "CustomWeaponMartialPerformancePolicy.cs"),
        "CustomWeaponMartialPerformanceIdentityPolicy",
        "CustomWeaponMartialPerformanceSelectionPolicy<T>",
        "CustomWeaponMartialPerformanceProficiencyPolicy",
        "directCategoryProficiency",
        "broadMartialProficiency",
        "katanaGripDependent",
        "activeValues.OrderBy")
    proficiency = policy.split(
        "internal static bool CanUse", 1)[1]
    if "displayName" in proficiency:
        raise AssertionError(
            "Martial Performance proficiency infers from display text")

    state_machine = require_tokens(root / (
        "src/KingmakerGunslinger/Firearms/FirearmStateMachine.cs"),
        "internal static FirearmState Repair(FirearmState state)",
        "Only a broken firearm can use the ordinary repair transition.",
        "FirearmCondition.Normal")
    repair_block = state_machine.split(
        "internal static FirearmState Repair(FirearmState state)", 1)[1]
    repair_block = repair_block.split(
        "internal static FirearmState OverhaulWrecked", 1)[0]
    for token in ("0,", "null,", "FirearmCondition.Normal"):
        if token not in repair_block:
            raise AssertionError(
                "Broken-to-Normal repair does not authoritatively return empty")

    transaction = require_tokens(root / (
        "src/KingmakerGunslinger/Recovery/"
        "FirearmRepairTransactionService.cs"),
        "RepairKitInventorySnapshot.Capture",
        "inventory.Remove(1)",
        "stateStore.Replace(beforeState, repairedState)",
        "RestoreState(stateStore, beforeState, repairedState)",
        "RestoreInventory(inventory, beforeInventory)")
    if "FirearmRepairStatus.Loaded" in transaction:
        raise AssertionError(
            "Ordinary repair still rejects a loaded firearm")

    notification = require_tokens(root / (
        "src/KingmakerGunslinger/Firearms/"
        "FirearmConditionNotificationDispatcher.cs"),
        "before == FirearmCondition.Normal",
        "after == FirearmCondition.Broken",
        "before == FirearmCondition.Broken",
        "after == FirearmCondition.Wrecked",
        "\"{0}'s {1} is now {2}.\"",
        "\"{0} is now {1}.\"",
        "_combatLog.Publish",
        "_notification.Publish")
    if notification.index("_combatLog.Publish") > notification.index(
            "_notification.Publish"):
        raise AssertionError(
            "Top notification precedes the combat-log entry")
    require_tokens(root / (
        "src/KingmakerGunslinger/Firearms/"
        "FirearmConditionTopNotification.cs"),
        "Kingmaker.UI.Common.UIUtility.SendWarning(System.String)",
        "Kingmaker.UI.WarningsText",
        "UIUtility.SendWarning(message)",
        "condition-notification.failed")

    for path in (
            "src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs",
            "src/KingmakerGunslinger/Deeds/DeadShotRuntime.cs",
            "src/KingmakerGunslinger/Scatter/ScatterShotRuntime.cs"):
        require_tokens(root / path,
            "PublishAfterCommittedDegradation")

    program = require_tokens(root / (
        "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        'Case("martial-performance.identity"',
        'Case("martial-performance.preview"',
        'Case("repair.transaction.loaded-single-shot-success"',
        'Case("repair.transaction.loaded-multi-round-success"',
        'Case("condition-notification.combat-before-top"',
        'Case("condition-notification.failure-isolated"')
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic tests")
    expected_prefix_counts = {
        'Case("martial-performance.': 11,
        'Case("repair.': 22,
        'Case("condition-notification.': 9,
    }
    for prefix, expected in expected_prefix_counts.items():
        if program.count(prefix) != expected:
            raise AssertionError(
                f"Focused test registration count changed for {prefix}")

    manifest = json.loads((root / "blueprints/blueprints.json")
        .read_text(encoding="utf-8"))
    martial_entries = [entry for entry in manifest["entries"]
        if entry["symbol"].startswith(
            "KMG.CustomWeapons.MartialPerformance.")]
    if len(martial_entries) != 7 or any(
            entry.get("status") != "active" for entry in martial_entries):
        raise AssertionError(
            "Expected exactly seven active Martial Performance identities")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.109.md",
        "Kingmaker Gunslinger 0.0.109",
        "KingmakerGunslinger-0.0.109-martial-performance-repair-notifications.zip",
        "1,348 tests", "1,325-test",
        "UIUtility.SendWarning(System.String)")
    require_tokens(root / (
        "docs/MARTIAL-PERFORMANCE-REPAIR-NOTIFICATIONS-QUALIFICATION.md"),
        "19d1ff4cf70845d094b0ec231473e97f",
        "Kingmaker.Blueprints.Classes.Selection.BlueprintFeatureSelection",
        "MartialPerformanceFeatureSelection",
        "Kingmaker.UI.Common.UIUtility.SendWarning(System.String)",
        "Kingmaker.UI.WarningsText")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "martialPerformanceFocusedCaseCount": 11,
        "repairFocusedCaseCount": 22,
        "conditionNotificationFocusedCaseCount": 9,
        "customMartialPerformanceCategoryCount": 7,
        "firearmMartialPerformanceCategoryCount": 3,
        "easternMartialPerformanceCategoryCount": 3,
        "elvenBranchedSpearMartialPerformanceCategoryCount": 1,
        "loadedRepairDiscardsAmmunition": True,
        "loadedRepairAtomicRollback": True,
        "nativeTopNotificationApi": (
            "Kingmaker.UI.Common.UIUtility.SendWarning(System.String)"),
        "combatLogBeforeTopNotification": True,
        "releaseCandidateCommit": (
            "2e99bb82ad90d4cf84640cb23ec945a2190b394d"),
        "releaseCandidateSourceStateSha256": (
            "803df3105ef08116ffde2858914daedd940c2f1beef5430d9aab9638dab2fa0d"),
        "runtimePackageSha256": (
            "5afbf228c916b5c17bfb16b25f3280f4802870e5cc2a4b8d2d5d7f126fb28f31"),
        "runtimeDllSha256": (
            "1cff818811bf2b89fbd35bdb75d53ce79426c0350d97cedfdcc6f14b6e77ced7"),
        "runtimeDllMvid": "a1824875-8468-44f9-b051-117329c91aa5",
        "loadedRepairRuntimeAssertionCount": 20,
        "notificationRuntimeAssertionCount": 10,
        "martialPerformanceRuntimeAssertionCount": 23,
        "cotwProfileRestorationVerified": True,
        "runtimeRequalificationPending": False,
        "supervisedNativeLevelUpPresentationPending": True,
        "supervisedTopScreenVisualPlacementPending": True,
        "supervisedLoadedMultiRoundInteractionPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.109 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(
            f"Martial/repair/notification {VERSION} validation failed: "
            f"{exception}", file=sys.stderr)
        return 1
    print(
        f"Martial/repair/notification {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
