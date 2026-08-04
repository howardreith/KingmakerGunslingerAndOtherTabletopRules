#!/usr/bin/env python3
"""Portable validator for the 0.0.64 fourth-playtest runtime/UX repair."""
from __future__ import annotations
import argparse
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_playtest63

VERSION = "0.0.64"
INFORMATIONAL_VERSION = "0.0.64-fourth-playtest-runtime-ux-repair"


def require(path: Path, *tokens: str) -> None:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks {missing}")


def validate(root: Path) -> None:
    # Retain every 0.0.63 structural contract while supplying the coherent
    # candidate identity to the shared historical validator.
    validate_playtest63.validate_sprint60.validate(
        root, VERSION, INFORMATIONAL_VERSION, 863, 183, 184)
    validate_playtest63.require(
        root / "src/KingmakerGunslinger/Feats/NativeFirearmFeatIntegration.cs",
        "GetFullSelectionItems", "new FeatureParam(parameter)",
        "NativeFirearmParametrizedBonus")
    require(root / "src/KingmakerGunslinger/Blueprints/FirearmWeaponPresentation.cs",
            "m_WeaponModel", "m_WeaponBeltModel", "m_WeaponSheathModel")
    require(root / "src/KingmakerGunslinger/Assets/FirearmVisualEquipmentHandler.cs",
            "crossbow", "quiver", "renderer.enabled = false")
    require(root / "src/KingmakerGunslinger/Firearms/FirearmQualitiesTooltipPatch.cs",
            "DescriptionTemplatesItem", "ItemQualities", "DescribeQualities")
    require(root / "src/KingmakerGunslinger/Firearms/FirearmConditionPresentation.cs",
            "Capacity ", "Misfire ", "Condition: ")
    require(root / "tools/New-RapidReloadIcon.ps1",
            "rapid-reload-chroma-source.png", "rapid-reload.png")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Playtest 0.0.64 validation failed: {exception}", file=sys.stderr)
        return 1
    print("Playtest 0.0.64 source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
