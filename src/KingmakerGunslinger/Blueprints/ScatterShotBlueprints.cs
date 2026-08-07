using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Scatter;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ScatterShotBlueprints
    {
        internal const string Symbol = "KMG.Firearms.ScatterShotAbility";
        private const string BurningHandsGuid =
            "4783c3709a74a794dbe7c8e7e0b1b038";

        internal static BlueprintAbility Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintAbility burningHands =
                BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    library, BurningHandsGuid,
                    "native Burning Hands cone presentation");
            AbilityDeliverProjectile nativeCone =
                (burningHands.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .OfType<AbilityDeliverProjectile>()
                    .Single(value => value.Type == AbilityProjectileType.Cone);
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                Symbol, () => Create(burningHands, nativeCone,
                    FirearmProjectileBlueprints.Projectile));
            Validate(ability);
            return ability;
        }

        private static BlueprintAbility Create(BlueprintAbility burningHands,
            AbilityDeliverProjectile nativeCone, BlueprintProjectile firearmProjectile)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_ScatterShot_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.ScatterShot.Name", "Scatter Shot"),
                LocalizationService.Create("KMG.ScatterShot.Description",
                    "Fire one Blunderbuss pellet load in a 15-foot cone. Make a separate attack at -2 against each creature; the weapon misfires only if every attack roll misfires."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Custom;
            result.CustomRange = new Feet(15f);
            result.CanTargetEnemies = false;
            result.CanTargetFriends = false;
            result.CanTargetSelf = false;
            result.CanTargetPoint = true;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = true;
            result.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            result.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            result.Animation = burningHands.Animation;
            result.HasFastAnimation = burningHands.HasFastAnimation;
            result.ActionType = UnitCommand.CommandType.Standard;
            // The native cone projectile view is preloaded through these resource
            // identifiers. Dropping them can leave a mechanically valid cone with
            // no visible fire presentation in a real area.
            result.ResourceAssetIds = burningHands.ResourceAssetIds == null
                ? Array.Empty<string>()
                : (string[])burningHands.ResourceAssetIds.Clone();
            result.ComponentsArray = new BlueprintComponent[] {
                ScatterShotAbilityLogic.Create(nativeCone, firearmProjectile) };
            return result;
        }

        internal static void Validate(BlueprintAbility ability)
        {
            if (ability == null || ability.ActionType != UnitCommand.CommandType.Standard ||
                ability.Range != AbilityRange.Custom ||
                ability.CustomRange.Value != 15 ||
                ability.CanTargetEnemies || ability.CanTargetFriends ||
                ability.CanTargetSelf || !ability.CanTargetPoint ||
                ability.ResourceAssetIds == null ||
                ability.ResourceAssetIds.Length == 0 ||
                ability.ComponentsArray.OfType<ScatterShotAbilityLogic>().Count() != 1)
                throw new InvalidOperationException(
                    "Scatter Shot ability blueprint contract is incomplete.");
            ScatterShotAbilityLogic delivery = ability.ComponentsArray
                .OfType<ScatterShotAbilityLogic>().Single();
            if (delivery.Type != AbilityProjectileType.Cone ||
                delivery.Length.Value != 15 || delivery.NeedAttackRoll ||
                delivery.Projectiles == null || delivery.Projectiles.Length == 0)
                throw new InvalidOperationException(
                    "Scatter Shot native cone presentation is incomplete.");
        }
    }
}
