#!/usr/bin/env python3
"""Validate the teleport polish + specialist cache repair release candidate."""
from __future__ import annotations
import argparse
import json
import sys
sys.dont_write_bytecode = True
from pathlib import Path

import validate_character_visibility123 as baseline

VERSION = "0.0.124"
INFORMATIONAL_VERSION = "0.0.124-teleport-polish-specialist"
PACKAGE = "KingmakerGunslinger-0.0.124-local-runtime.zip"
PACKAGE_SUFFIX = "teleport-polish-specialist"
DETERMINISTIC_TEST_COUNT = 1574
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
    baseline.STATIC_KEY = "teleportPolish124"
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    baseline.validate(root)
    # The 0.0.121-123 release records remain authoritative history.
    require_tokens(root / "docs/RELEASE-NOTES-0.0.123.md",
        "0.0.123-character-visibility-repair", "owner",
        "invisible bodies", "UpdateDollCoroutine", "AssetBundle.Unload(true)",
        "1,567", "run 13", "zero asset unloads",
        "character-creator visual lifecycle")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.124.md",
        INFORMATIONAL_VERSION, "owner",
        "special-spell cache", "Spellbook.PostLoad",
        "m_SpecialSpells", "favorite slot",
        "world extent", "settled-extent width", "somewhere else",
        "the target location", "no second confirmation",
        "1,574")
    static = json.loads((root / "validation/static-validation.json").read_text(encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the teleport polish specialist release.")
    state = static.get("teleportPolish124", {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": True,
        "compatibilityRuntimeQualificationPending": True,
        "specialistRootCauseConfirmed": "serialized per-book m_SpecialSpells cache derived only at feature activation and learn time; PostLoad never re-derives it",
        "loadSeamRepair": "Spellbook.PostLoad",
        "directCastGreaterTeleport": True,
        "greaterSuccessAnnouncementSuppressed": True,
        "arrivalCopyTemplates": True,
        "settledExtentRowContainment": True,
        "runtimeQualificationPending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"teleportPolish124 static mismatch: {key}")
    # The repair source contracts this release depends on.
    src = root / "src/KingmakerGunslinger/Spells/Teleportation"
    require_tokens(src / "TeleportSpecialistSpellCachePatches.cs",
        "Spellbook", "PostLoad", "AddSpecial", "Installed")
    require_tokens(src / "TeleportSpecialistSpellCachePolicy.cs",
        "ShouldRestoreSpecialMembership")
    require_tokens(src / "TeleportContextConfirmationPresenter.cs",
        "OpenDirect", "CanBegin", "Settle", "SuppressSuccessAnnouncement")
    require_tokens(src / "TeleportContextPresentation.cs",
        "ArrivalMessage", "ConfirmationSections", "SuppressSuccessAnnouncement",
        "Result.Arrival.TargetLocation", "Result.Arrival.SomewhereElse")
    require_tokens(src / "TeleportContextLayoutPolicy.cs",
        "ActionRowsWidth", "RowInsideNativeExtent")
    require_tokens(src / "TeleportationUiDivider.cs",
        "CreateRule", "CreateRowSeparator")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Teleport polish specialist {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Teleport polish specialist {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
