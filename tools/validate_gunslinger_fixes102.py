#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_craft_magic_items101 as baseline

VERSION = "0.0.102"
INFORMATIONAL_VERSION = (
    "0.0.102-starter-bokken-combat-log-acadamae-toggle")
PACKAGE = "KingmakerGunslinger-0.0.102-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1251
STATIC_KEY = "gunslingerFixes102"
CMI_FOCUSED_TEST_COUNT = 15
MISSION_FOCUSED_TEST_COUNT = 8
PACKAGE_SUFFIX = "starter-bokken-combat-log-acadamae-toggle"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"Gunslinger fixes file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks Gunslinger fixes token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.FOCUSED_TEST_COUNT = CMI_FOCUSED_TEST_COUNT
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.validate(root)

    transaction = require_tokens(root / (
        "src/KingmakerGunslinger/Gunsmithing/"
        "GunslingerStartingFirearmGrantTransaction.cs"),
        "ExactGunslingerLevel", "transition.PriorLevel != 0",
        "CompleteGrant(snapshot, true)", "RollbackInventory",
        "StartingAmmunitionCount = 20", "HasReceipt")
    require_tokens(root / (
        "src/KingmakerGunslinger/Gunsmithing/"
        "GunslingerStartingFirearmOwnershipPatch.cs"),
        "typeof(LevelUpController), \"Commit\"", "RespecCompanion",
        "successCallback", "CompleteTransition")
    if "GetMaxClass" in transaction:
        raise AssertionError(
            "Starter transaction regressed to overall/max-class detection")

    bokken = require_tokens(root / (
        "src/KingmakerGunslinger/Blueprints/"
        "BokkenFirearmSupplyVendorBlueprints.cs"),
        "BlueprintUnitLoot", "AmmunitionCount = 100",
        "RepairKitCount = 5", "OverhaulKitCount = 2",
        "GunsmithKitCount = 1", "supplies.GunsmithKit")
    oleg = require_tokens(root / (
        "src/KingmakerGunslinger/Blueprints/"
        "OlegFirearmSupplyCleanupBlueprints.cs"),
        "BlueprintSharedVendorTable", "ammunition.BlackPowder",
        "ammunition.PaperCartridge", "supplies.OverhaulKit",
        "supplies.GunsmithKit", "retained")
    if "BokkenAmmunitionVendorBlueprints" in bokken or \
            "OlegMaintenanceVendorBlueprints" in oleg:
        raise AssertionError("Obsolete early-vendor abstraction remains")

    require_tokens(root / (
        "src/KingmakerGunslinger/Diagnostics/NativeCombatLog.cs"),
        "BattleLogManager.LogView.AddLogEntry", "LogChannel.Combat",
        "PrefixIcon.None", "PlayerCombatLogPublicationService")
    require_tokens(root / (
        "src/KingmakerGunslinger/Diagnostics/"
        "PlayerCombatLogMessagePolicy.cs"),
        "PreferredMaximumLength = 100", "HardMaximumLength = 160",
        "contains internal terminology")
    production_root = root / "src/KingmakerGunslinger"
    warning_publishers = []
    for path in production_root.rglob("*.cs"):
        source = path.read_text(encoding="utf-8")
        if "IWarningNotificationUIHandler" in source or \
                "HandleWarning" in source:
            warning_publishers.append(str(path.relative_to(root)))
    if warning_publishers:
        raise AssertionError(
            f"Production retains warning-overlay publishers: "
            f"{warning_publishers}")

    require_tokens(root / (
        "src/KingmakerGunslinger/Acadamae/AcadamaeModeStatePolicy.cs"),
        "activatableIsOn", "off-marker-lingering",
        "on-marker-pending")
    acadamae = require_tokens(root / (
        "src/KingmakerGunslinger/Acadamae/AcadamaeCastingPatches.cs"),
        "ResolveEffectiveModeState", "matches[0].IsOn",
        "if (decision.Eligible) Invocations.Arm",
        "Invocations.Consume", "NativeCombatLog.Publish(\"acadamae\"")
    if "AccelerationModeActive = markerPresent" in acadamae:
        raise AssertionError("Acadamae regressed to marker-only authority")

    runner = require_tokens(root / (
        "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        "mission.starter-transition-matrix",
        "mission.vendor-six-row-transaction",
        "mission.combat-log-no-warning-overlay",
        "mission.acadamae-effective-mode-matrix")
    if runner.count('Case("mission.') != MISSION_FOCUSED_TEST_COUNT:
        raise AssertionError(
            f"Expected {MISSION_FOCUSED_TEST_COUNT} mission tests")
    require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs"),
        "disposable-gunslinger-multiclass-commit",
        "observe-vendor-table-contracts",
        "disposable-firearm-penetration",
        "disposable-acadamae-graduate")
    require_tokens(root / f"docs/RELEASE-NOTES-{VERSION}.md",
        f"Kingmaker Gunslinger {VERSION}", f"{PACKAGE_SUFFIX}.zip",
        "CraftMagicItems.dll", f"{DETERMINISTIC_TEST_COUNT:,}")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "starterProductionFirearmKinds": 2,
        "starterPowderCount": 20,
        "starterBallCount": 20,
        "starterGunsmithKitCount": 1,
        "bokkenFixedSupplyRows": 6,
        "olegProjectOwnedSupplyRows": 0,
        "warningOverlayPublisherCount": 0,
        "combatLogHardMaximumLength": 160,
        "acadamaeActivatableStateAuthoritative": True,
        "combatLogVisualPlacementHumanGated": True,
        "acadamaeFreshProcessPersistenceHumanGated": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Gunslinger fixes static mismatch: {key}")

    pending = state.get("missionRuntimeQualificationPending")
    qualified = (
        state.get("starterRuntimeQualified"),
        state.get("vendorRuntimeQualified"),
        state.get("combatLogRuntimeQualified"),
        state.get("acadamaeRuntimeQualified"),
    )
    pass_count = state.get("missionRuntimeScenarioPassCount")
    if pending is True:
        if qualified != (False, False, False, False) or pass_count != 0:
            raise AssertionError(
                "Pending mission records contradictory runtime evidence")
    elif pending is False:
        if qualified != (True, True, True, True) or not isinstance(
                pass_count, int) or pass_count < 10:
            raise AssertionError(
                "Completed mission lacks guarded runtime evidence")
    else:
        raise AssertionError("Mission runtime qualification state is missing")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Gunslinger Fixes {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Gunslinger Fixes {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
