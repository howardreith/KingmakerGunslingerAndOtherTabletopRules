#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_feature75

VERSION = "0.0.76"
INFORMATIONAL_VERSION = "0.0.76-acadamae-mode-fatigue-icon-repair"
PACKAGE = "KingmakerGunslinger-0.0.76-local-runtime.zip"


def validate(root: Path) -> None:
    validate_feature75.VERSION = VERSION
    validate_feature75.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.PACKAGE = PACKAGE
    validate_feature75.validate_paper74.VERSION = VERSION
    validate_feature75.validate_paper74.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate_paper74.PACKAGE = PACKAGE
    validate_feature75.validate_paper74.validate_compatibility72.VERSION = VERSION
    validate_feature75.validate_paper74.validate_compatibility72.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate_paper74.validate_compatibility72.validate_playtest71.VERSION = VERSION
    validate_feature75.validate_paper74.validate_compatibility72.validate_playtest71.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate(root)
    ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if "Kingmaker Gunslinger - 0.0.76 ACADAMAE-MODE-FATIGUE-ICON-REPAIR" not in ui:
        raise AssertionError("0.0.76 Acadamae repair build label missing")
    for relative in (
        "ACADAMAE-PLAYTEST-REPAIR-JOURNAL.md",
        "ACADAMAE-PLAYTEST-REPAIR-IMPLEMENTATION-REPORT.md",
        "docs/ACADAMAE-PLAYTEST-REPAIR-QUALIFICATION.md",
        "planning/ACADAMAE-PLAYTEST-REPAIR-MISSION.md",
        "planning/ACADAMAE-PLAYTEST-REPAIR-MATRIX.md",
        "src/KingmakerGunslinger/Blueprints/AcadamaeGraduateModeBlueprints.cs",
        "assets-source/original-icons/cord-of-stubborn-resolve/SOURCE.md",
        "assets-source/original-icons/cord-of-stubborn-resolve/cord-of-stubborn-resolve-chroma-source.png",
        "assets/game/icons/cord-of-stubborn-resolve.png",
        "tools/New-CordOfStubbornResolveIcon.ps1",
    ):
        if not (root / relative).is_file():
            raise AssertionError(f"Acadamae repair file missing: {relative}")
    manifest = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    entries = {entry["symbol"]: entry for entry in manifest["entries"]}
    expected = {
        "KMG.Feats.AcadamaeGraduate": "7939ff087cb843729448589ba2de19f1",
        "KMG.Items.CordOfStubbornResolve": "c4b804d9ebf941b4842b0a461a2b6b6d",
        "KMG.Feats.AcadamaeGraduateModeMarker": "b5fc52ec666640318f8921d5fa60ec39",
        "KMG.Feats.UseAcadamaeGraduate": "a780ab99b76849ed825729808e2bbf29",
    }
    for symbol, guid in expected.items():
        if entries.get(symbol, {}).get("guid") != guid:
            raise AssertionError(f"Acadamae repair identity mismatch: {symbol}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Acadamae repair {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Acadamae repair {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
