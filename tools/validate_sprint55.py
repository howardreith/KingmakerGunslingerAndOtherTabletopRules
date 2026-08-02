#!/usr/bin/env python3
"""Portable source validator for Sprint 55 with inherited Sprint 54 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint54

VERSION = "0.0.55"
INFORMATIONAL_VERSION = "0.0.55-s55-slingers-luck"
TEST_COUNT = 807

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 55 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT, active_count: int = 101,
             total_count: int = 102) -> None:
    root = root.resolve()
    validate_sprint54.validate(root, version, informational_version,
                               test_count, active_count, total_count)
    require_tokens(read(root, "planning/SPRINT-55-ENTRY-CRITERIA.md"),
        ["exactly 2 grit", "exactly 1 grit", "second result",
         "two independent"], "Sprint 55 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/SlingersLuckService.cs") + read(root,
        "src/KingmakerGunslinger/Deeds/SlingersLuckDecision.cs"),
        ["SavingThrow ? 2 : 1", "LevelTooLow", "InsufficientGrit",
         "request.SecondRoll"], "Sprint 55 fixed-cost policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint55Tests.cs"),
        ["SlingersLuckSavingThrowCostAndSecondResult",
         "SlingersLuckSkillCheckCostAndSecondResult",
         "SlingersLuckGritGatesAreFixed", "SlingersLuckInvalidInputRejected"],
        "Sprint 55 tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/SlingersLuckRollAccess.cs"),
        ["RulebookEvent.Dice.D20", "GetSetMethod(true)", "setter.IsPublic",
         "RuleSavingThrow", "RuleSkillCheck"], "Sprint 55 exact roll access")
    reroll_handlers = read(root,
        "src/KingmakerGunslinger/Deeds/SlingersLuckSavingThrowReroll.cs") + read(
        root, "src/KingmakerGunslinger/Deeds/SlingersLuckSkillCheckReroll.cs")
    require_tokens(reroll_handlers, ["rule.D20.Value", "second.Value"],
                   "Sprint 55 natural-roll handlers")
    if "BaseRollResult" in reroll_handlers:
        raise RuntimeError(
            "Sprint 55 handlers must not validate modifier-inclusive roll totals")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/SlingersLuckBlueprints.cs"),
        ["Cost = cost", "fixed costs cannot be reduced",
         "SlingersLuckSavingThrowReroll", "SlingersLuckSkillCheckReroll"],
        "Sprint 55 blueprints")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["SlingersLuckBlueprints.Register", "slingersLuck.Feature"],
        "Sprint 55 progression")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunObserveSlingersLuckNativeRerolls",
         "slingers-luck-native-rule-contracts",
         "slingers-luck-post-trigger-replacement",
         "RunDisposableGunslingerSlingersLuck",
         "CallComponents<IInitiatorRulebookHandler<",
         "slingers-luck-saving-reroll", "slingers-luck-skill-reroll"],
        "Sprint 55 guarded observer and feature scenario")
    print("Sprint 55 source invariant validation passed with inherited Sprint 54 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try: validate(args.root)
    except Exception as exception:
        print(f"Sprint 55 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
