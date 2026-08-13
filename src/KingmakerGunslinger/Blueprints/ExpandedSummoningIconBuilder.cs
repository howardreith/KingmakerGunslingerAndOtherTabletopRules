using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningIconBuilder
    {
        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            SummonIconCatalog.Validate();
            var exact = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (SummonIconSourceSpec source in SummonIconCatalog.Sources)
            {
                Sprite icon = Resolve(library, source);
                if (icon == null) throw new InvalidOperationException(
                    "Exact summon icon source has no usable icon: " +
                    source.CreatureKey + "/" + source.SourceGuid + ".");
                exact.Add(source.CreatureKey, icon);
            }
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog
                .GenerateVariants(family))
            {
                Sprite icon;
                if (!exact.TryGetValue(variant.Creature.Key, out icon)) continue;
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
        }

        private static Sprite Resolve(LibraryScriptableObject library,
            SummonIconSourceSpec source)
        {
            if (source.Kind == SummonIconSourceKind.UnitPortrait)
            {
                BlueprintUnit unit = BlueprintLibraryLookup.RequireExact<
                    BlueprintUnit>(library, source.SourceGuid,
                        "summon icon portrait source");
                return unit.PortraitSafe == null ? null :
                    unit.PortraitSafe.SmallPortrait;
            }
            if (source.Kind == SummonIconSourceKind.Ability)
                return BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                    library, source.SourceGuid, "summon icon ability source").Icon;
            return BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                library, source.SourceGuid,
                    "summon icon weapon source").Icon;
        }

        private static void Set(IDictionary<string, BlueprintScriptableObject>
            bySymbol, string symbol, Sprite icon)
        {
            BlueprintAbility ability = bySymbol[symbol] as BlueprintAbility;
            if (ability == null) throw new InvalidOperationException(
                "Summon icon target type mismatch: " + symbol + ".");
            BlueprintUnitFactAccess.Resolve().SetIcon(ability, icon);
        }
    }
}
