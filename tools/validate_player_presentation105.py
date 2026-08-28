#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_summon_same_turn104 as baseline

VERSION = "0.0.105"
INFORMATIONAL_VERSION = (
    "0.0.105-player-facing-presentation-item-discoverability")
PACKAGE = "KingmakerGunslinger-0.0.105-local-runtime.zip"
PACKAGE_SUFFIX = "player-facing-presentation-item-discoverability"
DETERMINISTIC_TEST_COUNT = 1315
STATIC_KEY = "playerPresentation105"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.105 mission file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.105 mission token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.validate(root)

    require_tokens(root / (
        "src/KingmakerGunslinger/BrownFur/"
        "BrownFurArchetypeOrdering.cs"),
        "0579e8ed3ded006b2ef40c7fc5ed226c",
        "1a84628d8fcd0c6a2a89a9c9c24b52a5",
        "56155d681d350f5a2658a83237171f14",
        "841a65dccb4a08e03360c76b4d6980cd",
        "b46e7ee9cbf002370c1e64b9daf9e3f2",
        "IsKnownCombinedArchetype")
    require_tokens(root / (
        "src/KingmakerGunslinger/BrownFur/"
        "BrownFurPublicationTransaction.cs"),
        "InsertBefore<T>", "InsertBeforeMutation<T>",
        "boundaryFound", "preserved-later")
    require_tokens(root / (
        "src/KingmakerGunslinger/BrownFur/"
        "BrownFurOptionalExtensionCoordinator.cs"),
        '.InsertBefore("cotw-arcanist-archetypes"',
        "brownFurIndex >= combinedIndex",
        "before the installed combined Arcanist archetype block")

    require_tokens(root / (
        "src/KingmakerGunslinger/Presentation/"
        "PlayerFacingTextPolicy.cs"),
        "ForbiddenPhrases", "stable weapon", "exact weapon",
        "genuine sneak", "damage-stat replacement", "Kingmaker has no",
        "native bane", "native weapon-size", "KMG_", "<null>", "â€")
    require_tokens(root / (
        "src/KingmakerGunslinger/Acquisition/"
        "ProjectMagicItemDiscoverabilityPolicy.cs"),
        "ExpectedItemCount = 30", "distinct-targets",
        "distinct-exact-areas", "temporary-area", "obscure-target",
        "campaign-area-density", 'CordAreaName = "CapitalTavern_Indoor"',
        '"9572baf3952095f41abda1fb25055cce"')
    require_tokens(root / (
        "src/KingmakerGunslinger/Blueprints/"
        "CordOfStubbornResolveBlueprints.cs"),
        "RichHuman_treasure_chest_04 (1)", "CapitalTavern_Indoor",
        "LegacyAcquisitionGuid", "LegacyAcquisitionArea",
        "_mutations.Count != 2")

    runtime = require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        "ObserveProjectItemPresentation", "weapons.Length == 55",
        "items.Length == 56", "custom.Length == 12",
        "ProjectMagicItemDiscoverabilityPolicy.Audit(observations)",
        "legacyCordRows == 0", "brownFurOrderingExact",
        "firstCombinedArchetypeIndex")

    program = require_tokens(root / (
        "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        "brown-fur.publication-before-combined",
        "brown-fur.publication-no-boundary",
        "brown-fur.publication-ordered-rollback",
        "presentation.player-text-policy",
        "acquisition.distributed-campaign",
        "acquisition.unsafe-target-rejection",
        "acquisition.source-target-contracts",
        "presentation.source-tooltip-contracts")
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic tests")

    require_tokens(root / (
        "docs/PLAYER-FACING-PRESENTATION-AND-ITEM-"
        "DISCOVERABILITY-QUALIFICATION.md"),
        "0fe38002fc022ad5a04d65430eb461046cd9cc3c",
        "Brown-Fur Transmuter", "CapitalTavern_Indoor",
        "29 exact areas", "Guarded runtime",
        "20260828T0235417947135Z-observe-gunslinger-presentation",
        "20260828T0237409495792Z-observe-rare-firearm-acquisition",
        "20260828T0240171423403Z-observe-capital-cord-vendor",
        "20260828T0316455326107Z-working-save-smoke",
        "260/260 assertions")
    require_tokens(root / (
        "planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md"),
        "0.0.105", "CapitalTavern_Indoor",
        "FinalDungeon3", "MonsterLairHodag")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "projectMagicItemCount": 30,
        "distinctTargetCount": 30,
        "distinctExactAreaCount": 29,
        "projectWeaponCount": 55,
        "playerFacingItemCount": 56,
        "projectEnchantmentCount": 12,
        "combinedArchetypeIdentityCount": 5,
        "cordCapitalInnRequired": True,
        "retiredCordTargetCleanupRequired": True,
        "temporaryAreaTargetsAllowed": False,
        "obscureTargetNamesAllowed": False,
        "runtimeQualificationPending": False,
        "humanOrganicPacingAcceptancePending": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.105 static mismatch: {key}")
def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Player Presentation {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Player Presentation {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
