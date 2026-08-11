using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.ElementsSystem;
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
using Kingmaker.UnitLogic.Mechanics.Components;
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
        private const string InvisibleStalkerUnitSymbol =
            "KMG.Summoning.Unit.InvisibleStalker";
        private const string ShadowDemonUnitSymbol =
            "KMG.Summoning.Unit.ShadowDemon";
        private const string ShadowDemonCombatTraitsSymbol =
            "KMG.Summoning.Special.ShadowDemon.CombatTraits";

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
        private const string DumbBrainGuid = "5abc8884c6f15204c8604cb01a2efbab";
        private const string NaturalArmor6Guid = "987ba44303e88054c9504cb3083ba0c9";
        private const string NaturalInvisibilityGuid = "94b2838e8a492c44ebf89e7fe7a75a62";
        private const string IncorporealGuid = "c4a7f98d743bc784c9d4cf2105852c39";
        private const string ElementalSubtypeGuid = "198fd8924dabcb5478d0f78bd453c586";
        private const string AirSubtypeGuid = "dd3d0c7f4f57f304cbdbb68170b1b775";
        private const string ChaoticSubtypeGuid = "1dd712e7f147ab84bad6ffccd21a878d";
        private const string EvilSubtypeGuid = "5279fc8380dd9ba419b4471018ffadd1";
        private const string ColdImmunityGuid = "9ae23798a9284e044ad2716a772a410e";
        private const string PoisonImmunityGuid = "7e3f3228be49cce49bda37f7901bf246";
        private const string CombatReflexesGuid = "0f8939ae6f220984e8fb568abbdfba95";
        private const string LightningReflexesGuid = "15e7da6645a7f3d41bdad7c8c4b9de1e";
        private const string WeaponFocusSlamGuid = "8c046dfa8d1c64247af0e830a5909510";
        private const string WeaponFocusClawGuid = "153937f44fcd42a429a286a10babd82d";
        private const string LargeAirSlamGuid = "72aa06bd4e7a8fa4db8a20d1b5f1a103";
        private const string LargeClawGuid = "c76f72a862d168d44838206524366e1c";
        private const string LargeBiteGuid = "ec35ef997ed5a984280e1a6d87ae80a8";

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
            ConfigureInvisibleStalker(library, Require<BlueprintUnit>(bySymbol,
                InvisibleStalkerUnitSymbol));
            BlueprintBuff shadowTraits = Require<BlueprintBuff>(bySymbol,
                ShadowDemonCombatTraitsSymbol);
            ConfigureShadowDemonCombatTraits(shadowTraits);
            ConfigureShadowDemon(library, Require<BlueprintUnit>(bySymbol,
                ShadowDemonUnitSymbol), shadowTraits);
        }

        private static AddClassLevels OutsiderLevels(
            LibraryScriptableObject library, int hitDice)
        {
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<
                BlueprintCharacterClass>(library, OutsiderClassGuid,
                    "native outsider class");
            levels.Levels = hitDice;
            levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown;
            levels.Skills = new[] { StatType.SkillPerception,
                StatType.SkillMobility, StatType.SkillPersuasion };
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            return levels;
        }

        private static BlueprintUnit.UnitBody NaturalBody(
            BlueprintItemWeapon primary, BlueprintItemWeapon[] additional,
            BlueprintItemWeapon[] secondary)
        {
            return new BlueprintUnit.UnitBody {
                DisableHands = false,
                PrimaryHand = primary,
                AdditionalLimbs = additional ?? Array.Empty<BlueprintItemWeapon>(),
                AdditionalSecondaryLimbs = secondary ??
                    Array.Empty<BlueprintItemWeapon>(),
                QuickSlots = Array.Empty<
                    Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>()
            };
        }

        private static void ConfigureUnitCore(BlueprintUnit unit, string key,
            string displayName, Alignment alignment, Size size, int strength,
            int dexterity, int constitution, int intelligence, int wisdom,
            int charisma, int speedFeet)
        {
            var unitName = ScriptableObject.CreateInstance<SharedStringAsset>();
            unitName.String = LocalizationService.Create(
                "KMG.ExpandedSummoning." + key + ".Unit.Name", displayName);
            unit.LocalizedName = unitName;
            unit.Alignment = alignment;
            unit.Size = size;
            unit.Strength = strength;
            unit.Dexterity = dexterity;
            unit.Constitution = constitution;
            unit.Intelligence = intelligence;
            unit.Wisdom = wisdom;
            unit.Charisma = charisma;
            unit.Speed = new Feet(speedFeet);
            unit.BaseAttackBonus = 0;
            unit.MaxHP = 0;
            unit.StartingInventory = Array.Empty<BlueprintItem>();
        }

        private static AddDamageResistanceEnergy Energy(DamageEnergyType type,
            int value)
        {
            var result = ScriptableObject.CreateInstance<
                AddDamageResistanceEnergy>();
            result.Type = type;
            result.Value = Simple(value);
            return result;
        }

        private static ContextActionDealDamage ColdDamage(int diceCount)
        {
            return new ContextActionDealDamage {
                DamageType = new DamageTypeDescription {
                    Type = DamageType.Energy,
                    Energy = DamageEnergyType.Cold
                },
                Value = new ContextDiceValue {
                    DiceType = DiceType.D6,
                    DiceCountValue = Simple(diceCount),
                    BonusValue = Simple(0)
                }
            };
        }

        private static void ConfigureInvisibleStalker(
            LibraryScriptableObject library, BlueprintUnit unit)
        {
            BlueprintItemWeapon slam = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeAirSlamGuid,
                    "Invisible Stalker 2d6 slam");
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.InvisibleStalkerHitDice)
            };
            unit.Body = NaturalBody(slam, new[] { slam },
                Array.Empty<BlueprintItemWeapon>());
            unit.Brain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "bounded natural-attack brain");
            ConfigureUnitCore(unit, "InvisibleStalker", "Invisible Stalker",
                Alignment.TrueNeutral, Size.Medium,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerStrength,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerDexterity,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerConstitution,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerIntelligence,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerWisdom,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerCharisma,
                ExpandedSummoningSpecialProfiles.InvisibleStalkerSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    NaturalArmor6Guid, "natural armor +6"),
                Feature(library, ElementalSubtypeGuid, "elemental subtype"),
                Feature(library, AirSubtypeGuid, "air subtype"),
                Feature(library, ExtraplanarSubtypeGuid, "extraplanar subtype"),
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, CombatReflexesGuid, "Combat Reflexes"),
                Feature(library, LightningReflexesGuid, "Lightning Reflexes"),
                Feature(library, WeaponFocusSlamGuid, "Weapon Focus (slam)"),
                BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                    NaturalInvisibilityGuid, "attack-safe natural invisibility")
            };
        }

        private static void ConfigureShadowDemonCombatTraits(BlueprintBuff buff)
        {
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(ExpandedSummoningSpecialProfiles
                .ShadowDemonDamageReduction);
            dr.Or = true;
            dr.BypassedByMaterial = true;
            dr.Material = PhysicalDamageMaterial.ColdIron;
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Good;
            var acid = Energy(DamageEnergyType.Acid,
                ExpandedSummoningSpecialProfiles.ShadowDemonEnergyResistance);
            var fire = Energy(DamageEnergyType.Fire,
                ExpandedSummoningSpecialProfiles.ShadowDemonEnergyResistance);
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>();
            sr.Value = Simple(ExpandedSummoningSpecialProfiles
                .ShadowDemonSpellResistance);
            sr.AddCR = false;
            var cold = ScriptableObject.CreateInstance<
                AddInitiatorAttackWithWeaponTrigger>();
            cold.OnlyHit = true;
            cold.AllNaturalAndUnarmed = true;
            cold.Action = new ActionList { Actions = new GameAction[] {
                ColdDamage(ExpandedSummoningSpecialProfiles
                    .ShadowDemonColdDamageDice)
            }};
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = new BlueprintComponent[] {
                dr, acid, fire, sr, cold
            };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.ShadowDemon.CombatTraits.Name",
                    "Shadow Demon Combat Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.ShadowDemon.CombatTraits.Description",
                    "DR 10/cold iron or good, acid and fire resistance 10, spell resistance 17, and 1d6 cold damage on natural attacks."),
                null);
        }

        private static void ConfigureShadowDemon(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintBuff combatTraits)
        {
            BlueprintItemWeapon claw = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeClawGuid,
                    "Shadow Demon 1d6 claw");
            BlueprintItemWeapon bite = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeBiteGuid,
                    "Shadow Demon 1d8 bite");
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.ShadowDemonHitDice)
            };
            unit.Body = NaturalBody(claw, new[] { claw }, new[] { bite });
            unit.Brain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "bounded natural-attack brain");
            ConfigureUnitCore(unit, "ShadowDemon", "Shadow Demon",
                Alignment.ChaoticEvil, Size.Medium,
                ExpandedSummoningSpecialProfiles.ShadowDemonStrength,
                ExpandedSummoningSpecialProfiles.ShadowDemonDexterity,
                ExpandedSummoningSpecialProfiles.ShadowDemonConstitution,
                ExpandedSummoningSpecialProfiles.ShadowDemonIntelligence,
                ExpandedSummoningSpecialProfiles.ShadowDemonWisdom,
                ExpandedSummoningSpecialProfiles.ShadowDemonCharisma,
                ExpandedSummoningSpecialProfiles.ShadowDemonSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                Feature(library, ChaoticSubtypeGuid, "chaotic subtype"),
                Feature(library, EvilSubtypeGuid, "evil subtype"),
                Feature(library, ExtraplanarSubtypeGuid, "extraplanar subtype"),
                Feature(library, IncorporealGuid, "incorporeal defenses"),
                Feature(library, ColdImmunityGuid, "cold immunity"),
                Feature(library, ElectricityImmunityGuid, "electricity immunity"),
                Feature(library, PoisonImmunityGuid, "poison immunity"),
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                Feature(library, WeaponFocusClawGuid, "Weapon Focus (claw)"),
                combatTraits
            };
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
                Feature(library, ImprovedInitiativeGuid, "Improved Initiative"),
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    NaturalArmor4Guid, "natural armor +4"),
                Feature(library, ElectricityImmunityGuid, "electricity immunity"),
                Feature(library, GoodSubtypeGuid, "good subtype"),
                Feature(library, LawfulSubtypeGuid, "lawful subtype"),
                Feature(library, ExtraplanarSubtypeGuid, "extraplanar subtype"),
                Feature(library, AirborneGuid, "airborne movement"),
                BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library,
                    AuraOfMenaceBuffGuid, "native Aura of Menace"),
                defenses
            };
        }

        private static BlueprintFeature Feature(LibraryScriptableObject library,
            string guid, string purpose)
        { return BlueprintLibraryLookup.RequireExact<BlueprintFeature>(library,
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
