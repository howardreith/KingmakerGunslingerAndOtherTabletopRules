using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Scatter;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ScatterShotBlueprints
    {
        internal const string Symbol = "KMG.Firearms.ScatterShotAbility";

        internal static BlueprintAbility Register(BlueprintRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                Symbol, Create);
            Validate(ability);
            return ability;
        }

        private static BlueprintAbility Create()
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_ScatterShot_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.ScatterShot.Name", "Scatter Shot"),
                LocalizationService.Create("KMG.ScatterShot.Description",
                    "Fire one Blunderbuss pellet load in a 15-foot cone. Make a separate attack at -2 against each creature; the weapon misfires only if every attack roll misfires."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Weapon;
            result.CanTargetEnemies = true;
            result.CanTargetFriends = true;
            result.CanTargetSelf = false;
            result.CanTargetPoint = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = true;
            result.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            result.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Special;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.ResourceAssetIds = Array.Empty<string>();
            result.ComponentsArray = new BlueprintComponent[] {
                ScatterShotAbilityLogic.Create() };
            return result;
        }

        internal static void Validate(BlueprintAbility ability)
        {
            if (ability == null || ability.ActionType != UnitCommand.CommandType.Standard ||
                ability.Range != AbilityRange.Weapon || !ability.CanTargetEnemies ||
                !ability.CanTargetFriends || ability.CanTargetSelf ||
                ability.ComponentsArray.OfType<ScatterShotAbilityLogic>().Count() != 1)
                throw new InvalidOperationException(
                    "Scatter Shot ability blueprint contract is incomplete.");
        }
    }
}
