using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static BlueprintFeature[] ConfigureTrumpet31Feats(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintUnit unit)
        {
            TrumpetBlindFight.VerifyContract();
            var nativeBlind = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                "4e219f5894ad0ea4daa0699e28c37b1d", "captured native Blind-Fight presentation/melee flat-footed consumer");
            var blind = Require<BlueprintFeature>(bySymbol, Trumpet31Prefix + "BlindFight");
            CopyFields(nativeBlind, blind); blind.name = InternalName(Trumpet31Prefix + "BlindFight");
            var melee = ScriptableObject.CreateInstance<TrumpetBlindFight>(); melee.OwningBlueprint = unit;
            var flatFooted = nativeBlind.ComponentsArray.OfType<FlatFootedIgnore>().Single();
            blind.ComponentsArray = new BlueprintComponent[] {
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(flatFooted), melee };
            var nativePersuasive = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
                "86d93a5891d299d4983bdc6ef3987afd", "captured native Persuasive presentation");
            var persuasive = Require<BlueprintFeature>(bySymbol, Trumpet31Prefix + "Persuasive");
            CopyFields(nativePersuasive, persuasive); persuasive.name = InternalName(Trumpet31Prefix + "Persuasive");
            var named = ScriptableObject.CreateInstance<TrumpetPersuasive>(); named.OwningBlueprint = unit;
            persuasive.ComponentsArray = new BlueprintComponent[] { named };
            var access = BlueprintUnitFactAccess.Resolve();
            access.Configure(blind, LocalizationService.Create(Trumpet31Prefix + "BlindFight.Name", "Blind-Fight"),
                LocalizationService.Create(Trumpet31Prefix + "BlindFight.Description",
                    "In melee, reroll a failed concealment miss chance once and retain Blind-Fight's native defenses against unseen melee attackers. Ranged attacks gain no reroll or blindness AC compensation. This feat grants no gaze immunity."), nativeBlind.Icon);
            access.Configure(persuasive, LocalizationService.Create(Trumpet31Prefix + "Persuasive.Name", "Persuasive"),
                LocalizationService.Create(Trumpet31Prefix + "Persuasive.Description",
                    "+2 on Diplomacy and Intimidate, increasing to +4 for a skill with at least10 ranks. The archon's printed Diplomacy has14 ranks; its unlisted Intimidate has0. No Perception or Bluff bonus."), nativePersuasive.Icon);
            return new[] { blind, persuasive };
        }
    }
}
