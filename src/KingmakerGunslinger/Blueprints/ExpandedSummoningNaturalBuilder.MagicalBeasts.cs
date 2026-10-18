using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningNaturalBuilder
    {
        private static void ConfigureSprint26MagicalBeasts(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            const string natural = "KMG.Summoning.Natural.";
            var gore = Require<BlueprintItemWeapon>(bySymbol, natural + "Gorgon.Gore2d8");
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(library,
                NativeGore1d8Guid, "native gore"), gore, natural + "Gorgon.Gore2d8", 2, DiceType.D8);
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(library,
                NativeHoof1d4Guid, "native hoof"), Require<BlueprintItemWeapon>(bySymbol,
                    natural + "Gorgon.Hoof1d6"), natural + "Gorgon.Hoof1d6", 1, DiceType.D6);
            var bite = Require<BlueprintItemWeapon>(bySymbol, natural + "Bulette.Bite2d8");
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(library,
                "d889d3f8edd9ee54a9623c8127b0546f", "native animated bulette bite"),
                bite, natural + "Bulette.Bite2d8", 2, DiceType.D8);
            // A private type preserves the native Bulette attack animation while
            // carrying the printed threat range without granting Improved Critical.
            var biteType = Require<BlueprintWeaponType>(bySymbol, natural + "Bulette.BiteType");
            CopyFields(bite.Type, biteType);
            biteType.name = InternalName(natural + "Bulette.BiteType");
            SetField(biteType, "m_CriticalRollEdge", 19);
            SetField(bite, "m_Type", biteType);
            var claw = Require<BlueprintItemWeapon>(bySymbol, natural + "Bulette.Claw2d6");
            ConfigureWeapon(BlueprintLibraryLookup.RequireExact<BlueprintItemWeapon>(library,
                "c47ddfd181931764aa508b7b5aa27710", "native animated bulette claw"),
                claw, natural + "Bulette.Claw2d6", 2, DiceType.D6);

            ConfigureMagicalBeastChassis(Require<BlueprintFeature>(bySymbol,
                natural + "Gorgon.Chassis"), natural + "Gorgon.Chassis",
                Require<BlueprintUnit>(bySymbol, Sprint26MagicalBeastRules.GorgonUnit), gore, null);
            ConfigureMagicalBeastChassis(Require<BlueprintFeature>(bySymbol,
                natural + "Bulette.Chassis"), natural + "Bulette.Chassis",
                Require<BlueprintUnit>(bySymbol, Sprint26MagicalBeastRules.BuletteUnit), bite, claw);
            ConfigureTheropodUnitType(Require<BlueprintUnitType>(bySymbol,
                natural + "Gorgon.UnitType"), natural + "Gorgon.UnitType", "Gorgon",
                "A living armored bull with petrifying breath. This charter-selected Summon Monster VIII addition is not a core-table entry.",
                new BlueprintUnitFact[] { Require<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>(
                    bySymbol, Sprint26MagicalBeastRules.GorgonPrefix + "Breath") });
            ConfigureTheropodUnitType(Require<BlueprintUnitType>(bySymbol,
                natural + "Bulette.UnitType"), natural + "Bulette.UnitType", "Bulette",
                "An armored landshark. Its surface-only leap follows legal charge paths; burrowing and tremorsense are unmodeled.",
                new BlueprintUnitFact[] { Require<Kingmaker.UnitLogic.Abilities.Blueprints.BlueprintAbility>(
                    bySymbol, Sprint26MagicalBeastRules.BulettePrefix + "Leap") });
        }

        private static void ConfigureMagicalBeastChassis(BlueprintFeature feature,
            string symbol, BlueprintUnit owner, BlueprintItemWeapon primary,
            BlueprintItemWeapon claw)
        {
            var strength = ScriptableObject.CreateInstance<MagicalBeastPrimaryStrength>();
            strength.OwningBlueprint = owner;
            strength.Weapon = primary;
            strength.SavageBite = claw != null;
            var components = new List<BlueprintComponent> { strength };
            if (claw == null)
            {
                var armor = ScriptableObject.CreateInstance<Kingmaker.UnitLogic.FactLogic.AddStatBonus>();
                armor.Stat = Kingmaker.EntitySystem.Stats.StatType.AC;
                armor.Value = 11;
                armor.Descriptor = Kingmaker.Enums.ModifierDescriptor.NaturalArmor;
                components.Add(armor);
            }
            if (claw != null)
            {
                var clawStrength = ScriptableObject.CreateInstance<MagicalBeastPrimaryStrength>();
                clawStrength.OwningBlueprint = owner;
                clawStrength.Weapon = claw;
                components.Add(clawStrength);
                // The printed native feat remains granted. It references generic
                // BiteType, so its +1 must also bind this private Bulette type.
                var focus = ScriptableObject.CreateInstance<WeaponFocus>();
                focus.WeaponType = primary.Type;
                focus.AttackBonus = 1;
                components.Add(focus);
            }
            feature.name = InternalName(symbol);
            feature.IsClassFeature = false;
            feature.HideInUI = true;
            feature.ComponentsArray = components.ToArray();
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create(symbol + ".Name", "Natural Attack Chassis"),
                LocalizationService.Create(symbol + ".Description",
                    "Carries the creature's printed natural-attack Strength contributions."), null);
        }
    }
}
