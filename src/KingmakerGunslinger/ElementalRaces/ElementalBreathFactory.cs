using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    [Serializable]
    public sealed class ElementalBreathParameters : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintAbility Ability;

        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt == null || Owner == null || Ability == null ||
                !ReferenceEquals(evt.Blueprint, Ability)) return;
            evt.ReplaceCasterLevel = Math.Max(1, Owner.Progression.CharacterLevel);
            evt.ReplaceSpellLevel = ElementalBreathPolicy.HalfLevel(Owner.Progression.CharacterLevel);
            evt.ReplaceStat = StatType.Constitution;
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }
    }

    internal static class ElementalBreathFactory
    {
        internal const string AcidDonorGuid = "fa5ee5f4cd5c6394f8b497c773f8e14a";
        internal const string AcidProjectileGuid = "f6544caac8fe528489327cd86a84b025";
        internal const string SickenedGuid = "4e42460798665fd4cb9173ffa7ada323";

        internal static ElementalTraitDailyAbilityBlueprints Register(
            LibraryScriptableObject library, BlueprintRegistry registry, ElementalAlternateTraitId trait)
        {
            if (trait != ElementalAlternateTraitId.AcidBreath && trait != ElementalAlternateTraitId.OozeBreath)
                return null;
            bool ooze = trait == ElementalAlternateTraitId.OozeBreath;
            string prefix = "KMG.ElementalRaces.Traits.Undine." + trait;
            string name = ooze ? "Ooze Breath" : "Acid Breath";
            string description = ElementalBreathPolicy.Description(ooze);
            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, AcidDonorGuid, "native acid cone delivery");
            AbilityDeliverProjectile delivery = donor.ComponentsArray.OfType<AbilityDeliverProjectile>().Single();
            if (donor.Icon == null || delivery.Type != AbilityProjectileType.Cone || delivery.NeedAttackRoll ||
                delivery.Projectiles == null || delivery.Projectiles.Length != 1 ||
                delivery.Projectiles[0] == null || delivery.Projectiles[0].AssetGuid != AcidProjectileGuid ||
                donor.SpellDescriptor != SpellDescriptor.Acid)
                throw new InvalidOperationException("The exact native acid cone donor contract changed.");
            BlueprintBuff sickened = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(
                library, SickenedGuid, "native non-poison Sickened");
            if ((sickened.GetComponent<SpellDescriptorComponent>().Descriptor & SpellDescriptor.Poison) != 0)
                throw new InvalidOperationException("Native Sickened must not classify this breath as poison.");

            BlueprintAbilityResource resource = registry.Register<BlueprintAbilityResource>(prefix + ".Resource", () =>
            {
                var value = ScriptableObject.CreateInstance<BlueprintAbilityResource>();
                value.name = (prefix + ".Resource").Replace('.', '_');
                value.LocalizedName = LocalizationService.Create(prefix + ".Resource.Name", name + " Uses");
                value.LocalizedDescription = LocalizationService.Create(prefix + ".Resource.Description", description);
                ElementalRaceAbilityFactory.ConfigureBaseAmount(value, 1);
                return value;
            });
            BlueprintAbility ability = registry.Register<BlueprintAbility>(prefix + ".Ability", () =>
            {
                BlueprintAbility value = BlueprintCloneService.Clone(donor, (prefix + ".Ability").Replace('.', '_'));
                value.Type = AbilityType.Supernatural;
                value.ActionType = UnitCommand.CommandType.Standard;
                value.SetIsFullRoundAction(false);
                value.Range = AbilityRange.Projectile;
                value.CanTargetPoint = true;
                value.CanTargetEnemies = true;
                value.CanTargetFriends = true;
                value.CanTargetSelf = false;
                value.EffectOnAlly = value.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
                value.SpellResistance = false;
                value.Parent = null;
                value.Hidden = false;
                value.ActionBarAutoFillIgnored = false;
                value.AvailableMetamagic = (Metamagic)0;
                value.MaterialComponent = new BlueprintAbility.MaterialComponentData();
                value.ResourceAssetIds = Array.Empty<string>();

                // Fresh components and action graph; not a donor-array edit.
                var cone = ScriptableObject.CreateInstance<AbilityDeliverProjectile>();
                cone.Type = AbilityProjectileType.Cone;
                cone.Length = ElementalBreathPolicy.ConeFeet.Feet();
                cone.LineWidth = ElementalBreathPolicy.ConeFeet.Feet();
                cone.Projectiles = (BlueprintProjectile[])delivery.Projectiles.Clone();
                cone.NeedAttackRoll = false;
                var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
                descriptor.Descriptor = SpellDescriptor.Acid;
                var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
                effect.SavingThrowType = SavingThrowType.Reflex;
                if (ooze)
                {
                    var apply = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
                    apply.Buff = sickened;
                    apply.IsFromSpell = false;
                    apply.IsNotDispelable = true;
                    apply.DurationValue = new ContextDurationValue { Rate = DurationRate.Rounds,
                        DiceType = DiceType.Zero, DiceCountValue = Simple(0),
                        BonusValue = Simple(ElementalBreathPolicy.SickenedRounds) };
                    var saved = ScriptableObject.CreateInstance<ContextActionConditionalSaved>();
                    saved.Succeed = Actions();
                    saved.Failed = Actions(apply);
                    effect.Actions = Actions(PositiveDiceDamage(true), saved);
                }
                else effect.Actions = Actions(PositiveDiceDamage(false));
                value.ComponentsArray = new BlueprintComponent[] { descriptor, cone, DamageRank(),
                    ElementalRaceAbilityFactory.ResourceCost(resource, true), effect };
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create(prefix + ".Ability.Name", name),
                    LocalizationService.Create(prefix + ".Ability.Description", description), donor.Icon);
                return ElementalComponentIdentity.Prepare(value);
            });
            var parameters = ScriptableObject.CreateInstance<ElementalBreathParameters>();
            parameters.Ability = ability;
            return new ElementalTraitDailyAbilityBlueprints { Resource = resource, Ability = ability,
                ParameterComponent = parameters, Mechanics = new BlueprintScriptableObject[] { resource, ability } };
        }

        private static GameAction PositiveDiceDamage(bool ooze)
        {
            var damage = ScriptableObject.CreateInstance<ContextActionDealDamage>();
            damage.DamageType = new DamageTypeDescription { Type = DamageType.Energy, Energy = DamageEnergyType.Acid };
            damage.Value = new ContextDiceValue { DiceType = ooze ? DiceType.D4 : DiceType.D8,
                DiceCountValue = Rank(), BonusValue = Simple(0) };
            damage.IsAoE = true;
            damage.HalfIfSaved = true;
            var positive = ScriptableObject.CreateInstance<ContextConditionCompare>();
            FieldInfo comparison = typeof(ContextConditionCompare).GetField("m_Type",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (comparison == null || !comparison.FieldType.IsEnum)
                throw new MissingFieldException(typeof(ContextConditionCompare).FullName, "m_Type");
            comparison.SetValue(positive, Enum.Parse(comparison.FieldType, "Greater"));
            positive.CheckValue = Rank();
            positive.TargetValue = Simple(0);
            var conditional = ScriptableObject.CreateInstance<Conditional>();
            conditional.ConditionsChecker = new ConditionsChecker { Conditions = new Condition[] { positive } };
            conditional.IfTrue = Actions(damage);
            conditional.IfFalse = Actions();
            // Native damage has a minimum-one clamp even for zero dice. Skip
            // only the damage packet at level one; Ooze's saved branch still runs.
            return conditional;
        }

        private static ContextRankConfig DamageRank()
        {
            var rank = ScriptableObject.CreateInstance<ContextRankConfig>();
            Set(rank, "m_Type", AbilityRankType.Default);
            Set(rank, "m_BaseValueType", ContextRankBaseValueType.CharacterLevel);
            Set(rank, "m_Progression", ContextRankProgression.Div2);
            Set(rank, "m_UseMin", true);
            Set(rank, "m_Min", 0);
            Set(rank, "m_UseMax", true);
            Set(rank, "m_Max", ElementalBreathPolicy.MaximumDamageDice);
            return rank;
        }

        private static void Set(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || !field.FieldType.IsInstanceOfType(value))
                throw new MissingFieldException(target.GetType().FullName, name);
            field.SetValue(target, value);
        }

        private static ContextValue Rank()
        { return new ContextValue { ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.Default }; }
        private static ContextValue Simple(int value)
        { return new ContextValue { ValueType = ContextValueType.Simple, Value = value }; }
        private static ActionList Actions(params GameAction[] actions)
        { return new ActionList { Actions = actions }; }
    }
}
