using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private const string NativeBleed2d6Guid =
            "16249b8075ab8684ca105a78a047a5ef";

        private static void ConfigureTheropodSpecials(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            TheropodRulesPolicy.Validate();
            BlueprintUnit unit = Require<BlueprintUnit>(bySymbol,
                "KMG.Summoning.Unit.Tyrannosaurus");
            BlueprintItemWeapon bite = Require<BlueprintItemWeapon>(bySymbol,
                TheropodRulesPolicy.TyrannosaurusBiteSymbol);
            if (unit.Body == null || unit.Body.PrimaryHand == null ||
                !ReferenceEquals((BlueprintItemWeapon)unit.Body.PrimaryHand,
                    bite) || bite.Type == null ||
                bite.Type.DamageType.Type != DamageType.Physical ||
                (bite.Type.DamageType.Physical.Form &
                    (PhysicalDamageForm.Piercing |
                     PhysicalDamageForm.Slashing)) == 0)
                throw new InvalidOperationException(
                    "Tyrannosaurus requires its exact piercing/slashing " +
                    "primary bite before special mechanics are configured.");

            ConfigurePowerfulBite(Require<BlueprintFeature>(bySymbol,
                TheropodRulesPolicy.TyrannosaurusPowerfulBiteSymbol),
                unit, bite);
            ConfigureBleedingCritical(library,
                Require<BlueprintFeature>(bySymbol,
                    TheropodRulesPolicy.TyrannosaurusBleedingCriticalSymbol),
                unit, bite);

            BlueprintBuff swallowed = Require<BlueprintBuff>(bySymbol,
                TheropodRulesPolicy.TyrannosaurusSwallowedSymbol);
            ConfigureTyrannosaurusSwallowedState(library, swallowed);
            ConfigureGrabber(library, bySymbol,
                "KMG.Summoning.Unit.Tyrannosaurus",
                TheropodRulesPolicy.TyrannosaurusCombatTraitsSymbol,
                "Tyrannosaurus",
                "Tyrannosaurus Grab and Swallow Whole",
                "An exact bite hit lets the tyrannosaurus attempt to grab " +
                "a foe its own size or smaller with +4 on the grapple check. " +
                "A foe at least one size category smaller that it began the " +
                "later round holding in its mouth is swallowed only after a " +
                "new successful check; periodic interior damage is 2d8+11.",
                new GrabSpec {
                    Primary = true,
                    MaxSizeDelta =
                        TheropodRulesPolicy
                            .TyrannosaurusGrabMaximumTargetSizeDelta,
                    Hold = Require<BlueprintBuff>(bySymbol,
                        GrappleHoldSymbol),
                    Grappled = Require<BlueprintBuff>(bySymbol,
                        GrappleGrappledSymbol),
                    Swallowed = swallowed,
                    SwallowDelta =
                        TheropodRulesPolicy
                            .TyrannosaurusSwallowMaximumTargetSizeDelta,
                    ReleaseSwallowedOnVictimDeath = true
                });
        }

        private static void ConfigurePowerfulBite(BlueprintFeature feature,
            BlueprintUnit owner, BlueprintItemWeapon bite)
        {
            var component = ScriptableObject.CreateInstance<
                TyrannosaurusPowerfulBite>();
            component.OwningBlueprint = owner;
            component.Bite = bite;
            feature.name = InternalName(
                TheropodRulesPolicy.TyrannosaurusPowerfulBiteSymbol);
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.ComponentsArray = new BlueprintComponent[] { component };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Tyrannosaurus.PowerfulBite.Name",
                    "Powerful Bite"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Tyrannosaurus.PowerfulBite.Description",
                    "A tyrannosaurus applies twice its current Strength " +
                    "modifier to damage with its bite."),
                bite.Icon);
        }

        private static void ConfigureBleedingCritical(
            LibraryScriptableObject library, BlueprintFeature feature,
            BlueprintUnit owner, BlueprintItemWeapon bite)
        {
            BlueprintBuff bleed = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativeBleed2d6Guid,
                    "native 2d6 bleed payload");
            var component = ScriptableObject.CreateInstance<
                TyrannosaurusBleedingCritical>();
            component.OwningBlueprint = owner;
            component.Bite = bite;
            component.Bleed = bleed;
            feature.name = InternalName(
                TheropodRulesPolicy.TyrannosaurusBleedingCriticalSymbol);
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.ComponentsArray = new BlueprintComponent[] { component };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Tyrannosaurus.BleedingCritical.Name",
                    "Bleeding Critical"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Tyrannosaurus.BleedingCritical.Description",
                    "A confirmed critical hit with the tyrannosaurus's bite " +
                    "causes 2d6 bleed damage. The native bleed state owns " +
                    "its stacking, healing and combat-end cleanup."),
                bleed.Icon);
        }

        private static void ConfigureTyrannosaurusSwallowedState(
            LibraryScriptableObject library, BlueprintBuff swallowed)
        {
            BlueprintBuff nativeSwallowed = BlueprintLibraryLookup.RequireExact<
                BlueprintBuff>(library, NativePurpleWormSwallowedGuid,
                    "native purple worm swallowed state");
            BlueprintComponent[] native = nativeSwallowed.ComponentsArray ??
                Array.Empty<BlueprintComponent>();
            AddStatBonus[] penalties = native.OfType<AddStatBonus>().ToArray();
            if (native.Length != 3 ||
                    native.OfType<AddFactContextActions>().Count() != 1 ||
                    penalties.Length != 2 ||
                    !penalties.Any(value =>
                        value.Stat == StatType.AdditionalCMB &&
                        value.Value == -2) ||
                    !penalties.Any(value =>
                        value.Stat == StatType.Dexterity &&
                        value.Value == -4) ||
                    penalties.Any(value =>
                        value.Descriptor !=
                            ModifierDescriptor.UntypedStackable ||
                        value.ScaleByBasicAttackBonus))
                throw new InvalidOperationException(
                    "The native swallowed-state non-damage contract changed; " +
                    "re-audit it before constructing Tyrannosaurus swallow.");

            var crush = ScriptableObject.CreateInstance<
                ContextActionDealDamage>();
            crush.DamageType = new DamageTypeDescription {
                Type = DamageType.Physical,
                Physical = new DamageTypeDescription.PhysicalData {
                    Form = PhysicalDamageForm.Bludgeoning }
            };
            crush.Value = new ContextDiceValue {
                DiceType = DiceType.D8,
                DiceCountValue = Simple(
                    TheropodRulesPolicy.TyrannosaurusSwallowDiceCount),
                BonusValue = Simple(
                    TheropodRulesPolicy.TyrannosaurusSwallowFlatDamage)
            };
            crush.IgnoreCritical = true;
            var cadence = ScriptableObject.CreateInstance<
                AddFactContextActions>();
            cadence.Activated = new ActionList {
                Actions = Array.Empty<GameAction>() };
            cadence.Deactivated = new ActionList {
                Actions = Array.Empty<GameAction>() };
            cadence.NewRound = new ActionList {
                Actions = new GameAction[] { crush } };

            swallowed.name = InternalName(
                TheropodRulesPolicy.TyrannosaurusSwallowedSymbol);
            swallowed.Stacking = StackingType.Replace;
            swallowed.IsClassFeature = false;
            swallowed.ComponentsArray = penalties.Select(
                    ExpandedSummoningAbilityBuilder.DeepCloneComponent)
                .Concat(new BlueprintComponent[] { cadence }).ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(swallowed,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Tyrannosaurus.Swallowed.Name",
                    "Swallowed Whole"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Tyrannosaurus.Swallowed.Description",
                    "Swallowed by a summoned tyrannosaurus: crushed for " +
                    "2d8+11 each round, unable to act, with a break-free " +
                    "attempt each round. Kingmaker does not model the " +
                    "printed interior AC 17 or 15 hit points."),
                null);
        }
    }
}
