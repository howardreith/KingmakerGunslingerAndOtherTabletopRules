#!/usr/bin/env python3
from __future__ import annotations
import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_summoning78

VERSION = "0.0.79"
INFORMATIONAL_VERSION = "0.0.79-elven-branched-spear"
PACKAGE = "KingmakerGunslinger-0.0.79-local-runtime.zip"


def validate(root: Path) -> None:
    validate_summoning78.VERSION = VERSION
    validate_summoning78.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_summoning78.PACKAGE = PACKAGE
    validate_summoning78.MILESTONE_LABEL = "ELVEN-BRANCHED-SPEAR"
    validate_summoning78.validate(root)

    package_script = (root / "scripts/package.ps1").read_text(encoding="utf-8")
    if "$($info.Id)-$($info.Version)-elven-branched-spear.zip" not in package_script:
        raise AssertionError("Elven Branched Spear package identity missing")
    if "expanded-summoning.zip" in package_script:
        raise AssertionError("Package script can still select the Expanded Summoning archive")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Elven Branched Spear {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Elven Branched Spear {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
