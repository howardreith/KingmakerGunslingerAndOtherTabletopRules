#!/usr/bin/env python3
"""Validate the Word of Recall Oracle scroll eligibility repair release candidate."""
from __future__ import annotations
import argparse
import json
import sys
sys.dont_write_bytecode = True
from pathlib import Path

import validate_settlement_button125 as baseline

VERSION = "0.0.126"
INFORMATIONAL_VERSION = "0.0.126-word-of-recall-oracle"
PACKAGE = "KingmakerGunslinger-0.0.126-local-runtime.zip"
PACKAGE_SUFFIX = "word-of-recall-oracle"
DETERMINISTIC_TEST_COUNT = 1611
MANIFEST_TOTAL = 1886
MANIFEST_ACTIVE = 1884
STATIC_KEY = "wordOfRecallOracle126"


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
    # The 0.0.125 release record remains authoritative history.
    require_tokens(root / "docs/RELEASE-NOTES-0.0.125.md",
        INFORMATIONAL_VERSION, "owner", "narrow native button")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.126.md",
        "0.0.126-word-of-recall-oracle", "Owner reported",
        "Word of Recall", "Oracle", "zero-UMD",
        "Final-live", "level 6", "Cleric 6 and Druid 8",
        "1,581")
    static = json.loads((root / "validation/static-validation.json").read_text(encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the Word of Recall Oracle release.")
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": True,
        "ownerReportedOracleScrollFailureConfirmed": True,
        "nativeClericAndDruidRegistrationUnchanged": True,
        "scrollAndSpellGuidsUnchanged": True,
        "runtimeQualificationPending": False,
        "compatibilityRuntimeQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"wordOfRecallOracle126 static mismatch: {key}")
    # The optional Oracle reconciliation source contracts this release depends on.
    reconciler = root / "src/KingmakerGunslinger/Spells/Teleportation/TeleportationFinalLiveReconciler.cs"
    require_tokens(reconciler,
        "32c02466b2364c8a906e6e4761175099",
        "OracleWordOfRecallLevel = 6",
        "context.FeatureModules.Active.TeleportationSpells",
        "BlueprintBootstrap.TeleportationPublication == null",
        "duplicate.final-live",
        "ReconcileOptional(library, wordOfRecall);",
        "m_SpellsFiltered",
        "IsWordOfRecall")
    require_tokens(root / "src/KingmakerGunslinger/Main.cs",
        "Spells.Teleportation.TeleportationFinalLiveReconciler.AttachFirstUpdate(context);")
    # The native base publication must remain exactly as released.
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/TeleportationSpellListPublication.cs",
        "Resolve(library, ClericListId, 6, spells.WordOfRecall)",
        "Resolve(library, DruidListId, 8, spells.WordOfRecall)")
    # The guarded runtime scenario must prove the genuine Oracle reader.
    require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.TeleportationScrolls.cs",
        "oracle-final-list-recall-exactly-once",
        "oracle-native-classlist-eligibility",
        "oracle-reader-row-offered",
        "oracle-reader-not-know-spell",
        "oracle-sanctuary-row-composed",
        "oracle-sanctuary-cast-exactly-one",
        "oracle-optional-absent-safe",
        "oracle-activation-skipped")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Word of Recall Oracle {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Word of Recall Oracle {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
