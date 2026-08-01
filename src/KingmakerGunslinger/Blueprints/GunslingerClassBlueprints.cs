using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class GunslingerClassBlueprintSet
    {
        internal GunslingerClassBlueprintSet(BlueprintCharacterClass characterClass,
            BlueprintProgression progression, BlueprintFeature proficiencies)
        {
            CharacterClass = characterClass ?? throw new ArgumentNullException("characterClass");
            Progression = progression ?? throw new ArgumentNullException("progression");
            Proficiencies = proficiencies ?? throw new ArgumentNullException("proficiencies");
        }
        internal BlueprintCharacterClass CharacterClass { get; private set; }
        internal BlueprintProgression Progression { get; private set; }
        internal BlueprintFeature Proficiencies { get; private set; }
        internal int Count { get { return 3; } }
    }

    internal static class GunslingerClassBlueprints
    {
        internal const string ClassSymbol = "KMG.Classes.GunslingerClass";
        internal const string ProgressionSymbol = "KMG.Classes.GunslingerProgression";
        internal const string ProficienciesSymbol = "KMG.Classes.GunslingerProficiencies";

        private const string FighterClassGuid = "48ac8db94d5de7645906c7d0ad3bcfbd";
        private const string FullBaseAttackGuid = "b3057560ffff3514299e8b93e7648a9d";
        private const string GoodSaveGuid = "ff4662bde9e75f145853417313842751";
        private const string PoorSaveGuid = "dc0c7c1aba755c54f96c089cdf7d14a3";
        private const string SimpleWeaponGuid = "e70ecf1ed95ca2f40b754f1adb22bbdd";
        private const string MartialWeaponGuid = "203992ef5b35c864390b4e4a1e200629";
        private const string LightArmorGuid = "6d3728d4e9c9898458fe5e9532951132";

        internal static GunslingerClassBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry,
            BlueprintFeature firearmProficiency)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            if (firearmProficiency == null) throw new ArgumentNullException("firearmProficiency");

            BlueprintCharacterClass fighter = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                library, FighterClassGuid, "Gunslinger presentation source Fighter class");
            BlueprintStatProgression fullBab = BlueprintLibraryLookup.RequireExact<BlueprintStatProgression>(
                library, FullBaseAttackGuid, "full base attack progression");
            BlueprintStatProgression goodSave = BlueprintLibraryLookup.RequireExact<BlueprintStatProgression>(
                library, GoodSaveGuid, "good save progression");
            BlueprintStatProgression poorSave = BlueprintLibraryLookup.RequireExact<BlueprintStatProgression>(
                library, PoorSaveGuid, "poor save progression");
            BlueprintFeature simple = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, SimpleWeaponGuid, "simple weapon proficiency");
            BlueprintFeature martial = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, MartialWeaponGuid, "martial weapon proficiency");
            BlueprintFeature lightArmor = BlueprintLibraryLookup.RequireExact<BlueprintFeature>(
                library, LightArmorGuid, "light armor proficiency");

            BlueprintFeature proficiencies = registry.Register<BlueprintFeature>(
                ProficienciesSymbol, () => CreateProficiencies(simple, martial,
                    lightArmor, firearmProficiency));
            BlueprintCharacterClass characterClass = registry.Register<BlueprintCharacterClass>(
                ClassSymbol, () => CreateClass(fighter, fullBab, goodSave, poorSave));
            BlueprintProgression progression = registry.Register<BlueprintProgression>(
                ProgressionSymbol, () => CreateProgression());

            characterClass.Progression = progression;
            progression.Classes = new[] { characterClass };
            progression.LevelEntries = CreateLevelEntries(proficiencies);
            Validate(characterClass, progression, proficiencies, fullBab, goodSave,
                poorSave, simple, martial, lightArmor, firearmProficiency);
            return new GunslingerClassBlueprintSet(characterClass, progression, proficiencies);
        }

        private static BlueprintFeature CreateProficiencies(params BlueprintUnitFact[] facts)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "KMG_Gunslinger_Proficiencies";
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.HideInUI = false;
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.name = "$KMG_Gunslinger_ProficiencyFacts";
            grant.Facts = facts;
            grant.DoNotRestoreMissingFacts = false;
            feature.ComponentsArray = new BlueprintComponent[] { grant };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.Gunslinger.Proficiencies.Name", "Gunslinger Proficiencies"),
                LocalizationService.Create("KMG.Gunslinger.Proficiencies.Description",
                    "Proficient with simple and martial weapons, firearms, and light armor."), null);
            return feature;
        }

        private static BlueprintCharacterClass CreateClass(BlueprintCharacterClass fighter,
            BlueprintStatProgression fullBab, BlueprintStatProgression goodSave,
            BlueprintStatProgression poorSave)
        {
            var result = ScriptableObject.CreateInstance<BlueprintCharacterClass>();
            result.name = "KMG_Gunslinger_Class";
            result.LocalizedName = LocalizationService.Create("KMG.Gunslinger.Class.Name", "Gunslinger");
            result.LocalizedDescription = LocalizationService.Create("KMG.Gunslinger.Class.Description",
                "A master of firearms who fights with grit, precision, and daring deeds.");
            result.m_Icon = fighter.Icon;
            result.HitDie = DiceType.D10;
            result.BaseAttackBonus = fullBab;
            result.FortitudeSave = goodSave;
            result.ReflexSave = goodSave;
            result.WillSave = poorSave;
            result.SkillPoints = 4;
            result.ClassSkills = new[] { StatType.SkillMobility, StatType.SkillAthletics,
                StatType.SkillPersuasion, StatType.SkillThievery,
                StatType.SkillKnowledgeWorld, StatType.SkillLoreNature,
                StatType.SkillLoreReligion, StatType.SkillPerception };
            result.StartingGold = fighter.StartingGold;
            result.StartingItems = Array.Empty<Kingmaker.Blueprints.Items.BlueprintItem>();
            result.Archetypes = Array.Empty<BlueprintArchetype>();
            result.RecommendedAttributes = new[] { StatType.Dexterity, StatType.Wisdom };
            result.NotRecommendedAttributes = Array.Empty<StatType>();
            result.MaleEquipmentEntities = fighter.MaleEquipmentEntities;
            result.FemaleEquipmentEntities = fighter.FemaleEquipmentEntities;
            result.EquipmentEntities = fighter.EquipmentEntities;
            result.PrimaryColor = fighter.PrimaryColor;
            result.SecondaryColor = fighter.SecondaryColor;
            return result;
        }

        private static BlueprintProgression CreateProgression()
        {
            var result = ScriptableObject.CreateInstance<BlueprintProgression>();
            result.name = "KMG_Gunslinger_Progression";
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            result.UIDeterminatorsGroup = Array.Empty<BlueprintFeatureBase>();
            result.UIGroups = Array.Empty<UIGroup>();
            result.Archetypes = Array.Empty<BlueprintArchetype>();
            BlueprintUnitFactAccess.Resolve().Configure(result,
                LocalizationService.Create("KMG.Gunslinger.Progression.Name", "Gunslinger"),
                LocalizationService.Create("KMG.Gunslinger.Progression.Description",
                    "Gunslinger class progression."), null);
            return result;
        }

        private static LevelEntry[] CreateLevelEntries(BlueprintFeature proficiencies)
        {
            var entries = new LevelEntry[20];
            for (int level = 1; level <= 20; level++)
                entries[level - 1] = new LevelEntry { Level = level,
                    Features = level == 1 ? new List<BlueprintFeatureBase> { proficiencies } :
                        new List<BlueprintFeatureBase>() };
            return entries;
        }

        private static void Validate(BlueprintCharacterClass characterClass,
            BlueprintProgression progression, BlueprintFeature proficiencies,
            BlueprintStatProgression fullBab, BlueprintStatProgression goodSave,
            BlueprintStatProgression poorSave, params BlueprintUnitFact[] facts)
        {
            if (characterClass.HitDie != DiceType.D10 || characterClass.SkillPoints != 4 ||
                !ReferenceEquals(characterClass.BaseAttackBonus, fullBab) ||
                !ReferenceEquals(characterClass.FortitudeSave, goodSave) ||
                !ReferenceEquals(characterClass.ReflexSave, goodSave) ||
                !ReferenceEquals(characterClass.WillSave, poorSave) ||
                !ReferenceEquals(characterClass.Progression, progression) ||
                progression.Classes.Length != 1 ||
                !ReferenceEquals(progression.Classes[0], characterClass) ||
                progression.LevelEntries.Length != 20)
                throw new InvalidOperationException("Gunslinger class chassis references are incomplete.");
            AddFacts grant = (AddFacts)proficiencies.ComponentsArray[0];
            if (grant.Facts.Length != facts.Length)
                throw new InvalidOperationException("Gunslinger proficiency grant count changed.");
            for (int index = 0; index < facts.Length; index++)
                if (!ReferenceEquals(grant.Facts[index], facts[index]))
                    throw new InvalidOperationException("Gunslinger proficiency identity changed.");
            for (int index = 0; index < progression.LevelEntries.Length; index++)
                if (progression.LevelEntries[index].Level != index + 1)
                    throw new InvalidOperationException("Gunslinger progression level order changed.");
        }
    }
}
