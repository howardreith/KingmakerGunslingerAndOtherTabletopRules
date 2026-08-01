#!/usr/bin/env python3
"""Portable source validator for Sprint 33 with inherited Sprint 32 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint32

VERSION = "0.0.33"
INFORMATIONAL_VERSION = "0.0.33-s33-capacity-advanced-firearms"
TEST_COUNT = 685

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 33 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT) -> None:
    root = root.resolve()
    validate_sprint32.validate(root, version, informational_version, test_count, 24, 25)
    require_tokens(read(root, "planning/SPRINT-33-ENTRY-CRITERIA.md"),
        ["Advanced firearms load all chambers", "partially loaded firearm",
         "exact pre-operation snapshots", "Two consecutive fresh-process PASS runs"],
        "Sprint 33 entry criteria")
    require_tokens(read(root, "src/KingmakerGunslinger/Reloading/FirearmReloadTransactionService.cs"),
        ["TryReloadBasicRounds", "roundsToLoad", "TryConsumeLoads",
         "rules.Capacity - beforeState.LoadedRounds"],
        "Sprint 33 multi-round reload transaction")
    require_tokens(read(root, "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        ['Case("capacity.reload-empty-to-full"', 'Case("capacity.reload-partial-top-up"',
         'Case("capacity.reload-write-failure-rolls-back"'], "Sprint 33 capacity tests")
    require_tokens(read(root, "src/KingmakerGunslinger/Firearms/FirearmDefinitions.cs"),
        ["CreateAdvancedRifle", "CreateAdvancedRevolver", "ReloadActionType.Move"],
        "Sprint 33 advanced definitions")
    require_tokens(read(root, "src/KingmakerGunslinger/Firearms/ProductionFirearmCatalog.cs"),
        ["advanced-rifle", "advanced-revolver", "5000", "4000"],
        "Sprint 33 advanced catalog")
    require_tokens(read(root, "src/KingmakerGunslinger/Firearms/FirearmStateTokenCatalog.cs"),
        ["CreateBasicCapacity", "rounds <= capacity", "rounds-", "LoadedNormalTokenId"],
        "Sprint 33 capacity token catalog")
    require_tokens(read(root, "src/KingmakerGunslinger/Misfires/FirearmMisfireConditionService.cs"),
        ["definition.Era == FirearmEra.Advanced", "AdvancedBrokenRemainsBroken",
         "ApplyMisfireDamage(postDischargeState)"], "Sprint 33 advanced misfire policy")
    require_tokens(read(root, "tests/KingmakerGunslinger.DomainTests/Sprint33Tests.cs"),
        ["CapacityVaultSixRoundRestart", "CapacityVaultTwoItemIsolation",
         "CapacityRepeatedDischargeIsolated", "VaultBackedFirearmStateRepository"],
        "Sprint 33 durable capacity tests")
    require_tokens(read(root, "src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs"),
        ["AdvancedRifleWeaponTypeSymbol", "AdvancedRevolverWeaponTypeSymbol",
         "ProductionFirearmCatalog.CreateAdvancedRifle()", "Count { get { return 10; } }"],
        "Sprint 33 advanced blueprints")
    require_tokens(read(root, "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunSprint33AdvancedCapacity", "firstLoad.RoundsLoaded == 6",
         "AdvancedBrokenRemainsBroken", "!explosion.RequiresBurstDamage"],
        "Sprint 33 guarded capacity scenario")
    require_tokens(read(root, "scripts/Test-Sprint33AdvancedCapacity.ps1"),
        ["advanced-capacity", "KMG_AUTOMATION_WORKING",
         "save-write-sentinel-retained", "steam-launch-only"],
        "Sprint 33 guarded capacity scenario tests")
    print("Sprint 33 source invariant validation passed with inherited Sprint 32 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 33 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
