using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Archetypes;
using KingmakerGunslinger.Reloading;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class MusketMasterBlueprintSet
    {
        internal MusketMasterBlueprintSet(BlueprintArchetype archetype,
            BlueprintFeature steadyAim, BlueprintFeature fastMusket,
            BlueprintFeature training, BlueprintAbility steadyAimAbility,
            BlueprintBuff steadyAimArmed)
        {
            Archetype = archetype;
            SteadyAim = steadyAim;
            FastMusket = fastMusket;
            Training = training;
            SteadyAimAbility = steadyAimAbility;
            SteadyAimArmed = steadyAimArmed;
        }
        internal BlueprintArchetype Archetype { get; private set; }
        internal BlueprintFeature SteadyAim { get; private set; }
        internal BlueprintFeature FastMusket { get; private set; }
        internal BlueprintFeature Training { get; private set; }
        internal BlueprintAbility SteadyAimAbility { get; private set; }
        internal BlueprintBuff SteadyAimArmed { get; private set; }
        internal int Count { get { return 5; } }
    }

    internal static class MusketMasterBlueprints
    {
        internal const string ArchetypeSymbol = "KMG.Archetypes.MusketMaster";
        internal const string SteadyAimSymbol = "KMG.Archetypes.SteadyAim";
        internal const string SteadyAimAbilitySymbol =
            "KMG.Archetypes.SteadyAimAbility";
        internal const string SteadyAimArmedSymbol =
            "KMG.Archetypes.SteadyAimArmed";
        internal const string FastMusketSymbol = "KMG.Archetypes.FastMusket";

        internal static MusketMasterBlueprintSet Register(BlueprintRegistry registry,
            GunslingerClassBlueprintSet gunslinger,
            FirearmTrainingBlueprintSet training, BlueprintFeature rapidMusket,
            BlueprintAbilityResource grit,
            params BlueprintItem[] startingItems)
        {
            BlueprintBuff steadyArmed = registry.Register<BlueprintBuff>(
                SteadyAimArmedSymbol, () => CreateSteadyArmed(grit));
            BlueprintAbility steadyAbility = registry.Register<BlueprintAbility>(
                SteadyAimAbilitySymbol, () => CreateSteadyAbility(steadyArmed, grit));
            BlueprintFeature steady = registry.Register<BlueprintFeature>(
                SteadyAimSymbol, () => CreateSteadyFeature(steadyAbility));
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
                training.Musket, steadyAbility, steadyArmed);
        }

        private static BlueprintBuff CreateSteadyArmed(
            BlueprintAbilityResource grit)
        {
            var result = ScriptableObject.CreateInstance<BlueprintBuff>();
            result.name = "KMG_SteadyAim_Armed";
            result.Stacking = StackingType.Replace;
            var handler = ScriptableObject.CreateInstance<SteadyAimAttackHandler>();
            handler.name = "$KMG_SteadyAim_Attack";
            handler.Grit = grit;
            result.ComponentsArray = new BlueprintComponent[] { handler };
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.SteadyAim.Armed.Name",
                    "Steady Aim Armed"),
                LocalizationService.Create("KMG.SteadyAim.Armed.Description",
                    "Your next two-handed direct firearm shot this turn gains 10 feet to its effective range increment."), null);
            return result;
        }

        private static BlueprintAbility CreateSteadyAbility(BlueprintBuff armed,
            BlueprintAbilityResource grit)
        {
            var result = ScriptableObject.CreateInstance<BlueprintAbility>();
            result.name = "KMG_SteadyAim_Ability";
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.SteadyAim.Ability.Name",
                    "Steady Aim"),
                LocalizationService.Create("KMG.SteadyAim.Ability.Description",
                    "As a move action while you have positive grit, arm your next two-handed direct firearm shot this turn. That shot treats its range increment as 10 feet longer. Scatter cones do not qualify. This spends no grit."), null);
            result.Type = AbilityType.Extraordinary;
            result.Range = AbilityRange.Personal;
            result.CanTargetSelf = true;
            result.CanTargetPoint = result.CanTargetEnemies =
                result.CanTargetFriends = false;
            result.SpellResistance = false;
            result.Hidden = false;
            result.ActionBarAutoFillIgnored = false;
            result.NeedEquipWeapons = false;
            result.EffectOnAlly = AbilityEffectOnUnit.Helpful;
            result.EffectOnEnemy = AbilityEffectOnUnit.None;
            result.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            result.ActionType = UnitCommand.CommandType.Move;
            result.ResourceAssetIds = Array.Empty<string>();
            result.LocalizedDuration = LocalizationService.Create(
                "KMG.SteadyAim.Ability.Duration", "Current turn or until used");
            result.LocalizedSavingThrow = LocalizationService.Create(
                "KMG.SteadyAim.Ability.SavingThrow", "None");
            var logic = ScriptableObject.CreateInstance<SteadyAimAbilityLogic>();
            logic.name = "$KMG_SteadyAim_Arm";
            logic.ArmedMarker = armed;
            logic.Grit = grit;
            result.ComponentsArray = new BlueprintComponent[] { logic };
            return result;
        }

        private static BlueprintFeature CreateSteadyFeature(
            BlueprintAbility ability)
        {
            BlueprintFeature result = CreatePassive("Steady Aim",
                "As a move action while you have positive grit, arm your next two-handed direct firearm shot this turn to increase its range increment by 10 feet. This spends no grit.");
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.name = "$KMG_SteadyAim_Grant";
            add.Facts = new BlueprintUnitFact[] { ability };
            add.DoNotRestoreMissingFacts = false;
            result.ComponentsArray = new BlueprintComponent[] { add };
            return result;
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
