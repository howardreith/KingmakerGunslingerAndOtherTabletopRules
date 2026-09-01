#!/usr/bin/env python3
"""Release gate for the ammunition, CMI, and player-facing repair."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_gunslinger_outfit_kitbash111 as baseline

VERSION = "0.0.112"
INFORMATIONAL_VERSION = "0.0.112-ammunition-cmi-copy-notifications"
PACKAGE = "KingmakerGunslinger-0.0.112-local-runtime.zip"
PACKAGE_SUFFIX = "ammunition-cmi-copy-notifications"
DETERMINISTIC_TEST_COUNT = 1372
STATIC_KEY = "ammunitionCmiCopyNotifications112"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.112 release file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.112 repair token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    cost = require_tokens(root / "src/KingmakerGunslinger/Ammunition/"
        "AmmunitionCraftingCostPolicy.cs", "DiscountPercent = 10",
        "CraftMagicItemsPriceScale = 0.60f", "Math.Max(1",
        "ForCombinedBatch")
    if "return Math.Max(1" not in cost:
        raise AssertionError("Ammunition crafting lost its explicit minimum cost")
    basic = require_tokens(root / "src/KingmakerGunslinger/Gunsmithing/"
        "CraftBasicAmmunitionAbilityLogic.cs",
        "AmmunitionCraftingCostPolicy.ForCombinedBatch", "Not enough gold.",
        "FirearmCraftingTransactionService.Complete")
    paper = require_tokens(root / "src/KingmakerGunslinger/Gunsmithing/"
        "CraftPaperCartridgesAbilityLogic.cs",
        "AmmunitionCraftingCostPolicy.ForBatch", "Not enough gold.",
        "FirearmCraftingTransactionService.Complete")
    transaction = require_tokens(root / "src/KingmakerGunslinger/Gunsmithing/"
        "FirearmCraftingTransactionService.cs", "SpendMoney(goldCost)",
        "GainMoney(missingMoney)", "caster.RemoveFact(marker)",
        "ammunition-craft.committed")
    if "goldCost < 1" not in transaction:
        raise AssertionError("Ammunition transaction permits free crafting")
    blueprints = require_tokens(root / "src/KingmakerGunslinger/Blueprints/"
        "GunsmithingCraftingBlueprints.cs", "pay 24 gp",
        "Paper Cartridge craft cost must equal 24 gp.",
        "Basic ammunition craft cost must equal 22 gp.")
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/"
        "GunsmithingBlueprints.cs", "24 gp for 20 Paper Cartridges")
    if "120 gp" in blueprints:
        raise AssertionError("Paper Cartridge card presents a stale craft cost")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestRunner.cs", "24 gp creates exactly 20 Paper Cartridges",
        "paperLogic.GoldCost == 24")

    coordinator = require_tokens(root / "src/KingmakerGunslinger/"
        "CraftMagicItemsCompatibility/"
        "CraftMagicItemsOptionalExtensionCoordinator.cs",
        "TryFinalizeTargetedGraph", "OnExternalToggleObservedPostfix",
        "RepeatTargetedFinalizationForQualification", "targeted-finalization")
    bridge = require_tokens(root / "src/KingmakerGunslinger/"
        "CraftMagicItemsCompatibility/CraftMagicItemsReflectionBridge.cs",
        "TryFinalizeLateAttachment", "BeforeEquipmentIndexes",
        "SynchronizeMundaneIndexes")
    policy = require_tokens(root / "src/KingmakerGunslinger/"
        "CraftMagicItemsCompatibility/CraftMagicItemsCompatibilityPolicy.cs",
        "MergeExactlyOnce", "AmmunitionTimedProjectTarget")
    lifecycle = coordinator + bridge + policy
    for prohibited in ("RebuildCompleteGraph", "Main.Load"):
        if prohibited in lifecycle:
            raise AssertionError(f"Synthetic CMI lifecycle operation remains: {prohibited}")
    cmi_tests = require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "CraftMagicItemsCompatibilityTests.cs", "syntheticToggleInvocation",
        "syntheticGraphRebuild", "LifecycleAndPackagingRemainOptional",
        "AmmunitionBatchEconomicsAreExact", "RepeatTargetedFinalizationForQualification")
    require_tokens(root / "src/KingmakerGunslinger/"
        "CraftMagicItemsCompatibility/CraftMagicItemsAmmunitionCostScope.cs",
        "CraftingCostsNoGold", "CraftingPriceScale", "Dispose")

    mode = require_tokens(root / "src/KingmakerGunslinger/Reloading/"
        "PaperCartridgeModeRuntime.cs", "toggle.IsOn", "Reconcile",
        "set_IsOn", "PostLoad", "RemoveFact")
    if "RawFacts.OfType<Buff>().Any" in mode:
        raise AssertionError("Paper marker is still sufficient toggle evidence")
    require_tokens(root / "src/KingmakerGunslinger/Reloading/"
        "ReloadAbilityPresentationPatches.cs", "InvalidatePaperMode",
        "ReloadQueuedPlanBinding", "IsCurrent")
    paper_tests = require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "PaperCartridgeFoundationTests.cs", "AmmunitionCraftingEconomyIsExact",
        "pay 24 gp", "toggle.IsOn", "ReloadQueuedPlanBinding",
        "PlayerFacingFailureReasonsAreScreenSafe")

    presentation = require_tokens(root / "src/KingmakerGunslinger/Presentation/"
        "PlayerFacingTextPolicy.cs", "IsScreenSafe", "ForbiddenPhrases")
    require_tokens(root / "src/KingmakerGunslinger/Reloading/"
        "ReloadPlayerFacingReasonPolicy.cs", "ForPlan", "Cannot reload now.",
        "No paper cartridges.", "No loose ammunition.")
    require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "FirearmConditionNotificationTests.cs", "WreckedMessageIsExactAndConcise",
        "NormalToBrokenDispatchesOnce", "BrokenToWreckedDispatchesOnce")
    audit = require_tokens(root / "docs/ITEM-DESCRIPTION-AUDIT.md",
        "Moonlit Fork", "Usable with Weapon Finesse.",
        "Project Item Description Audit", "Ammunition and supplies")

    program = require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "Program.cs", 'Case("paper-crafting.economy-exact"',
        'Case("presentation.failure-reasons-safe"',
        'Case("craft-magic-items.lifecycle-package"')
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic test cases")
    package = require_tokens(root / "scripts/package.ps1",
        "$($info.Id)-$($info.Version)-ammunition-cmi-copy-notifications.zip")
    notes = require_tokens(root / "docs/RELEASE-NOTES-0.0.112.md",
        "Kingmaker Gunslinger 0.0.112",
        "KingmakerGunslinger-0.0.112-ammunition-cmi-copy-notifications.zip",
        "10%-of-", "no synthetic CMI toggle", "1,372")
    require_tokens(root / "CHANGELOG.md",
        "0.0.112-ammunition-cmi-copy-notifications", "Paper Cartridges")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("0.0.112 static release identity mismatch")
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "discountPercent": 10,
        "minimumCost": 1,
        "basicBatchCost": 22,
        "paperBatchCost": 24,
        "paperToggleAuthoritative": "activatable-is-on",
        "paperMarkerReconciliation": True,
        "cmiSyntheticToggleProhibited": True,
        "cmiTargetedBridgeIdempotent": True,
        "descriptionAudit": "docs/ITEM-DESCRIPTION-AUDIT.md",
        "screenSafeReasons": True,
        "brokenWreckedNotificationsExactOnce": True,
        "guardedRuntimeRequestCallbackObserved": False,
    }
    state = static.get(STATIC_KEY, {})
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.112 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Ammunition/CMI/player-copy {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Ammunition/CMI/player-copy {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())