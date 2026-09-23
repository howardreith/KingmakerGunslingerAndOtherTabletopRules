#!/usr/bin/env python3
"""Validate release 0.0.137 metadata while retaining all inherited gates.

Mechanical acceptance is the domain suite and guarded runtime evidence, not
these documentation/metadata checks. This release ships under explicit owner
authorization after the guarded native scenario passed on the candidate, with
three pre-existing harness expectations corrected at the owner's direction.
"""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_rapid_reload_gate136 as baseline

VERSION = "0.0.137"
INFORMATIONAL_VERSION = "0.0.137-rapid-reload-combat-feat"
PACKAGE = "KingmakerGunslinger-0.0.137-local-runtime.zip"
PACKAGE_SUFFIX = "rapid-reload-combat-feat"
DETERMINISTIC_TEST_COUNT = 1704


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)
    metadata = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    state = metadata["rapidReloadCombatFeat137"]
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": True,
        "parentGroups": ["Feat", "CombatFeat"],
        "genericSelectionHelperChanged": False,
        "publicationChanged": False,
        "proficiencyGateChanged": False,
        "nativeScenarioExecuted": True,
        "nativeScenarioOverallPass": True,
        "harnessExpectationsCorrectedByOwnerDirection": True,
        "classificationAssertionPassed": True,
        "fighterBonusSlotAcquisitionPassed": True,
        "ownerAuthorizedRelease": True,
        "saveLoadCompatibilityVerified": False,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(
                f"Rapid Reload combat-feat release metadata mismatch: {key}")
    if len(state.get("harnessCorrections") or []) != 3:
        raise AssertionError("The three harness corrections must stay recorded")
    baseline.baseline.baseline.baseline.baseline.require_tokens(
        root / "docs/RELEASE-NOTES-0.0.137.md",
        INFORMATIONAL_VERSION, "owner authorized", "Rapid Reload",
        "combat feat", "Fighter", "PASS", "uninstall")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Rapid Reload combat-feat {VERSION} validation failed: {exc}",
              file=sys.stderr)
        return 1
    print(f"Rapid Reload combat-feat {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
