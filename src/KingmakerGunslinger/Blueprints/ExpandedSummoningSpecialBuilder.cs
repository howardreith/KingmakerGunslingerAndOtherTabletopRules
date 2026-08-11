using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static class ExpandedSummoningSpecialBuilder
    {
        private const string LanternUnitSymbol =
            "KMG.Summoning.Unit.LanternArchon";
        private const string LanternRaySymbol =
            "KMG.Summoning.Special.LanternArchon.LightRay";
        private const string LanternAiSymbol =
            "KMG.Summoning.Special.LanternArchon.LightRayAi";
        private const string LanternBrainSymbol =
            "KMG.Summoning.Special.LanternArchon.Brain";
        private const string LanternDefenseSymbol =
            "KMG.Summoning.Special.LanternArchon.Defenses";

        private const string NativeRayGuid = "33e8997912cf76b4c99dca0445082804";
        private const string NativeRayAiGuid = "dcfc5e9aec5bea540b36caf754989164";
        private const string OutsiderClassGuid = "92ab5f2fe00631b44810deffcc1a97fd";
        private const string ImprovedInitiativeGuid = "797f25d709f559546b29e7bcb181cc74";
        private const string NaturalArmor4Guid = "16fc201a83edcde4cbd64c291ebe0d07";
        private const string ElectricityImmunityGuid = "cd1e5ab641a833c49994aff99db98952";
        private const string GoodSubtypeGuid = "23247ff3b44fd3a42ab752cd04e629b0";
        private const string LawfulSubtypeGuid = "56af493a739e14f44aa56a6cba0b477b";
        private const string ExtraplanarSubtypeGuid = "136fa0343d5b4b348bdaa05d83408db3";
        private const string AirborneGuid = "70cffb448c132fa409e49156d013b175";
        private const string AuraOfMenaceBuffGuid = "1ce4878b5e714f659d0854a12f4b3cf2";

        internal static void Configure(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            ExpandedSummoningSpecialProfiles.Validate();
            BlueprintAbility ray = Require<BlueprintAbility>(bySymbol,
                LanternRaySymbol);
            BlueprintAiCastSpell ai = Require<BlueprintAiCastSpell>(bySymbol,
                LanternAiSymbol);
            BlueprintBrain brain = Require<BlueprintBrain>(bySymbol,
                LanternBrainSymbol);
            BlueprintBuff defenses = Require<BlueprintBuff>(bySymbol,
                LanternDefenseSymbol);
            ConfigureRay(library, ray);
            ConfigureAi(library, ai, ray);
            brain.name = InternalName(LanternBrainSymbol);
            brain.Actions = new BlueprintAiAction[] { ai };
            ConfigureDefenses(defenses);
            ConfigureUnit(library, Require<BlueprintUnit>(bySymbol,
                LanternUnitSymbol), ray, brain, defenses);
        }

        private static void ConfigureRay(LibraryScriptableObject library,
            BlueprintAbility ray)
        {
            BlueprintAbility native = BlueprintLibraryLookup.RequireExact<BlueprintAbility>(
                library, NativeRayGuid, "native Ghaele two-projectile light ray");
            ExpandedSummoningAbilityBuilder.CopyFields(native, ray);
            ray.name = InternalName(LanternRaySymbol);
            ray.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            ray.Range = AbilityRange.Custom;
            ray.CustomRange = new Feet(
                ExpandedSummoningSpecialProfiles.LanternRayRangeFeet);
            ray.CanTargetSelf = false;
            ray.CanTargetFriends = false;
            ray.SpellResistance = false;
            AbilityDeliverProjectile delivery = ray.ComponentsArray
                .OfType<AbilityDeliverProjectile>().Single();
            delivery.Length = new Feet(
                ExpandedSummoningSpecialProfiles.LanternRayRangeFeet);
            if ((delivery.Projectiles ?? Array.Empty<BlueprintProjectile>()).Length !=
                ExpandedSummoningSpecialProfiles.LanternRayProjectiles)
                throw new InvalidOperationException(
                    "Native Lantern ray donor no longer has two projectiles.");
            ContextActionDealDamage damage = FindDamage(ray.ComponentsArray);
            damage.Value = new ContextDiceValue {
                DiceType = DiceType.D6,
                DiceCountValue = Simple(
                    ExpandedSummoningSpecialProfiles.LanternRayDiceCount),
                BonusValue = Simple(0)
            };
            damage.DamageType.Type = DamageType.Direct;
            BlueprintUnitFactAccess.Resolve().Configure(ray,
                LocalizationService.Create("KMG.ExpandedSummoning.LanternArchon.LightRay.Name",
                    "Light Ray"),
                LocalizationService.Create("KMG.ExpandedSummoning.LanternArchon.LightRay.Description",
                    "Fires two ranged-touch rays to 30 feet. Each ray deals 1d6 direct light damage."),
                native.Icon);
        }

        private static void ConfigureAi(LibraryScriptableObject library,
            BlueprintAiCastSpell ai, BlueprintAbility ray)
        {
            BlueprintAiCastSpell native = BlueprintLibraryLookup.RequireExact<
                BlueprintAiCastSpell>(library, NativeRayAiGuid,
                    "native Ghaele light-ray AI action");
            CopyFields(native, ai);
            ai.name = InternalName(LanternAiSymbol);
            ai.Ability = ray;
            ai.Variant = null;
            ai.CooldownRounds = 0;
            ai.StartCooldownRounds = 0;
        }

        private static void ConfigureDefenses(BlueprintBuff defenses)
        {
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(ExpandedSummoningSpecialProfiles.LanternDamageReduction);
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Evil;
            var poison = ScriptableObject.CreateInstance<
                SavingThrowBonusAgainstDescriptor>();
            poison.SpellDescriptor = SpellDescriptor.Poison;
            poison.ModifierDescriptor = ModifierDescriptor.Racial;
            poison.Value = ExpandedSummoningSpecialProfiles.LanternPoisonSaveBonus;
            poison.Bonus = Simple(0);
            var evilSave = ScriptableObject.CreateInstance<
                SavingThrowBonusAgainstAlignment>();
            evilSave.Alignment = AlignmentComponent.Evil;
            evilSave.Descriptor = ModifierDescriptor.Resistance;
            evilSave.Value = ExpandedSummoningSpecialProfiles
                .LanternEvilSaveAndAcBonus;
            var evilAc = ScriptableObject.CreateInstance<
                ArmorClassBonusAgainstAlignment>();
            evilAc.alignment = AlignmentComponent.Evil;
            evilAc.Descriptor = ModifierDescriptor.Deflection;
            evilAc.Value = ExpandedSummoningSpecialProfiles
                .LanternEvilSaveAndAcBonus;
            defenses.Stacking = StackingType.Replace;
            defenses.IsClassFeature = true;
            defenses.ComponentsArray = new BlueprintComponent[] {
                dr, poison, evilSave, evilAc
            };
            BlueprintUnitFactAccess.Resolve().Configure(defenses,
                LocalizationService.Create("KMG.ExpandedSummoning.LanternArchon.Defenses.Name",
                    "Archon Defenses"),
                LocalizationService.Create("KMG.ExpandedSummoning.LanternArchon.Defenses.Description",
                    "Damage reduction 10/evil, archon poison resistance, and defenses against evil creatures."),
                null);
        }

        private static void ConfigureUnit(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintAbility ray, BlueprintBrain brain,
            BlueprintBuff defenses)
        {
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<
                BlueprintCharacterClass>(library, OutsiderClassGuid,
                    "native outsider class");
            levels.Levels = ExpandedSummoningSpecialProfiles.LanternHitDice;
            levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown;
            levels.Skills = new[] { StatType.SkillPerception,
                StatType.SkillMobility, StatType.SkillPersuasion };
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var grant = ScriptableObject.CreateInstance<
                AddAbilityToCharacterComponent>();
            grant.Abilities = new[] { ray };
            unit.ComponentsArray = new BlueprintComponent[] { levels, grant };
            unit.Body = new BlueprintUnit.UnitBody {
                DisableHands = true,
                AdditionalLimbs = Array.Empty<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>(),
                AdditionalSecondaryLimbs = Array.Empty<Kingmaker.Blueprints.Items.Weapons.BlueprintItemWeapon>(),
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>()
            };
            unit.Brain = brain;
            var unitName = ScriptableObject.CreateInstance<SharedStringAsset>();
            unitName.String = LocalizationService.Create(
                "KMG.ExpandedSummoning.LanternArchon.Unit.Name", "Lantern Archon");
            unit.LocalizedName = unitName;
            unit.Alignment = Alignment.LawfulGood;
            unit.Size = Size.Small;
            unit.Strength = ExpandedSummoningSpecialProfiles.LanternStrength;
            unit.Dexterity = ExpandedSummoningSpecialProfiles.LanternDexterity;
            unit.Constitution = ExpandedSummoningSpecialProfiles.LanternConstitution;
            unit.Intelligence = ExpandedSummoningSpecialProfiles.LanternIntelligence;
            unit.Wisdom = ExpandedSummoningSpecialProfiles.LanternWisdom;
            unit.Charisma = ExpandedSummoningSpecialProfiles.LanternCharisma;
            unit.Speed = new Feet(ExpandedSummoningSpecialProfiles.LanternSpeedFeet);
            unit.BaseAttackBonus = 0;
            unit.MaxHP = 0;
            unit.StartingInventory = Array.Empty<BlueprintItem>();
            unit.AddFacts = new BlueprintUnitFact[] {
                Fact(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Fact(library, NaturalArmor4Guid, "natural armor +4"),
                Fact(library, ElectricityImmunityGuid, "electricity immunity"),
                Fact(library, GoodSubtypeGuid, "good subtype"),
                Fact(library, LawfulSubtypeGuid, "lawful subtype"),
                Fact(library, ExtraplanarSubtypeGuid, "extraplanar subtype"),
                Fact(library, AirborneGuid, "airborne movement"),
                Fact(library, AuraOfMenaceBuffGuid, "native Aura of Menace"),
                defenses
            };
        }

        private static BlueprintUnitFact Fact(LibraryScriptableObject library,
            string guid, string purpose)
        { return BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
            guid, purpose); }

        private static ContextActionDealDamage FindDamage(
            IEnumerable<BlueprintComponent> components)
        {
            var found = new List<ContextActionDealDamage>();
            foreach (BlueprintComponent component in components)
                Find(component, found, new HashSet<object>());
            if (found.Count != 1) throw new InvalidOperationException(
                "Lantern light ray requires exactly one damage action.");
            return found[0];
        }

        private static void Find(object value, IList<ContextActionDealDamage> found,
            ISet<object> seen)
        {
            if (value == null || value is string || value.GetType().IsValueType ||
                value is BlueprintScriptableObject || !seen.Add(value)) return;
            ContextActionDealDamage damage = value as ContextActionDealDamage;
            if (damage != null) { found.Add(damage); return; }
            foreach (FieldInfo field in Fields(value.GetType()))
            {
                object child = field.GetValue(value);
                System.Collections.IEnumerable sequence = child as
                    System.Collections.IEnumerable;
                if (sequence != null && !(child is string))
                    foreach (object item in sequence) Find(item, found, seen);
                else Find(child, found, seen);
            }
        }

        private static void CopyFields(object source, object target)
        {
            foreach (FieldInfo field in Fields(source.GetType()))
            {
                if (field.Name == "m_AssetGuid" || field.IsInitOnly ||
                    field.DeclaringType == typeof(UnityEngine.Object)) continue;
                object value = field.GetValue(source);
                Array array = value as Array;
                field.SetValue(target, array == null ? value : array.Clone());
            }
        }

        private static IEnumerable<FieldInfo> Fields(Type type)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            for (Type current = type; current != null; current = current.BaseType)
                foreach (FieldInfo field in current.GetFields(flags)) yield return field;
        }

        private static ContextValue Simple(int value)
        { return new ContextValue { ValueType = ContextValueType.Simple, Value = value }; }
        private static T Require<T>(IDictionary<string, BlueprintScriptableObject> values,
            string symbol) where T : BlueprintScriptableObject
        { return (T)values[symbol]; }
        private static string InternalName(string symbol)
        { return symbol.Replace('.', '_').Replace('-', '_'); }
    }
}
