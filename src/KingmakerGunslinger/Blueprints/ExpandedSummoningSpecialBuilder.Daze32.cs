using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static BlueprintAbility ConfigureLillendDaze32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            string symbol = Lillend32Prefix + "Daze";
            var spell = Require<BlueprintAbility>(bySymbol, symbol);
            CloneMephitSpell(library, spell, "55f14bc84d7c85446b07a1b5dd6b2b4c", "native Daze cantrip");
            spell.name = InternalName(symbol); spell.Parent = null; spell.Type = AbilityType.Spell;
            spell.ActionType = UnitCommand.CommandType.Standard; spell.SetIsFullRoundAction(false);
            if (spell.GetComponent<CantripComponent>() == null || !spell.SpellResistance)
                throw new InvalidOperationException("Native Daze cantrip/SR contract changed.");
            var nativeEffect = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                "9934fedff1b14994ea90205d189c8759", "native Daze condition");
            var effect = Require<BlueprintBuff>(bySymbol, symbol + "Buff");
            CopyFields(nativeEffect, effect); effect.name = InternalName(symbol + "Buff");
            effect.ComponentsArray = nativeEffect.ComponentsArray.Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            var immunity = Require<BlueprintBuff>(bySymbol, symbol + "Immunity");
            immunity.name = InternalName(symbol + "Immunity"); immunity.Stacking = StackingType.Replace;
            var nativeBlock = ScriptableObject.CreateInstance<SpecificBuffImmunity>(); nativeBlock.Buff = nativeEffect;
            var privateBlock = ScriptableObject.CreateInstance<SpecificBuffImmunity>(); privateBlock.Buff = effect;
            immunity.ComponentsArray = new BlueprintComponent[] { nativeBlock, privateBlock };
            BlueprintUnitFactAccess.Resolve().Configure(immunity,
                LocalizationService.Create(symbol + ".Immunity.Name", "Daze Immunity"),
                LocalizationService.Create(symbol + ".Immunity.Description", "Immune to Daze for one minute after being affected."), spell.Icon);
            var immune = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
            immune.Buff = immunity; immune.IsNotDispelable = true; immune.IsFromSpell = false;
            immune.DurationValue = new ContextDurationValue { Rate = DurationRate.Minutes, BonusValue = Simple(1),
                DiceCountValue = Simple(0), DiceType = Kingmaker.RuleSystem.DiceType.Zero };
            var activation = effect.GetComponents<AddFactContextActions>().Single();
            // Only successful effect activation grants immunity. The native
            // RuleApplyBuff immunity component blocks later applications; it
            // does not remove this already active one-round condition.
            activation.Activated = new ActionList { Actions = activation.Activated.Actions.Concat(new GameAction[] { immune }).ToArray() };
            var run = spell.GetComponents<AbilityEffectRunAction>().Single();
            var saved = run.Actions.Actions.Single() as ContextActionConditionalSaved;
            var apply = saved?.Failed.Actions.Single() as ContextActionApplyBuff;
            if (run.SavingThrowType != Kingmaker.EntitySystem.Stats.SavingThrowType.Will || saved == null || saved.Succeed.Actions.Length != 0 ||
                apply == null || !ReferenceEquals(apply.Buff, nativeEffect))
                throw new InvalidOperationException("Native Daze failed-save effect contract changed.");
            apply.Buff = effect; apply.Permanent = false; apply.UseDurationSeconds = false;
            apply.DurationValue = new ContextDurationValue { Rate = DurationRate.Rounds, BonusValue = Simple(1),
                DiceCountValue = Simple(0), DiceType = Kingmaker.RuleSystem.DiceType.Zero };
            var refuse = ScriptableObject.CreateInstance<AbilityTargetHasFact>();
            refuse.Inverted = true; refuse.CheckedFacts = new BlueprintUnitFact[] { immunity };
            spell.ComponentsArray = spell.ComponentsArray.Concat(new BlueprintComponent[] { refuse }).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(spell,
                LocalizationService.Create(symbol + ".Name", "Daze"),
                LocalizationService.Create(symbol + ".Description",
                    "A humanoid of 4 Hit Dice or fewer takes no actions for one round. Will negates; spell resistance applies. Once affected, the creature is immune to Daze for one minute."), spell.Icon);
            return spell;
        }
    }
}
