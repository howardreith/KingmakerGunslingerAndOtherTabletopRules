#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_firearm_audio96 as baseline

VERSION = "0.0.97"
INFORMATIONAL_VERSION = "0.0.97-compatibility-attribution-audit"
PACKAGE = "KingmakerGunslinger-0.0.97-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1228
STATIC_KEY = "compatibilityAttribution97"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"Compatibility-attribution file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks compatibility-attribution token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.validate(root)

    required = (
        "docs/KMG-COMPATIBILITY-ATTRIBUTION-AUDIT.md",
        "src/KingmakerGunslinger/Compatibility/"
        "CompatibilityAssetAttributionPlan.cs",
        "src/KingmakerGunslinger/RuntimeTesting/"
        "CompatibilityAttributionRuntimeControl.cs",
        "src/KingmakerGunslinger/RuntimeTesting/"
        "CompatibilityAssetAttributionScenario.cs",
        "scripts/compatibility/Collect-KmgCompatibilityAttributionLog.ps1",
        "tests/KingmakerGunslinger.DomainTests/CompatibilityAttributionTests.cs",
        "docs/RELEASE-NOTES-0.0.97.md",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(
                f"Compatibility-attribution file missing: {relative}")

    report = require_tokens(root / required[0],
        "A: Favored Class / Helpful", "Not reproduced",
        "B: polymorph / view teardown", "C: KMG asset warnings",
        "External", "This audit found no KMG production defect",
        "Remaining uncertainty")
    if "KMG-caused" in report:
        raise AssertionError("Audit report unexpectedly classifies a KMG defect")

    require_tokens(root / required[1],
        "AllSuppressed", "FirearmsOnly", "SpearsOnly", "EasternOnly",
        "AllEnabled", "TryResolve")
    require_tokens(root / required[2],
        "RuntimeTestRequestParser", "ObserveKmgCompatibilityAssetAttribution",
        "processLocal=true;saveState=false")
    require_tokens(root / required[3],
        "GetAllAssetNames", "shader.isSupported", "mesh.isReadable",
        "MissingSerializedComponents", "no-save-owned-state")
    require_tokens(root / required[4],
        "unsupportedShaderAllPassesRemoved", "invalidParticleMeshReadWrite",
        "missingSerializedScript", "lightmapModeMismatch",
        "zeroSurfaceArea", "missingMainTexProperty")
    require_tokens(root / required[5],
        "AssetPlansAreExact", "AssetPlansFailClosed",
        "GuardedRuntimeBoundaryIsExact",
        "AssetInventoryAndLogCollectionAreBounded")
    require_tokens(root / required[6],
        "Kingmaker Gunslinger 0.0.97",
        "KingmakerGunslinger-0.0.97-compatibility-attribution-audit.zip",
        "No KMG", "production defect was established.")

    project = (root / "src/KingmakerGunslinger/"
        "KingmakerGunslinger.csproj").read_text(encoding="utf-8")
    test_project = (root / "tests/KingmakerGunslinger.DomainTests/"
        "KingmakerGunslinger.DomainTests.csproj").read_text(encoding="utf-8")
    runner = (root / "tests/KingmakerGunslinger.DomainTests/"
        "Program.cs").read_text(encoding="utf-8")
    for source in ("CompatibilityAssetAttributionPlan.cs",
                   "CompatibilityAttributionRuntimeControl.cs",
                   "CompatibilityAssetAttributionScenario.cs"):
        if source not in project:
            raise AssertionError(f"Main project compile list lacks {source}")
    if "CompatibilityAttributionTests.cs" not in test_project or \
            runner.count("compat-attribution.") != 4:
        raise AssertionError("Four focused attribution tests are not registered")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    contract = static.get(STATIC_KEY, {})
    expected_static = {
        "guardedAssetSuppressionOnly": True,
        "assetInventoryQualified": True,
        "favoredClassAttributionComplete": True,
        "polymorphAttributionComplete": True,
        "assetWarningAttributionComplete": True,
        "noKmgProductionRepair": True,
    }
    for key, value in expected_static.items():
        if contract.get(key) != value:
            raise AssertionError(
                f"Compatibility-attribution static validation mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Compatibility Attribution {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Compatibility Attribution {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
