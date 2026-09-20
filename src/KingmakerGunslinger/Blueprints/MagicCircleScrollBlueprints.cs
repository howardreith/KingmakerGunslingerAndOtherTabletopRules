using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class MagicCircleScrollBlueprints
    {
        // Native third-level Protection from Energy scroll: 375 gp, CL5.
        internal const string DonorId = "c2cadc1c19cc4b54eb73b987f540ddca";
        internal static BlueprintItemEquipmentUsable Create(LibraryScriptableObject library,
            string alignment, BlueprintAbility spell, Sprite icon)
        {
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintItemEquipmentUsable>(library, DonorId,
                "native level-three protective scroll economics and activation");
            if (donor.Type != UsableItemType.Scroll || donor.Cost != 375 || donor.SpellLevel != 3 || donor.CasterLevel != 5)
                throw new InvalidOperationException("Native level-three protective scroll contract changed.");
            var scroll = BlueprintCloneService.Clone(donor, "KMG_MagicCircle_" + alignment + "_Scroll");
            scroll.Ability = spell;
            scroll.SpellLevel = 3;
            scroll.CasterLevel = 5;
            scroll.Charges = 1;
            scroll.SpendCharges = true;
            scroll.RestoreChargesOnRest = false;
            // Replace the complete donor component inventory. The only native
            // component is CopyScroll, whose old Protection association must not
            // be retained or mutated through a shared clone instance.
            if (donor.ComponentsArray.Length != 1 || donor.ComponentsArray.OfType<CopyScroll>().Count() != 1)
                throw new InvalidOperationException("Native protective scroll components changed.");
            var copy = ScriptableObject.CreateInstance<CopyScroll>();
            copy.name = "$KMG_MagicCircle_CopyCanonicalSpell";
            copy.CustomSpell = spell;
            scroll.ComponentsArray = new BlueprintComponent[] { copy };
            var access = BlueprintItemAccess.Resolve();
            string key = "KMG.MagicCircle." + alignment + ".Scroll";
            access.Configure(scroll, LocalizationService.Create(key + ".Name", "Scroll of Magic Circle against " + alignment),
                LocalizationService.Create(key + ".Description", spell.Description), LocalizationService.Create(key + ".Flavor", string.Empty), donor.Cost, donor.Weight);
            // Intentional family sharing: the actual item uses the approved
            // alignment painting. No unrelated donor glyph or proprietary shell
            // is copied into a distributed bitmap. Native item frames stay native.
            access.SetIcon(scroll, icon);
            return scroll;
        }
    }
}
