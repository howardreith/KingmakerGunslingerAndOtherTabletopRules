using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class GunsmithingBlueprints
    {
        internal const string Symbol = "KMG.Classes.Gunsmithing";
        internal const string InternalName = "KMG_Gunsmithing_Feature";
        internal const string MaintenanceGrantName = "$KMG_GrantGunsmithingMaintenance";

        internal static BlueprintFeature Register(BlueprintRegistry registry,
            BlueprintAbility repairAbility,
            BlueprintAbility craftingAbility, BlueprintAbility paperCraftingAbility)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (repairAbility == null) throw new ArgumentNullException("repairAbility");
            if (craftingAbility == null) throw new ArgumentNullException("craftingAbility");
            if (paperCraftingAbility == null) throw new ArgumentNullException("paperCraftingAbility");
            BlueprintFeature feature = registry.Register<BlueprintFeature>(Symbol,
                () => Create(repairAbility, craftingAbility, paperCraftingAbility));
            Validate(feature, repairAbility, craftingAbility, paperCraftingAbility);
            return feature;
        }

        internal static void Validate(BlueprintFeature feature,
            BlueprintAbility repairAbility,
            BlueprintAbility craftingAbility, BlueprintAbility paperCraftingAbility)
        {
            if (feature == null) throw new ArgumentNullException("feature");
            if (!string.Equals(feature.name, InternalName, StringComparison.Ordinal) ||
                feature.Ranks != 1 || !feature.IsClassFeature || feature.HideInUI)
                throw new InvalidOperationException("Gunsmithing feature identity or visibility is invalid.");
            AddFacts[] grants = feature.ComponentsArray.OfType<AddFacts>().ToArray();
            if (feature.ComponentsArray.Length != 1 || grants.Length != 1 ||
                !string.Equals(grants[0].name, MaintenanceGrantName, StringComparison.Ordinal) ||
                grants[0].DoNotRestoreMissingFacts || grants[0].Facts == null ||
                grants[0].Facts.Length != 3 ||
                !ReferenceEquals(grants[0].Facts[0], repairAbility) ||
                !ReferenceEquals(grants[0].Facts[1], craftingAbility) ||
                !ReferenceEquals(grants[0].Facts[2], paperCraftingAbility))
                throw new InvalidOperationException("Gunsmithing maintenance grant is invalid.");
        }

        private static BlueprintFeature Create(BlueprintAbility repairAbility,
            BlueprintAbility craftingAbility,
            BlueprintAbility paperCraftingAbility)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = InternalName;
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.HideInUI = false;
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.name = MaintenanceGrantName;
            grant.Facts = new BlueprintUnitFact[]
                { repairAbility, craftingAbility, paperCraftingAbility };
            grant.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] { grant };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Gunslinger.Gunsmithing.Name", "Gunsmithing"),
                LocalizationService.Create("KMG.Gunslinger.Gunsmithing.Description",
                    "You can repair a Broken firearm to Normal with one full-round Repair Firearm action outside combat using a reusable Gunsmith's Kit; nothing is consumed and surviving loaded ammunition is preserved. Damaged carried firearms (including Wrecked ones) are also restored to Normal automatically after a completed full rest. Once per rest, the same kit lets you choose either 22 gp for 20 Black Powder Charges plus 20 Lead Balls, or 24 gp for 20 Paper Cartridges. The recipes share one entitlement. Gunslingers gain this feature automatically at 1st level."), null);
            return feature;
        }
    }
}
