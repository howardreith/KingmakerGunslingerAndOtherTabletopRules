using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningIconBuilder
    {
        internal static void Configure(
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            if (bySymbol == null) throw new ArgumentNullException("bySymbol");
            SummonIconCatalog.Validate();
            if (ExpandedSummoningProjectIcons.LoadedCount != 77 ||
                ExpandedSummoningProjectIcons.FallbackCount != 0)
                throw new InvalidOperationException(
                    "Project summon icons were not loaded exactly once.");
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog
                .GenerateVariants(family))
            {
                Sprite icon = ExpandedSummoningProjectIcons.Require(
                    variant.Creature.Key);
                string symbol = ExpandedSummoningIdentityCatalog.AbilitySymbol(
                    variant);
                Set(bySymbol, symbol, icon);
                if (family == SummonFamily.Monster &&
                    variant.Creature.MonsterTemplated)
                {
                    Set(bySymbol, symbol + ".Celestial", icon);
                    Set(bySymbol, symbol + ".Fiendish", icon);
                }
            }
            foreach (SummonNativeExpansionSpec native in
                SummonNativeExpansionCatalog.All)
                Set(bySymbol, native.Symbol,
                    ExpandedSummoningProjectIcons.Require(native.IconKey));
        }

        private static void Set(IDictionary<string, BlueprintScriptableObject>
            bySymbol, string symbol, Sprite icon)
        {
            BlueprintScriptableObject value;
            BlueprintAbility ability = bySymbol.TryGetValue(symbol, out value) ?
                value as BlueprintAbility : null;
            if (ability == null) throw new InvalidOperationException(
                "Summon icon target type mismatch: " + symbol + ".");
            BlueprintUnitFactAccess.Resolve().SetIcon(ability, icon);
        }
    }
}
