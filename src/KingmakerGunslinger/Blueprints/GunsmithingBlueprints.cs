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
            BlueprintAbility overhaulAbility, BlueprintAbility repairAbility,
            BlueprintAbility craftingAbility)
        {
            if (registry == null) throw new ArgumentNullException("registry");
            if (overhaulAbility == null) throw new ArgumentNullException("overhaulAbility");
            if (repairAbility == null) throw new ArgumentNullException("repairAbility");
            if (craftingAbility == null) throw new ArgumentNullException("craftingAbility");
            if (ReferenceEquals(overhaulAbility, repairAbility))
                throw new ArgumentException("Gunsmithing requires distinct maintenance abilities.");
            BlueprintFeature feature = registry.Register<BlueprintFeature>(Symbol,
                () => Create(overhaulAbility, repairAbility, craftingAbility));
            Validate(feature, overhaulAbility, repairAbility, craftingAbility);
            return feature;
        }

        internal static void Validate(BlueprintFeature feature,
            BlueprintAbility overhaulAbility, BlueprintAbility repairAbility,
            BlueprintAbility craftingAbility)
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
                !ReferenceEquals(grants[0].Facts[0], overhaulAbility) ||
                !ReferenceEquals(grants[0].Facts[1], repairAbility) ||
                !ReferenceEquals(grants[0].Facts[2], craftingAbility))
                throw new InvalidOperationException("Gunsmithing maintenance grant is invalid.");
        }

        private static BlueprintFeature Create(BlueprintAbility overhaulAbility,
            BlueprintAbility repairAbility, BlueprintAbility craftingAbility)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = InternalName;
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.HideInUI = false;
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.name = MaintenanceGrantName;
            grant.Facts = new BlueprintUnitFact[]
                { overhaulAbility, repairAbility, craftingAbility };
            grant.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] { grant };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Gunslinger.Gunsmithing.Name", "Gunsmithing"),
                LocalizationService.Create("KMG.Gunslinger.Gunsmithing.Description",
                    "You can repair Broken firearms with a Firearm Repair Kit and overhaul Wrecked firearms with a Firearm Overhaul Kit. Once per rest, a non-consumable Gunsmith's Kit lets you spend one uninterrupted minute and 22 gp to craft 20 Black Powder Charges and 20 Lead Balls. Gunslingers gain this feature automatically at 1st level."), null);
            return feature;
        }
    }
}
