using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningNaturalBuilder
    {
        private static void ConfigureAnkylosaurus(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var type = Require<BlueprintUnitType>(bySymbol, AnkylosaurusRulesPolicy.UnitTypeSymbol);
            type.name = InternalName(AnkylosaurusRulesPolicy.UnitTypeSymbol);
            type.KnowledgeStat = StatType.SkillLoreNature;
            type.Name = LocalizationService.Create("KMG.ExpandedSummoning.Ankylosaurus.Type.Name", "Ankylosaurus");
            type.Description = LocalizationService.Create("KMG.ExpandedSummoning.Ankylosaurus.Type.Description", "A huge armored dinosaur with a broad plated back and a heavy tail club.");
            type.Image = null;
            var feature = Require<BlueprintFeature>(bySymbol, AnkylosaurusRulesPolicy.StunFeatureSymbol);
            type.SignatureAbilities = new BlueprintUnitFact[] { feature };
            var dazed = Require<BlueprintBuff>(bySymbol, AnkylosaurusRulesPolicy.DazedSymbol);
            var stunned = Require<BlueprintBuff>(bySymbol, AnkylosaurusRulesPolicy.StunnedSymbol);
            ConfigureAnkylosaurusCondition(library, dazed, "d2e35b870e4ac574d9873b36402487e5",
                AnkylosaurusRulesPolicy.DazedSymbol, UnitCondition.Dazed, SpellDescriptor.Daze,
                "Dazed", "Dazed for one round by an ankylosaurus tail strike.");
            ConfigureAnkylosaurusCondition(library, stunned, "09d39b38bb7c6014394b6daced9bacd3",
                AnkylosaurusRulesPolicy.StunnedSymbol, UnitCondition.Stunned, SpellDescriptor.Stun,
                "Stunned", "Stunned for 1d4 rounds by a confirmed critical ankylosaurus tail strike.");
            var handler = ScriptableObject.CreateInstance<AnkylosaurusTailControl>();
            handler.name = "$KMG_Ankylosaurus_Stun";
            handler.OwningBlueprint = Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit.Ankylosaurus");
            handler.AnimalClass = BlueprintLibraryLookup.RequireExact<BlueprintCharacterClass>(library, AnimalClassGuid, "animal racial HD");
            handler.Tail = Require<BlueprintItemWeapon>(bySymbol, Tail3d6Symbol);
            handler.Dazed = dazed; handler.Stunned = stunned;
            feature.name = InternalName(AnkylosaurusRulesPolicy.StunFeatureSymbol);
            feature.IsClassFeature = false; feature.HideInUI = false;
            feature.ComponentsArray = new BlueprintComponent[] { handler };
            BlueprintUnitFactAccess.Resolve().Configure(feature,
                LocalizationService.Create("KMG.ExpandedSummoning.Ankylosaurus.Stun.Name", "Stun (Ex)"),
                LocalizationService.Create("KMG.ExpandedSummoning.Ankylosaurus.Stun.Description",
                    "A creature struck by this ankylosaurus's tail must make a Fortitude save (DC 10 + half its animal hit dice + its current Strength modifier). Failure dazes the creature for one round. On a confirmed critical hit, failure stuns it for 1d4 rounds instead. A hit absorbed by damage reduction still qualifies."), null);
        }

        private static void ConfigureAnkylosaurusCondition(LibraryScriptableObject library,
            BlueprintBuff target, string nativeGuid, string symbol, UnitCondition condition,
            SpellDescriptor descriptor, string name, string description)
        {
            var native = BlueprintLibraryLookup.RequireExact<BlueprintBuff>(library, nativeGuid, "native " + name);
            var component = native.GetComponents<AddCondition>().Single(value => value.Condition == condition);
            CopyFields(native, target);
            target.name = InternalName(symbol);
            target.IsClassFeature = false;
            // Stack creates independent finite instances; Replace and Summ
            // would destroy another source or add durations respectively.
            target.Stacking = StackingType.Stack;
            var descriptors = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptors.Descriptor = descriptor;
            target.ComponentsArray = new BlueprintComponent[] {
                ExpandedSummoningAbilityBuilder.DeepCloneComponent(component), descriptors
            };
            BlueprintUnitFactAccess.Resolve().Configure(target,
                LocalizationService.Create("KMG.ExpandedSummoning.Ankylosaurus." + name + ".Name", name),
                LocalizationService.Create("KMG.ExpandedSummoning.Ankylosaurus." + name + ".Description", description),
                native.Icon);
        }
    }
}
