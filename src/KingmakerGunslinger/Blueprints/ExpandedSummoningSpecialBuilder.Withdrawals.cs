using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        /// <summary>
        /// Repair R1: one load-time fixer per creature that lost a
        /// capability. Runs after every creature is configured, so a builder
        /// that replaces a unit's component array cannot drop it. A fresh
        /// summon never has the withdrawn facts, so the fixer only ever acts
        /// on a summon saved before the withdrawal.
        /// </summary>
        private static void ConfigureWithdrawnCapabilityFixers(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            WithholdDonorInheritedSunder(library, bySymbol);
            foreach (WithdrawnCapabilitySpec spec in SummonPlaytestWithdrawalPolicy.All)
            {
                BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, spec.UnitSymbol);
                BlueprintUnitFact[] facts = spec.FactSymbols
                    .Select(symbol => Require<BlueprintUnitFact>(bySymbol, symbol))
                    .Concat(spec.NativeFeatGuids.Select(guid =>
                        (BlueprintUnitFact)BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                            library, guid, "native feat withdrawn from " + spec.CreatureKey)))
                    .ToArray();
                // A withdrawn fact must really be gone from a fresh summon;
                // otherwise this would only hide a grant the builder still makes.
                if (facts.Any(fact => (unit.AddFacts ?? Array.Empty<BlueprintUnitFact>())
                        .Contains(fact)))
                    throw new InvalidOperationException(
                        "Withdrawn capability is still granted to " + spec.CreatureKey + ".");
                BlueprintAiAction[] actions = spec.AiActionSymbols
                    .Select(symbol => Require<BlueprintAiAction>(bySymbol, symbol))
                    .Concat(spec.NativeAiActionGuids.Select(guid =>
                        BlueprintLibraryLookup.RequireExact<BlueprintAiAction>(
                            library, guid, "native AI action withdrawn from " + spec.CreatureKey)))
                    .ToArray();
                if (unit.Brain != null && actions.Any(action =>
                        (unit.Brain.Actions ?? Array.Empty<BlueprintAiAction>()).Contains(action)))
                    throw new InvalidOperationException(
                        "Withdrawn AI action is still in the brain of " + spec.CreatureKey + ".");
                var fixer = ScriptableObject.CreateInstance<SummonWithdrawnCapabilityFixer>();
                fixer.name = SummonPlaytestWithdrawalPolicy.FixerName(spec.CreatureKey);
                fixer.OwningBlueprint = unit;
                fixer.Facts = facts;
                fixer.AiActions = actions;
                fixer.Resources = spec.ResourceSymbols
                    .Select(symbol => Require<BlueprintAbilityResource>(bySymbol, symbol)).ToArray();
                unit.ComponentsArray = (unit.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .Where(value => !(value is SummonWithdrawnCapabilityFixer))
                    .Concat(new BlueprintComponent[] { fixer }).ToArray();
            }
        }

        /// <summary>
        /// SUNDER-01: the Greater and Elder Earth Elementals are clones of
        /// native donors that carry Improved Sunder in their own AddFacts and
        /// the shared native GreaterEarthElementalBrain (SunderArmorAiAction
        /// plus an attack scored around sundered targets). Each KMG unit drops
        /// the feat and takes the unmodified native Dumb Monster Brain, the
        /// ordinary-attack brain the Huge Earth Elemental already uses. Only
        /// these KMG-owned unit blueprints change; the native feat, brain,
        /// actions and donors do not. Any other input fails closed.
        /// </summary>
        private static void WithholdDonorInheritedSunder(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintBrain sunderBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library,
                SummonPlaytestWithdrawalPolicy.NativeGreaterEarthElementalBrainGuid, "native Greater Earth Elemental brain");
            BlueprintBrain ordinaryBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library,
                SummonPlaytestWithdrawalPolicy.NativeDumbMonsterBrainGuid, "native ordinary-attack brain");
            BlueprintAiAction sunderAi = BlueprintLibraryLookup.RequireExact<BlueprintAiAction>(library,
                SummonPlaytestWithdrawalPolicy.NativeSunderArmorAiActionGuid, "native Sunder Armor AI action");
            if ((ordinaryBrain.Actions ?? Array.Empty<BlueprintAiAction>()).Contains(sunderAi))
                throw new InvalidOperationException("The native ordinary-attack brain unexpectedly sunders.");
            foreach (string key in SummonPlaytestWithdrawalPolicy.DonorInheritedSunderKeys)
            {
                WithdrawnCapabilitySpec spec = SummonPlaytestWithdrawalPolicy.For(key);
                BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, spec.UnitSymbol);
                if (!ReferenceEquals(unit.Brain, sunderBrain) && !ReferenceEquals(unit.Brain, ordinaryBrain))
                    throw new InvalidOperationException("Unexpected inherited brain on " + key + ".");
                unit.Brain = ordinaryBrain;
                BlueprintUnitFact[] facts = unit.AddFacts ?? Array.Empty<BlueprintUnitFact>();
                unit.AddFacts = facts.Where(fact => fact == null ||
                    SummonPlaytestWithdrawalPolicy.GrantedNativeFeats(key, new[] { fact.AssetGuid }).Length == 1)
                    .ToArray();
            }
        }
    }
}
