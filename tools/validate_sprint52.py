#!/usr/bin/env python3
"""Portable source validator for Sprint 52 with inherited Sprint 51 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint51

VERSION = "0.0.52"
INFORMATIONAL_VERSION = "0.0.52-s52-lightning-reload"
TEST_COUNT = 790

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 52 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT, active_count: int = 90,
             ledger_count: int = 91) -> None:
    root = root.resolve()
    validate_sprint51.validate(root, version, informational_version,
                               test_count, active_count, ledger_count)
    require_tokens(read(root, "planning/SPRINT-52-ENTRY-CRITERIA.md"),
        ["swift action once per round", "positive current grit",
         "free-action route", "two independent fresh-process"],
        "Sprint 52 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/LightningReloadService.cs") + read(root,
        "src/KingmakerGunslinger/Deeds/LightningReloadDecision.cs"),
        ["UsedThisRound", "MissingAmmunition", "RoundsToLoad", "GritCost"],
        "Sprint 52 policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint52Tests.cs"),
        ["LightningReloadAvailableWithoutGritSpend",
         "LightningReloadRequiresPositiveGritAndRoundAvailability",
         "LightningReloadPreservesEligibleBrokenState",
         "LightningReloadUnitUseIsIndependent",
         "LightningReloadRejectsStateAndResourceGates",
         "LightningReloadInvalidInputRejected"], "Sprint 52 tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/LightningReloadBlueprints.cs"),
        ["UnitCommand.CommandType.Swift", "LightningReloadRoundMarker",
         "LightningReloadAbilityLogic"], "Sprint 52 blueprints")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/LightningReloadRuntime.cs"),
        ["TryReloadRounds", "ReadGrit", "RemoveFact(marker)"],
        "Sprint 52 runtime")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["LightningReloadBlueprints.Register", "lightningReload.Feature"],
        "Sprint 52 progression")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerLightningReload",
         "lightning-reload-first-use", "lightning-reload-round-gate",
         "lightning-reload-broken-and-grit"], "Sprint 52 guarded scenario")
    print("Sprint 52 source invariant validation passed with inherited Sprint 51 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 52 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
