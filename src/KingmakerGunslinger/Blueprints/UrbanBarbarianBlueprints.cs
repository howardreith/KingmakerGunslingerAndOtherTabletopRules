using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.UrbanBarbarian;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal sealed class UrbanBarbarianBlueprintSet
    {
        internal UrbanBarbarianBlueprintSet(BlueprintCharacterClass barbarian,
            BlueprintArchetype archetype, BlueprintFeature proficiency,
            BlueprintFeature crowdControl, BlueprintFeature controlledRage,
            BlueprintFeature greaterDefault, BlueprintFeature mightyDefault,
            BlueprintBuff rageBuff, BlueprintAbility selector,
            BlueprintFeature[] selectionFacts, BlueprintAbility[] allocations,
            BlueprintFeature nativeGreaterRage, BlueprintFeature nativeTirelessRage,
            BlueprintFeature nativeMightyRage, BlueprintBuff nativeRageBuff)
        {
            BarbarianClass = barbarian; Archetype = archetype;
            Proficiency = proficiency; CrowdControl = crowdControl;
            ControlledRage = controlledRage; GreaterDefault = greaterDefault;
            MightyDefault = mightyDefault; RageBuff = rageBuff;
            Selector = selector; SelectionFacts = selectionFacts;
            AllocationAbilities = allocations; NativeGreaterRage = nativeGreaterRage;
            NativeTirelessRage = nativeTirelessRage;
            NativeMightyRage = nativeMightyRage; NativeRageBuff = nativeRageBuff;
        }

        internal BlueprintCharacterClass BarbarianClass { get; private set; }
        internal BlueprintArchetype Archetype { get; private set; }
        internal BlueprintFeature Proficiency { get; private set; }
        internal BlueprintFeature CrowdControl { get; private set; }
        internal BlueprintFeature ControlledRage { get; private set; }
        internal BlueprintFeature GreaterDefault { get; private set; }
        internal BlueprintFeature MightyDefault { get; private set; }
        internal BlueprintBuff RageBuff { get; private set; }
        internal BlueprintAbility Selector { get; private set; }
        internal BlueprintFeature[] SelectionFacts { get; private set; }
        internal BlueprintAbility[] AllocationAbilities { get; private set; }
        internal BlueprintFeature NativeGreaterRage { get; private set; }
        internal BlueprintFeature NativeTirelessRage { get; private set; }
        internal BlueprintFeature NativeMightyRage { get; private set; }
        internal BlueprintBuff NativeRageBuff { get; private set; }
        internal int Count { get { return UrbanBarbarianIdentityCatalog.IdentityCount; } }
    }

    internal static class UrbanBarbarianBlueprints
    {
        internal const string BarbarianClassGuid =
            "f7d7eb166b3dd594fb330d085df41853";
        internal const string NativeProficiencyGuid =
            "acc15a2d19f13864e8cce3ba133a1979";
        internal const string FastMovementGuid =
            "d294a5dddd0120046aae7d4eb6cbc4fc";
        internal const string RageFeatureGuid =
            "2479395977cfeeb46b482bc3385f4647";
        internal const string RageBuffGuid =
            "da8ce41ac3cd74742b80984ccc3c9613";
        internal const string GreaterRageGuid =
            "ce49c579fe0bcc647a32c96929fae982";
        internal const string TirelessRageGuid =
            "ca9343d75a83a2745a22fa11c383153a";
        internal const string MightyRageGuid =
            "06a7e5b60020ad947aed107d82d1f897";
        internal const string MediumArmorProficiencyGuid =
            "46f4fb320f35704488ba3d513397789d";

        private static readonly string[] RetainedRageComponentTypes = {
            "Kingmaker.Designers.Mechanics.Buffs.BuffParticleEffectPlay",
            "Kingmaker.UnitLogic.Mechanics.Components.AddFactContextActions",
            "Kingmaker.UnitLogic.Mechanics.Components.ContextRankConfig",
            "Kingmaker.UnitLogic.Mechanics.Components.ContextCalculateSharedValue",
            "Kingmaker.UnitLogic.FactLogic.ForbidSpellCasting",
            "Kingmaker.Blueprints.Classes.Spells.SpellDescriptorComponent" };

        internal static UrbanBarbarianBlueprintSet Register(
            LibraryScriptableObject library, BlueprintRegistry registry)
        {
            if (library == null) throw new ArgumentNullException("library");
            if (registry == null) throw new ArgumentNullException("registry");
            BlueprintCharacterClass barbarian = Require<BlueprintCharacterClass>(
                library, BarbarianClassGuid, "native Barbarian class");
            BlueprintFeature nativeProficiency = Require<BlueprintFeature>(library,
                NativeProficiencyGuid, "native Barbarian proficiency");
            BlueprintFeature fastMovement = Require<BlueprintFeature>(library,
                FastMovementGuid, "native Fast Movement");
            BlueprintFeature rageFeature = Require<BlueprintFeature>(library,
                RageFeatureGuid, "native Rage feature");
            BlueprintBuff nativeRage = Require<BlueprintBuff>(library, RageBuffGuid,
                "finalized native Rage buff");
            BlueprintFeature greaterRage = Require<BlueprintFeature>(library,
                GreaterRageGuid, "native Greater Rage");
            BlueprintFeature tirelessRage = Require<BlueprintFeature>(library,
                TirelessRageGuid, "native Tireless Rage");
            BlueprintFeature mightyRage = Require<BlueprintFeature>(library,
                MightyRageGuid, "native Mighty Rage");

            BlueprintFeature proficiency = registry.Register<BlueprintFeature>(
                UrbanBarbarianIdentityCatalog.Proficiency,
                () => CreateProficiency(nativeProficiency));
            BlueprintFeature crowd = registry.Register<BlueprintFeature>(
                UrbanBarbarianIdentityCatalog.CrowdControl,
                () => CreateCrowdControl(fastMovement.Icon));

            var allocationValues = new List<ControlledRageAllocation>();
            foreach (ControlledRageTier tier in new[] {
                ControlledRageTier.Ordinary, ControlledRageTier.Greater,
                ControlledRageTier.Mighty })
                allocationValues.AddRange(
                    ControlledRageAllocationPolicy.Generate(tier));

            var facts = new Dictionary<ControlledRageAllocation, BlueprintFeature>();
            foreach (ControlledRageAllocation allocation in allocationValues)
                facts.Add(allocation, registry.Register<BlueprintFeature>(
                    UrbanBarbarianIdentityCatalog.SelectionFeature(allocation),
                    () => CreateSelectionFact(allocation, rageFeature.Icon)));

            var abilities = new Dictionary<BlueprintAbility,
                ControlledRageAllocation>();
            foreach (ControlledRageAllocation allocation in allocationValues)
            {
                ControlledRageAllocation captured = allocation;
                BlueprintFeature[] tierFacts = allocationValues.Where(value =>
                    value.Total == captured.Total).Select(value => facts[value])
                    .ToArray();
                BlueprintAbility ability = registry.Register<BlueprintAbility>(
                    UrbanBarbarianIdentityCatalog.SelectionAbility(captured),
                    () => CreateAllocationAbility(captured, facts[captured],
                        tierFacts, rageFeature.Icon));
                abilities.Add(ability, captured);
            }

            BlueprintAbility selector = registry.Register<BlueprintAbility>(
                UrbanBarbarianIdentityCatalog.Selector,
                () => CreateSelector(abilities.Keys.ToArray(), rageFeature.Icon));
            BlueprintFeature controlled = registry.Register<BlueprintFeature>(
                UrbanBarbarianIdentityCatalog.ControlledRage,
                () => CreateControlledRageFeature(selector, rageFeature.Icon));
            BlueprintFeature greaterDefault = registry.Register<BlueprintFeature>(
                UrbanBarbarianIdentityCatalog.GreaterDefault,
                () => CreateTierDefault("Greater Controlled Rage",
                    ControlledRageTier.Greater, rageFeature.Icon));
            BlueprintFeature mightyDefault = registry.Register<BlueprintFeature>(
                UrbanBarbarianIdentityCatalog.MightyDefault,
                () => CreateTierDefault("Mighty Controlled Rage",
                    ControlledRageTier.Mighty, rageFeature.Icon));
            BlueprintBuff urbanRage = registry.Register<BlueprintBuff>(
                UrbanBarbarianIdentityCatalog.RageBuff,
                () => CreateUrbanRageBuff(nativeRage));
            BlueprintArchetype archetype = registry.Register<BlueprintArchetype>(
                UrbanBarbarianIdentityCatalog.Archetype,
                () => CreateArchetype(barbarian, nativeProficiency,
                    fastMovement, proficiency, crowd, controlled, greaterDefault,
                    mightyDefault, rageFeature.Icon));

            ControlledRageRuntime.Configure(nativeRage, urbanRage, controlled,
                greaterRage, mightyRage, selector, abilities, facts);
            UrbanCotwCompatibilityRuntime.Reconcile(nativeRage, urbanRage);
            var set = new UrbanBarbarianBlueprintSet(barbarian, archetype,
                proficiency, crowd, controlled, greaterDefault, mightyDefault,
                urbanRage, selector, facts.Values.ToArray(), abilities.Keys.ToArray(),
                greaterRage, tirelessRage, mightyRage, nativeRage);
            Validate(set, nativeProficiency, fastMovement, rageFeature);
            return set;
        }

        private static BlueprintFeature CreateProficiency(
            BlueprintFeature nativeProficiency)
        {
            AddFacts native = (nativeProficiency.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).OfType<AddFacts>().Single();
            BlueprintUnitFact[] retained = (native.Facts ??
                Array.Empty<BlueprintUnitFact>()).Where(value => value != null &&
                    !string.Equals(value.AssetGuid, MediumArmorProficiencyGuid,
                        StringComparison.Ordinal)).ToArray();
            if (native.Facts == null || native.Facts.Length != 5 ||
                retained.Length != 4 || retained.Any(value => string.Equals(
                    value.AssetGuid, MediumArmorProficiencyGuid,
                    StringComparison.Ordinal)))
                throw new InvalidOperationException(
                    "Native Barbarian proficiency did not expose the exact five-fact contract.");
            var add = ScriptableObject.CreateInstance<AddFacts>();
            add.Facts = retained;
            return CreateFeature("KMG_UrbanBarbarian_Proficiency",
                "Urban Barbarian Proficiencies",
                "Urban barbarians are proficient with all simple and martial weapons, light armor, and shields except tower shields. They are not proficient with medium armor.",
                nativeProficiency.Icon, add);
        }

        private static BlueprintFeature CreateCrowdControl(Sprite icon)
        {
            return CreateFeature("KMG_UrbanBarbarian_CrowdControl",
                "Crowd Control",
                "While adjacent to at least two active hostile creatures, you gain a +1 untyped bonus on attack rolls and a +1 dodge bonus to Armor Class. Adjacency is measured edge to edge and is not increased by weapon reach.",
                icon, ScriptableObject.CreateInstance<CrowdControlComponent>());
        }

        private static BlueprintFeature CreateSelectionFact(
            ControlledRageAllocation allocation, Sprite icon)
        {
            BlueprintFeature feature = CreateFeature(
                "KMG_UrbanBarbarian_" + allocation.Symbol.Replace(".", "_"),
                "Controlled Rage Allocation: " + allocation.Name,
                allocation.Description, icon);
            feature.HideInUI = true;
            feature.HideInCharacterSheetAndLevelUp = true;
            return feature;
        }

        private static BlueprintAbility CreateAllocationAbility(
            ControlledRageAllocation allocation, BlueprintFeature selection,
            BlueprintFeature[] tierSelections, Sprite icon)
        {
            BlueprintAbility ability = CreatePersonalAbility(
                "KMG_UrbanBarbarian_Allocation_T" + allocation.Total + "_S" +
                    allocation.Strength + "_D" + allocation.Dexterity + "_C" +
                    allocation.Constitution,
                allocation.Name, allocation.Description +
                    " This free selection persists until changed and cannot be changed while raging.",
                icon);
            var logic = ScriptableObject.CreateInstance<ControlledRageAbilityLogic>();
            logic.Selection = selection;
            logic.TierSelections = tierSelections;
            logic.Tier = allocation.Total;
            ability.ComponentsArray = new BlueprintComponent[] { logic };
            return ability;
        }

        private static BlueprintAbility CreateSelector(BlueprintAbility[] variants,
            Sprite icon)
        {
            BlueprintAbility ability = CreatePersonalAbility(
                "KMG_UrbanBarbarian_ControlledRage_Selector",
                "Controlled Rage Allocation",
                "Choose how to allocate the current Controlled Rage morale bonus among Strength, Dexterity, and Constitution in +2 increments. Only allocations for your current Rage tier are shown. The selected allocation is marked and applies only while raging.",
                icon);
            var component = ScriptableObject.CreateInstance<AbilityVariants>();
            component.Variants = variants;
            ability.ComponentsArray = new BlueprintComponent[] { component };
            return ability;
        }

        private static BlueprintFeature CreateControlledRageFeature(
            BlueprintAbility selector, Sprite icon)
        {
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.Facts = new BlueprintUnitFact[] { selector };
            var selection = ScriptableObject.CreateInstance<
                ControlledRageSelectionController>();
            selection.Tier = (int)ControlledRageTier.Ordinary;
            return CreateFeature("KMG_UrbanBarbarian_ControlledRage",
                "Controlled Rage",
                "When raging, allocate a +4 morale bonus among Strength, Dexterity, and Constitution in +2 increments. The pool increases to +6 with Greater Rage and +8 with Mighty Rage. Controlled Rage grants no ordinary Rage attack bonus, damage bonus, temporary hit points, Will bonus, or AC penalty, and it does not prevent Intelligence-, Dexterity-, or Charisma-based skills. It retains the normal Rage resource, fatigue, spellcasting restriction, Rage powers, and Rage equivalence.",
                icon, grant, selection);
        }

        private static BlueprintFeature CreateTierDefault(string name,
            ControlledRageTier tier, Sprite icon)
        {
            var selection = ScriptableObject.CreateInstance<
                ControlledRageSelectionController>();
            selection.Tier = (int)tier;
            BlueprintFeature feature = CreateFeature(
                "KMG_UrbanBarbarian_" + name.Replace(" ", string.Empty), name,
                "Unlocks the +" + (int)tier +
                    " Controlled Rage allocation pool and initializes its independent selection to full Strength.",
                icon, selection);
            feature.HideInUI = true;
            feature.HideInCharacterSheetAndLevelUp = true;
            return feature;
        }

        private static BlueprintBuff CreateUrbanRageBuff(BlueprintBuff native)
        {
            BlueprintBuff buff = UnityEngine.Object.Instantiate(native);
            buff.name = "KMG_UrbanBarbarian_ControlledRage_Buff";
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create("KMG.UrbanBarbarian.RageBuff.Name",
                    "Controlled Rage"),
                LocalizationService.Create(
                    "KMG.UrbanBarbarian.RageBuff.Description",
                    "The selected current-tier morale bonuses to Strength, Dexterity, and Constitution are active. Normal Rage resource spending, fatigue, spellcasting restriction, and Rage-power integration remain in effect."),
                native.Icon);
            BlueprintComponent[] source = native.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            var retained = new List<BlueprintComponent>();
            foreach (string typeName in RetainedRageComponentTypes)
            {
                BlueprintComponent[] matches = source.Where(value => value != null &&
                    string.Equals(value.GetType().FullName, typeName,
                        StringComparison.Ordinal)).ToArray();
                if (matches.Length != 1)
                    throw new InvalidOperationException(
                        "Finalized native Rage lifecycle component was not singular: " +
                        typeName + ";count=" + matches.Length);
                retained.Add(UnityEngine.Object.Instantiate(matches[0]));
            }
            retained.Add(ScriptableObject.CreateInstance<
                ControlledRageAbilityScoreBonus>());
            buff.ComponentsArray = retained.ToArray();
            buff.IsClassFeature = true;
            return buff;
        }

        private static BlueprintArchetype CreateArchetype(
            BlueprintCharacterClass barbarian, BlueprintFeature nativeProficiency,
            BlueprintFeature fastMovement, BlueprintFeature proficiency,
            BlueprintFeature crowd, BlueprintFeature controlled,
            BlueprintFeature greaterDefault, BlueprintFeature mightyDefault,
            Sprite icon)
        {
            var archetype = ScriptableObject.CreateInstance<BlueprintArchetype>();
            archetype.name = "KMG_UrbanBarbarian_Archetype";
            archetype.LocalizedName = LocalizationService.Create(
                "KMG.UrbanBarbarian.Name", "Urban Barbarian");
            archetype.LocalizedDescription = LocalizationService.Create(
                "KMG.UrbanBarbarian.Description",
                "Urban barbarians thrive amid crowded streets and close-quarters conflict. They trade medium armor and wilderness training for urban skills, Crowd Control, and a precise Controlled Rage whose physical bonuses can be split among Strength, Dexterity, and Constitution.");
            SetParent(archetype, barbarian);
            SetIcon(archetype, icon);
            archetype.OverrideAttributeRecommendations = false;
            archetype.RecommendedAttributes = Array.Empty<StatType>();
            archetype.NotRecommendedAttributes = Array.Empty<StatType>();
            archetype.ReplaceClassSkills = true;
            archetype.ClassSkills = new[] { StatType.SkillAthletics,
                StatType.SkillMobility, StatType.SkillKnowledgeWorld,
                StatType.SkillPerception, StatType.SkillPersuasion };
            archetype.ReplaceStartingEquipment = false;
            archetype.StartingGold = barbarian.StartingGold;
            archetype.StartingItems = Array.Empty<BlueprintItem>();
            archetype.RemoveFeatures = new[] { Entry(1, fastMovement,
                nativeProficiency) };
            archetype.AddFeatures = new[] { Entry(1, proficiency, crowd,
                controlled), Entry(11, greaterDefault), Entry(20, mightyDefault) };
            archetype.ComponentsArray = Array.Empty<BlueprintComponent>();
            return archetype;
        }

        private static BlueprintFeature CreateFeature(string assetName,
            string name, string description, Sprite icon,
            params BlueprintComponent[] components)
        {
            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = assetName;
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.ComponentsArray = components ?? Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(assetName + ".Name", name),
                LocalizationService.Create(assetName + ".Description", description),
                icon);
            return feature;
        }

        private static BlueprintAbility CreatePersonalAbility(string assetName,
            string name, string description, Sprite icon)
        {
            var ability = ScriptableObject.CreateInstance<BlueprintAbility>();
            ability.name = assetName;
            ability.Type = AbilityType.Extraordinary;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetSelf = true;
            ability.CanTargetPoint = ability.CanTargetEnemies =
                ability.CanTargetFriends = false;
            ability.SpellResistance = false;
            ability.ActionType = UnitCommand.CommandType.Free;
            ability.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Self;
            ability.ActionBarAutoFillIgnored = false;
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.LocalizedDuration = LocalizationService.Create(
                assetName + ".Duration", "Until changed");
            ability.LocalizedSavingThrow = LocalizationService.Create(
                assetName + ".SavingThrow", "None");
            ability.ComponentsArray = Array.Empty<BlueprintComponent>();
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(assetName + ".Name", name),
                LocalizationService.Create(assetName + ".Description", description),
                icon);
            return ability;
        }

        private static void Validate(UrbanBarbarianBlueprintSet set,
            BlueprintFeature nativeProficiency, BlueprintFeature fastMovement,
            BlueprintFeature rageFeature)
        {
            if (set.Count != 70 || set.SelectionFacts.Length != 31 ||
                set.AllocationAbilities.Length != 31 ||
                set.Archetype.GetParentClass() != set.BarbarianClass ||
                !set.Archetype.ReplaceClassSkills ||
                set.Archetype.ClassSkills.Length != 5 ||
                set.Archetype.RemoveFeatures.Length != 1 ||
                set.Archetype.RemoveFeatures[0].Features.Count != 2 ||
                !set.Archetype.RemoveFeatures[0].Features.Contains(nativeProficiency) ||
                !set.Archetype.RemoveFeatures[0].Features.Contains(fastMovement) ||
                set.Archetype.RemoveFeatures.SelectMany(value => value.Features)
                    .Contains(rageFeature))
                throw new InvalidOperationException(
                    "Urban Barbarian blueprint validation failed.");
        }

        private static LevelEntry Entry(int level,
            params BlueprintFeatureBase[] features)
        { return new LevelEntry { Level = level, Features = features.ToList() }; }

        private static T Require<T>(LibraryScriptableObject library, string guid,
            string label) where T : BlueprintScriptableObject
        { return BlueprintLibraryLookup.RequireExact<T>(library, guid, label); }

        private static void SetParent(BlueprintArchetype archetype,
            BlueprintCharacterClass parent)
        {
            FieldInfo field = typeof(BlueprintArchetype).GetField("m_ParentClass",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(BlueprintCharacterClass))
                throw new MissingFieldException(typeof(BlueprintArchetype).FullName,
                    "m_ParentClass");
            field.SetValue(archetype, parent);
        }

        private static void SetIcon(BlueprintArchetype archetype, Sprite icon)
        {
            FieldInfo field = typeof(BlueprintArchetype).GetField("m_Icon",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || field.FieldType != typeof(Sprite))
                throw new MissingFieldException(typeof(BlueprintArchetype).FullName,
                    "m_Icon");
            field.SetValue(archetype, icon);
        }
    }
}
