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
MILESTONE_LABEL = "ELVEN-BRANCHED-SPEAR"
PACKAGE_SUFFIX = "elven-branched-spear"


def validate(root: Path) -> None:
    validate_summoning78.VERSION = VERSION
    validate_summoning78.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_summoning78.PACKAGE = PACKAGE
    validate_summoning78.MILESTONE_LABEL = MILESTONE_LABEL
    validate_summoning78.validate(root)

    package_script = (root / "scripts/package.ps1").read_text(encoding="utf-8")
    effective_suffix = ("bodyguard-in-harms-way" if VERSION in {"0.0.90", "0.0.91", "0.0.92", "0.0.93", "0.0.94", "0.0.95"}
        else PACKAGE_SUFFIX)
    if f"$($info.Id)-$($info.Version)-{effective_suffix}.zip" not in package_script:
        raise AssertionError(f"{MILESTONE_LABEL} package identity missing")
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
