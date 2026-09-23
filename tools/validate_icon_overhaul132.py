#!/usr/bin/env python3
"""Validate release 0.0.132 metadata while retaining all inherited gates.

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

VERSION = "0.0.132"
INFORMATIONAL_VERSION = "0.0.132-icon-art-overhaul"
PACKAGE = "KingmakerGunslinger-0.0.132-local-runtime.zip"
PACKAGE_SUFFIX = "icon-art-overhaul"
# Later candidates that append exact identities override these totals.
MANIFEST_TOTAL = 1913
MANIFEST_ACTIVE = 1911


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = ("KingmakerGunslinger-0.0.132-local-runtime.zip"
        if VERSION == "0.0.132" else PACKAGE)
    baseline.PACKAGE_SUFFIX = ("icon-art-overhaul"
        if VERSION == "0.0.132" else PACKAGE_SUFFIX)
    # This feature branch appends exact save identities to the published ledger.
    # The original prefix and every protected-art gate remain authoritative.
    from validate_magic_circle import validate as validate_magic_circle
    validate_magic_circle(root)
    if VERSION == "0.0.132":
        baseline.DETERMINISTIC_TEST_COUNT = 1660
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    # The assigned Magic Circle feature fixes a proven shared source bug.
    # Its gate validates the two exact edits and retains the original digest
    # for every other Protection control/publication file.
    import validate_midgame_firearms116 as protection_baseline
    from validate_magic_circle import PROTECTION_SOURCE_SHA256
    old_protection_digest = protection_baseline.PROTECTION_CONTROL_SOURCE_SHA256
    try:
        protection_baseline.PROTECTION_CONTROL_SOURCE_SHA256 = PROTECTION_SOURCE_SHA256
        baseline.validate(root)
    finally:
        protection_baseline.PROTECTION_CONTROL_SOURCE_SHA256 = old_protection_digest
    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))["iconOverhaul132"]
    expected = {
        "deterministicTestCount": 1657,
        "publicReleaseAuthorized": True,
        "scrollItemIconsNativeShellComposite": True,
        "scrollItemIconsRuntimeQualified": True,
        "runtimeQualificationPending": False,
        "compatibilityRuntimeQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Native scroll shell release metadata mismatch: {key}")
    baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.132.md",
        "0.0.132-icon-art-overhaul" if VERSION != "0.0.132" else INFORMATIONAL_VERSION,
        "full release", "owner",
        "native", "composites", "NOT RUN")
    baseline.require_tokens(root /
        "reports/icon-overhaul/SCROLL-ITEM-ICON-QUALIFICATION.json",
        "asset compositing on the exact native scroll shell",
        '"status": "PASS"')
    baseline.require_tokens(root / "docs/ICON-ART-GUIDE.md",
        "ASSET COMPOSITES on the exact native scroll shell")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Native scroll shell {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Native scroll shell {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
