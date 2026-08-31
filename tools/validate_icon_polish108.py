#!/usr/bin/env python3
from __future__ import annotations

import argparse
import hashlib
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_icon_overhaul107 as baseline

VERSION = "0.0.108"
INFORMATIONAL_VERSION = "0.0.108-icon-art-polish-round-2"
PACKAGE = "KingmakerGunslinger-0.0.108-local-runtime.zip"
PACKAGE_SUFFIX = "icon-art-polish-round-2"
DETERMINISTIC_TEST_COUNT = 1365
STATIC_KEY = "iconPolishRoundTwo108"

SELECTOR_HASHES = {
    "firearm-monogram-pistol.png":
        "ec9ed32c71b137f8d8b65184b6e92e946d034a2ef329cd0f8fe7f52194e3f07d",
    "firearm-monogram-musket.png":
        "7bb189ad50bc578217adeca1d280e31312053a13cddf112760a5611eb79a82ee",
    "firearm-monogram-blunderbuss.png":
        "65272c2ccfca2c3a766e0b11767e44ea65b1e9b05038dcb7d07c97f3fdce89f7",
}
CORD_SOURCE_HASH = (
    "54bb3426f8cd651758c6bce733904045fb30a84dd7b452d72bdf111abeb481e1")
CORD_RUNTIME_HASH = (
    "101e1b2fbd7083c5db20be1a0ee40840bc8201520dff83be0acd9bae06f91a6a")


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.108 icon-polish file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks icon-polish token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    require_tokens(
        root / "scripts/Test-IconPolishRound2Assets.ps1",
        "$protected = [ordered]@{", "$protected.Count",
        "bakedFrame", "runtimeAspect",
        "Icon polish Round 2 validation passed")
    require_tokens(
        root / "tools/icon-art/New-IconOverhaulAssets.ps1",
        "Draw-SelectorField", "full-bleed", "no baked frame")
    require_tokens(
        root / "tools/New-CordOfStubbornResolveIcon.ps1",
        "cord-of-stubborn-resolve-oblique-source.png",
        "runtimeAlphaBounds", "runtimeAspect")
    require_tokens(
        root / "docs/reports/icon-polish-round-2-report.md",
        "Pre-mission protected manifest", "In-scope before hashes",
        "Implementation, qualification, and final identity",
        "No remote Git operation")
    require_tokens(
        root / "docs/RELEASE-NOTES-0.0.108.md",
        "Kingmaker Gunslinger 0.0.108",
        "KingmakerGunslinger-0.0.108-icon-art-polish-round-2.zip",
        "1,325 tests", "not screenshots of automated native-menu navigation")

    spec = json.loads((root / (
        "assets-source/original-icons/firearm-feats/icon-spec.json"))
        .read_text(encoding="utf-8"))
    if spec.get("schemaVersion") != 4 or spec.get("bakedFrame") is not False:
        raise AssertionError("Selector specification is not the no-frame schema")
    if len(spec.get("monograms", [])) != 3:
        raise AssertionError("Selector specification does not contain B/M/P")
    for name, expected_hash in SELECTOR_HASHES.items():
        path = root / "assets/game/icons" / name
        if sha256(path) != expected_hash:
            raise AssertionError(f"Polished selector hash mismatch: {name}")

    cord_manifest = json.loads((root / (
        "assets-source/original-icons/cord-of-stubborn-resolve/"
        "cord-of-stubborn-resolve-assets.json")).read_text(encoding="utf-8"))
    if cord_manifest.get("schemaVersion") != 2:
        raise AssertionError("Cord manifest schema is stale")
    if cord_manifest.get("runtimeDimensions") != [128, 128]:
        raise AssertionError("Cord runtime dimensions are not 128x128")
    if cord_manifest.get("runtimeAlphaBounds") != [6, 32, 116, 64]:
        raise AssertionError("Cord runtime alpha bounds changed")
    if cord_manifest.get("runtimeAspect") != 1.8125:
        raise AssertionError("Cord runtime silhouette is not belt-like")
    if cord_manifest.get("cornerAlpha") != [0, 0, 0, 0]:
        raise AssertionError("Cord corners are not transparent")
    cord_source = root / cord_manifest["sourcePath"]
    cord_runtime = root / cord_manifest["runtimePath"]
    if sha256(cord_source) != CORD_SOURCE_HASH:
        raise AssertionError("Cord source hash mismatch")
    if sha256(cord_runtime) != CORD_RUNTIME_HASH:
        raise AssertionError("Cord runtime hash mismatch")

    references = root / "docs/reference/icon-polish-round-2/references"
    if len(list((references / "originals").glob("*.png"))) != 5:
        raise AssertionError("Expected all five full-resolution references")
    if len(list((references / "crops").glob("*.png"))) != 5:
        raise AssertionError("Expected all five focused reference crops")

    evidence_root = root / "docs/reports/icon-polish-round-2"
    evidence = json.loads((evidence_root / "runtime-after/manifest.json")
        .read_text(encoding="utf-8"))
    screenshots = evidence.get("screenshots", [])
    if len(screenshots) != 6:
        raise AssertionError("Expected six curated Round 2 runtime frames")
    for record in screenshots:
        if record.get("width") != 1920 or record.get("height") != 1200:
            raise AssertionError("Curated runtime frame is not 1920x1200")
        path = root / "docs/reports" / record["file"]
        if sha256(path) != record.get("sha256"):
            raise AssertionError(f"Curated frame hash mismatch: {path.name}")
    for path in (
            evidence_root / "exact-size-preview.png",
            root / "docs/reports/icon-polish-round-2-before-after-contact-sheet.png"):
        if not path.is_file():
            raise AssertionError(f"Visual deliverable missing: {path}")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "protectedFileCount": 30,
        "selectorIconCount": 3,
        "suppliedOriginalCount": 5,
        "suppliedCropCount": 5,
        "curatedRuntimeFrameCount": 6,
        "cordRuntimeAspect": 1.8125,
        "guardedVisualEvidencePass": True,
        "dependentFeatRuntimePass": True,
        "cordRuntimePass": True,
        "workingSaveSmokePass": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.108 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(
            f"Icon Polish {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Icon Polish {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
