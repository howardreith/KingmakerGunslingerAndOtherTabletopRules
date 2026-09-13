from __future__ import annotations
import argparse
import json
import sys
sys.dont_write_bytecode = True
from pathlib import Path

import validate_unified_repair121 as baseline

VERSION = "0.0.122"
INFORMATIONAL_VERSION = "0.0.122-teleportation-completion"
PACKAGE = "KingmakerGunslinger-0.0.122-local-runtime.zip"
PACKAGE_SUFFIX = "teleportation-completion"
DETERMINISTIC_TEST_COUNT = 1567
STATIC_KEY = "teleportationCompletion122"
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
    baseline.STATIC_KEY = STATIC_KEY
    baseline.MANIFEST_TOTAL = MANIFEST_TOTAL
    baseline.MANIFEST_ACTIVE = MANIFEST_ACTIVE
    baseline.validate(root)
    # The 0.0.121 unified-repair records remain authoritative history.
    require_tokens(root / "docs/RELEASE-NOTES-0.0.121.md",
        "0.0.121-unified-firearm-maintenance", "owner",
        "exactly one maintenance action", "reusable Gunsmith",
        "KNOWN-ISSUES.md", "1,554", "32/32", "16/16")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.122.md",
        "0.0.122-teleportation-completion", "owner",
        "request-bound activation gate", "one native activation",
        "Hassuf fallback", "no refill", "1,561", "36/36", "15/12/8/4")
    static = json.loads((root / "validation/static-validation.json").read_text(encoding="utf-8"))
    if static.get("version") != VERSION or \
            static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the teleportation completion release.")
    state = static.get("teleportationCompletion122", {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": True,
        "ownerAcceptedPr": 12,
        "scrollsRuntimeAssertions": 36,
        "persistencePhases": "15/12/8/4",
        "moduleOnNoRefill": True,
        "ordinaryUseRefusedBeforeActivation": True,
        "variantActivationGuard": True,
        "inAreaFallbackNeedNotRun": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"teleportationCompletion122 static mismatch: {key}")
    # The completion source contracts the release depends on.
    src = root / "src/KingmakerGunslinger"
    require_tokens(src / "Spells/Teleportation/TeleportationScrollActivationGate.cs",
        "Authorization", "Authorized", "HasStrategicScrollFact")
    require_tokens(src / "Spells/Teleportation/TeleportationScrollAdapter.cs",
        "TryUseFromInventory", "RefusedUnspent", "FailedSpent",
        "SameContract", "AssociatedSpell")
    require_tokens(src / "Blueprints/TeleportationScrollVendorPublication.cs",
        "DecideSupplier", "FallbackArcaneTableId", "FaultInjection")
    require_tokens(src / "Blueprints/TeleportationScrollBlueprints.cs",
        "new Kingmaker.Blueprints.Items.Components.CopyScroll { CustomSpell = spell }")
    require_tokens(src / "Blueprints/TeleportationSpellBlueprints.cs",
        "ability.CanTargetSelf = true", "TeleportationScrollActivationGate.Authorized")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Teleportation completion {VERSION} validation failed: {exc}", file=sys.stderr)
        return 1
    print(f"Teleportation completion {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
