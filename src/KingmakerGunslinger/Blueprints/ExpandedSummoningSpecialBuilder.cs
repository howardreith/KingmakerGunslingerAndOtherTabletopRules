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
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
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
        private const string SalamanderUnitSymbol =
            "KMG.Summoning.Unit.Salamander";
        private const string SalamanderTailSymbol =
            "KMG.Summoning.Special.Salamander.Tail";
        private const string SalamanderCombatTraitsSymbol =
            "KMG.Summoning.Special.Salamander.CombatTraits";
        private const string SuccubusUnitSymbol =
            "KMG.Summoning.Unit.Succubus";
        private const string SuccubusDominateSymbol =
            "KMG.Summoning.Special.Succubus.Dominate";
        private const string SuccubusDominationSymbol =
            "KMG.Summoning.Special.Succubus.Domination";
        private const string SuccubusDominateAiSymbol =
            "KMG.Summoning.Special.Succubus.DominateAi";
        private const string SuccubusBrainSymbol =
            "KMG.Summoning.Special.Succubus.Brain";
        private const string SuccubusCombatTraitsSymbol =
            "KMG.Summoning.Special.Succubus.CombatTraits";

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
        private const string NaturalArmor7Guid = "e73864391ccf0894997928443a29d755";
        private const string FireImmunityGuid = "11ac3433adfa74642a93111624376070";
        private const string FireSubtypeGuid = "23dc7b90d148b9d439f48e015a520a9c";
        private const string DrMagic10Guid = "ac5a99153e1790941b7bb93c06586ea5";
        private const string WeaponFocusSpearGuid = "8c7e86088025ad3448849d4972335dc8";
        private const string StandardSpearGuid = "4abc27631e2894f4b8b70270e31694f1";
        private const string LargeTailGuid = "ae822725634c6f0418b8c48bd29df255";
        private const string NativeGrabGuid = "efc1e80fb41e06544be46604983806d6";
        private const string NativeDominateGuid = "d7cbd2004ce66a042aeab2e95a3c5c61";
        private const string NativeDominationGuid = "c0f4e1c24c9cd334ca988ed1bd9d201f";
        private const string NativeEnergyDrainGuid = "ab966bf06859119419989ccb0061ba39";
        private const string DodgeGuid = "97e216dbb46ae3c4faef90cf6bbe6fd5";
        private const string WeaponFinesseGuid = "90e54424d682d104ab36436bd527af09";
        private const string AberrationTypeGuid = "3bec99efd9a363242a6c8d9957b75e91";
        private const string ConstructTypeGuid = "fd389783027d63343b4a5634bd81645f";
        private const string DragonTypeGuid = "455ac88e22f55804ab87c2467deff1d6";
        private const string FeyTypeGuid = "018af8005220ac94a9a4f47b3e9c2b4e";
        private const string OutsiderTypeGuid = "9054d3988d491d944ac144e27b6bc318";
        private const string PlantTypeGuid = "706e61781d692a042b35941f14bc41c5";
        private const string AnimalTypeGuid = "a95311b3dc996964cbaa30ff9965aaf6";
        private const string MonstrousHumanoidTypeGuid = "57614b50e8d86b24395931fffc5e409b";
        private const string MagicalBeastTypeGuid = "625827490ea69d84d8e599a33929fdc6";
        private const string VerminTypeGuid = "09478937695300944a179530664e42ec";
        private const string UndeadTypeGuid = "734a29b693e9ec346ba2951b27987e33";

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
            BlueprintItemWeapon salamanderTail = Require<BlueprintItemWeapon>(
                bySymbol, SalamanderTailSymbol);
            BlueprintBuff salamanderTraits = Require<BlueprintBuff>(bySymbol,
                SalamanderCombatTraitsSymbol);
            ConfigureSalamanderTail(library, salamanderTail);
            ConfigureSalamanderCombatTraits(library, salamanderTraits,
                salamanderTail);
            ConfigureSalamander(library, Require<BlueprintUnit>(bySymbol,
                SalamanderUnitSymbol), salamanderTail, salamanderTraits);
            BlueprintBuff domination = Require<BlueprintBuff>(bySymbol,
                SuccubusDominationSymbol);
            BlueprintAbility dominate = Require<BlueprintAbility>(bySymbol,
                SuccubusDominateSymbol);
            BlueprintAiCastSpell dominateAi = Require<BlueprintAiCastSpell>(
                bySymbol, SuccubusDominateAiSymbol);
            BlueprintBrain succubusBrain = Require<BlueprintBrain>(bySymbol,
                SuccubusBrainSymbol);
            BlueprintBuff succubusTraits = Require<BlueprintBuff>(bySymbol,
                SuccubusCombatTraitsSymbol);
            ConfigureSuccubusDomination(library, domination);
            ConfigureSuccubusDominate(library, dominate, domination);
            ConfigureSuccubusAi(dominateAi, dominate, succubusBrain);
            ConfigureSuccubusCombatTraits(library, succubusTraits);
            ConfigureSuccubus(library, Require<BlueprintUnit>(bySymbol,
                SuccubusUnitSymbol), dominate, succubusBrain, succubusTraits);
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
        { return EnergyDamage(DamageEnergyType.Cold, diceCount); }

        private static ContextActionDealDamage EnergyDamage(
            DamageEnergyType energy, int diceCount)
        {
            return new ContextActionDealDamage {
                DamageType = new DamageTypeDescription {
                    Type = DamageType.Energy,
                    Energy = energy
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

        private static void ConfigureSalamanderTail(
            LibraryScriptableObject library, BlueprintItemWeapon tail)
        {
            BlueprintItemWeapon native = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeTailGuid,
                    "native animated tail weapon");
            CopyFields(native, tail);
            tail.name = InternalName(SalamanderTailSymbol);
            tail.ComponentsArray = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            SetField(tail, "m_OverrideDamageDice", true);
            SetField(tail, "m_DamageDice", new DiceFormula(2, DiceType.D6));
            SetField(tail, "m_Enchantments", Array.Empty<
                Kingmaker.Blueprints.Items.Ecnchantments.BlueprintWeaponEnchantment>());
        }

        private static void ConfigureSalamanderCombatTraits(
            LibraryScriptableObject library, BlueprintBuff buff,
            BlueprintItemWeapon tail)
        {
            BlueprintFeature nativeGrab = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeGrabGuid,
                    "native bounded grab/constrict graph");
            AddInitiatorAttackWithWeaponTrigger grab =
                (AddInitiatorAttackWithWeaponTrigger)
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                    nativeGrab.ComponentsArray.OfType<
                        AddInitiatorAttackWithWeaponTrigger>().Single());
            grab.WeaponType = tail.Type;
            ContextActionDealDamage constrict = FindDamage(
                new BlueprintComponent[] { grab });
            constrict.Value = new ContextDiceValue {
                DiceType = DiceType.D6,
                DiceCountValue = Simple(ExpandedSummoningSpecialProfiles
                    .SalamanderConstrictDice),
                BonusValue = Simple(ExpandedSummoningSpecialProfiles
                    .SalamanderConstrictBonus)
            };
            ManeuverBonus grappleBonus = (ManeuverBonus)
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                    nativeGrab.ComponentsArray.OfType<ManeuverBonus>().Single());
            var heat = ScriptableObject.CreateInstance<
                AddInitiatorAttackWithWeaponTrigger>();
            heat.OnlyHit = true;
            heat.Action = new ActionList { Actions = new GameAction[] {
                EnergyDamage(DamageEnergyType.Fire,
                    ExpandedSummoningSpecialProfiles.SalamanderHeatDice)
            }};
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = new BlueprintComponent[] {
                grab, grappleBonus, heat
            };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Salamander.CombatTraits.Name",
                    "Salamander Heat and Constrict"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Salamander.CombatTraits.Description",
                    "Successful attacks deal 1d6 fire damage; tail hits can grab and constrict for 2d6+4 damage."), null);
        }

        private static void ConfigureSalamander(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintItemWeapon tail,
            BlueprintBuff combatTraits)
        {
            BlueprintItemWeapon spear = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, StandardSpearGuid,
                    "standard 1d8 spear");
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.SalamanderHitDice)
            };
            unit.Body = NaturalBody(spear, Array.Empty<BlueprintItemWeapon>(),
                new[] { tail });
            unit.Brain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "bounded natural-attack brain");
            ConfigureUnitCore(unit, "Salamander", "Salamander",
                Alignment.ChaoticEvil, Size.Medium,
                ExpandedSummoningSpecialProfiles.SalamanderStrength,
                ExpandedSummoningSpecialProfiles.SalamanderDexterity,
                ExpandedSummoningSpecialProfiles.SalamanderConstitution,
                ExpandedSummoningSpecialProfiles.SalamanderIntelligence,
                ExpandedSummoningSpecialProfiles.SalamanderWisdom,
                ExpandedSummoningSpecialProfiles.SalamanderCharisma,
                ExpandedSummoningSpecialProfiles.SalamanderSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    NaturalArmor7Guid, "natural armor +7"),
                Feature(library, DrMagic10Guid, "DR 10/magic"),
                Feature(library, FireSubtypeGuid, "fire subtype"),
                Feature(library, ExtraplanarSubtypeGuid, "extraplanar subtype"),
                Feature(library, WeaponFocusSpearGuid, "Weapon Focus (spear)"),
                combatTraits
            };
        }

        private static void ConfigureSuccubusDomination(
            LibraryScriptableObject library, BlueprintBuff buff)
        {
            BlueprintBuff native = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativeDominationGuid,
                    "native Dominate Person target state");
            CopyFields(native, buff);
            buff.name = InternalName(SuccubusDominationSymbol);
            var components = (native.ComponentsArray ??
                Array.Empty<BlueprintComponent>()).Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToList();
            components.Add(ScriptableObject.CreateInstance<
                RemoveBuffIfCasterIsMissing>());
            buff.ComponentsArray = components.ToArray();
            buff.ResourceAssetIds = Array.Empty<string>();
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.Domination.Name",
                    "Succubus Domination"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.Domination.Description",
                    "A bounded domination effect that ends when its summoned caster is missing."),
                native.Icon);
        }

        private static void ConfigureSuccubusDominate(
            LibraryScriptableObject library, BlueprintAbility ability,
            BlueprintBuff domination)
        {
            BlueprintAbility native = BlueprintLibraryLookup.RequireExact<
                BlueprintAbility>(library, NativeDominateGuid,
                    "native Dominate Person presentation");
            ExpandedSummoningAbilityBuilder.CopyFields(native, ability);
            ability.name = InternalName(SuccubusDominateSymbol);
            ability.Type = AbilityType.SpellLike;
            ability.Hidden = false;
            ability.ActionBarAutoFillIgnored = false;
            ability.Parent = null;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            var spell = ScriptableObject.CreateInstance<SpellComponent>();
            spell.School = SpellSchool.Enchantment;
            var descriptor = ScriptableObject.CreateInstance<
                SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.MindAffecting |
                SpellDescriptor.Compulsion;
            var parameters = ScriptableObject.CreateInstance<
                ContextCalculateAbilityParams>();
            parameters.StatType = StatType.Charisma;
            parameters.ReplaceCasterLevel = true;
            parameters.CasterLevel = Simple(
                ExpandedSummoningSpecialProfiles.SuccubusHitDice);
            parameters.ReplaceSpellLevel = true;
            parameters.SpellLevel = Simple(4);
            var targets = ScriptableObject.CreateInstance<AbilityTargetHasFact>();
            targets.Inverted = true;
            targets.CheckedFacts = new BlueprintUnitFact[] {
                UnitFact(library, AberrationTypeGuid, "aberration type"),
                UnitFact(library, ConstructTypeGuid, "construct type"),
                UnitFact(library, DragonTypeGuid, "dragon type"),
                UnitFact(library, FeyTypeGuid, "fey type"),
                UnitFact(library, OutsiderTypeGuid, "outsider type"),
                UnitFact(library, PlantTypeGuid, "plant type"),
                UnitFact(library, AnimalTypeGuid, "animal type"),
                UnitFact(library, MonstrousHumanoidTypeGuid,
                    "monstrous humanoid type"),
                UnitFact(library, MagicalBeastTypeGuid, "magical beast type"),
                UnitFact(library, VerminTypeGuid, "vermin type"),
                UnitFact(library, UndeadTypeGuid, "undead type")
            };
            var apply = new ContextActionApplyBuff {
                Buff = domination,
                DurationValue = new ContextDurationValue {
                    Rate = DurationRate.Rounds,
                    BonusValue = ExpandedSummoningSpecialProfiles
                        .SuccubusDominateRounds
                },
                IsNotDispelable = false
            };
            var saved = new ContextActionConditionalSaved {
                Failed = new ActionList { Actions = new GameAction[] { apply } },
                Succeed = new ActionList { Actions = Array.Empty<GameAction>() }
            };
            var effect = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            effect.SavingThrowType = SavingThrowType.Will;
            effect.Actions = new ActionList { Actions = new GameAction[] { saved } };
            ability.ComponentsArray = new BlueprintComponent[] {
                spell, descriptor, parameters, targets, effect
            };
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.Dominate.Name",
                    "Dominate Person"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.Dominate.Description",
                    "A humanoid that fails a Will save is dominated for 3 rounds. The effect ends immediately if the summoned succubus is gone."),
                native.Icon);
        }

        private static void ConfigureSuccubusAi(BlueprintAiCastSpell ai,
            BlueprintAbility dominate, BlueprintBrain brain)
        {
            ai.name = InternalName(SuccubusDominateAiSymbol);
            ai.Ability = dominate;
            ai.Variant = null;
            ai.BaseScore = 2;
            ai.CooldownRounds = 1;
            ai.StartCooldownRounds = 0;
            ai.ActorConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.TargetConsiderations = Array.Empty<Kingmaker.Controllers.Brain
                .Blueprints.Considerations.Consideration>();
            ai.Locators = Array.Empty<EntityReference>();
            brain.name = InternalName(SuccubusBrainSymbol);
            brain.Actions = new BlueprintAiAction[] { ai };
        }

        private static void ConfigureSuccubusCombatTraits(
            LibraryScriptableObject library, BlueprintBuff buff)
        {
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(ExpandedSummoningSpecialProfiles
                .SuccubusDamageReduction);
            dr.Or = true;
            dr.BypassedByMaterial = true;
            dr.Material = PhysicalDamageMaterial.ColdIron;
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Good;
            var acid = Energy(DamageEnergyType.Acid,
                ExpandedSummoningSpecialProfiles.SuccubusEnergyResistance);
            var cold = Energy(DamageEnergyType.Cold,
                ExpandedSummoningSpecialProfiles.SuccubusEnergyResistance);
            var sr = ScriptableObject.CreateInstance<AddSpellResistance>();
            sr.Value = Simple(ExpandedSummoningSpecialProfiles
                .SuccubusSpellResistance);
            BlueprintFeature nativeDrain = BlueprintLibraryLookup.RequireExact<
                BlueprintFeature>(library, NativeEnergyDrainGuid,
                    "native energy-drain attack trigger");
            AddInitiatorAttackWithWeaponTrigger drain =
                (AddInitiatorAttackWithWeaponTrigger)
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(
                    nativeDrain.ComponentsArray.OfType<
                        AddInitiatorAttackWithWeaponTrigger>().Single());
            drain.OnlyOnFirstHit = true;
            ContextActionDealDamage drainAction = FindDamage(
                new BlueprintComponent[] { drain });
            drainAction.EnergyDrainType = EnergyDrainType.Temporary;
            drainAction.Duration = new ContextDurationValue {
                Rate = DurationRate.Rounds,
                BonusValue = ExpandedSummoningSpecialProfiles
                    .SuccubusEnergyDrainRounds
            };
            drainAction.Value = new ContextDiceValue {
                DiceType = DiceType.Zero,
                DiceCountValue = Simple(0),
                BonusValue = Simple(1)
            };
            buff.Stacking = StackingType.Replace;
            buff.IsClassFeature = true;
            buff.ComponentsArray = new BlueprintComponent[] {
                dr, acid, cold, sr, drain
            };
            BlueprintUnitFactAccess.Resolve().Configure(buff,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.CombatTraits.Name",
                    "Succubus Combat Traits"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Succubus.CombatTraits.Description",
                    "Demonic defenses and a first-hit energy drain that applies one temporary negative level for one round."), null);
        }

        private static void ConfigureSuccubus(LibraryScriptableObject library,
            BlueprintUnit unit, BlueprintAbility dominate,
            BlueprintBrain brain, BlueprintBuff combatTraits)
        {
            BlueprintItemWeapon claw = BlueprintLibraryLookup.RequireExact<
                BlueprintItemWeapon>(library, LargeClawGuid,
                    "Succubus 1d6 claw");
            var grant = ScriptableObject.CreateInstance<
                AddAbilityToCharacterComponent>();
            grant.Abilities = new[] { dominate };
            unit.ComponentsArray = new BlueprintComponent[] {
                OutsiderLevels(library,
                    ExpandedSummoningSpecialProfiles.SuccubusHitDice), grant
            };
            unit.Body = NaturalBody(claw, new[] { claw },
                Array.Empty<BlueprintItemWeapon>());
            unit.Brain = brain;
            ConfigureUnitCore(unit, "Succubus", "Succubus",
                Alignment.ChaoticEvil, Size.Medium,
                ExpandedSummoningSpecialProfiles.SuccubusStrength,
                ExpandedSummoningSpecialProfiles.SuccubusDexterity,
                ExpandedSummoningSpecialProfiles.SuccubusConstitution,
                ExpandedSummoningSpecialProfiles.SuccubusIntelligence,
                ExpandedSummoningSpecialProfiles.SuccubusWisdom,
                ExpandedSummoningSpecialProfiles.SuccubusCharisma,
                ExpandedSummoningSpecialProfiles.SuccubusSpeedFeet);
            unit.AddFacts = new BlueprintUnitFact[] {
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    NaturalArmor7Guid, "natural armor +7"),
                Feature(library, ChaoticSubtypeGuid, "chaotic subtype"),
                Feature(library, EvilSubtypeGuid, "evil subtype"),
                Feature(library, ExtraplanarSubtypeGuid, "extraplanar subtype"),
                Feature(library, FireImmunityGuid, "fire immunity"),
                Feature(library, ElectricityImmunityGuid, "electricity immunity"),
                Feature(library, PoisonImmunityGuid, "poison immunity"),
                Feature(library, DodgeGuid, "Dodge"),
                Feature(library, WeaponFinesseGuid, "Weapon Finesse"),
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

        private static BlueprintUnitFact UnitFact(
            LibraryScriptableObject library, string guid, string purpose)
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

        private static void SetField(object target, string name, object value)
        {
            FieldInfo field = Fields(target.GetType()).SingleOrDefault(candidate =>
                candidate.Name == name);
            if (field == null) throw new MissingFieldException(
                target.GetType().FullName, name);
            field.SetValue(target, value);
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
