#!/usr/bin/env python3
"""Validate the 0.0.139 Expanded Summoning Phase 1 release.

Retains every inherited gate. Phase 1 adds the charter's Sprints 3-8 creature
work, corrected and requalified on one candidate commit and closed out under
the owner's decision of 2026-09-26, which accepted a named engine limitation:
an active summon grab, hold, swallow or engulf, its mouth occupancy and the
held-target rake are session-scoped, and a save and a reload release them
cleanly. That acceptance must stay recorded here, must not be described as
grapple persistence, and must not be re-labelled a blocked item.
"""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_better_vendors138 as baseline
import validate_expanded_summoning_phase1

VERSION = "0.0.139"
INFORMATIONAL_VERSION = "0.0.139-expanded-summoning-phase1"
PACKAGE = "KingmakerGunslinger-0.0.139-local-runtime.zip"
PACKAGE_SUFFIX = "expanded-summoning-phase1"
DETERMINISTIC_TEST_COUNT = 1806
STATIC_KEY = "expandedSummoningPhase1Release139"
ACCEPTED_LIMITATION = "ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    require_tokens = baseline.require_tokens
    forbid_tokens = baseline.forbid_tokens

    info = json.loads((root / "Info.json").read_text(encoding="utf-8"))
    if info.get("Version") != VERSION:
        raise AssertionError("Info.json must carry the active version")

    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))[STATIC_KEY]
    expected = {
        "releaseVersion": VERSION,
        "releaseInformationalVersion": INFORMATIONAL_VERSION,
        "package": PACKAGE,
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": True,
        "ownerAuthorizedRelease": True,
        "candidateOnly": False,
        "mergedPullRequest": 23,
        "compatibilityRuntimeQualificationPending": False,
        "ownerAcceptedEngineLimitation": ACCEPTED_LIMITATION,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Expanded Summoning release metadata mismatch: {key}")
    if not isinstance(state.get("nativeRuntimeQualified"), bool):
        raise AssertionError("Native runtime qualification must be recorded explicitly")
    if state.get("nativeRuntimeQualified") and not state.get("nativeRuntimeEvidence"):
        raise AssertionError("A native qualification claim needs recorded evidence")
    if not state.get("ownerReleaseInstruction"):
        raise AssertionError("The owner's release instruction must stay recorded")

    # Phase 1's own source and record gates, re-run at the release head.
    validate_expanded_summoning_phase1.validate(root)

    notes = root / "docs/RELEASE-NOTES-0.0.139.md"
    require_tokens(notes, INFORMATIONAL_VERSION, "Expanded Summoning",
                   "owner authorized", ACCEPTED_LIMITATION, "session-scoped",
                   "uninstall")
    # The accepted limitation is never dressed up as persistence.
    forbid_tokens(notes, "grapple persistence", "persists across a save")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Expanded Summoning Phase 1 {VERSION} validation failed: {exc}",
              file=sys.stderr)
        return 1
    print(f"Expanded Summoning Phase 1 {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
