#!/usr/bin/env python3
"""Portable source validator for Sprint 60 player-facing presentation."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint59

VERSION = "0.0.60"
INFORMATIONAL_VERSION = "0.0.60-s60-player-presentation"
TEST_COUNT = 827

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 60 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint59.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 125, 126)
    require_tokens(read(root, "planning/SPRINT-60-ENTRY-CRITERIA.md"),
        ["non-hidden feature or ability", "approved fallback icon",
         "Progression UI groups", "presentation metadata only"],
        "Sprint 60 criteria")
    presentation = read(root,
        "src/KingmakerGunslinger/Blueprints/PlayerFacingPresentation.cs")
    require_tokens(presentation,
        ["SetIconIfMissing", "feature.HideInUI", "ability.Hidden",
         "StartsWith(\"KMG_\"", "selection.AllFeatures", "OfType<AddFacts>",
         "progression.UIGroups", "features.ToList()"],
        "Sprint 60 presentation graph")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["PlayerFacingPresentation.Apply(progression, characterClass.Icon)"],
        "Sprint 60 bootstrap integration")
    print("Sprint 60 source validation passed with inherited Sprint 59 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try: validate(args.root)
    except Exception as exception:
        print(f"Sprint 60 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
