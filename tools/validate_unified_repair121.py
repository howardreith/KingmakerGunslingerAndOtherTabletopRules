#!/usr/bin/env python3
"""Validate the unified firearm maintenance release candidate."""
from __future__ import annotations
import argparse
import json
import sys
sys.dont_write_bytecode = True
from pathlib import Path
import validate_teleportation119 as baseline

VERSION = "0.0.121"
INFORMATIONAL_VERSION = "0.0.121-unified-firearm-maintenance"
DETERMINISTIC_TEST_COUNT = 1561
MANIFEST_TOTAL = 1886
MANIFEST_ACTIVE = 1884
ELEMENTAL_TOTAL = 241
ELEMENTAL_ACTIVE = 240


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = "KingmakerGunslinger-0.0.121-local-runtime.zip"
    baseline.PACKAGE_SUFFIX = "unified-firearm-maintenance"
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = "unifiedRepair121"
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    baseline.baseline.ELEMENTAL_TOTAL = ELEMENTAL_TOTAL
    baseline.baseline.ELEMENTAL_ACTIVE = ELEMENTAL_ACTIVE
    baseline.baseline.TRAIT_MECHANICS_IMPLEMENTATION_PENDING = False
    baseline.validate(root)
    # The 0.0.120 elemental release records remain authoritative history.
    baseline.require_tokens(root / "ELEMENTAL-RACES-COMPLETION.md",
        "8e5eeae7973c71ca4b78dc8216d00d815af7ea26",
        "72520eeb66a036da50473bec7b2e0019ca6304f6",
        "dd0a344226c31e34d251702f5832e75525137260",
        "Nereid Fascination", "Treacherous Earth", "0.0.120")
    baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.121.md",
        INFORMATIONAL_VERSION, "owner",
        "exactly one maintenance action", "reusable Gunsmith",
        "KNOWN-ISSUES.md", "1,554", "32/32", "16/16")
    baseline.require_tokens(root / "KNOWN-ISSUES.md",
        "Unified firearm repair has one unresolved owner decision",
        "Wrecked", "combat")
    static = json.loads((root / "validation/static-validation.json").read_text(encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the unified repair release.")
    state = static.get("unifiedRepair121", {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": True,
        "unifiedRepairRuntimeAssertions": 32,
        "oldSaveAliasRuntimeAssertions": 16,
        "wreckedCombatAvailabilityOwnerDecisionPending": True,
        "retiredKitVendorSweepQualified": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"unifiedRepair121 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Unified repair {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Unified repair {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
