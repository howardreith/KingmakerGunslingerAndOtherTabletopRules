using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Spells.Teleportation;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class TeleportationScrollBlueprintSet
    {
        internal TeleportationScrollBlueprintSet(BlueprintItemEquipmentUsable teleport,
            BlueprintItemEquipmentUsable greater, BlueprintItemEquipmentUsable recall)
        { Teleport = teleport; GreaterTeleport = greater; WordOfRecall = recall; }
        internal BlueprintItemEquipmentUsable Teleport { get; private set; }
        internal BlueprintItemEquipmentUsable GreaterTeleport { get; private set; }
        internal BlueprintItemEquipmentUsable WordOfRecall { get; private set; }
    }

    // Real native scroll items pointing at the canonical strategic spell
    // abilities (never duplicate scroll-only spells). The donors are verified
    // native scrolls whose cost/caster-level pairs already match the approved
    // design values, so native merchant pricing stays consistent.
    internal static class TeleportationScrollBlueprints
    {
        internal const int IdentityCount = 3;
        internal const string TeleportSymbol = "KMG.Spells.Teleport.Scroll";
        internal const string GreaterTeleportSymbol = "KMG.Spells.GreaterTeleport.Scroll";
        internal const string WordOfRecallSymbol = "KMG.Spells.WordOfRecall.Scroll";
        // Verified native scroll donors (observe-teleportation-native-contracts):
        // 1125 gp / CL 9, 2275 gp / CL 13 and 1650 gp / CL 11 respectively.
        internal const string TeleportDonorId = "02086fbbda266ed4b8e9124abe5abd75";
        internal const string GreaterTeleportDonorId = "0033529da3b90bd226232e1962ca34ba";
        internal const string WordOfRecallDonorId = "00843bddf42908953a0d77e7155c20f0";

        internal static TeleportationScrollBlueprintSet Register(LibraryScriptableObject library,
            BlueprintRegistry registry, TeleportationSpellBlueprintSet spells)
        {
            if (spells == null) throw new ArgumentNullException("spells");
            var teleport = registry.Register<BlueprintItemEquipmentUsable>(TeleportSymbol, () => Create(library,
                TeleportDonorId, "Teleport", spells.Teleport, cost: 1125, casterLevel: 9, spellLevel: 5,
                "This scroll holds a single use of the strategic world-map Teleport. Use it by selecting a previously visited destination on the world map and choosing its scroll action."));
            var greater = registry.Register<BlueprintItemEquipmentUsable>(GreaterTeleportSymbol, () => Create(library,
                GreaterTeleportDonorId, "GreaterTeleport", spells.GreaterTeleport, cost: 2275, casterLevel: 13, spellLevel: 7,
                "This scroll holds a single use of the strategic world-map Greater Teleport. Use it by selecting a previously visited destination on the world map and choosing its scroll action."));
            var recall = registry.Register<BlueprintItemEquipmentUsable>(WordOfRecallSymbol, () => Create(library,
                WordOfRecallDonorId, "WordOfRecall", spells.WordOfRecall, cost: 1650, casterLevel: 11, spellLevel: 6,
                "This scroll holds a single use of the strategic world-map Word of Recall. Use it by selecting your sanctuary destination on the world map and choosing its scroll action."));
            Validate(teleport, spells.Teleport);
            Validate(greater, spells.GreaterTeleport);
            Validate(recall, spells.WordOfRecall);
            var result = new TeleportationScrollBlueprintSet(teleport, greater, recall);
            if (new[] { teleport.AssetGuid, greater.AssetGuid, recall.AssetGuid }.Distinct(StringComparer.Ordinal).Count() != IdentityCount)
                throw new InvalidOperationException("Strategic scroll identities must be distinct.");
            return result;
        }

        private static BlueprintItemEquipmentUsable Create(LibraryScriptableObject library, string donorId,
            string key, BlueprintAbility spell, int cost, int casterLevel, int spellLevel, string description)
        {
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintItemEquipmentUsable>(library, donorId,
                "native scroll donor " + key);
            if (donor.Cost != cost || donor.CasterLevel != casterLevel)
                throw new InvalidOperationException("Native scroll donor cost/caster-level contract differs: " + key);
            var scroll = BlueprintCloneService.Clone(donor, "KMG_ScrollOf" + key);
            // Stackable native scroll presentation; the donor's icon, weight and
            // unidentified labels are already the correct native scroll texts.
            BlueprintItemAccess.Resolve().Configure(scroll,
                LocalizationService.Create("KMG.Teleportation.Scroll." + key + ".Name",
                    "Scroll of " + key.Replace("GreaterTeleport", "Greater Teleport").Replace("WordOfRecall", "Word of Recall")),
                LocalizationService.Create("KMG.Teleportation.Scroll." + key + ".Description", description),
                LocalizationService.Create("KMG.Teleportation.Scroll." + key + ".Flavor", string.Empty),
                cost, donor.Weight);
            scroll.CasterLevel = casterLevel;
            scroll.SpellLevel = spellLevel;
            scroll.Ability = spell;
            // Exact native scroll activation semantics: one charge, consumed on
            // a successful activation, never restored by rest, and a UMD check
            // for readers whose class lists lack the spell.
            scroll.SpendCharges = true;
            scroll.Charges = 1;
            scroll.RestoreChargesOnRest = false;
            // RequireUMDIfCasterHasNoSpellInSpellList is derived from the item
            // type; a genuine scroll (Type == Scroll) always requires UMD from
            // readers whose class lists lack the spell.
            if (scroll.Type != UsableItemType.Scroll)
                throw new InvalidOperationException("Native scroll donor item type differs: " + key);
            // The scroll/copy learning association must resolve to the same
            // canonical strategic spell, never a scroll-only duplicate.
            var copies = scroll.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>().ToArray();
            if (copies.Length != 1)
                throw new InvalidOperationException("Native scroll CopyScroll contract differs: " + key);
            copies[0].CustomSpell = spell;
            return scroll;
        }

        private static void Validate(BlueprintItemEquipmentUsable scroll, BlueprintAbility spell)
        {
            if (scroll.Ability == null || scroll.Ability != spell)
                throw new InvalidOperationException("Strategic scroll does not reference its canonical spell ability.");
            var copy = scroll.ComponentsArray.OfType<Kingmaker.Blueprints.Items.Components.CopyScroll>().SingleOrDefault();
            if (copy == null || copy.CustomSpell != scroll.Ability)
                throw new InvalidOperationException("Strategic scroll copy learning does not reference the canonical spell.");
            if (scroll.Cost <= 0 || scroll.CasterLevel <= 0 || scroll.SpellLevel <= 0)
                throw new InvalidOperationException("Strategic scroll economics are incomplete.");
        }
    }
}
