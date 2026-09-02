#!/usr/bin/env python3
"""Release gate for the 0.0.113 paper-mode save-load hotfix."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_gunslinger_outfit_kitbash111 as baseline

VERSION = "0.0.113"
INFORMATIONAL_VERSION = "0.0.113-save-load-hotfix"
PACKAGE = "KingmakerGunslinger-0.0.113-local-runtime.zip"
PACKAGE_SUFFIX = "save-load-hotfix"
DETERMINISTIC_TEST_COUNT = 1387
STATIC_KEY = "saveLoadReconciliation113"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.113 hotfix file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.113 hotfix token(s): {missing}")
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
        "PaperCartridgeModeRuntime.cs", "toggle.IsOn", "set_IsOn",
        "Interlocked.Increment", "InvalidatePaperMode",
        "partially hydrated unit")
    for prohibited in ("Reconcile", "UnitEntityData", "PostLoad", "AddFact",
                       "RemoveFact", ".Stop(", "IsOn =", "unit.Buffs"):
        if prohibited in mode:
            raise AssertionError(
                f"Paper-mode load safety violation remains: {prohibited}")
    require_tokens(root / "src/KingmakerGunslinger/Reloading/"
        "ReloadAbilityPresentationPatches.cs", "InvalidatePaperMode",
        "ReloadQueuedPlanBinding", "IsCurrent")
    named_descriptions = require_tokens(root / "src/KingmakerGunslinger/"
        "Blueprints/EasternWeaponNamedBlueprints.cs",
        "A border warden's blade, worn smooth by long patrols.",
        "Its blade bears the shape of an ancient bough.")
    if ": string.Empty;" in named_descriptions:
        raise AssertionError("Named Eastern items can still inherit donor descriptions")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestRunner.cs", "toggleOffReadOnly",
        "markerCountBeforeRead", "markerCountAfterRead",
        "Pistol plus Rapid Reload uses loose ammunition and a Move action")
    paper_tests = require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "PaperCartridgeFoundationTests.cs", "AmmunitionCraftingEconomyIsExact",
        "ModeReadOnlySourceContract", "toggle.IsOn", "ReloadQueuedPlanBinding",
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
        'Case("paper-mode.read-only-save-load-contract"',
        'Case("presentation.failure-reasons-safe"',
        'Case("craft-magic-items.lifecycle-package"')
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic test cases")
    package = require_tokens(root / "scripts/package.ps1",
        "$($info.Id)-$($info.Version)-save-load-hotfix.zip")
    notes = require_tokens(root / "docs/RELEASE-NOTES-0.0.113.md",
        "Kingmaker Gunslinger 0.0.113",
        "KingmakerGunslinger-0.0.113-save-load-hotfix.zip",
        "read-only", "UnitEntityData.PostLoad", "1,373")
    require_tokens(root / "CHANGELOG.md",
        "0.0.113-save-load-hotfix", "Paper Cartridges")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("0.0.113 static release identity mismatch")
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
    }
    state = static.get(STATIC_KEY, {})
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.113 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Save-load hotfix {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Save-load hotfix {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
