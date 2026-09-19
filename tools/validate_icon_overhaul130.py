#!/usr/bin/env python3
"""Validate release 0.0.130 metadata while retaining all inherited gates.

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

VERSION = "0.0.130"
INFORMATIONAL_VERSION = "0.0.130-icon-art-overhaul"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = "KingmakerGunslinger-0.0.130-local-runtime.zip"
    baseline.PACKAGE_SUFFIX = "icon-art-overhaul"
    baseline.validate(root)
    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))["iconOverhaul130"]
    expected = {
        "deterministicTestCount": 1653,
        "publicReleaseAuthorized": True,
        "allNinetyImagesOwnerApproved": True,
        "racialActionWidgetBindingQualified": True,
        "higherFeatRootsTwelveCombinationsQualified": True,
        "runtimeQualificationPending": False,
        "compatibilityRuntimeQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Icon overhaul release metadata mismatch: {key}")
    baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.130.md",
        INFORMATIONAL_VERSION, "full release", "owner",
        "90", "icon art overhaul", "NOT RUN")
    baseline.require_tokens(root / "planning/ICON-OVERHAUL-STATE.md",
        "0.0.130-icon-art-overhaul")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Icon overhaul {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Icon overhaul {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
