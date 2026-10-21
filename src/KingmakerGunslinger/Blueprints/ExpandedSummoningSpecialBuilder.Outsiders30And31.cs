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
            traits.ComponentsArray = new BlueprintComponent[] {
                dr, immunity, Energy(DamageEnergyType.Acid, 10),
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
            var claw = BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(
                library, NativeClaw1d4Guid, "native Medium 1d4 claw");
            // Native UnitAttack skips the primary attack when hands are
            // disabled. Reuse the established natural-body path: one primary
            // claw plus one additional claw, with no synthetic empty-hand grant.
            unit.Body = NaturalBody(claw, new[] { claw },
                Array.Empty<BlueprintItemWeapon>());
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
    }
}
