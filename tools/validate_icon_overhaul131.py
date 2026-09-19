#!/usr/bin/env python3
"""Validate release 0.0.131 metadata while retaining all inherited gates.

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

VERSION = "0.0.131"
INFORMATIONAL_VERSION = "0.0.131-icon-art-overhaul"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = "KingmakerGunslinger-0.0.131-local-runtime.zip"
    baseline.PACKAGE_SUFFIX = "icon-art-overhaul"
    baseline.validate(root)
    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))["iconOverhaul131"]
    expected = {
        "deterministicTestCount": 1657,
        "publicReleaseAuthorized": True,
        "scrollItemIconsComposed": True,
        "scrollItemIconsRuntimeQualified": True,
        "runtimeQualificationPending": False,
        "compatibilityRuntimeQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Scroll item icons release metadata mismatch: {key}")
    baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.131.md",
        INFORMATIONAL_VERSION, "full release", "owner",
        "scroll", "composed", "NOT RUN")
    baseline.require_tokens(root /
        "reports/icon-overhaul/SCROLL-ITEM-ICON-QUALIFICATION.json",
        '"status": "PASS"', "two consecutive")
    baseline.require_tokens(root / "docs/ICON-ART-GUIDE.md",
        "Owner-directed scroll item convention")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Scroll item icons {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Scroll item icons {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
