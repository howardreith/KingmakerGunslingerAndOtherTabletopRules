using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static BlueprintAbility ConfigureLillendLullaby32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            string symbol = Lillend32Prefix + "Lullaby";
            var spell = Require<BlueprintAbility>(bySymbol, symbol);
            var native = LillendNativeSpell32(library, "bb7ecad2d3d2c8247a38f44855c99061", "Sleep");
            var icon = ExpandedSummoningProjectIcons.Require("lullaby");
            CopyFields(native, spell);
            spell.name = InternalName(symbol); spell.Parent = null; spell.Type = AbilityType.Spell;
            spell.ActionType = UnitCommand.CommandType.Standard; spell.SetIsFullRoundAction(false);
            spell.Range = AbilityRange.Medium; spell.SpellResistance = true;
            spell.CanTargetPoint = true; spell.CanTargetEnemies = true; spell.CanTargetFriends = true;
            spell.CanTargetSelf = true; spell.EffectOnEnemy = spell.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            spell.Hidden = false; spell.NeedEquipWeapons = false;
            spell.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            spell.LocalizedDuration = LocalizationService.Create(symbol + ".Duration", "1 round/level after immediate concentration release");
            spell.LocalizedSavingThrow = LocalizationService.Create(symbol + ".SavingThrow", "Will negates");

            var buff = Require<BlueprintBuff>(bySymbol, symbol + "Buff");
            buff.name = InternalName(symbol + "Buff"); buff.Stacking = StackingType.Replace;
            var perception = ScriptableObject.CreateInstance<AddStatBonus>();
            perception.Stat = StatType.SkillPerception; perception.Value = -5;
            perception.Descriptor = ModifierDescriptor.Penalty;
            var sleepSave = ScriptableObject.CreateInstance<LillendLullabySleepPenalty32>();
            sleepSave.SpellDescriptor = SpellDescriptor.Sleep;
            sleepSave.ModifierDescriptor = ModifierDescriptor.Penalty;
            sleepSave.Value = -2; sleepSave.Bonus = Simple(0); sleepSave.OnlyPositiveValue = false;
            var buffDescriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            buffDescriptor.Descriptor = SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion;
            buff.ComponentsArray = new BlueprintComponent[] { perception, sleepSave, buffDescriptor };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(symbol + ".Buff.Name", "Lullaby"),
                LocalizationService.Create(symbol + ".Buff.Description",
                    "Drowsy and inattentive: -5 on Perception checks and -2 on Will saves against sleep effects."), icon);

            var around = ScriptableObject.CreateInstance<AbilityTargetsAround>();
            SetField(around, "m_Radius", new Feet(10));
            SetField(around, "m_TargetType", Kingmaker.UnitLogic.Abilities.Components.TargetType.Any);
            SetField(around, "m_IncludeDead", false); SetField(around, "m_SpreadSpeed", new Feet(0));
            var notUndead = ScriptableObject.CreateInstance<ContextConditionHasFact>();
            notUndead.Fact = Feature(library, UndeadTypeGuid, "Lullaby living target: undead type"); notUndead.Not = true;
            var notConstruct = ScriptableObject.CreateInstance<ContextConditionHasFact>();
            notConstruct.Fact = Feature(library, ConstructTypeGuid, "Lullaby living target: construct type"); notConstruct.Not = true;
            SetField(around, "m_Condition", new ConditionsChecker { Operation = Operation.And,
                Conditions = new Condition[] { notUndead, notConstruct } });

            var apply = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
            apply.Buff = buff; apply.IsFromSpell = true; apply.Permanent = false;
            // Owner-approved policy: release concentration immediately after the
            // normal cast. The native finite tail remains one round per live CL.
            // It is not a child of a performance or of the summon's lifetime.
            apply.DurationValue = new ContextDurationValue { Rate = DurationRate.Rounds,
                DiceType = DiceType.Zero, DiceCountValue = Simple(0),
                BonusValue = new ContextValue { ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.Default } };
            var saved = ScriptableObject.CreateInstance<ContextActionConditionalSaved>();
            saved.Succeed = new ActionList { Actions = Array.Empty<GameAction>() };
            saved.Failed = new ActionList { Actions = new GameAction[] { apply } };
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.SavingThrowType = SavingThrowType.Will;
            run.Actions = new ActionList { Actions = new GameAction[] { saved } };
            var rank = ScriptableObject.CreateInstance<ContextRankConfig>();
            SetField(rank, "m_BaseValueType", ContextRankBaseValueType.CasterLevel);
            SetField(rank, "m_Progression", ContextRankProgression.AsIs);
            SetField(rank, "m_UseMin", false); SetField(rank, "m_UseMax", false);
            var school = ScriptableObject.CreateInstance<SpellComponent>(); school.School = SpellSchool.Enchantment;
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.MindAffecting | SpellDescriptor.Compulsion;
            spell.ComponentsArray = new BlueprintComponent[] { around, run, rank, school, descriptor,
                ScriptableObject.CreateInstance<CantripComponent>() };
            BlueprintUnitFactAccess.Resolve().Configure(spell,
                LocalizationService.Create(symbol + ".Name", "Lullaby"),
                LocalizationService.Create(symbol + ".Description",
                    "Living creatures in a 10-foot burst become drowsy: -5 Perception and -2 Will against sleep. Will negates; spell resistance applies. This lillend releases concentration immediately, leaving one round per caster level (7 rounds at caster level 7)."), icon);
            return spell;
        }
    }
}
