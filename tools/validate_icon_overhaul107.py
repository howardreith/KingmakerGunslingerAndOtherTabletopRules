#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_fatigue_authority106 as baseline

VERSION = "0.0.107"
INFORMATIONAL_VERSION = "0.0.107-icon-art-overhaul"
PACKAGE = "KingmakerGunslinger-0.0.107-local-runtime.zip"
PACKAGE_SUFFIX = "icon-art-overhaul"
DETERMINISTIC_TEST_COUNT = 1357
STATIC_KEY = "iconOverhaul107"

EXPECTED_ASSET_KEYS = {
    "firearm-monogram-pistol",
    "firearm-monogram-musket",
    "firearm-monogram-blunderbuss",
    "rapid-reload",
    "early-pistol",
    "musket",
    "blunderbuss",
    "wakizashi",
    "katana",
    "nodachi",
    "night-without-moon",
    "heavens-measure",
    "world-tree-severer",
    "elven-branched-spear",
}


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.107 icon-overhaul file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks icon-overhaul token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.RELEASE_NOTES_VERSION = VERSION
    baseline.validate(root)

    firearm_kind = require_tokens(
        root / "src/KingmakerGunslinger/Firearms/FirearmKind.cs",
        "internal static class OfficialFirearmSupport",
        "private static readonly FirearmKind[] Official",
        "private static readonly FirearmKind[] Recognized",
        "FirearmKind.Blunderbuss",
        "FirearmKind.Rifle",
        "FirearmKind.Revolver",
        "internal static bool IsLegacy")
    official_block = firearm_kind.split(
        "private static readonly FirearmKind[] Official", 1)[1].split(
        "private static readonly FirearmKind[] Recognized", 1)[0]
    if "FirearmKind.Rifle" in official_block or \
            "FirearmKind.Revolver" in official_block:
        raise AssertionError("Legacy firearm kind returned to official support")

    require_tokens(
        root / "src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs",
        '"firearm-monogram-pistol"',
        '"firearm-monogram-musket"',
        '"firearm-monogram-blunderbuss"')
    require_tokens(
        root / "scripts/Test-IconOverhaulAssets.ps1",
        "$expectedKeys", "$retiredKeys",
        "Icon-overhaul asset validation passed")
    require_tokens(
        root / "docs/reports/icon-overhaul-report.md",
        "All 30 eastern items audited",
        "All 12 Elven Branched Spear items",
        "Rifle and Revolver surface audit",
        "Remote publication began",
        "explicit follow-up authorization")
    require_tokens(
        root / "docs/RELEASE-NOTES-0.0.107.md",
        "Kingmaker Gunslinger 0.0.107",
        "KingmakerGunslinger-0.0.107-icon-art-overhaul.zip",
        "exactly Blunderbuss, Musket, and Pistol",
        "1,325 tests")

    manifest_path = root / (
        "assets-source/original-icons/icon-overhaul-assets.json")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    records = manifest.get("records", [])
    keys = {record.get("key") for record in records}
    if len(records) != 14 or keys != EXPECTED_ASSET_KEYS:
        raise AssertionError(
            f"Expected exact 14-file icon-overhaul set, observed {sorted(keys)}")
    for record in records:
        source = root / record["sourcePath"]
        final = root / record["finalPath"]
        if not source.is_file() or not final.is_file():
            raise AssertionError(
                f"Icon-overhaul source/final pair is missing: {record['key']}")

    for retired in (
            "firearm-monogram-rifle.png",
            "firearm-monogram-revolver.png"):
        if (root / "assets/game/icons" / retired).exists():
            raise AssertionError(f"Retired selector icon returned: {retired}")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "officialFirearmKindCount": 3,
        "recognizedFirearmKindCount": 5,
        "selectorIconCount": 3,
        "finalAssetCount": 14,
        "easternItemCount": 30,
        "spearItemCount": 12,
        "retiredSelectorIconCount": 2,
        "guardedVisualEvidencePass": True,
        "workingSaveSmokePass": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.107 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(
            f"Icon Overhaul {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Icon Overhaul {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
