from __future__ import annotations
import argparse
import json
import sys
sys.dont_write_bytecode = True
from pathlib import Path

import validate_unified_repair121 as baseline

VERSION = "0.0.123"
INFORMATIONAL_VERSION = "0.0.123-character-visibility-repair"
PACKAGE = "KingmakerGunslinger-0.0.123-local-runtime.zip"
PACKAGE_SUFFIX = "character-visibility-repair"
DETERMINISTIC_TEST_COUNT = 1567
MANIFEST_TOTAL = 1886
MANIFEST_ACTIVE = 1884


def require_tokens(path: Path, *tokens: str) -> str:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks release contract(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    baseline.validate(root)
    # The 0.0.121 and 0.0.122 release records remain authoritative history.
    require_tokens(root / "docs/RELEASE-NOTES-0.0.121.md",
        "0.0.121-unified-firearm-maintenance", "owner",
        "exactly one maintenance action", "reusable Gunsmith",
        "KNOWN-ISSUES.md", "1,554", "32/32", "16/16")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.122.md",
        "0.0.122-teleportation-completion", "owner",
        "request-bound activation gate", "one native activation",
        "Hassuf fallback", "no refill", "1,561", "36/36", "15/12/8/4")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.123.md",
        INFORMATIONAL_VERSION, "owner",
        "invisible bodies", "UpdateDollCoroutine", "AssetBundle.Unload(true)",
        "1,567", "run 13", "zero asset unloads",
        "character-creator visual lifecycle")
    static = json.loads((root / "validation/static-validation.json").read_text(encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the character visibility repair release.")
    state = static.get("characterVisibility123", {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": True,
        "ownerAcceptedMissionReport": True,
        "rootCauseConfirmed": "creator doll-update removal passes and counter-based cache cleanups destroy shared visual resources",
        "unfixedBuildAllProxiesDestroyed": 28,
        "lifecycleQualificationRun13Pass": True,
        "mainMenuFourRaceCreatorRun14Pass": True,
        "workingSaveSmokePass": True,
        "remainingMatrixCellsDisclosed": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"characterVisibility123 static mismatch: {key}")
    # The repair source contracts the release depends on.
    src = root / "src/KingmakerGunslinger/ElementalRaces/Visuals"
    require_tokens(src / "ElementalVisualResourceRecovery.cs",
        "Heal", "ReplaceOwnedRegistration", "RebindNativeDependency")
    require_tokens(src / "ElementalVisualResourceRecoveryPolicy.cs",
        "ClassifyOwnedResource", "ClassifyNativeDependency",
        "IsRecoverable", "MinimumReportIntervalSeconds")
    require_tokens(src / "ElementalCharGenVisualRetentionPatch.cs",
        "CleanupLoadedCache", "OnDisable", "EnsureCreatorResourcesRetained")
    require_tokens(src / "ElementalRaceVisualResourceRegistry.cs",
        "AssessDamage", "ArmRetentionCounters", "ProtectedInnerAssets")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Character visibility repair {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Character visibility repair {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
