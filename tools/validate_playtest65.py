#!/usr/bin/env python3
"""Portable validator for the 0.0.65 visual/native-feat repair."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_playtest64

VERSION = "0.0.65"
INFORMATIONAL_VERSION = "0.0.65-fifth-playtest-visual-native-feat-repair"


def require(path: Path, *tokens: str) -> None:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks {missing}")


def validate(root: Path) -> None:
    validate_playtest64.validate_playtest63.validate_sprint60.validate(
        root, VERSION, INFORMATIONAL_VERSION, 865, 183, 184)
    require(root / "src/KingmakerGunslinger/Feats/NativeFirearmFeatIntegration.cs",
            "ExtractSelectionItems", "IEnumerable<IFeatureSelectionItem>",
            "NativeFirearmFeatLevelUpMenuPatch")
    require(root / "src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs",
            "FirearmWeaponPresentation.Apply(clone, spec.Definition);")
    require(root / "src/KingmakerGunslinger/Audio/FirearmSoundRuntime.cs",
            "AkSoundEngine.PostEvent", "AkBankManager.LoadBank", "id!=0")
    legacy = (root / "src/KingmakerGunslinger/Assets/FirearmAssetRuntime.cs").read_text(encoding="utf-8")
    if any(token in legacy for token in ("AudioSource", "AudioClip", "PlayOneShot", "KMG_FirearmAudio")):
        raise AssertionError("obsolete Unity firearm audio backend remains")
    require(root / "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
            "itemVisual", "itemMatch && itemVisual && itemIconDistinct")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Playtest 0.0.65 validation failed: {exception}", file=sys.stderr)
        return 1
    print("Playtest 0.0.65 source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
