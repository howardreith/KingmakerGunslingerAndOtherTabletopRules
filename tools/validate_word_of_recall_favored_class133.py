#!/usr/bin/env python3
"""Validate release 0.0.133 metadata while retaining all inherited gates.

Mechanical acceptance is the domain suite and guarded runtime evidence, not
these documentation/metadata checks.
"""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_icon_overhaul132 as baseline

VERSION = "0.0.133"
INFORMATIONAL_VERSION = "0.0.133-word-of-recall-favored-class"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = "KingmakerGunslinger-0.0.133-local-runtime.zip"
    baseline.PACKAGE_SUFFIX = "word-of-recall-favored-class"
    # The live deterministic-suite count propagates through the 0.0.128
    # module's global into every inherited live Program.cs gate; the
    # superseded 0.0.128/0.0.132 release records stay at their archived
    # 1,657-case snapshots.
    baseline.baseline.DETERMINISTIC_TEST_COUNT = 1669
    baseline.ARCHIVED_TEST_COUNT = 1657
    baseline.baseline.ARCHIVED_TEST_COUNT = 1657
    baseline.validate(root)
    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))["wordOfRecallFavoredClass133"]
    expected = {
        "deterministicTestCount": 1669,
        "publicReleaseAuthorized": False,
        "favoredClassPickGateVariants": True,
        "favoredClassRuntimeQualified": True,
        "runtimeQualificationPending": False,
        "compatibilityRuntimeQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Favored Class release metadata mismatch: {key}")
    baseline.baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.133.md",
        INFORMATIONAL_VERSION, "favored class", "Favored Class",
        "variants", "pick gate", "pending owner approval")
    baseline.baseline.require_tokens(root /
        "Z-WORD-OF-RECALL-FAVORED-CLASS-STATE.md",
        "variantRecall=1", "66/66")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Favored Class {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Favored Class {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
