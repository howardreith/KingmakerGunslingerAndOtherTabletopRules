#!/usr/bin/env python3
"""Portable source validator for Sprint 34 with inherited Sprint 33 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint33

VERSION = "0.0.34"
INFORMATIONAL_VERSION = "0.0.34-s34-gunslinger-class-chassis"
TEST_COUNT = 691

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 34 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint33.validate(root, VERSION, INFORMATIONAL_VERSION, TEST_COUNT, 27, 28)
    require_tokens(read(root, "planning/SPRINT-34-ENTRY-CRITERIA.md"),
        ["d10 hit dice", "full base attack bonus", "levels 1 through 20",
         "multiclass, level-up, respec"], "Sprint 34 entry criteria")
    require_tokens(read(root, "src/KingmakerGunslinger/Classes/GunslingerClassChassis.cs"),
        ["MaximumLevel = 20", "HitDie = 10", "SkillRanksPerLevel = 4",
         "GoodSave(level)", "PoorSave(level)"], "Sprint 34 class chassis")
    require_tokens(read(root, "tests/KingmakerGunslinger.DomainTests/Sprint34Tests.cs"),
        ["ClassChassisExactRows", "ClassChassisCompleteMonotonic",
         "ClassChassisInvalidLevel"], "Sprint 34 chassis tests")
    require_tokens(read(root, "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunClassBlueprintContractObservation", "root.Progression.CharacterClasses",
         "characterClass.BaseAttackBonus.AssetGuid", "DescribeLevelOneFeatures",
         "DescribeProficiencies", "DescribeDirectProficiency",
         "DescribeStartingItems", "characterClass.StartingItems"],
        "Sprint 34 class-contract observation")
    require_tokens(read(root, "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["KMG.Classes.GunslingerClass", "FullBaseAttackGuid",
         "SimpleWeaponGuid", "LightArmorGuid", "CreateLevelEntries",
         "GunslingerClassCatalogPublication", "CharacterClasses = published",
         "startingPistol, blackPowder, leadBall", "StartingItems.Length != 3"],
        "Sprint 34 production class blueprints")
    require_tokens(read(root, "scripts/Test-Sprint34ProductionClass.ps1"),
        ["exact-native-progression-identities", "level-one-grants-aggregate",
         "twenty-exact-level-rows", "manifest-has-exact-production-symbols"],
        "Sprint 34 production class tests")
    require_tokens(read(root, "scripts/Test-Sprint34CharacterCreationContractObservation.ps1"),
        ["scenario-allowlisted", "save-free-autonomous", "metadata-only",
         "no-construction"], "Sprint 34 character-creation contract observer tests")
    require_tokens(read(root, "scripts/Test-Sprint34DisposableChargenConstruction.ps1"),
        ["exact-source", "detached", "finally-disposed", "snapshots-verified"],
        "Sprint 34 disposable chargen construction tests")
    print("Sprint 34 source invariant validation passed with inherited Sprint 33 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 34 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
