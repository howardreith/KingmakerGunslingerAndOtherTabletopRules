using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private const string FeyCharmBuff32 = "KMG.Summoning.FeyCelestial.CharmBuff";
        private const string FeySleepBuff32 = "KMG.Summoning.FeyCelestial.SleepBuff";
        private const string NativeCharmBuff32 = "9dc29118addce3d48ae9b92be953b5b4";
        private const string NativeSleepBuff32 = "c9937d7846aa9ae46991e9f298be644a";

        // Native59 captured these exact terminal blueprints surviving their
        // summoned source. Private mappings use the already shipped native
        // missing-caster component; no global sweep or new lifecycle manager.
        private static void ConfigureFeyCelestialControlBuffs32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            foreach (bool sleep in new[] { false, true })
            {
                string symbol = sleep ? FeySleepBuff32 : FeyCharmBuff32;
                var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                    sleep ? NativeSleepBuff32 : NativeCharmBuff32, "observed Sprint32 native control terminal");
                var buff = Require<BlueprintBuff>(bySymbol, symbol);
                CopyFields(native, buff); buff.name = InternalName(symbol);
                buff.ComponentsArray = native.ComponentsArray.Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                    .Concat(new BlueprintComponent[] { ScriptableObject.CreateInstance<RemoveBuffIfCasterIsMissing>(),
                        ScriptableObject.CreateInstance<FeyControlCasterCleanup32>() }).ToArray();
                buff.ResourceAssetIds = Array.Empty<string>();
                BlueprintUnitFactAccess.Resolve().Configure(buff,
                    LocalizationService.Create(symbol + ".Name", sleep ? "Sleep" : "Charm"),
                    LocalizationService.Create(symbol + ".Description",
                        "This summoned creature's control effect ends when its source disappears."), native.Icon);
            }
        }

        private static void BindFeyCelestialSleep32(BlueprintAbility ability,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var applications = FeySleepApplications32(ability.GetComponents<AbilityEffectRunAction>().Single().Actions).ToArray();
            // The native graph has separate living and gated undead branches.
            // Preserve both eligibility paths and rebind their identical effect.
            if (applications.Length != 2 || applications.Any(apply => apply.Buff?.AssetGuid != NativeSleepBuff32))
                throw new InvalidOperationException("Native Sleep terminals differ from the selected two-branch mapping; count=" +
                    applications.Length + "; terminals=" + string.Join(",", applications.Select(apply =>
                        (apply.Buff?.AssetGuid ?? "<missing>") + ":" + (apply.Buff?.name ?? "<missing>"))));
            foreach (var apply in applications) apply.Buff = Require<BlueprintBuff>(bySymbol, FeySleepBuff32);
            // Preserve native shared HD, target selection, duration and saves.
        }

        private static IEnumerable<ContextActionApplyBuff> FeySleepApplications32(ActionList list)
        {
            foreach (var action in list.Actions)
            {
                var apply = action as ContextActionApplyBuff;
                if (apply != null) { yield return apply; continue; }
                if (action is ContextActionChangeSharedValue || action is ContextActionSpawnFx) continue;
                var conditional = action as Conditional;
                var save = action as ContextActionSavingThrow;
                var saved = action as ContextActionConditionalSaved;
                ActionList[] children = conditional != null ? new[] { conditional.IfTrue, conditional.IfFalse } :
                    save != null ? new[] { save.Actions } : saved != null ? new[] { saved.Succeed, saved.Failed } : null;
                if (children == null) throw new InvalidOperationException("Unexpected native Sleep action " + action.GetType().Name);
                foreach (var child in children)
                    foreach (var leaf in FeySleepApplications32(child)) yield return leaf;
            }
        }

        private static void BindFeyCelestialCharm32(BlueprintAbility ability, string symbol,
            IDictionary<string, BlueprintScriptableObject> bySymbol, DurationRate duration)
        {
            if (duration != DurationRate.Hours && duration != DurationRate.Days)
                throw new InvalidOperationException("Only the selected PF1 Charm durations are supported.");
            var applications = LillendCharmApplications32(ability.GetComponents<AbilityEffectRunAction>().Single().Actions).ToArray();
            if (applications.Length != 2 || applications.Any(apply => apply.Buff?.AssetGuid != NativeCharmBuff32))
                throw new InvalidOperationException("Observed native Charm terminal graph changed.");
            foreach (var apply in applications)
            {
                apply.Buff = Require<BlueprintBuff>(bySymbol, FeyCharmBuff32);
                apply.Permanent = false; apply.UseDurationSeconds = false;
                apply.DurationValue = new ContextDurationValue { Rate = duration,
                    DiceType = DiceType.Zero, DiceCountValue = Simple(0),
                    BonusValue = new ContextValue { ValueType = ContextValueType.Rank, ValueRank = AbilityRankType.Default } };
            }
            var rank = ability.GetComponents<ContextRankConfig>().SingleOrDefault();
            if (rank == null)
            {
                rank = ScriptableObject.CreateInstance<ContextRankConfig>();
                ability.ComponentsArray = ability.ComponentsArray.Concat(new BlueprintComponent[] { rank }).ToArray();
            }
            SetField(rank, "m_BaseValueType", ContextRankBaseValueType.CasterLevel);
            SetField(rank, "m_Progression", ContextRankProgression.AsIs);
            SetField(rank, "m_UseMin", false); SetField(rank, "m_UseMax", false);
            ability.LocalizedDuration = LocalizationService.Create(symbol + ".Duration",
                duration == DurationRate.Days ? "1 day/level, ending when the summon disappears" :
                "1 hour/level, ending when the summon disappears");
            AlternateSummoningAdapterBuilder.ResetComponentCaches(ability);
        }

        private static BlueprintAbility LillendOwnedControlSpell32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, bool sleep)
        {
            string symbol = Lillend32Prefix + (sleep ? "Sleep" : "CharmPerson");
            var spell = Require<BlueprintAbility>(bySymbol, symbol);
            CloneMephitSpell(library, spell, sleep ? "bb7ecad2d3d2c8247a38f44855c99061" :
                "1af9d5995090e5a4185a30decf0959ad", "native Lillend owned control spell");
            spell.name = InternalName(symbol); spell.Parent = null; spell.Type = AbilityType.Spell;
            if (sleep) BindFeyCelestialSleep32(spell, bySymbol);
            else BindFeyCelestialCharm32(spell, symbol, bySymbol, DurationRate.Hours);
            BlueprintUnitFactAccess.Resolve().Configure(spell,
                LocalizationService.Create(symbol + ".Name", sleep ? "Sleep" : "Charm Person"),
                LocalizationService.Create(symbol + ".Description", sleep ?
                    "Native Sleep with a shared four-Hit-Dice budget. The effect ends when this summon disappears." :
                    "Charm an eligible humanoid using native Charm behavior. Will negates; spell resistance applies. Duration: one hour per caster level, ending when this summon disappears."), spell.Icon);
            return spell;
        }

        private static BlueprintAbility LillendOwnedHoldPerson32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            const string nativeBuffGuid = "11cb2fe4fe9c44b448cfe1788ae1ab59";
            string symbol = Lillend32Prefix + "HoldPerson";
            var spell = Require<BlueprintAbility>(bySymbol, symbol);
            CloneMephitSpell(library, spell, "c7104f7526c4c524f91474614054547e", "observed native Hold Person");
            spell.name = InternalName(symbol); spell.Parent = null; spell.Type = AbilityType.Spell;
            var nativeBuff = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                nativeBuffGuid, "observed native Hold Person terminal");
            var condition = nativeBuff.GetComponent<BuffStatusCondition>();
            if (condition == null || condition.Condition != Kingmaker.UnitLogic.UnitCondition.Paralyzed ||
                !condition.SaveEachRound || condition.SaveType != Kingmaker.EntitySystem.Stats.SavingThrowType.Will)
                throw new InvalidOperationException("Native Hold Person automatic repeat-save contract changed.");
            var buff = Require<BlueprintBuff>(bySymbol, symbol + "Buff");
            CopyFields(nativeBuff, buff); buff.name = InternalName(symbol + "Buff");
            buff.ComponentsArray = nativeBuff.ComponentsArray.Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .Concat(new BlueprintComponent[] { ScriptableObject.CreateInstance<RemoveBuffIfCasterIsMissing>(),
                    ScriptableObject.CreateInstance<FeyControlCasterCleanup32>() }).ToArray();
            buff.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(symbol + "Buff.Name", "Hold Person"),
                LocalizationService.Create(symbol + "Buff.Description",
                    "Paralyzed, with an automatic Will save each round. Ends when this summon disappears."), nativeBuff.Icon);
            var applications = LillendCharmApplications32(spell.GetComponents<AbilityEffectRunAction>().Single().Actions).ToArray();
            if (applications.Length == 0 || applications.Any(apply => apply.Buff?.AssetGuid != nativeBuffGuid))
                throw new InvalidOperationException("Native Hold Person terminal graph changed.");
            foreach (var apply in applications) apply.Buff = buff;
            // Owner adaptation 2026-10-26: keep the native automatic round save
            // without an action debit. Preserve normal initial Will/SR, humanoid
            // checks, rounds/level, casting actions and the real bard spell slot.
            BlueprintUnitFactAccess.Resolve().Configure(spell,
                LocalizationService.Create(symbol + ".Name", "Hold Person"),
                LocalizationService.Create(symbol + ".Description",
                    "Paralyze an eligible humanoid. Will negates; spell resistance applies. An automatic Will save each round ends the effect on success, without using an action. Lasts one round per caster level, ending when this summon disappears."), spell.Icon);
            AlternateSummoningAdapterBuilder.ResetComponentCaches(spell);
            return spell;
        }
    }
}
