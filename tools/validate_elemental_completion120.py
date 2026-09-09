#!/usr/bin/env python3
"""Validate the two-trait completion candidate while preserving the public baseline."""
from __future__ import annotations
import argparse
import json
import sys
sys.dont_write_bytecode = True
from pathlib import Path
import validate_teleportation119 as baseline

VERSION = "0.0.120"
INFORMATIONAL_VERSION = "0.0.120-elemental-races-completion"
DETERMINISTIC_TEST_COUNT = 1542
MANIFEST_TOTAL = 1883
MANIFEST_ACTIVE = 1881
ELEMENTAL_TOTAL = 241
ELEMENTAL_ACTIVE = 240

def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = "KingmakerGunslinger-0.0.120-local-runtime.zip"
    baseline.PACKAGE_SUFFIX = "elemental-races-completion"
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = "elementalCompletion120"
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    baseline.baseline.ELEMENTAL_TOTAL = ELEMENTAL_TOTAL
    baseline.baseline.ELEMENTAL_ACTIVE = ELEMENTAL_ACTIVE
    baseline.baseline.TRAIT_MECHANICS_IMPLEMENTATION_PENDING = False
    baseline.validate(root)
    baseline.require_tokens(root / "ELEMENTAL-RACES-COMPLETION.md",
        "8e5eeae7973c71ca4b78dc8216d00d815af7ea26",
        "72520eeb66a036da50473bec7b2e0019ca6304f6",
        "dd0a344226c31e34d251702f5832e75525137260",
        "Nereid Fascination", "Treacherous Earth", "0.0.120")
    baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.120.md",
        INFORMATIONAL_VERSION, "owner authorized", "Treacherous Earth is selectable", "runtime qualification remains partial")
    static = json.loads((root / "validation/static-validation.json").read_text(encoding="utf-8"))
    if static.get("elementalCompletion120", {}).get("publicReleaseAuthorized") is not True:
        raise AssertionError("The completion release requires explicit owner authorization.")
    state = static["elementalCompletion120"]
    if state.get("remainingFinalArtifactQualificationWaivedByOwner") is not True or state.get("selectableAlternateTraitCount") != 21 or state.get("treacherousOrdinaryPublication") is not True:
        raise AssertionError("Release scope or owner-waived qualification was misrepresented.")
    if state.get("treacherousPlayerRespecSaveQualificationWaivedByOwner") is not True or state.get("traitRuntimeQualificationPending") is not True or state.get("traitPersistenceQualificationPending") is not True:
        raise AssertionError("Owner-waived Treacherous runtime/save qualification must remain explicitly incomplete.")
    if static.get("elementalCompletion120", {}).get("traitMechanicIdentityCount") != 32:
        raise AssertionError("Candidate mechanic inventory must include all eleven new auxiliary identities.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f'Elemental completion validation failed: {exc}', file=sys.stderr)
        return 1
    print("Elemental completion 0.0.120 source validation passed.")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
