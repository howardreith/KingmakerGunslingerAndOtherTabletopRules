#!/usr/bin/env python3
"""Portable source validator for Sprint 54 with inherited Sprint 53 checks."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint53

VERSION = "0.0.54"
INFORMATIONAL_VERSION = "0.0.54-s54-menacing-shot"
TEST_COUNT = 801

def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        raise RuntimeError(f"Required Sprint 54 file is missing: {relative}")
    return path.read_text(encoding="utf-8")

def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        raise RuntimeError(f"{label} is missing required token(s): {missing}")

def validate(root: Path) -> None:
    root = root.resolve()
    validate_sprint53.validate(root, VERSION, INFORMATIONAL_VERSION,
                               TEST_COUNT, 96, 97)
    require_tokens(read(root, "planning/SPRINT-54-ENTRY-CRITERIA.md"),
        ["30-foot-radius", "floor(Gunslinger level / 2)", "change atomically",
         "two independent feature PASS"], "Sprint 54 criteria")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/MenacingShotService.cs") + read(root,
        "src/KingmakerGunslinger/Deeds/MenacingShotTargetDecision.cs") + read(root,
        "src/KingmakerGunslinger/Deeds/MenacingShotDecision.cs"),
        ["GunslingerLevel < 15", "DifficultyClass", "RadiusMeters",
         "IsLiving"], "Sprint 54 policy")
    require_tokens(read(root,
        "tests/KingmakerGunslinger.DomainTests/Sprint54Tests.cs"),
        ["MenacingShotEligibleExactValues", "MenacingShotLevelAndWisdomDc",
         "MenacingShotFirearmAndGritGates", "MenacingShotLivingRadiusBoundary",
         "MenacingShotRejectionsAreAtomic", "MenacingShotInvalidInputRejected"],
        "Sprint 54 tests")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/MenacingShotBlueprints.cs"),
        ["d2aeac47450c76347aebbc02e4f463e0", "BlueprintCloneService.Clone",
         "SpellDescriptor.Fear", "MenacingShotAbilityLogic.Create"],
        "Sprint 54 exact native Fear clone")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Deeds/MenacingShotAbilityLogic.cs"),
        ["new Feet(30f)", "context.Params.DC", "context.Params.CasterLevel",
         "Resources.Spend", "Resources.Restore", "IsUndead"],
        "Sprint 54 runtime delivery")
    require_tokens(read(root,
        "src/KingmakerGunslinger/Blueprints/GunslingerClassBlueprints.cs"),
        ["MenacingShotBlueprints.Register", "menacingShot.Feature"],
        "Sprint 54 progression")
    require_tokens(read(root,
        "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs"),
        ["RunDisposableGunslingerMenacingShot",
         "menacing-shot-native-contract", "menacing-shot-transaction",
         "menacing-shot-params"], "Sprint 54 guarded scenario")
    print("Sprint 54 source invariant validation passed with inherited Sprint 53 checks.")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try: validate(args.root)
    except Exception as exception:
        print(f"Sprint 54 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
