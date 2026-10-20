using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private const string GiantPrefix = "KMG.Summoning.Giant.";
        private const string HillGiantUnit = "KMG.Summoning.Unit.HillGiant";
        private const string StoneGiantUnit = "KMG.Summoning.Unit.StoneGiant";
        private const string FireGiantUnit = "KMG.Summoning.Unit.FireGiant";
        private const string GiantGreatclub = GiantPrefix + "Greatclub2d8";
        private const string GiantGreatsword = GiantPrefix + "Greatsword3d6";
        private const string GiantSlam = GiantPrefix + "Slam1d8";
        private const string HillRock = GiantPrefix + "HillRock1d8";
        private const string StoneRock = GiantPrefix + "StoneRock1d8";
        private const string FireRock = GiantPrefix + "FireRock1d8";
        private const string HillRockType = GiantPrefix + "HillRockType";
        private const string StoneRockType = GiantPrefix + "StoneRockType";
        private const string FireRockType = GiantPrefix + "FireRockType";
        private const string HillHide = GiantPrefix + "HillHideArmor";
        private const string FireHalfPlate = GiantPrefix + "FireHalfPlate";
        private const string HillType = GiantPrefix + "HillUnitType";
        private const string StoneType = GiantPrefix + "StoneUnitType";
        private const string FireType = GiantPrefix + "FireUnitType";
        private const string StoneNaturalArmor = GiantPrefix + "StoneNaturalArmor11";
        private const string HillTraits = GiantPrefix + "HillTraits";
        private const string StoneTraits = GiantPrefix + "StoneTraits";
        private const string FireTraits = GiantPrefix + "FireTraits";
        private const string FireImprovedOverrun = GiantPrefix + "FireImprovedOverrun";
        private const string GiantCleaveAction = GiantPrefix + "CleaveAction";
        private const string GiantCleaveFeature = GiantPrefix + "CleaveFeature";

        private const string NativeHumanoidClass = "6ab4526f94d2e3e439af0599a29b6675";
        private const string NativeHill = "61dd71e874b16f742a0f783314105553";
        private const string NativeFrost = "590cd3d5e76fdc649a5f97bc984cd3c4";
        private const string NativeGreatclub = "5bd7125b30dd3904cab6a80863e3affc";
        private const string NativeGreatsword = "9fde0cebe3ec77648b109df71f480d1a";
        private const string NativeSlam = "72aa06bd4e7a8fa4db8a20d1b5f1a103";
        private const string NativeRock = "b99a9581f9d4828499484e7ca8bdaf95";
        private const string NativeHide = "caef4e3a5dace8e49a938cafc0a9b3e6";
        private const string NativeHalfPlate = "ed6bbd7ecd050c04690fe11d4c3b3f7d";
        private const string NativeNaturalArmorNine = "da6417809bdedfa468dd2fd0cc74be92";
        private const string NativeNaturalArmorEight = "b9342e2a6dc5165489ba3412c50ca3d1";
        private const string NativeGiantSubtype = "f9c388137f4faa74aac9065a68b56880";
        private const string NativeIntimidatingProwess = "d76497bfc48516e45a0831628f767a0f";
        private const string NativePointBlankShot = "0da0c194d6e1d43419eb8d990b28e0ab";
        private const string NativePreciseShot = "8f3d1e6b4be006f4d896081f2f889665";
        private const string NativeImprovedSunder = "9719015edcbf142409592e2cbaab7fe1";
        private const string NativeCleaveAction = "6447d104a2222c14d9c9b8a36e4eb242";
        private const string NativeCleaveFeature = "d809b6c4ff2aaff4fa70d712a70f7d7b";
        private const string NativeGreatCleaveFeature = "cc9c862ef2e03af4f89be5088851ea35";

        private static void ConfigureGiantHumanoids(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            BlueprintItemWeapon club = GiantWeapon(library, bySymbol, GiantGreatclub,
                NativeGreatclub, 2, DiceType.D8);
            BlueprintItemWeapon sword = GiantWeapon(library, bySymbol, GiantGreatsword,
                NativeGreatsword, 3, DiceType.D6);
            BlueprintItemWeapon slam = GiantWeapon(library, bySymbol, GiantSlam,
                NativeSlam, 1, DiceType.D8);
            BlueprintItemWeapon hillRock = GiantRock(library, bySymbol,
                HillRock, HillRockType, 120);
            BlueprintItemWeapon stoneRock = GiantRock(library, bySymbol,
                StoneRock, StoneRockType, 180);
            BlueprintItemWeapon fireRock = GiantRock(library, bySymbol,
                FireRock, FireRockType, 120);
            // The installed native Flaming property supplies one fire die on
            // this rock item only. It is never on a sword or slam.
            SetField(fireRock, "m_Enchantments", new[] {
                BlueprintLibraryLookup.RequireExact<BlueprintWeaponEnchantment>(
                    library, EasternWeaponNamedBlueprints.FlamingGuid,
                    "native flaming damage component for Heated Rock") });
            BlueprintItemArmor hide = GiantArmor(library, bySymbol,
                HillHide, NativeHide);
            BlueprintItemArmor halfPlate = GiantArmor(library, bySymbol,
                FireHalfPlate, NativeHalfPlate);
            ConfigureStoneNaturalArmor(Require<BlueprintFeature>(bySymbol,
                StoneNaturalArmor));
            ConfigureFireImprovedOverrun(library,
                Require<BlueprintFeature>(bySymbol, FireImprovedOverrun));
            ConfigureGiantCleave(library, bySymbol);
            ConfigureGiantUnit(library, bySymbol, HillGiantUnit, HillType,
                HillTraits, "HillGiant", "Hill Giant", Alignment.ChaoticEvil,
                10, 25, 8, 19, 6, 10, 7, 45, 7, 3, 3, 3, 4, 0,
                club, slam, hillRock, hide,
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library, NativeNaturalArmorNine, "hill natural armor"),
                extraplanar,
                new[] { NativeIntimidatingProwess,
                    "9972f33f977fc724c838e59641b2fca5" },
                true);
            ConfigureGiantUnit(library, bySymbol, StoneGiantUnit, StoneType,
                StoneTraits, "StoneGiant", "Stone Giant", Alignment.TrueNeutral,
                12, 27, 15, 19, 10, 12, 10, 54, 8, 4, 4, 8, 9, 6,
                club, slam, stoneRock, null,
                Require<BlueprintFeature>(bySymbol, StoneNaturalArmor),
                extraplanar,
                new[] { "175d1577bb6c9a04baf88eec99c66334",
                    NativePointBlankShot, "9972f33f977fc724c838e59641b2fca5",
                    NativePreciseShot }, false);
            ConfigureGiantUnit(library, bySymbol, FireGiantUnit, FireType,
                FireTraits, "FireGiant", "Fire Giant", Alignment.LawfulEvil,
                15, 31, 9, 21, 10, 14, 10, 67, 9, 5, 5, 9, 8, 0,
                sword, slam, fireRock, halfPlate,
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library, NativeNaturalArmorEight, "fire natural armor"),
                extraplanar,
                new[] { NativeGreatCleaveFeature,
                    NativeImprovedSunder,
                    "175d1577bb6c9a04baf88eec99c66334",
                    "9972f33f977fc724c838e59641b2fca5" }, true,
                new[] { FireSubtypeGuid, FireImmunityGuid, ColdVulnerabilityGuid });
            ExpandedSummoningVisualVariantPatch.Register(new SummonVisualVariant(
                InternalName(StoneGiantUnit), new SummonVisualTintProfile(
                    "stone-giant", 0.66f, 0.68f, 0.69f, null, null, null)));
            ExpandedSummoningVisualVariantPatch.Register(new SummonVisualVariant(
                InternalName(FireGiantUnit), new SummonVisualTintProfile(
                    "fire-giant", 0.86f, 0.45f, 0.33f, null, null, null)));
        }

        // The installed discriminator has differed between standalone and
        // combined profiles. Accept only the two audited native feat facts,
        // then set our private delivery to Great Cleave explicitly: Hill has
        // one follow-up and Fire can continue. Native facts stay untouched.
        private static void ConfigureGiantCleave(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintAbility nativeAction = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, NativeCleaveAction,
                    "native Cleave execution");
            BlueprintFeature nativeFeature = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeCleaveFeature,
                    "native Cleave grant");
            BlueprintFeature greater = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeGreatCleaveFeature,
                    "native Great Cleave discriminator");
            BlueprintAbility action = Require<BlueprintAbility>(bySymbol,
                GiantCleaveAction);
            ExpandedSummoningAbilityBuilder.CopyFields(nativeAction, action);
            action.name = InternalName(GiantCleaveAction);
            action.ComponentsArray = (nativeAction.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            AbilityCustomCleave nativeCustom = nativeAction.GetComponents<
                AbilityCustomCleave>().Single();
            string nativeDiscriminator = nativeCustom.GreaterFeature == null ?
                null : nativeCustom.GreaterFeature.AssetGuid;
            if (!string.Equals(nativeDiscriminator, nativeFeature.AssetGuid,
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(nativeDiscriminator, greater.AssetGuid,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "Installed Cleave consumer no longer has audited wiring: expected=" +
                    nativeFeature.AssetGuid + " or " + greater.AssetGuid +
                    "; actual=" + (nativeDiscriminator ?? "<null>"));
            int deliveryIndex = Array.FindIndex(action.ComponentsArray,
                value => value is AbilityCustomCleave);
            if (deliveryIndex < 0 || action.ComponentsArray.Count(
                value => value is AbilityCustomCleave) != 1)
                throw new InvalidOperationException(
                    "Cloned Cleave has an ambiguous delivery component.");
            var delivery = ScriptableObject.CreateInstance<
                SummonGiantCleaveDelivery>();
            delivery.GreaterFeature = greater;
            action.ComponentsArray[deliveryIndex] = delivery;

            BlueprintFeature feature = Require<BlueprintFeature>(bySymbol,
                GiantCleaveFeature);
            CopyFields(nativeFeature, feature);
            feature.name = InternalName(GiantCleaveFeature);
            feature.ComponentsArray = (nativeFeature.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            AddFacts grant = feature.GetComponents<AddFacts>().Single();
            if (grant.Facts == null || grant.Facts.Length != 1 ||
                !(grant.Facts[0] is BlueprintAbility) ||
                !string.Equals(grant.Facts[0].AssetGuid, nativeAction.AssetGuid,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "Installed Cleave feature no longer has audited action grant.");
            grant.Facts = new BlueprintUnitFact[] { action };
        }

        private static BlueprintItemWeapon GiantWeapon(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            string symbol, string donorGuid, int count, DiceType die)
        {
            BlueprintItemWeapon donor = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, donorGuid, symbol + " donor");
            BlueprintItemWeapon result = Require<BlueprintItemWeapon>(bySymbol, symbol);
            CopyFields(donor, result);
            result.name = InternalName(symbol);
            result.ComponentsArray = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(result, "m_Size", Size.Large);
            SetField(result, "m_OverrideDamageDice", true);
            SetField(result, "m_DamageDice", new DiceFormula(count, die));
            SetField(result, "m_Enchantments",
                Array.Empty<BlueprintWeaponEnchantment>());
            SetField(result, "m_Cost", 0);
            result.IsNonRemovable = true;
            return result;
        }

        private static BlueprintItemWeapon GiantRock(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            string symbol, string typeSymbol, int rangeFeet)
        {
            BlueprintItemWeapon donor = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, NativeRock, "native rock attack");
            BlueprintWeaponType type = Require<BlueprintWeaponType>(bySymbol,
                typeSymbol);
            CopyFields(donor.Type, type);
            type.name = InternalName(typeSymbol);
            type.ComponentsArray = (donor.Type.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(type, "m_BaseDamage", new DiceFormula(1, DiceType.D8));
            SetField(type, "m_AttackRange", new Feet(rangeFeet));
            BlueprintItemWeapon result = Require<BlueprintItemWeapon>(bySymbol,
                symbol);
            CopyFields(donor, result);
            result.name = InternalName(symbol);
            result.ComponentsArray = (donor.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(result, "m_Type", type);
            SetField(result, "m_Size", Size.Large);
            SetField(result, "m_OverrideDamageDice", true);
            SetField(result, "m_DamageDice", new DiceFormula(1, DiceType.D8));
            SetField(result, "m_Enchantments",
                Array.Empty<BlueprintWeaponEnchantment>());
            SetField(result, "m_Cost", 0);
            SetField(result, "m_Weight", 0f);
            result.IsNonRemovable = true;
            return result;
        }

        private static BlueprintItemArmor GiantArmor(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            string symbol, string donorGuid)
        {
            BlueprintItemArmor donor = BlueprintLibraryLookup.RequireExact<
                BlueprintItemArmor>(library, donorGuid, symbol + " donor");
            BlueprintItemArmor result = Require<BlueprintItemArmor>(bySymbol,
                symbol);
            CopyFields(donor, result);
            result.name = InternalName(symbol);
            result.ComponentsArray = Array.Empty<BlueprintComponent>();
            SetField(result, "m_Size", Size.Large);
            SetField(result, "m_Enchantments",
                Array.Empty<BlueprintEquipmentEnchantment>());
            SetField(result, "m_Cost", 0);
            result.IsNonRemovable = true;
            return result;
        }

        private static void ConfigureStoneNaturalArmor(BlueprintFeature feature)
        {
            var bonus = ScriptableObject.CreateInstance<AddStatBonus>();
            bonus.Stat = StatType.AC;
            bonus.Descriptor = ModifierDescriptor.NaturalArmor;
            bonus.Value = 11;
            feature.name = InternalName(StoneNaturalArmor);
            feature.IsClassFeature = true;
            feature.HideInUI = true;
            feature.ComponentsArray = new BlueprintComponent[] { bonus };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(StoneNaturalArmor + ".Name",
                    "Stone Giant Natural Armor"),
                LocalizationService.Create(StoneNaturalArmor + ".Description",
                    "The adult stone giant's +11 natural armor."), null);
        }

        // The installed-stack feat is absent from standalone 2.1.7b.
        // Grant the printed feat through a project-owned stock-native carrier.
        private static void ConfigureFireImprovedOverrun(
            LibraryScriptableObject library, BlueprintFeature feature)
        {
            BlueprintAbility overrun = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, NativeOverrunAbilityGuid,
                    "stock Fire Giant Overrun command");
            var grant = ScriptableObject.CreateInstance<AddFacts>();
            grant.Facts = new BlueprintUnitFact[] { overrun };
            var attack = ScriptableObject.CreateInstance<ManeuverBonus>();
            attack.Type = CombatManeuver.Overrun;
            attack.Bonus = 2;
            var defence = ScriptableObject.CreateInstance<ManeuverDefenceBonus>();
            defence.Type = CombatManeuver.Overrun;
            defence.Bonus = 2;
            feature.name = InternalName(FireImprovedOverrun);
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.ComponentsArray = new BlueprintComponent[] {
                grant, attack, defence };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(FireImprovedOverrun + ".Name",
                    "Improved Overrun"),
                LocalizationService.Create(FireImprovedOverrun + ".Description",
                    "Grants the overrun action, with +2 on overrun checks "
                    + "and defense against overrun."),
                ExpandedSummoningProjectIcons.Require("fire-giant"));
        }

        private static void ConfigureGiantUnit(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            string unitSymbol, string typeSymbol, string traitSymbol,
            string token, string displayName, Alignment alignment,
            int hitDice, int strength, int dexterity, int constitution,
            int intelligence, int wisdom, int charisma, int baseHitPoints,
            int fortitude, int reflex, int will, int perceptionRanks,
            int persuasionRanks, int stealthRanks, BlueprintItemWeapon weapon,
            BlueprintItemWeapon slam, BlueprintItemWeapon rock,
            BlueprintItemArmor armor, BlueprintUnitFact naturalArmor,
            BlueprintFeature extraplanar, string[] featGuids,
            bool focus, string[] extraFacts = null)
        {
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol, unitSymbol);
            BlueprintUnitType type = Require<BlueprintUnitType>(bySymbol,
                typeSymbol);
            type.name = InternalName(typeSymbol);
            type.Name = LocalizationService.Create(typeSymbol + ".Name",
                displayName);
            type.Description = LocalizationService.Create(typeSymbol +
                ".Description", "Adult humanoid of the giant subtype.");
            type.Image = null;
            type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            unit.Type = type;
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<
                BlueprintCharacterClass>(library, NativeHumanoidClass,
                    "giant humanoid racial Hit Dice");
            levels.Levels = hitDice;
            levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown;
            levels.Skills = Array.Empty<StatType>();
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<
                Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<
                Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var racial = ScriptableObject.CreateInstance<
                SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit;
            racial.BaseRacialHitPoints = baseHitPoints;
            racial.BaseFortitude = fortitude;
            racial.BaseReflex = reflex;
            racial.BaseWill = will;
            racial.PerceptionRanks = perceptionRanks;
            racial.PersuasionRanks = persuasionRanks;
            racial.StealthRanks = stealthRanks;
            unit.Skills = new BlueprintUnit.UnitSkills();
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial };
            unit.Body = new BlueprintUnit.UnitBody {
                PrimaryHand = weapon,
                PrimaryHandAlternative1 = slam,
                PrimaryHandAlternative2 = rock,
                EmptyHandWeapon = slam,
                AdditionalLimbs = Array.Empty<BlueprintItemWeapon>(),
                AdditionalSecondaryLimbs = Array.Empty<BlueprintItemWeapon>(),
                Armor = armor,
                ActiveHandSet = 0,
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment.
                    BlueprintItemEquipmentUsable>()
            };
            unit.Brain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "bounded giant attack brain");
            ConfigureUnitCore(unit, token, displayName, alignment, Size.Large,
                strength, dexterity, constitution, intelligence, wisdom,
                charisma, 40);
            BlueprintFeature traits = Require<BlueprintFeature>(bySymbol,
                traitSymbol);
            var proficiency = ScriptableObject.CreateInstance<AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { weapon.Category };
            proficiency.ArmorProficiencies = armor == null ?
                Array.Empty<ArmorProficiencyGroup>() :
                new[] { armor.Type.ProficiencyGroup };
            var equipment = ScriptableObject.CreateInstance<
                SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit;
            equipment.GreatWeapon = weapon;
            equipment.Slam = slam;
            equipment.Rock = rock;
            equipment.Armor = armor;
            var intimidate = ScriptableObject.CreateInstance<
                SummonGiantIntimidateClassSkill>();
            intimidate.OwningBlueprint = unit;
            var components = new List<BlueprintComponent> {
                proficiency, equipment, intimidate };
            var rockAttack = ScriptableObject.CreateInstance<
                SummonGiantRockAttack>();
            rockAttack.OwningBlueprint = unit;
            rockAttack.Rock = rock;
            rockAttack.Slam = slam;
            components.Add(rockAttack);
            var rockCatch = ScriptableObject.CreateInstance<
                SummonGiantRockCatch>();
            rockCatch.OwningBlueprint = unit;
            rockCatch.ImprovedStoneCatch = token == "StoneGiant";
            components.Add(rockCatch);
            if (token == "StoneGiant")
            {
                var rockyStealth = ScriptableObject.CreateInstance<
                    SummonStoneRockyStealth>();
                rockyStealth.OwningBlueprint = unit;
                components.Add(rockyStealth);
            }
            if (focus)
            {
                var weaponFocus = ScriptableObject.CreateInstance<WeaponFocus>();
                weaponFocus.WeaponType = weapon.Type;
                weaponFocus.AttackBonus = 1;
                components.Add(weaponFocus);
            }
            traits.name = InternalName(traitSymbol);
            traits.IsClassFeature = true;
            traits.HideInUI = false;
            traits.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(traitSymbol + ".Name",
                    displayName + " Combat Traits"),
                LocalizationService.Create(traitSymbol + ".Description",
                    "Adult giant weapon proficiency and summon-owned equipment."),
                null);
            var facts = new List<BlueprintUnitFact> { naturalArmor,
                Feature(library, NativeGiantSubtype, "giant subtype"),
                extraplanar, traits };
            foreach (string guid in featGuids)
                facts.Add(Feature(library, guid, displayName + " printed feat"));
            if (token == "HillGiant" || token == "FireGiant")
                facts.Add(Require<BlueprintFeature>(bySymbol,
                    GiantCleaveFeature));
            if (token == "FireGiant")
                facts.Add(Require<BlueprintFeature>(bySymbol,
                    FireImprovedOverrun));
            if (extraFacts != null)
                foreach (string guid in extraFacts)
                    facts.Add(Feature(library, guid, displayName + " subtype"));
            unit.AddFacts = facts.ToArray();
        }
    }
}
