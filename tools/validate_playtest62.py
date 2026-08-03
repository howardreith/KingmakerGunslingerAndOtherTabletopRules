#!/usr/bin/env python3
"""Portable validator for the 0.0.62 second-playtest UX repair."""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint60

VERSION = "0.0.62"
INFORMATIONAL_VERSION = "0.0.62-second-playtest-assets-and-ux"
ICON_NAMES = (
    "gunslinger-class", "firearm-proficiency", "gunsmithing", "grit",
    "deeds", "nimble", "bonus-feat", "gun-training", "true-grit",
    "rapid-reload", "weapon-focus-firearm", "deadeye",
    "gunslingers-dodge", "quick-clear", "reload-firearm", "repair-firearm",
    "overhaul-firearm", "early-pistol", "musket", "blunderbuss", "rifle",
    "revolver", "lead-ball", "black-powder", "repair-kit",
)

def require_tokens(path: Path, tokens: tuple[str, ...]) -> None:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.relative_to(path.parents[2])} lacks {missing}")

def validate(root: Path) -> None:
    validate_sprint60.validate(root, VERSION, INFORMATIONAL_VERSION, 846, 182, 183)
    ledger = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    if any(not isinstance(entry.get("notes"), str) or not entry["notes"].strip()
           for entry in ledger["entries"]):
        raise AssertionError("Every blueprint ledger entry requires nonempty runtime notes.")
    icon_dir = root / "assets" / "game" / "icons"
    for name in ICON_NAMES:
        icon = icon_dir / f"{name}.png"
        if not icon.is_file() or icon.read_bytes()[:8] != b"\x89PNG\r\n\x1a\n":
            raise AssertionError(f"Missing or invalid production icon: {icon}")
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs", (
        "FindLocalizedInsertionIndex", "CultureInfo.CurrentUICulture.CompareInfo",
        "Array.Copy(previous, insertion", "return new GunslingerClassCatalogPublication(previous, previous)",
    ))
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/PlayerFacingPresentation.cs", (
        "ConfigureTracks", "params BlueprintFeatureBase[][] tracks", "new UIGroup",
        "Player-facing ability tooltip metadata is incomplete",
    ))
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs", (
        '"nimble"', '"bonus-feat"', '"gun-training"', '"true-grit"',
        '"rapid-reload"', '"weapon-focus-firearm"',
    ))

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Playtest 0.0.62 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
