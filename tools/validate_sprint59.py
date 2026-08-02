#!/usr/bin/env python3
"""Portable source validator for Sprint 59 True Grit."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint58

VERSION = "0.0.59"
INFORMATIONAL_VERSION = "0.0.59-s59-true-grit"
TEST_COUNT = 827

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 59 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint58.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 125, 126)
    require_tokens(read(root, "planning/SPRINT-59-ENTRY-CRITERIA.md"),
        ["select two deeds", "minimum 0", "positive-grit/no-spend",
         "Slinger's Luck", "Cheat Death"], "Sprint 59 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/TrueGritService.cs") + read(root,
        "src/KingmakerGunslinger/Deeds/TrueGritCatalog.cs"),
        ["request.OrdinaryCost - 1", "request.CurrentGrit > 0",
         "TrueGritDeed.StunningShot", "IsValidPair"],
        "Sprint 59 centralized policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint59Tests.cs"),
        ["TrueGritCatalogExact", "TrueGritPairUniqueness",
         "TrueGritPositiveGateRemoval", "TrueGritVariableAndCheatDeath"],
        "Sprint 59 tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/TrueGritBlueprints.cs") + read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["KMG.Classes.TrueGritSelection", "ChoiceSymbols",
         "LevelEntries[19].Features.Add(trueGrit.Selection)"],
        "Sprint 59 selection blueprints")
    print("Sprint 59 source validation passed with inherited Sprint 58 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try: validate(args.root)
    except Exception as exception:
        print(f"Sprint 59 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
