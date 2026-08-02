using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Deeds;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class MenacingShotBlueprintSet
    {
        internal MenacingShotBlueprintSet(BlueprintFeature feature,
            BlueprintAbility ability)
        { Feature = feature; Ability = ability; }
        internal BlueprintFeature Feature { get; private set; }
        internal BlueprintAbility Ability { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class MenacingShotBlueprints
    {
        internal const string FeatureSymbol = "KMG.Deeds.MenacingShotFeature";
        internal const string AbilitySymbol = "KMG.Deeds.MenacingShotAbility";
        private const string NativeFearGuid = "d2aeac47450c76347aebbc02e4f463e0";

        internal static MenacingShotBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            BlueprintAbility fear = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, NativeFearGuid, "native Fear spell");
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(fear));
            BlueprintFeature feature = registry.Register<BlueprintFeature>(
                FeatureSymbol, () => CreateFeature(ability));
            Validate(feature, ability);
            return new MenacingShotBlueprintSet(feature, ability);
        }

        private static BlueprintAbility CreateAbility(BlueprintAbility fear)
        {
            BlueprintAbility result = BlueprintCloneService.Clone(fear,
                "KMG_MenacingShot_Ability");
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetFriends = result.CanTargetEnemies =
                result.CanTargetPoint = false;
            result.SpellResistance = false;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Special;
            result.ResourceAssetIds = Array.Empty<string>();
            BlueprintComponent[] retained = result.ComponentsArray.Where(value =>
                value is SpellDescriptorComponent ||
                value.GetType().FullName ==
                    "Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction" ||
                value.GetType().FullName ==
                    "Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig")
                .ToArray();
            result.ComponentsArray = retained.Concat(new BlueprintComponent[] {
                MenacingShotAbilityLogic.Create() }).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.MenacingShot.Ability.Name",
                    "Menacing Shot"),
                LocalizationService.Create("KMG.MenacingShot.Ability.Description",
                    "Spend 1 grit and one loaded firearm chamber to subject every living creature within 30 feet, including allies and yourself, to the native Fear effect. Will negates Frightened but leaves the creature Shaken for 1 round. DC is 10 + half Gunslinger level + Wisdom modifier."), null);
            return result;
        }

        private static BlueprintFeature CreateFeature(BlueprintAbility ability)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_MenacingShot_Feature";
            result.Ranks = 1; result.IsClassFeature = true; result.HideInUI = false;
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_MenacingShot_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.MenacingShot.Feature.Name",
                    "Menacing Shot"),
                LocalizationService.Create("KMG.MenacingShot.Feature.Description",
                    "Fire into the air to unleash a 30-foot burst using the exact native Fear conditions and immunities."), null);
            return result;
        }

        private static void Validate(BlueprintFeature feature,
            BlueprintAbility ability)
        {
            AddFacts grant = feature.ComponentsArray.OfType<AddFacts>().Single();
            SpellDescriptorComponent descriptor = ability.ComponentsArray
                .OfType<SpellDescriptorComponent>().Single();
            if (grant.Facts.Length != 1 || !ReferenceEquals(grant.Facts[0], ability) ||
                ability.ComponentsArray.OfType<MenacingShotAbilityLogic>().Count() != 1 ||
                ability.ComponentsArray.Count(value => value.GetType().FullName ==
                    "Kingmaker.UnitLogic.Abilities.Components.AbilityEffectRunAction") != 1 ||
                !descriptor.Descriptor.HasAnyFlag(SpellDescriptor.Fear) ||
                !descriptor.Descriptor.HasAnyFlag(SpellDescriptor.MindAffecting) ||
                ability.Range != AbilityRange.Personal || !ability.CanTargetSelf)
                throw new InvalidOperationException(
                    "Menacing Shot exact native Fear contract is incomplete.");
        }
    }
}
