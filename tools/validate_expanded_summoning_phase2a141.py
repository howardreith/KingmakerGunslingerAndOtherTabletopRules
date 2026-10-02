#!/usr/bin/env python3
"""Validate the 0.0.141 Expanded Summoning Phase 2A release boundary.

Sprints 9-11 are the published 0.0.141 build and are immutable. Development of
Sprints 12-21 continues on the same version, which is why the static record
carries a separate development section; this validator pins the published
boundary and guards that development state.

Sprints 12 and 13 have since qualified and published, and Sprint 14's three
insects are registered ahead of their own qualification and withheld. So the
suppression check runs in both directions: a creature that has qualified must
not be suppressed, and a creature that has not must be.
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
DETERMINISTIC_TEST_COUNT = 1985
STATIC_KEY = "expandedSummoningPhase2A141"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    require_tokens = baseline.baseline.require_tokens
    # Sprints 9-13 are qualified and published; Sprint 14's three insects are
    # registered and withheld. The published surface is therefore the
    # registered surface of 952 less the 48 withheld, which is the 904 that
    # every roster, census and player-path gate reconciles against.
    require_tokens(root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
        "RegisteredLogicalPlacementCount = 952",
        "SuppressedLogicalPlacementCount = 48",
        "RegisteredLogicalPlacementCount - SuppressedLogicalPlacementCount")
    visibility = (root / "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs").read_text(encoding="utf-8")
    for key in ('"dire-rat"', '"dog"', '"hyena"', '"goblin-dog"',
                '"shadow-mastiff"', '"wolverine"', '"poisonous-frog"'):
        if key in visibility:
            raise AssertionError(
                f"A qualified creature is still suppressed: {key}")
    # The other direction, which is the one that matters while a sprint is in
    # flight: an unqualified creature may be registered but may never be
    # published, and removing its key here is the act that publishes it.
    for key in ('"fire-beetle"', '"giant-ant-worker"', '"giant-ant-soldier"'):
        if key not in visibility:
            raise AssertionError(
                f"An unqualified Sprint 14 creature is not suppressed: {key}")
    require_tokens(root / "planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md",
        "Sprints 9-13", "904 published generated", "933 total choices")
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
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
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
        "authorizedSprintRange": "12-21",
        "publicReleaseAuthorized": False,
        "candidateOnly": True,
        "registeredGeneratedPlacements": 952,
        "suppressedGeneratedPlacements": 48,
        "publishedGeneratedPlacements": 904,
        "retainedNativeWrappers": 29,
        "visibleChoiceTotal": 933,
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
