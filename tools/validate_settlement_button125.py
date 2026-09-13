#!/usr/bin/env python3
"""Validate the settlement-teleport button width release candidate."""
from __future__ import annotations
import argparse
import json
import sys
sys.dont_write_bytecode = True
from pathlib import Path

import validate_teleport_polish124 as baseline

VERSION = "0.0.125"
INFORMATIONAL_VERSION = "0.0.125-settlement-button-width"
PACKAGE = "KingmakerGunslinger-0.0.125-local-runtime.zip"
PACKAGE_SUFFIX = "settlement-button-width"
DETERMINISTIC_TEST_COUNT = 1576
MANIFEST_TOTAL = 1886
MANIFEST_ACTIVE = 1884


def require_tokens(path: Path, *tokens: str) -> str:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks release contract(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = "settlementButton125"
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    baseline.validate(root)
    # The 0.0.124 release record remains authoritative history.
    require_tokens(root / "docs/RELEASE-NOTES-0.0.124.md",
        "0.0.124-teleport-polish-specialist", "owner",
        "special-spell cache", "Spellbook.PostLoad",
        "1,574")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.125.md",
        "0.0.125-settlement-button-width", "owner screenshot",
        "Settlement Teleport", "narrow native button",
        "label preferred width", "native padding",
        "native action region", "restored byte-for-byte",
        "no second confirmation", "1,576")
    static = json.loads((root / "validation/static-validation.json").read_text(encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the settlement button width release.")
    state = static.get("settlementButton125", {})
    expected = {
        "deterministicTestCount": globals().get("ARCHIVED_TEST_COUNT", DETERMINISTIC_TEST_COUNT),
        "publicReleaseAuthorized": True,
        "settlementButtonWidenedToLabel": "label preferred width at the native font size plus the control native padding, capped by the settled native action region",
        "settlementButtonRestoreExact": True,
        "settlementButtonNoCumulativeGrowth": True,
        "runtimeQualificationPending": True,
        "compatibilityRuntimeQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"settlementButton125 static mismatch: {key}")
    # The cosmetic widening source contracts this release depends on.
    src = root / "src/KingmakerGunslinger/Spells/Teleportation"
    require_tokens(src / "WorldMapPointSpellActionRuntime.cs",
        "WidenSettlementControl", "RestoreSettlementLayout", "SettlementControlLayout",
        "RelabelSettlementControl", "RestoreSettlementControl", "NativeActionExtent")
    require_tokens(src / "TeleportContextLayoutPolicy.cs",
        "SettlementButtonWidth", "ActionRowsWidth", "RowInsideNativeExtent")
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.TeleportationCoexistence.cs",
        "settlement-width-reopen", "settlement-width-restored-again", "settlement-geometry",
        "compact-rows-fit")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Settlement button width {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Settlement button width {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
