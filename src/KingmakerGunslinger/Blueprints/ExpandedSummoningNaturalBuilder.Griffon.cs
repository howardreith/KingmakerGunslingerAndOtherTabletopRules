using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningNaturalBuilder
    {
        private static void ConfigureGriffon(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            const string prefix = "KMG.Summoning.Natural.Griffon.";
            var unit = Require<BlueprintUnit>(bySymbol,"KMG.Summoning.Unit.Griffon");
            var native = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library,
                "da807351572c540478b703957d863e68", "measured native Manticore animation donor");
            var primary = native.Body.AdditionalLimbs[0];
            var claw = native.Body.AdditionalLimbs[1];
            var components = new List<BlueprintComponent>();
            foreach (var row in new[] { new[] { "Bite1d6", "6" }, new[] { "Talon1d6", "6" }, new[] { "Rake1d4", "4" } })
            {
                var weapon=Require<BlueprintItemWeapon>(bySymbol,prefix+row[0]);
                ConfigureWeapon(row[0]=="Bite1d6"?primary:claw,weapon,prefix+row[0],1,row[1]=="6"?DiceType.D6:DiceType.D4);
                var strength=ScriptableObject.CreateInstance<MagicalBeastPrimaryStrength>();
                strength.OwningBlueprint=unit;strength.Weapon=weapon;
                components.Add(strength);
            }
            foreach (var stat in new[] { StatType.SkillMobility, StatType.SkillPerception })
            {
                var racial=ScriptableObject.CreateInstance<AddStatBonus>();
                racial.Stat=stat;racial.Value=4;racial.Descriptor=ModifierDescriptor.Racial;
                components.Add(racial);
            }
            var chassis=Require<BlueprintFeature>(bySymbol,prefix+"Chassis");
            chassis.name=InternalName(prefix+"Chassis");chassis.HideInUI=true;chassis.IsClassFeature=false;
            chassis.ComponentsArray=components.ToArray();NameMagicalBeastRuntimeComponents(chassis);
            BlueprintUnitFactAccess.Resolve().Configure(chassis,
                LocalizationService.Create(prefix+"Chassis.Name","Griffon Natural Attacks"),
                LocalizationService.Create(prefix+"Chassis.Description","Each primary natural weapon adds live Strength once; racial Acrobatics and Perception bonuses remain racial modifiers."),null);
            ConfigureTheropodUnitType(Require<BlueprintUnitType>(bySymbol,prefix+"UnitType"),prefix+"UnitType","Griffon",
                "A winged lion-bodied magical beast with eagle forequarters. Pounce and conditional rake; no automatic grab. Ordinary-map flight uses 80 feet; separate land mode, Fly checks and passive senses are unmodeled.",Array.Empty<BlueprintUnitFact>());
            // Preserve the installed magical-beast lore consumer, rather than
            // inheriting the animal helper's default or inventing a new mapping.
            Require<BlueprintUnitType>(bySymbol,prefix+"UnitType").KnowledgeStat=native.Type.KnowledgeStat;
        }
    }
}
