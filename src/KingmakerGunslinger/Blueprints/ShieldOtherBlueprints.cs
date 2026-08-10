using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class ShieldOtherBlueprintSet
    {
        internal ShieldOtherBlueprintSet(BlueprintAbility ability,
            BlueprintBuff targetBuff)
        { Ability = ability; TargetBuff = targetBuff; }

        internal BlueprintAbility Ability { get; private set; }
        internal BlueprintBuff TargetBuff { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class ShieldOtherBlueprints
    {
        internal const string AbilitySymbol = "KMG.Spells.ShieldOther.Ability";
        internal const string TargetBuffSymbol = "KMG.Spells.ShieldOther.TargetBuff";
        internal const string ShieldOfFaithAbilityGuid =
            "183d5bb91dea3a1489a6db6c9cb64445";

        private const BindingFlags PrivateInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        internal static ShieldOtherBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");

            BlueprintAbility donor = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, ShieldOfFaithAbilityGuid, "native Shield of Faith ability");
            BlueprintBuff donorBuff = ResolveAppliedBuff(donor);
            BlueprintBuff targetBuff = registry.Register<BlueprintBuff>(
                TargetBuffSymbol, () => CreateTargetBuff(donorBuff));
            BlueprintAbility ability = registry.Register<BlueprintAbility>(
                AbilitySymbol, () => CreateAbility(donor, targetBuff));
            Validate(ability, targetBuff);
            return new ShieldOtherBlueprintSet(ability, targetBuff);
        }

        private static BlueprintBuff CreateTargetBuff(BlueprintBuff donor)
        {
            BlueprintBuff result = BlueprintCloneService.Clone(donor,
                "KMG_ShieldOther_TargetBuff");
            result.Stacking = StackingType.Replace;
            result.IsClassFeature = false;
            var ac = ScriptableObject.CreateInstance<AddStatBonus>();
            ac.name = "$KMG_ShieldOther_Deflection";
            ac.Stat = StatType.AC;
            ac.Value = 1;
            ac.Descriptor = ModifierDescriptor.Deflection;
            var saves = ScriptableObject.CreateInstance<BuffAllSavesBonus>();
            saves.name = "$KMG_ShieldOther_Resistance";
            saves.Value = 1;
            saves.Descriptor = ModifierDescriptor.Resistance;
            result.ComponentsArray = new BlueprintComponent[] { ac, saves };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.ShieldOther.TargetBuff.Name",
                    "Shield Other"),
                LocalizationService.Create("KMG.ShieldOther.TargetBuff.Description",
                    "The subject gains a +1 deflection bonus to AC and a +1 resistance bonus on all saving throws. Half of the subject's finalized hit point damage is transferred to the originating caster while the link remains valid."),
                donor.Icon);
            return result;
        }

        private static BlueprintAbility CreateAbility(BlueprintAbility donor,
            BlueprintBuff targetBuff)
        {
            BlueprintAbility result = BlueprintCloneService.Clone(donor,
                "KMG_ShieldOther_Ability");
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.ShieldOther.Ability.Name", "Shield Other"),
                LocalizationService.Create("KMG.ShieldOther.Ability.Description",
                    "This spell wards one allied creature. The subject gains a +1 deflection bonus to AC and a +1 resistance bonus on all saving throws. While the subject remains within close range, half of its finalized hit point damage is transferred to you. The paired 50-gp platinum-ring focus is abstracted in Kingmaker and is not consumed."),
                donor.Icon);
            result.Type = AbilityType.Spell;
            result.Range = AbilityRange.Close;
            result.CanTargetPoint = false;
            result.CanTargetEnemies = false;
            result.CanTargetFriends = true;
            result.CanTargetSelf = false;
            result.SpellResistance = false;
            result.ActionBarAutoFillIgnored = false;
            result.Hidden = false;
            result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.ActionType = UnitCommand.CommandType.Standard;
            result.AvailableMetamagic = donor.AvailableMetamagic;
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.ShieldOther.Ability.Duration", "1 hour/level");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.ShieldOther.Ability.SavingThrow", "None (harmless)");
            result.MaterialComponent = null;
            result.ResourceAssetIds = Array.Empty<string>();

            var rank = ScriptableObject.CreateInstance<ContextRankConfig>();
            rank.name = "$KMG_ShieldOther_CasterLevel";
            SetPrivate(rank, "m_Type", AbilityRankType.Default);
            SetPrivate(rank, "m_BaseValueType", ContextRankBaseValueType.CasterLevel);
            SetPrivate(rank, "m_Progression", ContextRankProgression.AsIs);
            var apply = new ContextActionApplyBuff
            {
                Buff = targetBuff,
                DurationValue = new ContextDurationValue
                {
                    Rate = DurationRate.Hours,
                    DiceType = DiceType.Zero,
                    DiceCountValue = 0,
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.Default
                    }
                },
                IsNotDispelable = false
            };
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.name = "$KMG_ShieldOther_ApplyTargetBuff";
            effect.Actions = new ActionList { Actions = new GameAction[] { apply } };
            AbilitySpawnFx fx = result.ComponentsArray.OfType<AbilitySpawnFx>()
                .SingleOrDefault();
            result.ComponentsArray = fx == null
                ? new BlueprintComponent[] { CreateSpellComponent(), rank, effect }
                : new BlueprintComponent[] { CreateSpellComponent(), rank, effect, fx };
            return result;
        }

        private static SpellComponent CreateSpellComponent()
        {
            var result = ScriptableObject.CreateInstance<SpellComponent>();
            result.name = "$KMG_ShieldOther_Abjuration";
            result.School = SpellSchool.Abjuration;
            return result;
        }

        private static BlueprintBuff ResolveAppliedBuff(BlueprintAbility donor)
        {
            ContextActionApplyBuff[] actions = donor.ComponentsArray
                .OfType<AbilityEffectRunAction>()
                .SelectMany(value => value.Actions.Actions)
                .OfType<ContextActionApplyBuff>().ToArray();
            if (actions.Length != 1 || actions[0].Buff == null)
                throw new InvalidOperationException(
                    "Native Shield of Faith did not expose one applied presentation buff.");
            return actions[0].Buff;
        }

        private static void SetPrivate(object instance, string name, object value)
        {
            FieldInfo field = instance.GetType().GetField(name, PrivateInstance);
            if (field == null)
                throw new MissingFieldException(instance.GetType().FullName, name);
            field.SetValue(instance, value);
        }

        private static void Validate(BlueprintAbility ability, BlueprintBuff buff)
        {
            SpellComponent spell = ability.ComponentsArray.OfType<SpellComponent>().Single();
            ContextActionApplyBuff apply = ability.ComponentsArray
                .OfType<AbilityEffectRunAction>().Single().Actions.Actions
                .OfType<ContextActionApplyBuff>().Single();
            AddStatBonus ac = buff.ComponentsArray.OfType<AddStatBonus>().Single();
            BuffAllSavesBonus saves = buff.ComponentsArray
                .OfType<BuffAllSavesBonus>().Single();
            if (spell.School != SpellSchool.Abjuration ||
                ability.Type != AbilityType.Spell ||
                ability.ActionType != UnitCommand.CommandType.Standard ||
                ability.Range != AbilityRange.Close || !ability.CanTargetFriends ||
                ability.CanTargetSelf || ability.CanTargetEnemies ||
                !ReferenceEquals(apply.Buff, buff) || apply.IsNotDispelable ||
                apply.DurationValue.Rate != DurationRate.Hours ||
                !apply.DurationValue.IsExtendable || ac.Stat != StatType.AC ||
                ac.Value != 1 || ac.Descriptor != ModifierDescriptor.Deflection ||
                saves.Value != 1 || saves.Descriptor != ModifierDescriptor.Resistance ||
                buff.Stacking != StackingType.Replace)
                throw new InvalidOperationException(
                    "Shield Other blueprint contract is incomplete.");
        }
    }
}
