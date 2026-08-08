using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class ArchetypeProficiencyBlueprintSet
    {
        internal ArchetypeProficiencyBlueprintSet(BlueprintFeature pistolero,
            BlueprintFeature musketMaster)
        { Pistolero = pistolero; MusketMaster = musketMaster; }
        internal BlueprintFeature Pistolero { get; private set; }
        internal BlueprintFeature MusketMaster { get; private set; }
        internal int Count { get { return 2; } }
    }

    internal static class ArchetypeProficiencyBlueprints
    {
        internal const string PistoleroSymbol =
            "KMG.Archetypes.PistoleroProficiencies";
        internal const string MusketMasterSymbol =
            "KMG.Archetypes.MusketMasterProficiencies";

        internal static ArchetypeProficiencyBlueprintSet Register(
            BlueprintRegistry registry, BlueprintFeature simple,
            BlueprintFeature martial, BlueprintFeature lightArmor,
            FirearmScopedProficiencyBlueprintSet scoped)
        {
            BlueprintFeature pistolero = registry.Register<BlueprintFeature>(
                PistoleroSymbol, () => Create("Pistolero", simple, martial,
                    lightArmor, scoped.OneHanded));
            BlueprintFeature musket = registry.Register<BlueprintFeature>(
                MusketMasterSymbol, () => Create("Musket Master", simple,
                    martial, lightArmor, scoped.TwoHanded));
            return new ArchetypeProficiencyBlueprintSet(pistolero, musket);
        }

        private static BlueprintFeature Create(string name,
            params BlueprintUnitFact[] facts)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_" + name.Replace(" ", "_") + "_Proficiencies";
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.Facts = facts;
            grant.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] { grant };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Archetypes." +
                    name.Replace(" ", "") + ".Proficiencies.Name",
                    name + " Proficiencies"),
                LocalizationService.Create("KMG.Archetypes." +
                    name.Replace(" ", "") + ".Proficiencies.Description",
                    "Proficient with simple and martial weapons, light armor, " +
                    (name == "Pistolero" ? "and one-handed firearms."
                        : "and two-handed firearms.")), null);
            return feature;
        }
    }
}
