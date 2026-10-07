using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
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
            if (ExpandedSummoningProjectIcons.LoadedCount !=
                    SummonIconCatalog.All.Count ||
                ExpandedSummoningProjectIcons.FallbackCount != 0)
                throw new InvalidOperationException(
                    "Project summon icons were not loaded exactly once.");
            foreach (SummonFamily family in new[] { SummonFamily.Monster,
                SummonFamily.NaturesAlly })
            foreach (SummonVariantSpec variant in ExpandedSummoningCatalog
                .GenerateVariants(family))
            {
                // Hidden registration still owns its icon for private exact
                // qualification routes; this does not publish a menu choice.
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
            // The published Wasp's inspectable creature type is its own
            // visible consumer, separate from the summon menu abilities.
            BlueprintScriptableObject waspType;
            if (!bySymbol.TryGetValue(
                    "KMG.Summoning.Natural.GiantWasp.UnitType", out waspType) ||
                !(waspType is BlueprintUnitType))
                throw new InvalidOperationException(
                    "Published Giant Wasp unit type is missing.");
            ((BlueprintUnitType)waspType).Image =
                ExpandedSummoningProjectIcons.Require("giant-wasp");
            foreach (string key in new[] { "viper", "constrictor-snake" })
            {
                string token = key == "viper" ? "Viper" : "ConstrictorSnake";
                BlueprintScriptableObject value;
                if (!bySymbol.TryGetValue("KMG.Summoning.Natural." + token + ".UnitType", out value) ||
                    !(value is BlueprintUnitType))
                    throw new InvalidOperationException("Registered snake unit type is missing.");
                ((BlueprintUnitType)value).Image = ExpandedSummoningProjectIcons.Require(key);
            }
            // Constrictor's passive species trait intentionally shares its
            // existing coiled-snake painting. No new command or pixels.
            BlueprintScriptableObject traits;
            if (!bySymbol.TryGetValue(SummonIconCatalog.ConstrictorTraitsSymbol, out traits) ||
                !(traits is BlueprintBuff) || traits.AssetGuid != "f83dfefcac58495c9a0f5c89a4483ddf")
                throw new InvalidOperationException("Exact Constrictor passive trait icon consumer is missing.");
            BlueprintUnitFactAccess.Resolve().SetIcon((BlueprintBuff)traits,
                ExpandedSummoningProjectIcons.Require(
                    SummonIconCatalog.PassiveTraitIconFor(SummonIconCatalog.ConstrictorTraitsSymbol)));
            // The Cyclops's own summon icon marks its Flash of Insight on the
            // action bar; the ability has no separate art of its own.
            Set(bySymbol, "KMG.Summoning.Special.Cyclops.FlashOfInsight",
                ExpandedSummoningProjectIcons.Require("cyclops"));
            // Sprint 8: the Cheetah's project sprint has no art of its own.
            Set(bySymbol, "KMG.Summoning.Special.Cheetah.Sprint",
                ExpandedSummoningProjectIcons.Require("cheetah"));
            // Sprint 6: the Giant Spider's project web has no art of its own.
            Set(bySymbol, "KMG.Summoning.Special.GiantSpider.Web",
                ExpandedSummoningProjectIcons.Require("giant-spider"));
            // Remove Stirge is a distinct prey action with its own original
            // hand-and-creature painting, separate from the summon portrait.
            Set(bySymbol, "KMG.Summoning.Special.Stirge.Remove",
                ExpandedSummoningProjectIcons.Require("remove-stirge"));
            // Sprint 5: the cloned native breaths and spells keep their native
            // art; the two project bursts (Dehydrate, Boiling Rain) have none
            // of their own and wear their mephit's summon icon.
            foreach (MephitVariantProfile profile in
                ExpandedSummoningSpecialProfiles.MephitVariants)
                foreach (KeyValuePair<string, string> slot in
                    ExpandedSummoningSpecialBuilder.MephitSpellLikeSlots(profile))
                    if (ExpandedSummoningSpecialBuilder.IsProjectMephitAbility(slot.Value))
                        Set(bySymbol, "KMG.Summoning.Special." +
                            ExpandedSummoningSpecialBuilder.MephitToken(profile.Key) +
                            "." + slot.Key,
                            ExpandedSummoningProjectIcons.Require(profile.Key));
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
