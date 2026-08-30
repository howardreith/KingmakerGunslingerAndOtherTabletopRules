#!/usr/bin/env python3
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_martial_repair_notifications109 as baseline

VERSION = "0.0.110"
INFORMATIONAL_VERSION = (
    "0.0.110-protection-from-alignment-control-immunity")
PACKAGE = "KingmakerGunslinger-0.0.110-local-runtime.zip"
PACKAGE_SUFFIX = "protection-from-alignment-control-immunity"
DETERMINISTIC_TEST_COUNT = 1359
STATIC_KEY = "protectionFromAlignment110"


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.110 mission file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.110 mission token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    descriptions = require_tokens(root / (
        "src/KingmakerGunslinger/Spells/ProtectionFromAlignment/"
        "ProtectionFromAlignmentDescriptions.cs"),
        "SpecificSpell(ProtectionAlignment alignment",
        "GenericSpell(bool communal)",
        "Buff(ProtectionAlignment alignment)",
        "+2 deflection bonus", "+2 resistance bonus",
        "domination, charm, or comparable mental-control effect",
        "recognized by this mod", "already active",
        "Protection from \" + ProtectionName(alignment)")
    for token in ("return \"Evil\"", "return \"Good\"",
            "return \"Law\"", "return \"Chaos\"", "return \"lawful\"",
            "return \"chaotic\""):
        if token not in descriptions:
            raise AssertionError(
                f"Protection player text lacks alignment token: {token}")

    publication = require_tokens(root / (
        "src/KingmakerGunslinger/Spells/ProtectionFromAlignment/"
        "ProtectionFromAlignmentPublication.cs"),
        "DescriptionTargets", "ProtectionDescriptionSpec",
        "LocalizationService.Create(", "factAccess.SetDescription(",
        "DescriptionMutation", "RollbackAll(componentMutations, descriptionMutations)",
        "descriptions-resolved=", "descriptions-patched=",
        "DescriptionTargets.Length")
    description_block = publication.split(
        "private static readonly ProtectionDescriptionSpec[] DescriptionTargets = {",
        1)[1].split("        };", 1)[0]
    if description_block.count("new ProtectionDescriptionSpec(") != 15:
        raise AssertionError(
            "Expected exactly 15 protection player-description targets")
    if description_block.count("typeof(BlueprintAbility)") != 10:
        raise AssertionError(
            "Expected exactly ten protection ability descriptions")
    if description_block.count("typeof(BlueprintBuff)") != 5:
        raise AssertionError(
            "Expected exactly five protection buff descriptions")
    for guid in (
            "433b1faf4d02cc34abb0ade5ceda47c4",
            "eee384c813b6d74498d1b9cc720d61f4",
            "2ac7637daeb2aa143a3bae860095b63e",
            "c3aafbbb6e8fc754fb8c82ede3280051",
            "1eaf1020e82028d4db55e6e464269e00",
            "2cadf6c6350e4684baa109d067277a45",
            "93f391b0c5a99e04e83bbfbe3bb6db64",
            "5bfd4cce1557d5744914f8f6d85959a4",
            "8b8ccc9763e3cc74bbf5acc9c98557b9",
            "0ec75ec95d9e39d47a23610123ba1bad",
            "4a6911969911ce9499bf27dde9bfcedc",
            "b19e788487556aa4397080ef3dbb3619",
            "744bec63273df53438c6b76aaaa78382",
            "a4742d7afde0f4f47b380abed025b219",
            "8deb9d5cef3472646ac5199eb9edfb87"):
        if guid not in description_block:
            raise AssertionError(
                f"Protection description inventory lacks {guid}")

    require_tokens(root / (
        "src/KingmakerGunslinger/Spells/ProtectionFromAlignment/"
        "ProtectionDescriptionPublicationPolicy.cs"),
        "KMG.ProtectionFromAlignment.", "AlreadyPublished",
        "unexpected owned key", "IsOwnedKey")
    require_tokens(root / (
        "src/KingmakerGunslinger/Blueprints/BlueprintUnitFactAccess.cs"),
        "GetDescription(BlueprintUnitFact fact)",
        "SetDescription(BlueprintUnitFact fact",
        "_description.GetValue(fact)", "_description.SetValue(fact, description)")
    require_tokens(root / (
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ";protectionDescriptions=", "ExpectedDescriptions == 15",
        "ResolvedDescriptions == 15", "PublishedDescriptions ==",
        "InvalidDescriptions == 0")

    program = require_tokens(root / (
        "tests/KingmakerGunslinger.DomainTests/Program.cs"),
        'Case("protection-alignment.player-descriptions"',
        'Case("protection-alignment.description-publication"')
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic tests")
    if program.count('Case("protection-alignment.') != 11:
        raise AssertionError(
            "Expected exactly 11 focused protection-alignment cases")
    require_tokens(root / (
        "tests/KingmakerGunslinger.DomainTests/"
        "ProtectionFromAlignmentControlImmunityTests.cs"),
        "PlayerFacingDescriptionsAreAccurateAndScoped",
        "DescriptionPublicationIsExactAndIdempotent",
        "Protection from Law", "Protection from Chaos",
        "ExistingControlLimitation", "new ProtectionDescriptionSpec(")

    require_tokens(root / (
        "docs/PROTECTION-FROM-ALIGNMENT-CONTROL-IMMUNITY.md"),
        "Player-facing description publication", "exact description targets",
        "leaves both the vanilla", "control effect is neither removed nor suppressed")
    require_tokens(root / "docs/RELEASE-NOTES-0.0.110.md",
        "Kingmaker Gunslinger 0.0.110",
        "KingmakerGunslinger-0.0.110-protection-from-alignment-control-immunity.zip",
        "1,359 tests", "15", "already-active domination",
        "complete Pathfinder tabletop paragraph")
    require_tokens(root / "CHANGELOG.md",
        "0.0.110-protection-from-alignment-control-immunity",
        "all five patched protection-buff tooltips", "1,359 tests")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    if static.get("milestone") != INFORMATIONAL_VERSION or \
            static.get("version") != VERSION:
        raise AssertionError("0.0.110 static release identity mismatch")
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "protectionBuffCount": 5,
        "playerDescriptionTargetCount": 15,
        "playerAbilityDescriptionCount": 10,
        "playerBuffDescriptionCount": 5,
        "registeredControlAbilityCount": 13,
        "registeredControlBuffCount": 8,
        "independentFeatureToggle": True,
        "descriptionPublicationStartupGated": True,
        "descriptionPublicationTransactional": True,
        "existingControlEffectsRemainActive": True,
        "broadDescriptorImmunityAllowed": False,
        "guardedRuntimeDescriptionInventoryRequired": True,
        "manualEncounterValidationOngoing": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.110 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(
            f"Protection from Alignment {VERSION} validation failed: "
            f"{exception}", file=sys.stderr)
        return 1
    print(
        f"Protection from Alignment {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
