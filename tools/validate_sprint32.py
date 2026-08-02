#!/usr/bin/env python3
"""Portable source validator for Sprint 32 with inherited Sprint 31 checks."""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_sprint31

VERSION = "0.0.32"
INFORMATIONAL_VERSION = "0.0.32-s32-scatter-attacks"
TEST_COUNT = 662


def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 32 file is missing: {relative}")
    return path.read_text(encoding="utf-8")


def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")


def validate(root: Path, version: str = VERSION,
             informational_version: str = INFORMATIONAL_VERSION,
             test_count: int = TEST_COUNT,
             expected_active_blueprints: int = validate_sprint31.EXPECTED_ACTIVE_BLUEPRINTS,
             expected_ledger_entries: int = validate_sprint31.EXPECTED_LEDGER_ENTRIES) -> None:
    root = root.resolve()
    validate_sprint31.validate(
        root,
        version=version,
        informational_version=informational_version,
        test_count=test_count,
        expected_active_blueprints=expected_active_blueprints,
        expected_ledger_entries=expected_ledger_entries,
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Scatter/ScatterConeDistanceService.cs"),
        ["authorizedDistanceFeet.HasValue", "MetersPerFoot", "definition.IsScatter", "five-foot step"],
        "Sprint 32 fail-closed cone distance boundary",
    )
    require_tokens(
        read(root, "planning/SPRINT-32-ENTRY-CRITERIA.md"),
        ["separate attack roll", "-2", "No cone length may be invented",
         "exact unit reference", "Two consecutive PASS runs"],
        "Sprint 32 entry criteria",
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Scatter/ScatterTargetPlanService.cs"),
        ["ReferenceIdentityComparer.Instance", "ScatterGeometryDisposition.Unknown",
         "ReferenceEquals(candidate.Unit, exactWielder)", "accepted.Sort"],
        "Sprint 32 scatter target planner",
    )
    require_tokens(
        read(root, "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        ['Case("scatter.plan-empty"', 'Case("scatter.plan-dedupe-reference"',
         'Case("scatter.plan-unknown-fails-closed"',
         'Case("scatter.candidate-invalid-distance"'],
        "Sprint 32 scatter target tests",
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Scatter/ScatterAttackVolleyService.cs"),
        ["ReferenceIdentityComparer.Instance", "IsScatter",
         "IsMisfire(definition.MisfireValue)", "count != plan.TargetCount"],
        "Sprint 32 scatter volley aggregation",
    )
    require_tokens(
        read(root, "docs/decisions/ADR-0037-native-scatter-cone-and-volley.md"),
        ["45-degree half-angle", "RuleAttackWithWeapon", "AttackBonusPenalty",
         "remains unavailable"],
        "Sprint 32 native cone decision",
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Scatter/ScatterDischargeService.cs"),
        ["deliveryPrerequisitesSatisfied", "RejectedBeforeDelivery",
         "_discharge.Evaluate(state,", "effectiveCondition",
         "result.RoundsConsumed"],
        "Sprint 32 one-discharge boundary",
    )
    require_tokens(
        read(root, "src/KingmakerGunslinger/Scatter/ScatterExplosionDamageService.cs"),
        ["RequiresBurstDamage", "AllRollsMisfire",
         "ScatterExplosionDamageDecision(true, 3)",
         "ScatterExplosionDamageDecision(true, 1)"],
        "Sprint 32 scatter explosion multiplier",
    )
    print("Sprint 32 source invariant validation passed with inherited Sprint 31 checks.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 32 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
