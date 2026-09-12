#!/usr/bin/env python3
"""Validate the 0.0.127 firearm maintenance, rest recovery, and misfire interruption candidate."""
from __future__ import annotations
import argparse
import json
import sys
sys.dont_write_bytecode = True
from pathlib import Path

import validate_word_of_recall_oracle126 as baseline

VERSION = "0.0.127"
INFORMATIONAL_VERSION = "0.0.127-firearm-maintenance"
PACKAGE = "KingmakerGunslinger-0.0.127-local-runtime.zip"
PACKAGE_SUFFIX = "firearm-maintenance"
DETERMINISTIC_TEST_COUNT = 1611
MANIFEST_TOTAL = 1886
MANIFEST_ACTIVE = 1884
STATIC_KEY = "firearmMaintenance127"


def require_tokens(path: Path, *tokens: str) -> str:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks release contract(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = "wordOfRecallOracle126"
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    baseline.validate(root)
    # The 0.0.126 release record remains authoritative history.
    require_tokens(root / "docs/RELEASE-NOTES-0.0.126.md",
        "0.0.126-word-of-recall-oracle", "owner")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.127.md",
        INFORMATIONAL_VERSION,
        "Z-FIREARM-MAINTENANCE",
        "out-of-combat, Broken-only full-round maintenance",
        "completed full rest",
        "misfire break",
        "PENDING")
    static = json.loads((root / "validation/static-validation.json").read_text(encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the firearm maintenance release.")
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": False,
        "fieldRepairBrokenOnlyOutOfCombat": True,
        "completedRestRestoresCarriedDamage": True,
        "committedBreakStopsSequence": True,
        "quickClearCombatRouteUnchanged": True,
        "runtimeQualificationPending": True,
        "compatibilityRuntimeQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"firearmMaintenance127 static mismatch: {key}")
    # Mission records must stay present and current.
    require_tokens(root / "Z-FIREARM-MAINTENANCE-MISSION.md",
        "Mission ID:** `Z-FIREARM-MAINTENANCE`")
    require_tokens(root / "docs/FIREARM-MAINTENANCE-CONTRACT.md",
        "Status: **IMPLEMENTED (0.0.127 candidate",
        "StopRestProcess",
        "Player.IsInCombat",
        "CommitConditionTransition")
    require_tokens(root / "docs/FIREARM-MAINTENANCE-ACCEPTANCE.md",
        "| F01 |", "| R10 |", "| A08 |", "| Q03 |")
    # Field repair is Broken-only and out-of-combat at every layer.
    require_tokens(root / "src/KingmakerGunslinger/Actions/FirearmActionPolicy.cs",
        "Cannot repair firearms during combat.",
        "This firearm is Wrecked and requires a completed full rest.")
    require_tokens(root / "src/KingmakerGunslinger/Recovery/FirearmRepairTransactionService.cs",
        "WreckedRequiresRest")
    require_tokens(root / "src/KingmakerGunslinger/Recovery/RepairCommandStartBinding.cs",
        '[HarmonyPatch(typeof(UnitUseAbility), "OnStart")]',
        "BlueprintBootstrap.RepairTestMusketAbility")
    # Rest restoration runs only at the genuine completed-rest boundary.
    require_tokens(root / "src/KingmakerGunslinger/Gunsmithing/CompletedRestMaintenancePatch.cs",
        '[HarmonyPatch(typeof(RestController), "StopRestProcess")]',
        "status.RestSucceeded",
        "!status.NightRandomEncounter",
        "!status.SkipTime",
        "FirearmStateMachine.Repair(beforeState)")
    require_tokens(root / "src/KingmakerGunslinger/Gunsmithing/CraftingRestResetPatch.cs",
        "ApplyRest")
    # A committed break stops the sequence before the next real shot.
    require_tokens(root / "src/KingmakerGunslinger/Misfires/FirearmMisfireRuntime.cs",
        "TryGetCommittedDegradation",
        "OnCommittedDegradation(")
    require_tokens(root / "src/KingmakerGunslinger/Firing/FreeActionFullAttackReloadPatch.cs",
        "full-attack.ended-after-committed-break")
    require_tokens(root / "src/KingmakerGunslinger/Firing/EmptyFirearmAttackCommandPatch.cs",
        "RejectInterrupted",
        "MayResumeCapturedAttack(",
        "MarkPlayerAttackFrame")
    # Quick Clear keeps its own combat route.
    require_tokens(root / "src/KingmakerGunslinger/Deeds/QuickClearRuntime.cs",
        "FirearmStateMachine.Repair(before)")
    # Player-facing documents state the new rules.
    require_tokens(root / "KNOWN-ISSUES.md",
        "RESOLVED (Z-FIREARM-MAINTENANCE, 0.0.127)")
    require_tokens(root / "CHANGELOG.md",
        "0.0.127-firearm-maintenance")
    require_tokens(root / "SMOKE-TEST-GUIDE.md",
        "Kingmaker Gunslinger 0.0.127 player smoke test",
        "Wrecked is rest-only",
        "Full-rest restoration")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Firearm maintenance {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Firearm maintenance {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
