using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.TurnBasedModifiers;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private const string Lemure30Prefix = "KMG.Summoning.OutsiderCelestial.Lemure.";

        private static void ConfigureLemure30(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintFeature extraplanar)
        {
            var unit = Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit.Lemure");
            var type = Require<BlueprintUnitType>(bySymbol, Lemure30Prefix + "UnitType");
            var traits = Require<BlueprintFeature>(bySymbol, Lemure30Prefix + "Traits");
            type.name = InternalName(Lemure30Prefix + "UnitType");
            type.Name = LocalizationService.Create(Lemure30Prefix + "UnitType.Name", "Lemure");
            type.Image = null;
            type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            traits.name = InternalName(Lemure30Prefix + "Traits");
            traits.IsClassFeature = false;
            traits.HideInUI = true;
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(5);
            dr.Or = true;
            dr.BypassedByAlignment = true;
            dr.Alignment = DamageAlignment.Good;
            dr.BypassedByMaterial = true;
            dr.Material = PhysicalDamageMaterial.Silver;
            var immunity = ScriptableObject.CreateInstance<BuffDescriptorImmunity>();
            immunity.CheckFact = false;
            immunity.Descriptor = SpellDescriptor.MindAffecting | SpellDescriptor.Poison;
            immunity.FactToCheck = null;
            immunity.IgnoreFeature = null;
            var spellImmunity = ScriptableObject.CreateInstance<SpellImmunityToSpellDescriptor>();
            spellImmunity.Descriptor = SpellDescriptor.MindAffecting | SpellDescriptor.Poison;
            spellImmunity.CasterIgnoreImmunityFact = null;
            traits.ComponentsArray = new BlueprintComponent[] {
                dr, immunity, spellImmunity, Energy(DamageEnergyType.Acid, 10),
                Energy(DamageEnergyType.Cold, 10)
            };
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Lemure30Prefix + "Traits.Name", "Lemure Traits"),
                LocalizationService.Create(Lemure30Prefix + "Traits.Description",
                    "Mindless; immune to mind-affecting effects, poison and fire. Damage reduction 5, bypassed by good OR silver. Acid and cold resistance 10. No skill ranks or feats."), null);
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                library, OutsiderClassGuid, "Lemure outsider racial Hit Dice");
            levels.Levels = 2;
            levels.RaceStat = StatType.Constitution;
            levels.LevelsStat = StatType.Unknown;
            levels.Skills = Array.Empty<StatType>();
            levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var racial = ScriptableObject.CreateInstance<SummonLemureRacialProfile>();
            racial.OwningBlueprint = unit;
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial };
            unit.Skills = new BlueprintUnit.UnitSkills();
            unit.Race = null;
            unit.Type = type;
            var claw = Sprint29Weapon(library, bySymbol, Lemure30Prefix + "Claw1d4",
                Lemure30Prefix + "ClawType", NativeClaw1d4Guid, Size.Medium, 1, DiceType.D4, 5);
            // The installed type getter subtracts four feet for raw ranges
            // at or below ten. Nine raw feet therefore produces the printed
            // five-foot native reach; only this private type is changed.
            SetField(claw.Type, "m_AttackRange", new Feet(9));
            // Native UnitAttack skips the primary attack when hands are
            // disabled. Reuse the established natural-body path: one primary
            // claw plus one additional claw, with no synthetic empty-hand grant.
            unit.Body = NaturalBody(claw, new[] { claw },
                Array.Empty<BlueprintItemWeapon>());
            // The installed primary-hand rule otherwise assumes a lone
            // natural attack and contributes 1.5x positive Strength. Reuse
            // the exact-owner/weapon correction already qualified for a
            // multiple-primary routine; the shared component is unchanged.
            var fullStrength = ScriptableObject.CreateInstance<DeinonychusPrimaryTalonStrength>();
            fullStrength.OwningBlueprint = unit;
            fullStrength.Talon = claw;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit;
            equipment.GreatWeapon = claw;
            traits.ComponentsArray = traits.ComponentsArray.Concat(
                new BlueprintComponent[] { fullStrength, equipment }).ToArray();
            unit.Brain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(
                library, DumbBrainGuid, "unchanged native ordinary attack brain");
            // Zero is the hidden candidate's absent-score representation, not
            // an invented Intelligence of 1. Native instantiation/AI must prove
            // it remains a functioning mindless summon before further expansion.
            ConfigureUnitCore(unit, "Lemure", "Lemure", Alignment.LawfulEvil,
                Size.Medium, 11, 10, 12, 0, 11, 5, 20);
            unit.AddFacts = new BlueprintUnitFact[] {
                Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, EvilSubtypeGuid, "evil subtype"),
                Feature(library, LawfulSubtypeGuid, "lawful subtype"),
                Feature(library, FireImmunityGuid, "fire immunity"),
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    NaturalArmor4Guid, "natural armor +4"),
                extraplanar, traits
            };
        }

        private const string Dretch30Prefix = "KMG.Summoning.OutsiderCelestial.Dretch.";
        private static void ConfigureDretch30(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintFeature extraplanar)
        {
            var unit = Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit.Dretch");
            var type = Require<BlueprintUnitType>(bySymbol, Dretch30Prefix + "UnitType");
            type.name = InternalName(Dretch30Prefix + "UnitType");
            type.Name = LocalizationService.Create(Dretch30Prefix + "UnitType.Name", "Dretch");
            type.Image = null; type.SignatureAbilities = Array.Empty<BlueprintUnitFact>();
            var traits = Require<BlueprintFeature>(bySymbol, Dretch30Prefix + "Traits");
            traits.name = InternalName(Dretch30Prefix + "Traits");
            traits.IsClassFeature = false; traits.HideInUI = true;
            var dr = ScriptableObject.CreateInstance<AddDamageResistancePhysical>();
            dr.Value = Simple(5); dr.Or = true;
            dr.BypassedByAlignment = true; dr.Alignment = DamageAlignment.Good;
            dr.BypassedByMaterial = true; dr.Material = PhysicalDamageMaterial.ColdIron;
            var poison = ScriptableObject.CreateInstance<BuffDescriptorImmunity>();
            poison.CheckFact = false; poison.Descriptor = SpellDescriptor.Poison;
            var poisonAbility = ScriptableObject.CreateInstance<SpellImmunityToSpellDescriptor>();
            poisonAbility.Descriptor = SpellDescriptor.Poison;
            var armor = ScriptableObject.CreateInstance<AddStatBonus>();
            armor.Stat = StatType.AC; armor.Value = 3; armor.Descriptor = ModifierDescriptor.NaturalArmor;
            var claw = Sprint29Weapon(library, bySymbol, Dretch30Prefix + "Claw1d4",
                Dretch30Prefix + "ClawType", NativeClaw1d4Guid, Size.Small, 1, DiceType.D4, 9);
            var bite = Sprint29Weapon(library, bySymbol, Dretch30Prefix + "Bite1d4",
                Dretch30Prefix + "BiteType", NativeBite1d6Guid, Size.Small, 1, DiceType.D4, 9);
            var strength = ScriptableObject.CreateInstance<DeinonychusPrimaryTalonStrength>();
            strength.OwningBlueprint = unit; strength.Talon = claw;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit; equipment.GreatWeapon = claw; equipment.Slam = bite;
            var fear = Require<BlueprintAbility>(bySymbol, Dretch30Prefix + "CauseFear");
            var cloud = Require<BlueprintAbility>(bySymbol, Dretch30Prefix + "StinkingCloud");
            var fearResource = Require<BlueprintAbilityResource>(bySymbol, Dretch30Prefix + "CauseFearResource");
            var cloudResource = Require<BlueprintAbilityResource>(bySymbol, Dretch30Prefix + "StinkingCloudResource");
            ConfigureNamedResource(fearResource, Dretch30Prefix + "CauseFearResource",
                Dretch30Prefix + "CauseFearResource", "Cause Fear", "One use for this Dretch summoning.", 1);
            ConfigureNamedResource(cloudResource, Dretch30Prefix + "StinkingCloudResource",
                Dretch30Prefix + "StinkingCloudResource", "Stinking Cloud", "One use for this Dretch summoning.", 1);
            Sprint29LimitedNativeSpell(library, fear, Dretch30Prefix + "CauseFear",
                "bd81a3931aa285a4f9844585b5d97e51", 2, 1, fearResource, "Cause Fear");
            Sprint29LimitedNativeSpell(library, cloud, Dretch30Prefix + "StinkingCloud",
                "68a9e6d7256f1354289a39003a46d826", 2, 3, cloudResource, "Stinking Cloud");
            // The native general cloud is deliberately retained unchanged. AI
            // may center it on an enemy; every eligible creature remains affected.
            cloud.CanTargetEnemies = true;
            var spawn = FindSpawnAreaEffect(cloud);
            if (spawn?.AreaEffect?.AssetGuid != "aa2e0a0fe89693f4e9205fd52c5ba3e5")
                throw new InvalidOperationException("Dretch general native cloud area changed.");
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit; parameters.Abilities = new[] { fear, cloud };
            parameters.SpellLevels = new[] { 1, 3 }; parameters.CasterLevel = 2;
            traits.ComponentsArray = new BlueprintComponent[] { dr, poison, poisonAbility,
                Energy(DamageEnergyType.Acid, 10), Energy(DamageEnergyType.Cold, 10),
                Energy(DamageEnergyType.Fire, 10), armor, strength, equipment, parameters,
                AddResource(fearResource), AddResource(cloudResource) };
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Dretch30Prefix + "Traits.Name", "Dretch Traits"),
                LocalizationService.Create(Dretch30Prefix + "Traits.Description",
                    "Demon: immune to electricity and poison; acid, cold and fire resistance 10. DR 5/good or cold iron. Cause Fear and the all-creature Stinking Cloud each once per summoning, CL 2. No nested summoning. Reference Escape Artist +5 requires its scoped combat consumer before qualification."), null);
            var levels = ScriptableObject.CreateInstance<AddClassLevels>();
            levels.CharacterClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(
                library, OutsiderClassGuid, "Dretch outsider racial Hit Dice");
            levels.Levels = 2; levels.RaceStat = StatType.Constitution; levels.LevelsStat = StatType.Unknown;
            levels.Skills = Array.Empty<StatType>(); levels.Archetypes = Array.Empty<BlueprintArchetype>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>(); levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.Selections = Array.Empty<SelectionEntry>();
            var racial = ScriptableObject.CreateInstance<SummonDretchRacialProfile>();
            racial.OwningBlueprint = unit;
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial };
            unit.Skills = new BlueprintUnit.UnitSkills(); unit.Race = null; unit.Type = type;
            unit.Body = NaturalBody(claw, new[] { claw, bite }, Array.Empty<BlueprintItemWeapon>());
            ConfigureUnitCore(unit, "Dretch", "Dretch", Alignment.ChaoticEvil,
                Size.Small, 12, 10, 14, 5, 11, 11, 20);
            unit.AddFacts = new BlueprintUnitFact[] { Feature(library, OutsiderTypeGuid, "outsider type"),
                Feature(library, ChaoticSubtypeGuid, "chaotic subtype"), Feature(library, EvilSubtypeGuid, "evil subtype"),
                Feature(library, ElectricityImmunityGuid, "demon electricity immunity"),
                Feature(library, "d09b20029e9abfe4480b356c92095623", "Dretch Toughness"), extraplanar, traits, fear, cloud };
            var fearAi = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Dretch30Prefix + "CauseFearAi"), Dretch30Prefix + "CauseFearAi", fear, 0);
            var cloudAi = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Dretch30Prefix + "StinkingCloudAi"), Dretch30Prefix + "StinkingCloudAi", cloud, 0);
            var fearEligibility = Require<DretchSpellAiEligibility>(bySymbol, Dretch30Prefix + "CauseFearEligibility");
            fearEligibility.name = InternalName(Dretch30Prefix + "CauseFearEligibility");
            fearEligibility.OwningBlueprint = unit; fearEligibility.Ability = fear; fearEligibility.MaximumHitDice = 5;
            var cloudEligibility = Require<DretchSpellAiEligibility>(bySymbol, Dretch30Prefix + "CloudEligibility");
            cloudEligibility.name = InternalName(Dretch30Prefix + "CloudEligibility");
            cloudEligibility.OwningBlueprint = unit; cloudEligibility.Ability = cloud; cloudEligibility.MaximumHitDice = 0;
            var safety = Require<Sprint29FogAiSafety>(bySymbol, Dretch30Prefix + "CloudSafety");
            safety.name = InternalName(Dretch30Prefix + "CloudSafety");
            safety.OwningBlueprint = unit; safety.FogArea = spawn.AreaEffect; safety.CenterOnCaster = false;
            fearAi.TargetConsiderations = new Kingmaker.Controllers.Brain.Blueprints.Considerations.Consideration[] { fearEligibility };
            cloudAi.TargetConsiderations = new Kingmaker.Controllers.Brain.Blueprints.Considerations.Consideration[] { cloudEligibility, safety };
            cloudAi.BaseScore = 4; fearAi.BaseScore = 3;
            var nativeBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library, DumbBrainGuid, "native attack fallback");
            var brain = Require<BlueprintBrain>(bySymbol, Dretch30Prefix + "Brain");
            CopyFields(nativeBrain, brain); brain.name = InternalName(Dretch30Prefix + "Brain");
            brain.ComponentsArray = (nativeBrain.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            brain.Actions = (nativeBrain.Actions ?? Array.Empty<BlueprintAiAction>())
                .Concat(new BlueprintAiAction[] { cloudAi, fearAi }).ToArray();
            unit.Brain = brain;
        }
    }
}
