#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_in_harms_way94 as baseline

VERSION = "0.0.95"
INFORMATIONAL_VERSION = "0.0.95-immediate-action-economy"
PACKAGE = "KingmakerGunslinger-0.0.95-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1218
STATIC_KEY = "immediateAction95"
IMMEDIATE_DEBT_IDENTITIES = {
    "KMG.Feats.InHarmsWayImmediatePending": (
        "a92164067bad3a85b1da48db5a787686", "BlueprintFeature"),
    "KMG.Feats.InHarmsWayImmediateChargedTurn": (
        "326e183f7791e83a38337c6a6d7a8644", "BlueprintFeature"),
}
EXPECTED_LEDGER_ENTRIES_BY_VERSION = {"0.0.115": 1759}
EXPECTED_ACTIVE_BLUEPRINTS_BY_VERSION = {"0.0.115": 1757}


def require_tokens(path: Path, *tokens: str) -> None:
    if not path.is_file():
        raise AssertionError(f"Immediate-action file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks immediate-action contract token(s): {missing}")


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.EXPECTED_LEDGER_ENTRIES = \
        EXPECTED_LEDGER_ENTRIES_BY_VERSION.get(VERSION, 1706)
    baseline.EXPECTED_ACTIVE_BLUEPRINTS = \
        EXPECTED_ACTIVE_BLUEPRINTS_BY_VERSION.get(VERSION, 1704)
    baseline.PROJECT_BLUEPRINT_COUNT = 14
    baseline.ADDITIONAL_IDENTITIES = dict(IMMEDIATE_DEBT_IDENTITIES)
    baseline.validate(root)

    required = (
        "docs/investigations/in-harms-way-immediate-action-economy.md",
        "src/KingmakerGunslinger/BodyguardFeats/ImmediateActionEconomyPolicy.cs",
        "src/KingmakerGunslinger/BodyguardFeats/ImmediateActionEconomyRuntime.cs",
        "src/KingmakerGunslinger/BodyguardFeats/ImmediateActionEconomyPatches.cs",
        "src/KingmakerGunslinger/BodyguardFeats/BodyguardActionEconomyAccess.cs",
        "tests/KingmakerGunslinger.DomainTests/ImmediateActionEconomyPolicyTests.cs",
        "tests/KingmakerGunslinger.DomainTests/BodyguardRuntimeContractTests.cs",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Immediate-action file missing: {relative}")

    require_tokens(root / required[0], "64528B2BB530979B",
        "HasSwiftAction", "TurnController.Prepare", "next actual turn",
        "RuleCheckTargetFlatFooted")
    require_tokens(root / required[1], "PendingNextTurn",
        "ChargedTurn", "ProtectorIsCurrentTurn", "FlatFooted",
        "OnActualTurnStarted", "OnActualTurnCompleted", "OnTurnDelayed")
    require_tokens(root / required[2], "ImmediatePending",
        "ImmediateChargedTurn", "TurnStatus.Delayed", "RestoreAfterLoad",
        "SwiftActionCooldownSeconds", "ClearAll")
    require_tokens(root / required[3],
        "typeof(TurnController), \"Prepare\"",
        "typeof(UnitCombatState.Cooldowns), \"Clear\"",
        "typeof(TurnController), \"Dispose\"",
        "typeof(UnitEntityData), \"HasSwiftAction\"",
        "typeof(UnitEntityData), \"PostLoad\"")
    require_tokens(root / required[4], "RuleCheckTargetFlatFooted",
        "TryAddPending", "TryRollbackImmediateAction")

    project = (root / "src/KingmakerGunslinger/"
        "KingmakerGunslinger.csproj").read_text(encoding="utf-8")
    test_project = (root / "tests/KingmakerGunslinger.DomainTests/"
        "KingmakerGunslinger.DomainTests.csproj").read_text(encoding="utf-8")
    runner = (root / "tests/KingmakerGunslinger.DomainTests/"
        "Program.cs").read_text(encoding="utf-8")
    for name in ("ImmediateActionEconomyPolicy.cs",
                 "ImmediateActionEconomyRuntime.cs",
                 "ImmediateActionEconomyPatches.cs"):
        if name not in project:
            raise AssertionError(f"Production file is not compiled: {name}")
    if "ImmediateActionEconomyPolicyTests.cs" not in test_project:
        raise AssertionError("Immediate-action tests are not compiled")
    if "immediate-economy.turn-aware" not in runner:
        raise AssertionError("Immediate-action tests are not registered")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Immediate Action Economy {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Immediate Action Economy {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
