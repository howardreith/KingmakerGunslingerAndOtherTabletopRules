#!/usr/bin/env python3
"""Validate release 0.0.129 metadata while retaining all inherited gates.

Mechanical acceptance is the domain suite and guarded runtime evidence, not
these documentation/metadata checks.
"""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_firearm_postrelease128 as baseline

VERSION = "0.0.129"
INFORMATIONAL_VERSION = "0.0.129-recall-and-smart-scrolls"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = "KingmakerGunslinger-0.0.129-local-runtime.zip"
    baseline.PACKAGE_SUFFIX = "recall-and-smart-scrolls"
    baseline.validate(root)
    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))["recallSmartScroll129"]
    expected = {
        "deterministicTestCount": 1632,
        "publicReleaseAuthorized": True,
        "nativeOracleNormalChoiceQualified": True,
        "oracleLearningExtraGrant": False,
        "recallDirectCast": True,
        "directSuccessAnnouncements": False,
        "automaticScrollReaders": True,
        "nativeScrollConsumptionPreserved": True,
        "runtimeQualificationPending": False,
        "compatibilityRuntimeQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Recall release metadata mismatch: {key}")
    baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.129.md",
        INFORMATIONAL_VERSION, "full release", "native level-up", "NOT RUN")
    baseline.require_tokens(root / "Z-RECALL-AND-SMART-SCROLL-STATE.md",
        "0.0.129", "full release", "ff5a1f09")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Recall {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Recall {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
