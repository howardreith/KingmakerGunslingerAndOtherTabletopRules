#!/usr/bin/env python3
"""Portable validator for the 0.0.67 player-path stabilization repair."""
from __future__ import annotations
import argparse
import re
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_playtest66

VERSION = "0.0.67"
INFORMATIONAL_VERSION = "0.0.67-seventh-playtest-player-path-repair"


def reject(path: Path, *tokens: str) -> None:
    text = path.read_text(encoding="utf-8")
    present = [token for token in tokens if token in text]
    if present:
        raise AssertionError(f"{path.name} retains rejected token(s): {present}")


def validate(root: Path) -> None:
    validate_playtest66.VERSION = VERSION
    validate_playtest66.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    validate_playtest66.validate(root, 877)

    program = (root / "tests/KingmakerGunslinger.DomainTests/Program.cs")
    declared_tests = len(re.findall(
        r'Case\("[^"]+",\s*[A-Za-z0-9_]+\)',
        program.read_text(encoding="utf-8")))
    if declared_tests != 877:
        raise AssertionError(
            f"Expected 877 declared source tests; observed {declared_tests}.")

    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Reloading/ReloadAbilityPresentationPatches.cs",
        "ReloadAbilityCommandTypePatch", "ref UnitCommand.CommandType __0",
        "ReloadAbilityPresentation.Command(action)")

    dodge_blueprints = root / (
        "src/KingmakerGunslinger/Blueprints/GunslingerDodgeBlueprints.cs")
    validate_playtest66.require(
        dodge_blueprints,
        "CastAnimationStyle.Immediate", "HasFastAnimation = true",
        "AbilityCasterHasNoFacts", "AbilityEffectRunAction",
        "ContextActionApplyBuff", "DurationRate.Rounds",
        "DiceType.Zero", "DiceCountValue = 0", "BonusValue = 1",
        "applyBuff.ToCaster = true", "applyBuff.Permanent = false",
        "applyBuff.IsNotDispelable = true", "applyBuff.AsChild = false",
        "result.IsClassFeature = false",
        "ability.ComponentsArray.Length != 4",
        "GunslingerDodgeArmorClassBonus")
    reject(
        dodge_blueprints,
        "GunslingerDodgeProneAbilityLogic.Create(marker, armorClassBuff)",
        "delivery.Duration != TimeSpan.FromSeconds(6d)")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Deeds/GunslingerDodgeProneAbilityLogic.cs",
        "class DodgeGritCostCalculator", "IAbilityResourceCostCalculator",
        "TrueGritDeed.GunslingersDodge")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Deeds/GunslingerDodgeArmorClassBonus.cs",
        "Owner.Stats.AC.AddModifier", "ModifierDescriptor.Dodge",
        "internal const int Bonus = 2", "Owner.Stats.AC.RemoveModifier")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Deeds/GunslingerDodgeExpirationPatch.cs",
        'HarmonyPatch(typeof(BuffCollection), "Tick")',
        "using KingmakerGunslinger.Blueprints;",
        "__instance.GetBuff(dodge.ArmorClassBuff)",
        "expiredByTimeLeft", "expiredByEndTime",
        "__instance.RemoveFact(buff)",
        "expiration.guard.removed")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj",
        'Compile Include="Deeds\\GunslingerDodgeExpirationPatch.cs"')
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs",
        "DODGE-EXPIRATION-R3", "Dodge expiration guard:")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Blueprints/ProjectAssetIcons.cs",
        'gunslinger.Dodge.ArmorClassBuff, Require("gunslingers-dodge")',
        'items.SetIcon(ammunition.LeadBall, Require("lead-ball"))',
        'items.SetIcon(ammunition.BlackPowder, Require("black-powder"))',
        'items.SetIcon(repairKit, Require("repair-kit"))',
        'items.SetIcon(supplies.GunsmithKit, Require("gunsmith-kit"))',
        'items.SetIcon(supplies.OverhaulKit, Require("overhaul-kit"))',
        'ValidateDistinctSupplyIcons', 'ValidateSupplyPublication')
    validate_playtest66.require(
        root / "tools/create_deterministic_package.py",
        'parser.add_argument("--expected-file-count", required=True',
        "Expected exactly {args.expected_file_count} staged package files")

    full_attack = root / (
        "src/KingmakerGunslinger/Firing/FreeActionFullAttackReloadPatch.cs")
    validate_playtest66.require(
        full_attack,
        "typeof(UnitAttack).GetMethod(\"OnAction\"",
        "ref UnitCommand.ResultType __result",
        "private static bool Prefix",
        "FullAttackAutoReloadPolicy.Evaluate",
        "targetAlive",
        "ReloadTestMusketRuntime.Evaluate",
        "ReloadTestMusketRuntime.Execute",
        "EndRemainingAttacks", "__result = UnitCommand.ResultType.Success")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Reloading/FullAttackAutoReloadPolicy.cs",
        "FullAttackReloadDecision.Reload", "FullAttackReloadDecision.EndFullAttack",
        "ContinueLoaded", "reloadAction != EffectiveReloadAction.Free",
        "!sameExactWeapon || !targetAlive")

    profile = root / (
        "src/KingmakerGunslinger/Assets/FirearmPresentationProfile.cs")
    validate_playtest66.require(
        profile,
        "native-fallback",
        "FirearmKind.Pistol, false, false",
        "FirearmKind.Revolver, false, false",
        "FirearmKind.Musket, false, false",
        "FirearmKind.Blunderbuss, false, false",
        "FirearmKind.Rifle, false, false")
    presentation = root / (
        "src/KingmakerGunslinger/Blueprints/FirearmWeaponPresentation.cs")
    validate_playtest66.require(
        presentation,
        "Resolve every prototype-backed presentation value",
        'Set(visual, "m_WhooshSound", string.Empty)',
        'Set(visual, "<Prototype>k__BackingField", null)',
        'Set(visual, "m_Projectiles"',
        "if (equipped != null)",
        "if (belt != null)",
        "if (sheath != null)")
    reject(presentation, "WeaponSoundType.None", "WeaponMissSoundType.None")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Assets/FirearmAssetRuntime.cs",
        "AssetBundle candidate", "Published firearm bundle transactionally",
        "nativeFallback=true", "Replace(Prefabs, prefabs)",
        "Replace(BeltPrefabs, beltPrefabs)", "candidate.Unload(false)")

    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Firearms/FirearmDefinitions.cs",
        "FirearmKind.Blunderbuss", "1,\n                10,")
    reject(
        root / "src/KingmakerGunslinger/Firing/EmptyFirearmAttackCommandPatch.cs",
        "Blunderbuss attack requires the granted Scatter Shot ability",
        "new UnitUseAbility(new AbilityData(granted)")
    reject(
        root / "src/KingmakerGunslinger/Firing/FirearmDischargeRuntime.cs",
        "PublishScatterOnlyWarning", "marker.Definition.IsScatter")

    scatter_runtime = root / (
        "src/KingmakerGunslinger/Scatter/ScatterShotRuntime.cs")
    scatter_text = scatter_runtime.read_text(encoding="utf-8")
    if "plan.TargetCount == 0" in scatter_text:
        raise ValueError(
            "Scatter Shot still rejects an empty cone before its qualified discharge.")
    validate_playtest66.require(
        scatter_runtime,
        "ExecuteResolved", "Targets.Resolve(caster, aimedPoint)",
        "Firing into an empty direction is still a completed discharge.",
        "Transition(firearm, expected, discharge.After)",
        "Committing the chamber transition is the point of discharge.",
        "LastAbilityResult = null")
    reject(scatter_runtime, "Transition(firearm, expected, before)")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Blueprints/ScatterShotBlueprints.cs",
        "4783c3709a74a794dbe7c8e7e0b1b038",
        "native Burning Hands cone presentation",
        "AbilityRange.Custom", "CustomRange = new Feet(15f)",
        "CanTargetPoint = true", "ScatterShotAbilityLogic.Create(nativeCone,",
        "burningHands.ResourceAssetIds")
    validate_playtest66.require(
        root / "src/KingmakerGunslinger/Scatter/ScatterShotAbilityLogic.cs",
        "class ScatterShotAbilityLogic : AbilityDeliverProjectile",
        "nativeCone.Length.Value != 15", "new[] { firearmProjectile }",
        "base.Deliver(context, target)",
        "presentation.failed", "ScatterShotRuntime.ExecuteFromAbility")

    validate_playtest66.require(
        root / "tests/KingmakerGunslinger.DomainTests/Program.cs",
        'Case("full-attack-reload.free-eligible"',
        'Case("full-attack-reload.nonfree-ended"',
        'Case("factory.early-blunderbuss-ordinary-range"',
        'Case("ac.blunderbuss-first-increment-touch"')


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Playtest {VERSION} validation failed: {exception}", file=sys.stderr)
        return 1
    print(f"Playtest {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
