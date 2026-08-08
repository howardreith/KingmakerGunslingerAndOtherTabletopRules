using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class PistoleroBlueprintSet
    {
        internal PistoleroBlueprintSet(BlueprintArchetype archetype,
            BlueprintFeature upCloseAndDeadly, BlueprintFeature twinShot,
            BlueprintFeature[] deedTiers, BlueprintFeature training)
        {
            Archetype = archetype;
            UpCloseAndDeadly = upCloseAndDeadly;
            TwinShotKnockdown = twinShot;
            DeedTiers = deedTiers;
            Training = training;
        }
        internal BlueprintArchetype Archetype { get; private set; }
        internal BlueprintFeature UpCloseAndDeadly { get; private set; }
        internal BlueprintFeature TwinShotKnockdown { get; private set; }
        internal BlueprintFeature[] DeedTiers { get; private set; }
        internal BlueprintFeature Training { get; private set; }
        internal int Count { get { return 6; } }
    }

    internal static class PistoleroBlueprints
    {
        internal const string ArchetypeSymbol = "KMG.Archetypes.Pistolero";
        internal const string UpCloseSymbol =
            "KMG.Archetypes.UpCloseAndDeadly";
        internal const string TwinShotSymbol =
            "KMG.Archetypes.TwinShotKnockdown";
        private static readonly string[] TierSymbols = {
            "KMG.Archetypes.PistoleroDeedsLevel1",
            "KMG.Archetypes.PistoleroDeedsLevel7",
            "KMG.Archetypes.PistoleroDeedsLevel11" };

        internal static PistoleroBlueprintSet Register(BlueprintRegistry registry,
            GunslingerClassBlueprintSet gunslinger,
            FirearmTrainingBlueprintSet training)
        {
            BlueprintFeature upClose = registry.Register<BlueprintFeature>(
                UpCloseSymbol, () => CreateFeature("Up Close and Deadly",
                    "Arm your next one-handed, non-scatter firearm attack this turn. After the result is known, spend exactly 1 grit to deal scaling precision damage on a hit or half of the same roll on a miss. This cost cannot be reduced by True Grit."));
            BlueprintFeature twin = registry.Register<BlueprintFeature>(
                TwinShotSymbol, () => CreateFeature("Twin Shot Knockdown",
                    "After two distinct one-handed firearm hits against the same target during your turn, you may spend 1 grit to knock that target prone."));
            BlueprintFeature[] tiers = {
                RegisterTier(registry, 0,
                    "Up Close and Deadly, Gunslinger's Dodge, and Quick Clear."),
                RegisterTier(registry, 1,
                    "Deadeye, Dead Shot, and Targeting deeds."),
                RegisterTier(registry, 2,
                    "Twin Shot Knockdown, Expert Loading, and Lightning Reload.") };
            BlueprintArchetype archetype = registry.Register<BlueprintArchetype>(
                ArchetypeSymbol, () => CreateArchetype(gunslinger, training,
                    upClose, twin, tiers));
            if (!gunslinger.CharacterClass.Archetypes.Any(value =>
                    ReferenceEquals(value, archetype)))
                gunslinger.CharacterClass.Archetypes = gunslinger.CharacterClass
                    .Archetypes.Concat(new[] { archetype }).ToArray();
            return new PistoleroBlueprintSet(archetype, upClose, twin, tiers,
                training.Pistol);
        }

        private static BlueprintArchetype CreateArchetype(
            GunslingerClassBlueprintSet g, FirearmTrainingBlueprintSet training,
            BlueprintFeature upClose, BlueprintFeature twin,
            BlueprintFeature[] tiers)
        {
            var result = ScriptableObject.CreateInstance<BlueprintArchetype>();
            result.name = "KMG_Pistolero_Archetype";
            result.LocalizedName = LocalizationService.Create(
                "KMG.Pistolero.Name", "Pistolero");
            result.LocalizedDescription = LocalizationService.Create(
                "KMG.Pistolero.Description",
                "A one-handed firearm specialist who fights at close range with speed and precision.");
            result.OverrideAttributeRecommendations = true;
            result.RecommendedAttributes = new[] { StatType.Dexterity,
                StatType.Wisdom };
            result.NotRecommendedAttributes = Array.Empty<StatType>();
            typeof(BlueprintArchetype).GetField("m_ParentClass",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
                    result, g.CharacterClass);
            result.ReplaceStartingEquipment = false;
            result.RemoveFeatures = new[] {
                Entry(1, g.Proficiencies, g.Deadeye.Feature, g.DeedTiers[0]),
                Entry(5, g.GunTraining.Selection),
                Entry(7, g.StartlingShot.Feature, g.DeedTiers[1]),
                Entry(9, g.GunTraining.Selection),
                Entry(11, g.BleedingWound.Feature, g.DeedTiers[2]),
                Entry(13, g.GunTraining.Selection),
                Entry(17, g.GunTraining.Selection) };
            result.AddFeatures = new[] {
                Entry(1, g.ArchetypeProficiencies.Pistolero, upClose, tiers[0]),
                Entry(5, training.Pistol),
                Entry(7, g.Deadeye.Feature, tiers[1]),
                Entry(9, training.Pistol),
                Entry(11, twin, tiers[2]),
                Entry(13, training.Pistol),
                Entry(17, training.Pistol) };
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            return result;
        }

        private static BlueprintFeature RegisterTier(BlueprintRegistry registry,
            int index, string description)
        {
            return registry.Register<BlueprintFeature>(TierSymbols[index], () =>
                CreateFeature("Pistolero Deeds — Level " +
                    new[] { 1, 7, 11 }[index], description));
        }

        private static BlueprintFeature CreateFeature(string name,
            string description)
        {
            var result = ScriptableObject.CreateInstance<BlueprintFeature>();
            result.name = "KMG_" + name.Replace(" ", "_");
            result.Ranks = 1;
            result.IsClassFeature = true;
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Pistolero." +
                    name.Replace(" ", "") + ".Name", name),
                LocalizationService.Create("KMG.Pistolero." +
                    name.Replace(" ", "") + ".Description", description), null);
            return result;
        }

        private static LevelEntry Entry(int level,
            params BlueprintFeatureBase[] features)
        { return new LevelEntry { Level = level, Features = features.ToList() }; }
    }
}
