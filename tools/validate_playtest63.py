#!/usr/bin/env python3
"""Portable validator for the 0.0.63 third-playtest repair."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_sprint60

VERSION = "0.0.63"
INFORMATIONAL_VERSION = "0.0.63-third-playtest-feats-reload-grit-dodge-assets"

def require(path: Path, *tokens: str) -> None:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks {missing}")

def validate(root: Path) -> None:
    validate_sprint60.validate(root, VERSION, INFORMATIONAL_VERSION,
                               862, 183, 184)
    require(root / "src/KingmakerGunslinger/Blueprints/FirearmFeatBlueprints.cs",
            "wrapper.HideInUI = true", "set.RapidReload }",
            "NativeFirearmFeatIntegration.Configure")
    require(root / "src/KingmakerGunslinger/Feats/NativeFirearmFeatIntegration.cs",
            "GetFullSelectionItems", "new FeatureParam(parameter)",
            "NativeFirearmParametrizedBonus")
    require(root / "src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs",
            "Never repaint native blueprints", "StartsWith(\"KMG_\"")
    require(root / "src/KingmakerGunslinger/Blueprints/ReloadTestMusketAbilityBlueprints.cs",
            "CreateDynamic", "hidden compatibility helpers")
    require(root / "src/KingmakerGunslinger/Reloading/ReloadAbilityPresentationPatches.cs",
            "get_ActionType", "get_RuntimeActionType", "get_RequireFullRoundAction")
    require(root / "src/KingmakerGunslinger/Deeds/GunslingerDodgeRuntime.cs",
            "dodge.ArmorClassBuff", "TimeSpan.FromSeconds(6d)")
    require(root / "src/KingmakerGunslinger/Grit/GritAbilityUiIntegration.cs",
            "AbilityResourceLogic", "RequiredResource = grit", "IsSpendResource = true",
            "GritAbilityResourceUiLogic", "public override void Spend")
    require(root / "src/KingmakerGunslinger/Recovery/OverhaulTestMusketAbilityLogic.cs",
            "WorkDurationSeconds = 60f", "TimeController.GameTime < completion",
            "ReferenceEquals(completed.Weapon, start.Weapon)")
    require(root / "src/KingmakerGunslinger/Recovery/OverhaulTestMusketRuntime.cs",
            "caster.Unit.IsInCombat", "one uninterrupted minute out of combat")

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Playtest 0.0.63 validation failed: {exception}", file=sys.stderr)
        return 1
    print("Playtest 0.0.63 source validation passed.")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
