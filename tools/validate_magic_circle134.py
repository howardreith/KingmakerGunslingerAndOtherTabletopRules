#!/usr/bin/env python3
"""Retain the inherited release gates and the exact Magic Circle contracts."""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_word_of_recall_favored_class133 as baseline

VERSION = "0.0.134"
INFORMATIONAL_VERSION = "0.0.134-magic-circle-alignment-spells"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = "KingmakerGunslinger-0.0.134-local-runtime.zip"
    baseline.PACKAGE_SUFFIX = "magic-circle-alignment-spells"
    baseline.DETERMINISTIC_TEST_COUNT = 1699
    baseline.validate(root)
    metadata = json.loads((root / "validation/static-validation.json").read_text(encoding="utf-8"))
    state = metadata["magicCircle134"]  # Historical acceptance; never transferred to new pixels.
    followup = metadata["magicCircleFollowup"]
    if followup.get("publicReleaseAuthorized") is not False or followup.get("deterministicTestCount") != 1699:
        raise AssertionError("Follow-up candidate must retain its own test count and no public release authorization")
    for key, value in {
        "deterministicTestCount": 1678,
        "publicReleaseAuthorized": True,
        "sharedProtectionControlAuthority": True,
        "preparationBindingBeforeMutation": True,
        "ownedRuntimeLeaseRecovery": True,
        "approvedArtworkPreserved": True,
        "bearerDeathEndsCircle": True,
        "compatibilityRuntimeQualificationPending": True,
    }.items():
        if state.get(key) != value:
            raise AssertionError(f"Magic Circle release metadata mismatch: {key}")
    baseline.baseline.baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.134.md",
        INFORMATIONAL_VERSION, "owner authorized", "new qualifying control",
        "KMG_AUTOMATION_WORKING", "Gamepad", "inward", "uninstall")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Magic Circle {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Magic Circle {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
