#!/usr/bin/env python3
"""Retain Magic Circle gates for the owner-approved 0.0.135 release metadata."""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_magic_circle134 as baseline

VERSION = "0.0.135"
INFORMATIONAL_VERSION = "0.0.135-magic-circle-alignment-spells"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.validate(root)
    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))["magicCircle135"]
    if state.get("publicReleaseAuthorized") is not True or state.get(
            "gameplayChangesAfterApproval") is not False:
        raise AssertionError("Release must retain owner approval and unchanged gameplay")
    if state.get("additionalTests") != (
            "NOT RUN: explicitly prohibited by the owner for release preparation."):
        raise AssertionError("Release must retain the explicit no-additional-tests record")
    notes = (root / "docs/RELEASE-NOTES-0.0.135.md").read_text(encoding="utf-8")
    for token in (INFORMATIONAL_VERSION, "owner authorized", "No additional tests",
                  "new qualifying control", "Paladin", "Antipaladin"):
        if token not in notes:
            raise AssertionError(f"Release notes omit {token}")


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
