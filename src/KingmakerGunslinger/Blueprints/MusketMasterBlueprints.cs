using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats;
using KingmakerGunslinger.Reloading;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class MusketMasterBlueprintSet
    {
        internal MusketMasterBlueprintSet(BlueprintArchetype archetype,
            BlueprintFeature steadyAim, BlueprintFeature fastMusket,
            BlueprintFeature training)
        {
            Archetype = archetype;
            SteadyAim = steadyAim;
            FastMusket = fastMusket;
            Training = training;
        }
        internal BlueprintArchetype Archetype { get; private set; }
        internal BlueprintFeature SteadyAim { get; private set; }
        internal BlueprintFeature FastMusket { get; private set; }
        internal BlueprintFeature Training { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class MusketMasterBlueprints
    {
        internal const string ArchetypeSymbol = "KMG.Archetypes.MusketMaster";
        internal const string SteadyAimSymbol = "KMG.Archetypes.SteadyAim";
        internal const string FastMusketSymbol = "KMG.Archetypes.FastMusket";

        internal static MusketMasterBlueprintSet Register(BlueprintRegistry registry,
            GunslingerClassBlueprintSet gunslinger,
            FirearmTrainingBlueprintSet training, BlueprintFeature rapidMusket,
            params BlueprintItem[] startingItems)
        {
            BlueprintFeature steady = registry.Register<BlueprintFeature>(
                SteadyAimSymbol, () => CreatePassive("Steady Aim",
                    "As a move action while this deed is available, arm your next two-handed direct firearm shot this turn to increase its range increment by 10 feet. This spends no grit."));
            BlueprintFeature fast = registry.Register<BlueprintFeature>(
                FastMusketSymbol, () => CreatePassive("Fast Musket",
                    "While this deed is available, reload two-handed firearms as if they were one-handed firearms."));
            BlueprintArchetype archetype = registry.Register<BlueprintArchetype>(
                ArchetypeSymbol, () => CreateArchetype(gunslinger, training,
                    rapidMusket, steady, fast, startingItems));
            if (!gunslinger.CharacterClass.Archetypes.Any(value =>
                    ReferenceEquals(value, archetype)))
                gunslinger.CharacterClass.Archetypes = gunslinger.CharacterClass
                    .Archetypes.Concat(new[] { archetype }).ToArray();
            FastMusketRuntime.Configure(fast, null);
            return new MusketMasterBlueprintSet(archetype, steady, fast,
                training.Musket);
        }

        private static BlueprintArchetype CreateArchetype(
            GunslingerClassBlueprintSet g, FirearmTrainingBlueprintSet training,
            BlueprintFeature rapidMusket, BlueprintFeature steady,
            BlueprintFeature fast, BlueprintItem[] startingItems)
        {
            if (startingItems == null || startingItems.Length != 4 ||
                startingItems.Any(value => value == null) ||
                startingItems.Distinct().Count() != 4)
                throw new InvalidOperationException(
                    "Musket Master requires four exact distinct starting items.");
            var archetype = ScriptableObject.CreateInstance<BlueprintArchetype>();
            archetype.name = "KMG_MusketMaster_Archetype";
            archetype.LocalizedName = LocalizationService.Create(
                "KMG.MusketMaster.Name", "Musket Master");
            archetype.LocalizedDescription = LocalizationService.Create(
                "KMG.MusketMaster.Description",
                "A two-handed firearm specialist who masters long arms and rapid loading.");
            archetype.OverrideAttributeRecommendations = true;
            archetype.RecommendedAttributes = new[] { StatType.Dexterity,
                StatType.Wisdom };
            archetype.NotRecommendedAttributes = Array.Empty<StatType>();
            typeof(BlueprintArchetype).GetField("m_ParentClass",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
                    archetype, g.CharacterClass);
            archetype.ReplaceStartingEquipment = true;
            archetype.StartingGold = g.CharacterClass.StartingGold;
            archetype.StartingItems = (BlueprintItem[])startingItems.Clone();
            archetype.RemoveFeatures = new[] {
                Entry(1, g.Proficiencies, g.Dodge.Feature),
                Entry(3, g.UtilityShot.Feature),
                Entry(5, g.GunTraining.Selection),
                Entry(9, g.GunTraining.Selection),
                Entry(13, g.GunTraining.Selection),
                Entry(17, g.GunTraining.Selection) };
            archetype.AddFeatures = new[] {
                Entry(1, g.ArchetypeProficiencies.MusketMaster, steady,
                    rapidMusket),
                Entry(3, fast),
                Entry(5, training.Musket), Entry(9, training.Musket),
                Entry(13, training.Musket), Entry(17, training.Musket) };
            archetype.ComponentsArray = Array.Empty<BlueprintComponent>();
            return archetype;
        }

        private static BlueprintFeature CreatePassive(string name,
            string description)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_" + name.Replace(" ", "_");
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Archetypes." +
                    name.Replace(" ", "") + ".Name", name),
                LocalizationService.Create("KMG.Archetypes." +
                    name.Replace(" ", "") + ".Description", description), null);
            return feature;
        }

        private static LevelEntry Entry(int level,
            params BlueprintFeatureBase[] features)
        { return new LevelEntry { Level = level, Features = features.ToList() }; }
    }
}
