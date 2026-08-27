#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_overhaul_summon_fatigue103 as baseline

VERSION = "0.0.104"
INFORMATIONAL_VERSION = "0.0.104-summon-same-turn-activation"
PACKAGE = "KingmakerGunslinger-0.0.104-local-runtime.zip"
PACKAGE_SUFFIX = "summon-same-turn-activation"
DETERMINISTIC_TEST_COUNT = 1305
STATIC_KEY = "summonSameTurn104"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.104 mission file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.104 mission token(s): {missing}")
    return text


def reject_tokens(path: Path, *tokens: str) -> None:
    text = require_tokens(path)
    present = [token for token in tokens if token in text]
    if present:
        raise AssertionError(
            f"{path.name} retains forbidden summon-turn token(s): {present}")


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.validate(root)

    policy = root / (
        "src/KingmakerGunslinger/Summoning/"
        "SummonSameTurnActivationPolicy.cs")
    require_tokens(policy,
        "NotInCombat", "RealTimeWithPause", "NotGenuineSummon",
        "OutsideCasterTurn", "AlreadyActed", "AlreadyEligible", "Repair",
        "HasLifecycle", "HasAppearanceLock", "LifecycleContextMatches",
        "AppearanceContextMatches",
        "NativeGraceSeconds")

    runtime = root / (
        "src/KingmakerGunslinger/Summoning/"
        "SummonSameTurnActivationRuntime.cs")
    require_tokens(runtime,
        'HarmonyPatch(typeof(RuleSummonUnit), "OnTrigger"',
        "SummonAcceleratedInvocationRuntime", "SourceAbilityContext",
        "ReferenceEquals(rule.Initiator", "SummonedUnitAppearBuff",
        "SummonedUnitBuff", "TryRepair", "RemoveFact",
        '[HarmonyPatch(typeof(UnitUseAbility), "OnEnded"',
        "SceneEntitiesState")
    reject_tokens(runtime, "SortedUnits.Add", ".CurrentTurn =",
        "ForceToEnd", ".Initiative =")

    scenario = require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/"
        "SummonSameTurnActivationScenario.cs"),
        "UnitUseAbility", "RuleCastSpell", "ContextActionSpawnMonster",
        "RuleSummonUnit", "CombatController", "TurnController",
        "SummonSameTurnCompatibilityQuickened",
        "SummonSameTurnCompatibilityAcadamae", "UnitBuffsController",
        "UnitActionController.UpdateCooldowns", "ShouldBeDestroyed",
        "IsSummonSameTurnCompatibilityScenario")
    if "new RuleSummonUnit" in scenario:
        raise AssertionError(
            "Guarded acceptance bypasses the actual summoning spell graph")

    require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs"),
        '"summon-same-turn-activation"',
        '"summon-same-turn-acadamae"',
        '"summon-same-turn-multiple"',
        '"summon-same-turn-native-control"',
        '"summon-same-turn-rtwp-control"',
        '"summon-same-turn-compatibility-quickened"',
        '"summon-same-turn-compatibility-acadamae"')
    require_tokens(root / "scripts/RuntimeAutomation.Common.ps1",
        "'summon-same-turn-activation'", "KMG_AUTOMATION_WORKING")

    program = require_tokens(root / (
        "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        "summon-same-turn.outside-combat", "summon-same-turn.rtw-p",
        "summon-same-turn.non-summon", "summon-same-turn.repair",
        "summon-same-turn.duplicate-callback",
        "summon-same-turn.multiple-units",
        "summon-same-turn.next-round",
        "summon-same-turn.runtime-boundary")
    if program.count('Case("summon-same-turn.') != 16:
        raise AssertionError("Expected 16 focused summon activation tests")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.104.md",
        "Kingmaker Gunslinger 0.0.104",
        "KingmakerGunslinger-0.0.104-summon-same-turn-activation.zip",
        "1,305")
    require_tokens(root / "SUMMON-SAME-TURN-ACTIVATION-JOURNAL.md",
        "pre-fix", "Quickened compatibility PASS",
        "Acadamae compatibility PASS")
    require_tokens(root / (
        "docs/SUMMON-SAME-TURN-ACTIVATION-QUALIFICATION.md"),
        "Failing baseline", "Standalone runtime matrix")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "focusedPolicyCaseCount": 16,
        "realPlayerSpellPathRequired": True,
        "ruleSummonUnitPostfixRequired": True,
        "genuineSummonProvenanceRequired": True,
        "arbitrarySpawnMutationAllowed": False,
        "turnOrderMutationAllowed": False,
        "casterActionMutationAllowed": False,
        "ordinarySummonTimingMutationAllowed": False,
        "rtwpMutationAllowed": False,
        "perSummonedUnitIdempotenceRequired": True,
        "standaloneRuntimeQualified": True,
        "cotwProfileQualified": True,
        "highestRiskProfileQualified": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.104 static mismatch: {key}")
    for key in ("finalRuntimeQualificationPending",
            "humanAcceptancePending"):
        if not isinstance(state.get(key), bool):
            raise AssertionError(
                f"0.0.104 qualification state missing: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Summon Same-Turn {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Summon Same-Turn {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
