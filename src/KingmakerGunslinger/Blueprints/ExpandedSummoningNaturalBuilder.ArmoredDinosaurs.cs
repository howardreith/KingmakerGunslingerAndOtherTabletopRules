using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Utility;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningNaturalBuilder
    {
        private static void ConfigureArmoredDinosaurs(
            LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            BlueprintItemWeapon gore = Require<BlueprintItemWeapon>(bySymbol,
                ArmoredDinosaurRulesPolicy.TriceratopsGoreSymbol);
            BlueprintItemWeapon tail = Require<BlueprintItemWeapon>(bySymbol,
                ArmoredDinosaurRulesPolicy.StegosaurusTailSymbol);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                    BlueprintItemWeapon>(library, NativeMastodonGoreGuid,
                        "native mastodon gore animation weapon"), gore,
                ArmoredDinosaurRulesPolicy.TriceratopsGoreSymbol, 2,
                Kingmaker.RuleSystem.DiceType.D10);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<
                    BlueprintItemWeapon>(library, NativeTail1d8Guid,
                        "native animated tail weapon"), tail,
                ArmoredDinosaurRulesPolicy.StegosaurusTailSymbol, 4,
                Kingmaker.RuleSystem.DiceType.D6);
            ConfigureStegosaurusTailType(tail,
                Require<BlueprintWeaponType>(bySymbol,
                    ArmoredDinosaurRulesPolicy.StegosaurusTailTypeSymbol));

            BlueprintUnit triceratops = Require<BlueprintUnit>(bySymbol,
                "KMG.Summoning.Unit.Triceratops");
            BlueprintUnit stegosaurus = Require<BlueprintUnit>(bySymbol,
                "KMG.Summoning.Unit.Stegosaurus");
            BlueprintFeature charge = Require<BlueprintFeature>(bySymbol,
                ArmoredDinosaurRulesPolicy.TriceratopsPowerfulChargeSymbol);
            BlueprintAbility trample = Require<BlueprintAbility>(bySymbol,
                ArmoredDinosaurRulesPolicy.TriceratopsTrampleSymbol);
            BlueprintFeature trip = Require<BlueprintFeature>(bySymbol,
                ArmoredDinosaurRulesPolicy.StegosaurusTripSymbol);

            ConfigureArmoredDinosaurUnitType(
                Require<BlueprintUnitType>(bySymbol,
                    ArmoredDinosaurRulesPolicy.TriceratopsUnitTypeSymbol),
                ArmoredDinosaurRulesPolicy.TriceratopsUnitTypeSymbol,
                "Triceratops",
                "A huge horned dinosaur with a heavy frill, a powerful charge and a trampling gait.",
                new BlueprintUnitFact[] { charge, trample });
            ConfigureArmoredDinosaurUnitType(
                Require<BlueprintUnitType>(bySymbol,
                    ArmoredDinosaurRulesPolicy.StegosaurusUnitTypeSymbol),
                ArmoredDinosaurRulesPolicy.StegosaurusUnitTypeSymbol,
                "Stegosaurus",
                "A huge plated dinosaur with a small head, raised hindquarters and a long four-spiked tail.",
                new BlueprintUnitFact[] { trip });

            ConfigureArmoredDinosaurChassis(library, triceratops, gore,
                Require<BlueprintFeature>(bySymbol,
                    ArmoredDinosaurRulesPolicy.TriceratopsChassisSymbol),
                ArmoredDinosaurRulesPolicy.TriceratopsChassisSymbol,
                "Triceratops Chassis", false, true);
            ConfigureArmoredDinosaurChassis(library, stegosaurus, tail,
                Require<BlueprintFeature>(bySymbol,
                    ArmoredDinosaurRulesPolicy.StegosaurusChassisSymbol),
                ArmoredDinosaurRulesPolicy.StegosaurusChassisSymbol,
                "Stegosaurus Chassis", true, false);
            ConfigureStegosaurusTrip(stegosaurus, tail, trip);
        }

        private static void ConfigureArmoredDinosaurUnitType(
            BlueprintUnitType type, string symbol, string displayName,
            string description, BlueprintUnitFact[] signatures)
        {
            type.name = InternalName(symbol);
            type.KnowledgeStat = StatType.SkillLoreNature;
            type.Name = LocalizationService.Create(
                "KMG.ExpandedSummoning." + displayName + ".Type.Name",
                displayName);
            type.Description = LocalizationService.Create(
                "KMG.ExpandedSummoning." + displayName +
                ".Type.Description", description);
            type.Image = null;
            type.SignatureAbilities = signatures ??
                Array.Empty<BlueprintUnitFact>();
        }

        private static void ConfigureStegosaurusTailType(
            BlueprintItemWeapon tail, BlueprintWeaponType target)
        {
            if (tail == null || tail.Type == null || target == null)
                throw new InvalidOperationException(
                    "Stegosaurus tail type inputs are incomplete.");
            CopyFields(tail.Type, target);
            target.name = InternalName(
                ArmoredDinosaurRulesPolicy.StegosaurusTailTypeSymbol);
            SetField(target, "m_AttackRange", new Feet(
                ArmoredDinosaurRulesPolicy.
                    StegosaurusTailTypeStoredRangeFeet));
            SetField(tail, "m_Type", target);
            if (target.AttackRange.Value != 5 ||
                !ReferenceEquals(tail.Type, target))
                throw new InvalidOperationException(
                    "Stegosaurus private tail type did not expose five feet before body reach.");
        }

        private static void ConfigureArmoredDinosaurChassis(
            LibraryScriptableObject library, BlueprintUnit owner,
            BlueprintItemWeapon weapon, BlueprintFeature feature,
            string symbol, string displayName, bool needsPrivateWeaponFocus,
            bool completesSkillFocus)
        {
            var components = new List<BlueprintComponent>();
            var reach = ScriptableObject.CreateInstance<AddStatBonus>();
            reach.Stat = StatType.Reach;
            reach.Value = ArmoredDinosaurRulesPolicy.ReachBonusFeet;
            reach.Descriptor = ModifierDescriptor.UntypedStackable;
            components.Add(reach);

            if (completesSkillFocus)
            {
                var perception = ScriptableObject.CreateInstance<AddStatBonus>();
                perception.Stat = StatType.SkillPerception;
                perception.Value = ArmoredDinosaurRulesPolicy
                    .TriceratopsSkillFocusCompletionBonus;
                // This is the second half of one feat, not a second feat
                // bonus.  UntypedStackable lets it coexist with Kingmaker's
                // fixed +3 native Skill Focus component.
                perception.Descriptor = ModifierDescriptor.UntypedStackable;
                components.Add(perception);
            }

            if (needsPrivateWeaponFocus)
            {
                var focus = ScriptableObject.CreateInstance<WeaponFocus>();
                focus.WeaponType = weapon.Type;
                focus.AttackBonus = 1;
                components.Add(focus);
            }

            BlueprintActivatableAbility powerAttack = BlueprintLibraryLookup
                .RequireExact<BlueprintActivatableAbility>(library,
                    "a7b339e4f6ff93a4697df5d7a87ff619",
                    "native Power Attack toggle");
            if (powerAttack.Buff == null)
                throw new InvalidOperationException(
                    "Native Power Attack toggle has no buff.");
            var correction = ScriptableObject.CreateInstance<
                ArmoredDinosaurPowerAttackCorrection>();
            correction.OwningBlueprint = owner;
            correction.Weapon = weapon;
            correction.PowerAttackBuff = powerAttack.Buff;
            correction.AdditionalDamage =
                ArmoredDinosaurRulesPolicy.PowerAttackAdditionalDamage;
            components.Add(correction);

            feature.name = InternalName(symbol);
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning." + displayName + ".Name",
                    displayName),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning." + displayName +
                    ".Description",
                    "Carries the adult creature's fifteen-foot reach and exact single-primary-weapon combat adjustments."),
                null);
        }

        private static void ConfigureStegosaurusTrip(BlueprintUnit owner,
            BlueprintItemWeapon tail, BlueprintFeature feature)
        {
            var rider = ScriptableObject.CreateInstance<StegosaurusTailTrip>();
            rider.name = "$KMG_Stegosaurus_TailTrip";
            rider.OwningBlueprint = owner;
            rider.Tail = tail;
            feature.name = InternalName(
                ArmoredDinosaurRulesPolicy.StegosaurusTripSymbol);
            feature.IsClassFeature = false;
            feature.HideInUI = false;
            feature.ComponentsArray = new BlueprintComponent[] { rider };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Stegosaurus.Trip.Name",
                    "Trip (Ex)"),
                LocalizationService.Create(
                    "KMG.ExpandedSummoning.Stegosaurus.Trip.Description",
                    "When this stegosaurus hits with its spiked tail, it makes one native trip combat maneuver against that creature. Failure never trips the stegosaurus in return."),
                null);
        }
    }
}
