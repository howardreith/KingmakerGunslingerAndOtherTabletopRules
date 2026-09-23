#!/usr/bin/env python3
"""Validate release 0.0.136 metadata while retaining all inherited gates.

Mechanical acceptance is the domain suite and guarded runtime evidence, not
these documentation/metadata checks. This release ships with its guarded
scenario unexecuted under an explicit owner waiver, and that waiver is required
to be recorded rather than hidden.
"""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_magic_circle134 as baseline

VERSION = "0.0.136"
INFORMATIONAL_VERSION = "0.0.136-rapid-reload-proficiency-gate"
PACKAGE = "KingmakerGunslinger-0.0.136-local-runtime.zip"
PACKAGE_SUFFIX = "rapid-reload-proficiency-gate"
DETERMINISTIC_TEST_COUNT = 1733


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)
    metadata = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    state = metadata["rapidReloadProficiencyGate"]
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": True,
        "parentPrerequisiteGrouping": "Prerequisite.GroupType.Any",
        "classPrerequisiteAdded": False,
        "legacyWrapperRepublished": False,
        # The release ships without native qualification under an explicit
        # owner waiver. Both facts must stay recorded together.
        "nativeRuntimeQualified": False,
        "ownerWaivedNativeQualification": True,
        "saveLoadCompatibilityVerified": False,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(
                f"Rapid Reload gate release metadata mismatch: {key}")
    if state.get("parentGateKinds") != ["Pistol", "Musket", "Blunderbuss"]:
        raise AssertionError("Rapid Reload parent gate kinds changed")
    findings = state.get("reviewFindingsAddressed") or []
    for marker in ("R1", "R2", "R3", "R4", "R5", "R6", "R7"):
        if not any(str(entry).startswith(marker) for entry in findings):
            raise AssertionError(f"Release metadata lost review finding {marker}")
    # The release notes must state the waiver in the owner's own terms rather
    # than implying the scenario ran.
    baseline.baseline.baseline.baseline.require_tokens(
        root / "docs/RELEASE-NOTES-0.0.136.md",
        INFORMATIONAL_VERSION, "owner authorized", "Rapid Reload",
        "firearm proficiency", "NOT RUN", "waived", "uninstall")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Rapid Reload gate {VERSION} validation failed: {exc}",
              file=sys.stderr)
        return 1
    print(f"Rapid Reload gate {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
