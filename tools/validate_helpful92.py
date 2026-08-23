#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_bodyguard91 as baseline

VERSION = "0.0.92"
INFORMATIONAL_VERSION = "0.0.92-helpful-aid-another"
PACKAGE = "KingmakerGunslinger-0.0.92-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1201


def require_tokens(path: Path, *tokens: str) -> None:
    if not path.is_file():
        raise AssertionError(f"Helpful gate file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks Helpful contract token(s): {missing}")


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.STATIC_KEY = "helpful92"
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.baseline.EXPECTED_LEDGER_ENTRIES = 1623
    baseline.baseline.EXPECTED_ACTIVE_BLUEPRINTS = 1622
    baseline.baseline.PROJECT_BLUEPRINT_COUNT = 7
    baseline.baseline.EXPECTED_IDENTITIES["KMG.Traits.HelpfulCombat"] = (
        "e4b29a7c8d5f4c1796ab03e1f72d8456", "BlueprintFeature")
    baseline.validate(root)

    required = (
        "docs/investigations/aid-another-cotw-favored-class.md",
        "src/KingmakerGunslinger/Blueprints/HelpfulCombatBlueprints.cs",
        "src/KingmakerGunslinger/AidAnotherCompatibility/AidAnotherGrantResolver.cs",
        "src/KingmakerGunslinger/AidAnotherCompatibility/CotwAidAnotherResolver.cs",
        "src/KingmakerGunslinger/AidAnotherCompatibility/FavoredClassTraitResolver.cs",
        "src/KingmakerGunslinger/AidAnotherCompatibility/AidAnotherOptionalExtensionCoordinator.cs",
        "src/KingmakerGunslinger/RuntimeTesting/AidAnotherCompatibilityObserver.cs",
        "tests/KingmakerGunslinger.DomainTests/AidAnotherCompatibilityTests.cs",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(f"Helpful gate file missing: {relative}")

    require_tokens(root / required[1], "KMG.Traits.HelpfulCombat",
        "KMG_HelpfulCombat_Trait", "FeatureGroup.Trait",
        "grant your ally a +3 bonus instead of a +2 bonus")
    require_tokens(root / required[2],
        "CombatHelpfulIncrement = 1", "HalflingHelpfulIncrement = 2",
        "NonHelpfulIncrement", "HelpfulVariant")
    require_tokens(root / required[3], "aid_another_config",
        "ContextRankBaseValueType.FeatureList",
        "ContextRankProgression.BonusValue", "aid_another_buffs")
    require_tokens(root / required[4], "c9bd9f6cc24f41e684a68e6510afc726",
        "43d763957f364315b5fff85f9e91ca51",
        "331ed3c4a988415785f71a37b826d0f1", "enable_traits")
    require_tokens(root / required[5], "FirstUpdate",
        "cotw-aid-another-feature-list", "favored-combat-all-features",
        "PrerequisiteNoFeature", "publication.idempotent")
    require_tokens(root / required[6],
        "aid-another-compatibility-contracts.json",
        "aid-another-live-values", "combat-helpful-plus-benevolent",
        "dual-helpful-plus-benevolent")

    catalog = json.loads((root / "compatibility/reference-catalog.json")
        .read_text(encoding="utf-8"))
    references = {entry["key"]: entry for entry in catalog["references"]}
    if references.get("favored-class", {}).get(
            "availabilityDisposition") != "UNAVAILABLE-LOCAL-REFERENCE":
        raise AssertionError("missing Favored Class compiled-reference boundary")
    profiles = json.loads((root / "compatibility/profiles.json")
        .read_text(encoding="utf-8"))["profiles"]
    by_id = {entry["id"]: entry for entry in profiles}
    required_profiles = (
        "gunslinger-call-of-the-wild-favored-class",
        "gunslinger-call-of-the-wild-favored-class-traits-disabled",
        "gunslinger-high-risk-combined-favored-class",
    )
    for profile_id in required_profiles:
        profile = by_id.get(profile_id)
        if not profile or profile["disposition"] != \
                "UNAVAILABLE-LOCAL-REFERENCE" or \
                profile["runtimeLoadableRequired"]:
            raise AssertionError(
                f"Favored Class blocked profile is not exact: {profile_id}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Helpful Aid Another {VERSION} validation failed: {exception}",
            file=sys.stderr)
        return 1
    print(f"Helpful Aid Another {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
