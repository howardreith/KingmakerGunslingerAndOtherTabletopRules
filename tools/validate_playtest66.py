#!/usr/bin/env python3
"""Portable validator for the 0.0.66 sixth-playtest repair."""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_playtest64

VERSION = "0.0.66"
INFORMATIONAL_VERSION = "0.0.66-sixth-playtest-btsl-animation-grit-crafting"


def require(path: Path, *tokens: str) -> None:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks {missing}")


def validate(root: Path, test_count: int = 865) -> None:
    current_version = json.loads((root / "Info.json").read_text(encoding="utf-8"))["Version"]
    manifest = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    has_shield_other = any(entry.get("symbol") ==
        "KMG.Spells.ShieldOther.Ability" for entry in manifest["entries"])
    has_expanded_summoning_reservations = any(entry.get("symbol", "").startswith(
        "KMG.Summoning.") for entry in manifest["entries"])
    active_count, ledger_count = ((1412, 1413) if has_expanded_summoning_reservations else
        ((254, 255) if has_shield_other else
        ((252, 253) if current_version == "0.0.76" else (250, 251))))
    validate_playtest64.validate_playtest63.validate_sprint60.validate(
        root, VERSION, INFORMATIONAL_VERSION, test_count, active_count, ledger_count)
    require(root / "src/KingmakerGunslinger/Feats/NativeFirearmFeatIntegration.cs",
            "ExtractSelectionItems", "IEnumerable<IFeatureSelectionItem>",
            "NativeFirearmFeatLevelUpMenuPatch")
    require(root / "src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs",
            "FirearmWeaponPresentation.Apply(clone, spec.Definition,",
            "FirearmProjectileBlueprints.Register(registry, lightType)")
    require(root / "src/KingmakerGunslinger/Audio/FirearmSoundRuntime.cs",
            "AkSoundEngine.PostEvent", "AkBankManager.LoadBank", "id!=0")
    legacy = (root / "src/KingmakerGunslinger/Assets/FirearmAssetRuntime.cs").read_text(encoding="utf-8")
    if any(token in legacy for token in ("AudioSource", "AudioClip", "PlayOneShot", "KMG_FirearmAudio")):
        raise AssertionError("obsolete Unity firearm audio backend remains")
    require(root / "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
            "itemVisual", "itemMatch && itemVisual && itemIconDistinct")
    require(root / "src/KingmakerGunslinger/Blueprints/BeneathStolenLandsVendorBlueprints.cs",
            "StandaloneHonestGuyTableGuid", "CampaignXellirenTableGuid", "200, 200, 10, 5, 1")
    require(root / "src/KingmakerGunslinger/Gunsmithing/CraftBasicAmmunitionAbilityLogic.cs",
            "BatchSize = 20", "Complete(context.Caster.Descriptor)",
            "FirearmCraftingTransactionService.Complete")
    require(root / "src/KingmakerGunslinger/Gunsmithing/FirearmCraftingTransactionService.cs",
            "SpendMoney(goldCost)", "GainMoney(missingMoney)",
            "caster.RemoveFact(marker)")
    require(root / "src/KingmakerGunslinger/Deeds/DeadeyeAbilityLogic.cs",
            "class DeadeyeGritResourceLogic", "Spend(RequiredResource", "AddBuff(m_ArmedBuff")
    require(root / "src/KingmakerGunslinger/Blueprints/FirearmProjectileBlueprints.cs",
            "KMG.Firearms.Projectile", "BlueprintCloneService.Clone(source")
    require(root / "src/KingmakerGunslinger/Assets/FirearmProjectileVisualPatch.cs",
            "FirearmProjectileBlueprints.Projectile", "renderer.enabled = false")
    require(root / "src/KingmakerGunslinger/Assets/FirearmPresentationProfile.cs",
            "FirearmKind.Pistol", "FirearmKind.Musket", "HolsterPolicy")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Playtest 0.0.66 validation failed: {exception}", file=sys.stderr)
        return 1
    print("Playtest 0.0.66 source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
