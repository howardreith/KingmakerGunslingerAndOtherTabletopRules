#!/usr/bin/env python3
"""Portable source validator for Sprint 35 with inherited Sprint 34 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint34

VERSION = "0.0.35"
INFORMATIONAL_VERSION = "0.0.35-s35-grit-resource"
TEST_COUNT = 703

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 35 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint34.validate(root, VERSION, INFORMATIONAL_VERSION, TEST_COUNT, 29, 30)
    require_tokens(read(root, "planning/SPRINT-35-ENTRY-CRITERIA.md"),
        ["Wisdom modifier, minimum 1", "Daily reset", "duplicate spend",
         "unrelated unit stores"], "Sprint 35 entry criteria")
    require_tokens(read(root, "src/KingmakerGunslinger/Grit/GritPoolService.cs"),
        ["CalculateMaximum", "ResetDaily", "ReconcileMaximum", "Spend(",
         "Restore(", "GritTransactionStatus.Duplicate"], "Sprint 35 grit service")
    require_tokens(read(root, "tests/KingmakerGunslinger.DomainTests/Sprint35Tests.cs"),
        ["GritMaximumWisdomMinimum", "GritSpendInsufficientAtomic",
         "GritDuplicateSpendRejected", "GritUnitGatesAreIsolated"],
        "Sprint 35 grit tests")
    require_tokens(read(root, "scripts/Test-Sprint35GritDomain.ps1"),
        ["wisdom-minimum", "atomic-spend", "operation-dedupe", "focused-tests"],
        "Sprint 35 focused source tests")
    require_tokens(read(root, "src/KingmakerGunslinger/Blueprints/GritBlueprints.cs"),
        ["BlueprintAbilityResource", "AddAbilityResources", "RestoreAmount = true",
         "RestoreOnLevelUp = false", "ConfigureBaseAmount(resource, 1)",
         "ConfigureEmptyArray(amountField.FieldType, amount, \"Class\")"],
        "Sprint 35 grit blueprints")
    require_tokens(read(root, "src/KingmakerGunslinger/Grit/GritResourceAmountBonus.cs"),
        ["IResourceAmountBonusHandler", "StatType.Wisdom", "wisdomModifier - 1",
         "resource != Resource"], "Sprint 35 Wisdom resource bonus")
    require_tokens(read(root, "scripts/Test-Sprint35GritBlueprints.ps1"),
        ["persistent-unit-resource", "wisdom-floor-formula", "level-one-grant",
         "manifest-identities"], "Sprint 35 grit blueprint tests")
    require_tokens(read(root, "scripts/Test-Sprint35DisposableGritResource.ps1"),
        ["scenario-allowlisted", "detached-only", "native-resource-path",
         "no-level-refill"], "Sprint 35 disposable grit runtime tests")
    require_tokens(read(root, "scripts/Test-Sprint35DisposableGritRest.ps1"),
        ["scenario-allowlisted", "detached-only", "native-rest-contract",
         "restored-to-maximum"], "Sprint 35 disposable grit-rest tests")
    print("Sprint 35 source invariant validation passed with inherited Sprint 34 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 35 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
