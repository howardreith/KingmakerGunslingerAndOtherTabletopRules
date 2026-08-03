#!/usr/bin/env python3
"""Portable validator for the 0.0.61 first-playtest repair."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint60

VERSION = "0.0.61"
INFORMATIONAL_VERSION = "0.0.61-first-playtest-repair"
ICON_NAMES = (
    "gunslinger-class", "firearm-proficiency", "gunsmithing", "grit",
    "deeds", "deadeye", "gunslingers-dodge", "quick-clear",
    "reload-firearm", "repair-firearm", "overhaul-firearm", "early-pistol",
    "musket", "blunderbuss", "rifle", "revolver", "lead-ball",
    "black-powder", "repair-kit",
)

def require_tokens(path: Path, tokens: tuple[str, ...]) -> None:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.relative_to(path.parents[2])} lacks {missing}")

def validate(root: Path) -> None:
    validate_sprint60.validate(root, VERSION, INFORMATIONAL_VERSION, 841,
                               157, 158)
    icon_dir = root / "assets" / "game" / "icons"
    for name in ICON_NAMES:
        icon = icon_dir / f"{name}.png"
        if not icon.is_file() or icon.read_bytes()[:8] != b"\x89PNG\r\n\x1a\n":
            raise AssertionError(f"Missing or invalid production icon: {icon}")
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/PlayerFacingPresentation.cs", (
        "Player-facing ability tooltip metadata is incomplete",
        "duration.IndexOf(\"<null>\"", "saving.IndexOf(\"<null>\"",
    ))
    for source in ("DeadeyeBlueprints.cs", "GunslingerDodgeBlueprints.cs"):
        require_tokens(root / "src/KingmakerGunslinger/Blueprints" / source, (
            "LocalizedDuration", '"Until triggered"',
            "LocalizedSavingThrow", '"None"',
        ))
    require_tokens(root / "src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs", (
        'Require("early-pistol")', 'Require("lead-ball")',
        'Require("black-powder")', '"reload-firearm"',
    ))

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Playtest 0.0.61 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
