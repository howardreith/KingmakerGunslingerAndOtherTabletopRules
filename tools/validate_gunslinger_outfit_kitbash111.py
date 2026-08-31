#!/usr/bin/env python3
"""Release gate for the Gunslinger native outfit kitbash."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_protection_from_alignment110 as baseline

VERSION = "0.0.111"
INFORMATIONAL_VERSION = "0.0.111-gunslinger-class-outfit-kitbash"
PACKAGE = "KingmakerGunslinger-0.0.111-local-runtime.zip"
PACKAGE_SUFFIX = "gunslinger-class-outfit-kitbash"
DETERMINISTIC_TEST_COUNT = 1370
STATIC_KEY = "gunslingerOutfitKitbash111"

MALE_IDS = (
    "6df8f61725a84294c8661bb9585eca97",
    "4c59d2b9740930145a27a4c693217d22",
)
FEMALE_IDS = (
    "beba0e0c7dcd5c64d97d767be3e72995",
    "a93ead19aae8afc4794c54f5bcf73168",
)


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"0.0.111 release file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks 0.0.111 kitbash token(s): {missing}")
    return text


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    catalog = require_tokens(
        root / "src/KingmakerGunslinger/Presentation/"
        "GunslingerClassAppearanceCatalog.cs",
        "DefaultPrimaryColor = 2", "DefaultSecondaryColor = 22",
        "ValidateAndCopy", "IsLowerHexAssetId")
    for asset_id in MALE_IDS + FEMALE_IDS:
        if asset_id not in catalog:
            raise AssertionError(
                f"Native Magus outfit asset identifier is missing: {asset_id}")
    if catalog.count("private static readonly string[]") != 2:
        raise AssertionError(
            "The kitbash catalog must expose exactly male and female native arrays")

    appearance = require_tokens(
        root / "src/KingmakerGunslinger/Presentation/"
        "GunslingerClassAppearance.cs",
        'RequireResolved("male", maleIds)',
        'RequireResolved("female", femaleIds)',
        "ResourcesLibrary.TryGetResource<EquipmentEntity>",
        "target.MaleEquipmentEntities = maleLinks",
        "target.FemaleEquipmentEntities = femaleLinks",
        "target.EquipmentEntities = sharedEntities",
        "target.PrimaryColor = GunslingerClassAppearanceCatalog.DefaultPrimaryColor",
        "target.SecondaryColor = GunslingerClassAppearanceCatalog.DefaultSecondaryColor")
    if appearance.index('RequireResolved("male", maleIds)') > appearance.index(
            "target.MaleEquipmentEntities = maleLinks"):
        raise AssertionError(
            "Male native asset resolution must complete before class mutation")
    if appearance.index('RequireResolved("female", femaleIds)') > appearance.index(
            "target.FemaleEquipmentEntities = femaleLinks"):
        raise AssertionError(
            "Female native asset resolution must complete before class mutation")

    require_tokens(root / "src/KingmakerGunslinger/Blueprints/"
        "GunslingerClassBlueprints.cs",
        "GunslingerClassAppearance.Apply(result);")
    scenarios = require_tokens(root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs",
        "gunslinger-outfit-audit",
        "gunslinger-outfit-candidate-render",
        "gunslinger-outfit-finalist-race-matrix",
        "gunslinger-outfit-production-compatibility",
        "gunslinger-outfit-production-motion",
        "gunslinger-outfit-production-persistence")
    if scenarios.count("GunslingerOutfit") < 8:
        raise AssertionError(
            "The guarded native outfit scenario inventory is incomplete")

    program = require_tokens(root / "tests/KingmakerGunslinger.DomainTests/"
        "Program.cs",
        'Case("outfit-audit.guarded-boundary"',
        'Case("outfit-render.production-persistence"',
        'Case("outfit-appearance.production"')
    if program.count('Case("') != DETERMINISTIC_TEST_COUNT:
        raise AssertionError(
            f"Expected {DETERMINISTIC_TEST_COUNT} deterministic test cases")
    if program.count('Case("outfit-') != 11:
        raise AssertionError(
            "Expected exactly eleven focused Gunslinger outfit cases")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.111.md",
        "Kingmaker Gunslinger 0.0.111",
        "KingmakerGunslinger-0.0.111-gunslinger-class-outfit-kitbash.zip",
        "Magus", "nine supported player races", "1,370 tests")
    require_tokens(root / "CHANGELOG.md",
        "0.0.111-gunslinger-class-outfit-kitbash",
        "native Magus base-and-accessory presentation",
        "nine-race/two-gender grid")
    require_tokens(root / "docs/GUNSLINGER-OUTFIT-KITBASH-QUALIFICATION.md",
        "qualified and policy-published at 93/100",
        "9-race x 2-gender matrix",
        "accepted three-launch persistence transaction")
    require_tokens(root / "planning/GUNSLINGER-OUTFIT-CANDIDATE-MATRIX.md",
        "magus-complete", "accepted at 93/100",
        "Magus pair")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    if (static.get("version") != VERSION or
            static.get("milestone") != INFORMATIONAL_VERSION):
        raise AssertionError("0.0.111 static release identity mismatch")
    state = static.get(STATIC_KEY, {})
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "nativeOnly": True,
        "customAssetsAdded": False,
        "maleLinkCount": 2,
        "femaleLinkCount": 2,
        "supportedRaceCount": 9,
        "genderCount": 2,
        "defaultPrimaryColor": 2,
        "defaultSecondaryColor": 22,
        "candidateScore": 93,
        "equipmentOverridesQualified": True,
        "motionQualified": True,
        "persistenceQualified": True,
        "runtimeQualificationPassed": True,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"0.0.111 static mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(
            f"Gunslinger outfit kitbash {VERSION} validation failed: "
            f"{exception}", file=sys.stderr)
        return 1
    print(f"Gunslinger outfit kitbash {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
