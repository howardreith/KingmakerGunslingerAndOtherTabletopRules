#!/usr/bin/env python3
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_feature75
import expanded_summoning_manifest

VERSION = "0.0.78"
INFORMATIONAL_VERSION = "0.0.78-expanded-summoning"
PACKAGE = "KingmakerGunslinger-0.0.78-local-runtime.zip"


def validate(root: Path) -> None:
    validate_feature75.VERSION = VERSION
    validate_feature75.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.PACKAGE = PACKAGE
    validate_feature75.validate_paper74.VERSION = VERSION
    validate_feature75.validate_paper74.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate_paper74.PACKAGE = PACKAGE
    compatibility = validate_feature75.validate_paper74.validate_compatibility72
    compatibility.VERSION = VERSION
    compatibility.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    compatibility.validate_playtest71.VERSION = VERSION
    compatibility.validate_playtest71.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_feature75.validate(root)

    ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if "Kingmaker Gunslinger - 0.0.78 EXPANDED-SUMMONING" not in ui:
        raise AssertionError("0.0.78 Expanded Summoning build label missing")
    required = (
        "planning/EXPANDED-SUMMONING-MISSION.md",
        "planning/EXPANDED-SUMMONING-ROSTER.md",
        "planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md",
        "planning/EXPANDED-SUMMONING-INVENTORY.md",
        "EXPANDED-SUMMONING-JOURNAL.md",
        "EXPANDED-SUMMONING-IMPLEMENTATION-REPORT.md",
        "EXPANDED-SUMMONING-STATE.json",
        "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
        "src/KingmakerGunslinger/Blueprints/ExpandedSummoningBlueprints.cs",
        "scripts/compatibility/Test-ExpandedSummoningCompatibilityProfiles.ps1",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Expanded Summoning file missing: {relative}")
    manifest = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    active = [entry for entry in manifest["entries"] if entry["status"] == "active"]
    reserved = [entry for entry in manifest["entries"] if entry["status"] == "reserved"]
    if len(manifest["entries"]) != 1413 or len(active) != 1412 or len(reserved) != 1:
        raise AssertionError("Expanded Summoning blueprint ledger count mismatch")
    expanded_summoning_manifest.validate(manifest, expanded_summoning_manifest.planned())
    bootstrap = (root / "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs").read_text(encoding="utf-8")
    if "ExpectedRegisteredBlueprintCount = 254 +" not in bootstrap:
        raise AssertionError("Expanded Summoning aggregate registration count mismatch")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Expanded Summoning {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Expanded Summoning {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
