using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Satyr32Prefix = "KMG.Summoning.FeyCelestial.Satyr.";
        // Hidden early-native slice: native adult archer shell and one-use Fear.
        // The complete melee routine and at-will graph remain acceptance work;
        // this slice must not publish or be described as a complete Satyr.
        private static void ConfigureSatyr32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol)
        {
            var unit = Require<BlueprintUnit>(bySymbol, "KMG.Summoning.Unit.Satyr");
            var donor = BlueprintLibraryLookup.RequireExact<BlueprintUnit>(library,
                "6114adae73a1338498412b3c63c946f8", "captured adult Satyr archer");
            var nativeLevels = donor.GetComponent<AddClassLevels>();
            if (nativeLevels == null || nativeLevels.Levels != 8 ||
                nativeLevels.CharacterClass?.AssetGuid != FeyClassGuid)
                throw new InvalidOperationException("Captured Satyr Fey8 contract changed.");
            var levels = (AddClassLevels)ExpandedSummoningAbilityBuilder.DeepCloneComponent(nativeLevels);
            levels.Skills = Array.Empty<StatType>();
            levels.Selections = Array.Empty<SelectionEntry>();
            levels.SelectSpells = Array.Empty<BlueprintAbility>();
            levels.MemorizeSpells = Array.Empty<BlueprintAbility>();
            levels.LevelsStat = StatType.Unknown;
            var racial = ScriptableObject.CreateInstance<SummonGiantRacialProfile>();
            racial.OwningBlueprint = unit; racial.BaseRacialHitPoints = 28;
            racial.BaseFortitude = 2; racial.BaseReflex = 6; racial.BaseWill = 6;
            racial.PerceptionRanks = 8; racial.PersuasionRanks = 8;
            racial.StealthRanks = 8; racial.MobilityRanks = 8;
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial };
            unit.Skills = new BlueprintUnit.UnitSkills(); unit.Race = null;
            ConfigureUnitCore(unit, "Satyr", "Satyr", Alignment.ChaoticNeutral,
                Size.Medium, 14, 15, 15, 12, 14, 19, 40);
            var traits = Require<BlueprintFeature>(bySymbol, Satyr32Prefix + "Traits");
            traits.name = InternalName(Satyr32Prefix + "Traits"); traits.HideInUI = true;
            var fear = Require<BlueprintAbility>(bySymbol, Satyr32Prefix + "Fear");
            var resource = Require<BlueprintAbilityResource>(bySymbol, Satyr32Prefix + "FearResource");
            ConfigureNamedResource(resource, Satyr32Prefix + "FearResource",
                Satyr32Prefix + "FearResource", "Fear", "One use for this Satyr summoning.", 1);
            Sprint29LimitedNativeSpell(library, fear, Satyr32Prefix + "Fear",
                "d2aeac47450c76347aebbc02e4f463e0", 8, 4, resource, "Fear");
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit; parameters.Abilities = new[] { fear };
            parameters.SpellLevels = new[] { 4 }; parameters.CasterLevel = 8;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit;
            equipment.GreatWeapon = unit.Body.PrimaryHand as BlueprintItemWeapon;
            if (equipment.GreatWeapon?.AssetGuid != "6d40d84e239bdf345b349ff52e3c00a9")
                throw new InvalidOperationException("Captured Satyr ordinary shortbow changed.");
            traits.ComponentsArray = new BlueprintComponent[] { AddResource(resource), parameters, equipment };
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Satyr32Prefix + "Traits.Name", "Satyr Traits"),
                LocalizationService.Create(Satyr32Prefix + "Traits.Description",
                    "DR5/cold iron. Fear once per summoning at caster level8, Charisma-based DC. No pipes or nested summoning."), null);
            unit.AddFacts = new BlueprintUnitFact[] {
                Feature(library, "018af8005220ac94a9a4f47b3e9c2b4e", "fey type"),
                Feature(library, "7661741dbb9604842a642457456fd0e4", "natural armor5"),
                Feature(library, "90e54424d682d104ab36436bd527af09", "Weapon Finesse"),
                Feature(library, "97e216dbb46ae3c4faef90cf6bbe6fd5", "Dodge"),
                Feature(library, "a3a24fe738fc836479d6f4e9a8b84bc4", "DR5/cold iron"),
                traits, fear
            };
            var ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Satyr32Prefix + "FearAi"), Satyr32Prefix + "FearAi", fear, 0);
            var eligibility = Require<SatyrControlEligibility>(bySymbol, Satyr32Prefix + "FearEligibility");
            eligibility.OwningBlueprint = unit; eligibility.Ability = fear; eligibility.FearCone = true;
            ai.TargetConsiderations = new Consideration[] { eligibility }; ai.BaseScore = 5;
            var nativeBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library,
                DumbBrainGuid, "ordinary native attack fallback");
            var brain = Require<BlueprintBrain>(bySymbol, Satyr32Prefix + "Brain");
            CopyFields(nativeBrain, brain); brain.name = InternalName(Satyr32Prefix + "Brain");
            brain.ComponentsArray = (nativeBrain.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            brain.Actions = (nativeBrain.Actions ?? Array.Empty<BlueprintAiAction>())
                .Concat(new BlueprintAiAction[] { ai }).ToArray();
            unit.Brain = brain;
        }
    }
}
