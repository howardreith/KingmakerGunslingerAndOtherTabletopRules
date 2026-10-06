#!/usr/bin/env python3
"""Validate the 0.0.141 Expanded Summoning Phase 2A release boundary.

Sprints 9-11 are the published 0.0.141 build and are immutable. Development of
Sprints 12-21 continues on the same version, which is why the static record
carries a separate development section; this validator pins the published
boundary and guards that development state.

Sprints 12-15 have since qualified and published. Sprint 16's complete hidden
candidate passed, so its publication candidate exposes six Dire Crocodile
placements. Public-route qualification is recorded separately from visibility.
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True

import validate_favored_class140 as baseline


VERSION = "0.0.141"
INFORMATIONAL_VERSION = "0.0.141-expanded-summoning-phase2a"
PACKAGE = "KingmakerGunslinger-0.0.141-local-runtime.zip"
PACKAGE_SUFFIX = "expanded-summoning-phase2a"
DETERMINISTIC_TEST_COUNT = 2030
STATIC_KEY = "expandedSummoningPhase2A141"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    require_tokens = baseline.baseline.require_tokens
    # Domain tests derive these counts independently from the catalog. This
    # static guard keeps metadata and publication suppression synchronized.
    require_tokens(root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
        "RegisteredLogicalPlacementCount = 976",
        "SuppressedLogicalPlacementCount = 0",
        "RegisteredLogicalPlacementCount - SuppressedLogicalPlacementCount")
    visibility = (root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs").read_text(encoding="utf-8")
    for key in ('"dire-rat"', '"dog"', '"hyena"', '"goblin-dog"',
                '"shadow-mastiff"', '"wolverine"', '"poisonous-frog"',
                '"fire-beetle"', '"giant-ant-worker"', '"giant-ant-soldier"',
                '"giant-ant-drone"', '"giant-stag-beetle"', '"dire-crocodile"'):
        if key in visibility:
            raise AssertionError(f"A qualified creature is still suppressed: {key}")
    require_tokens(root / "EXPANDED-SUMMONING-PHASE2-INVENTORY-RECONCILIATION.md",
        "832 + 29 = 861", "881db758", "d7822297",
        "visible in v0.0.140 and are hidden in v0.0.141")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.141.md",
        INFORMATIONAL_VERSION, "Sprints 9-11", "Sprint 12", "hidden",
        "owner visual review", "uninstall")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify 0.0.141")
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": 1992,
        "publicReleaseAuthorized": True,
        "candidateOnly": False,
        "releaseVersion": VERSION,
        "releaseInformationalVersion": INFORMATIONAL_VERSION,
        # These describe the immutable published v0.0.141 build, not the
        # current source; the development section below carries today's state.
        "publishedSprintRange": "9-11",
        "visibleGeneratedChoices": 832,
        "visibleChoiceTotal": 861,
        "retainedNativeWrappers": 29,
        "hiddenSprint12Placements": 68,
        "sprint12Published": False,
        "compatibilityRuntimeQualificationPending": False,
        "runtimeEvidence":
            "20260930T1626175528422Z-disposable-expanded-summoning",
        "ownerVisualReview": "NOT_PERFORMED_NONBLOCKING",
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(
                f"Expanded Summoning Phase 2A metadata mismatch: {key}")

    # Post-release Sprints 12-21 development continues on the same version.
    # The published ZIP is immutable, so the development state must declare
    # itself a candidate and must keep the derived inventory equation.
    development = static.get("expandedSummoningPhase2Development", {})
    expected_development = {
        "branch": "codex/expanded-summoning-phase2b-sprints14-17",
        "authorizedSprintRange": "9-21",
        "phase2CAuthorized": True,
        "phase2CStartDeferredUntilPhase2BOwnerAcceptance": True,
        "currentMissionLastSprint": 17,
        "publicReleaseAuthorized": False,
        "candidateOnly": True,
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "registeredGeneratedPlacements": 976,
        "suppressedGeneratedPlacements": 0,
        "publishedGeneratedPlacements": 976,
        "retainedNativeWrappers": 29,
        "visibleChoiceTotal": 1005,
        "mergeAuthorized": False,
        "newReleaseAuthorized": False,
        "sprint22Authorized": False,
        "ownerVisualReview": "NOT_PERFORMED_NONBLOCKING",
    }
    for key, value in expected_development.items():
        if development.get(key) != value:
            raise AssertionError(
                f"Phase 2 development metadata mismatch: {key}")
    if development.get("publishedGeneratedPlacements") + \
            development.get("retainedNativeWrappers") != \
            development.get("visibleChoiceTotal"):
        raise AssertionError(
            "Phase 2 development inventory equation does not balance")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Expanded Summoning Phase 2A {VERSION} validation failed: {exc}",
              file=sys.stderr)
        return 1
    print(f"Expanded Summoning Phase 2A {VERSION} validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
