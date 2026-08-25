#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_craft_magic_items100 as baseline

VERSION = "0.0.101"
INFORMATIONAL_VERSION = "0.0.101-craft-magic-items-compatibility"
PACKAGE = "KingmakerGunslinger-0.0.101-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1243
STATIC_KEY = "craftMagicItems101"
FOCUSED_TEST_COUNT = 15
PACKAGE_SUFFIX = "craft-magic-items-compatibility"


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.FOCUSED_TEST_COUNT = FOCUSED_TEST_COUNT
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.HUMAN_ACCEPTANCE_PENDING = False
    baseline.validate(root)

    report = (root / "docs/CRAFT-MAGIC-ITEMS-COMPATIBILITY-REPORT.md") \
        .read_text(encoding="utf-8")
    required = (
        "0.0.101-craft-magic-items-compatibility",
        "Human acceptance status: **accepted**",
        "explicitly accepted the installed 0.0.100 candidate",
    )
    missing = [token for token in required if token not in report]
    if missing:
        raise AssertionError(
            f"CMI 0.0.101 acceptance report lacks token(s): {missing}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Craft Magic Items Release {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Craft Magic Items Release {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
