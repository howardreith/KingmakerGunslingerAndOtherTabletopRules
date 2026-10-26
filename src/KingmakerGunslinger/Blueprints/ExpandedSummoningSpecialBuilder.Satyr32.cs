using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        internal const string Satyr32Prefix = "KMG.Summoning.FeyCelestial.Satyr.";
        // Hidden adult profile. Native spell resolution and normal equipment
        // commands remain responsible for saves, costs and melee/ranged actions.
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
            racial.PerceptionRanks = 6; racial.PersuasionRanks = 8;
            racial.StealthRanks = 8;
            var nature = ScriptableObject.CreateInstance<FeyCelestialNatureKnowledge32>();
            nature.OwningBlueprint = unit; nature.TrainedContribution = 9; nature.FeyClassSkill = true;
            unit.ComponentsArray = new BlueprintComponent[] { levels, racial, nature };
            unit.Skills = new BlueprintUnit.UnitSkills(); unit.Race = null;
            ConfigureUnitCore(unit, "Satyr", "Satyr", Alignment.ChaoticNeutral,
                Size.Medium, 14, 15, 15, 12, 14, 19, 40);
            var bow = unit.Body.PrimaryHand as BlueprintItemWeapon;
            if (bow?.AssetGuid != "6d40d84e239bdf345b349ff52e3c00a9")
                throw new InvalidOperationException("Captured Satyr ordinary shortbow changed.");
            var dagger = Sprint29Weapon(library, bySymbol, Satyr32Prefix + "Dagger",
                Satyr32Prefix + "DaggerType", "aa514dbf4c3d61f4e9c0738bd4d373cb",
                Size.Medium, 1, DiceType.D4, 0);
            var horns = Sprint29Weapon(library, bySymbol, Satyr32Prefix + "Horns",
                Satyr32Prefix + "HornsType", "73ed4e955295e62469fe471f1d49d9ef",
                Size.Medium, 1, DiceType.D6, 0);
            unit.Body = new BlueprintUnit.UnitBody { PrimaryHand = bow,
                PrimaryHandAlternative1 = dagger, ActiveHandSet = 0,
                AdditionalLimbs = Array.Empty<BlueprintItemWeapon>(),
                AdditionalSecondaryLimbs = new[] { horns },
                QuickSlots = Array.Empty<Kingmaker.Blueprints.Items.Equipment.BlueprintItemEquipmentUsable>() };
            var traits = Require<BlueprintFeature>(bySymbol, Satyr32Prefix + "Traits");
            traits.name = InternalName(Satyr32Prefix + "Traits"); traits.HideInUI = true;
            var fear = Require<BlueprintAbility>(bySymbol, Satyr32Prefix + "Fear");
            var resource = Require<BlueprintAbilityResource>(bySymbol, Satyr32Prefix + "FearResource");
            ConfigureNamedResource(resource, Satyr32Prefix + "FearResource",
                Satyr32Prefix + "FearResource", "Fear", "One use for this Satyr summoning.", 1);
            Sprint29LimitedNativeSpell(library, fear, Satyr32Prefix + "Fear",
                "d2aeac47450c76347aebbc02e4f463e0", 8, 4, resource, "Fear");
            BindSatyrFearTerminals32(library, fear, bySymbol);
            var charm = ConfigureSatyrAtWill32(library, bySymbol, "CharmPerson",
                "1af9d5995090e5a4185a30decf0959ad", "Charm Person");
            var sleep = ConfigureSatyrAtWill32(library, bySymbol, "Sleep",
                "bb7ecad2d3d2c8247a38f44855c99061", "Sleep");
            var parameters = ScriptableObject.CreateInstance<Sprint29GiantSpellLikeParameters>();
            parameters.OwningBlueprint = unit; parameters.Abilities = new[] { fear, charm, sleep };
            parameters.SpellLevels = new[] { 4, 1, 1 }; parameters.CasterLevel = 8;
            var equipment = ScriptableObject.CreateInstance<SummonGiantEquipmentOwnership>();
            equipment.OwningBlueprint = unit;
            equipment.GreatWeapon = dagger; equipment.RangedWeapon = bow; equipment.Slam = horns;
            var proficiency = ScriptableObject.CreateInstance<Kingmaker.UnitLogic.FactLogic.AddProficiencies>();
            proficiency.WeaponProficiencies = new[] { dagger.Category, bow.Category };
            proficiency.ArmorProficiencies = Array.Empty<ArmorProficiencyGroup>();
            var perception = ScriptableObject.CreateInstance<AddStatBonus>();
            perception.Stat = StatType.SkillPerception; perception.Value = 4;
            perception.Descriptor = ModifierDescriptor.Racial;
            var stealth = ScriptableObject.CreateInstance<AddStatBonus>();
            stealth.Stat = StatType.SkillStealth; stealth.Value = 4;
            stealth.Descriptor = ModifierDescriptor.Racial;
            var intimidate = ScriptableObject.CreateInstance<SatyrIntimidateRanks32>();
            intimidate.OwningBlueprint = unit;
            traits.ComponentsArray = new BlueprintComponent[] { AddResource(resource), parameters,
                equipment, proficiency, perception, stealth, intimidate };
            BlueprintUnitFactAccess.Resolve().Configure(traits,
                LocalizationService.Create(Satyr32Prefix + "Traits.Name", "Satyr Traits"),
                LocalizationService.Create(Satyr32Prefix + "Traits.Description",
                    "DR5/cold iron. Charm Person and Sleep at will; Fear once per summoning. Caster level8 and Charisma-based DCs. Dagger and secondary horns in melee, with an ordinary shortbow alternative. No pipes or nested summoning. Dancing Lights and Ghost Sound are unavailable."), null);
            unit.AddFacts = new BlueprintUnitFact[] {
                Feature(library, "018af8005220ac94a9a4f47b3e9c2b4e", "fey type"),
                BlueprintLibraryLookup.RequireExact<BlueprintUnitFact>(library,
                    "7661741dbb9604842a642457456fd0e4", "captured native natural armor5 fact"),
                Feature(library, "90e54424d682d104ab36436bd527af09", "Weapon Finesse"),
                Feature(library, "97e216dbb46ae3c4faef90cf6bbe6fd5", "Dodge"),
                Feature(library, "2a6091b97ad940943b46262600eaeaeb", "Mobility"),
                Feature(library, "f74c6bdf5c5f5374fb9302ecdc1f7d64", "Skill Focus Perception"),
                Feature(library, "a3a24fe738fc836479d6f4e9a8b84bc4", "DR5/cold iron"),
                traits, fear, charm, sleep
            };
            var ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Satyr32Prefix + "FearAi"), Satyr32Prefix + "FearAi", fear, 0);
            var eligibility = Require<SatyrControlEligibility>(bySymbol, Satyr32Prefix + "FearEligibility");
            eligibility.OwningBlueprint = unit; eligibility.Ability = fear; eligibility.FearCone = true;
            ai.TargetConsiderations = new Consideration[] { eligibility }; ai.BaseScore = 5;
            var charmAi = ConfigureSatyrControlAi32(bySymbol, unit, "CharmPerson", charm, 4);
            Require<SatyrControlEligibility>(bySymbol, Satyr32Prefix + "CharmPersonEligibility").ControlBuffGuid =
                Require<BlueprintBuff>(bySymbol, FeyCharmBuff32).AssetGuid;
            var sleepAi = ConfigureSatyrControlAi32(bySymbol, unit, "Sleep", sleep, 4);
            var sleepEligibility = Require<SatyrControlEligibility>(bySymbol, Satyr32Prefix + "SleepEligibility");
            sleepEligibility.MaximumHitDice = 4; sleepEligibility.RejectedCondition = UnitCondition.Sleeping;
            // The captured native Sleep graph selects enemies in ten feet
            // and has one shared four-HD budget. Preserve that native mapping.
            var switches = new List<BlueprintAiAction>();
            foreach (bool ranged in new[] { false, true })
            {
                string suffix = ranged ? "Ranged" : "Melee";
                var choice = Require<SatyrWeaponChoice32>(bySymbol, Satyr32Prefix + suffix + "Eligibility");
                choice.OwningBlueprint = unit; choice.Ranged = ranged;
                var change = Require<BlueprintAiSwitchWeapon>(bySymbol, Satyr32Prefix + suffix + "Ai");
                change.name = InternalName(Satyr32Prefix + suffix + "Ai");
                change.SwitchTo = ranged ? SwitchMode.Ranged : SwitchMode.Melee;
                change.BaseScore = 8; change.CooldownRounds = 1;
                change.ActorConsiderations = new Consideration[] { choice };
                change.TargetConsiderations = Array.Empty<Consideration>();
                switches.Add(change);
            }
            var nativeBrain = BlueprintLibraryLookup.RequireExact<BlueprintBrain>(library,
                DumbBrainGuid, "ordinary native attack fallback");
            var brain = Require<BlueprintBrain>(bySymbol, Satyr32Prefix + "Brain");
            CopyFields(nativeBrain, brain); brain.name = InternalName(Satyr32Prefix + "Brain");
            brain.ComponentsArray = (nativeBrain.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                .Select(ExpandedSummoningAbilityBuilder.DeepCloneComponent).ToArray();
            brain.Actions = (nativeBrain.Actions ?? Array.Empty<BlueprintAiAction>())
                .Concat(new BlueprintAiAction[] { ai, charmAi, sleepAi }).Concat(switches).ToArray();
            unit.Brain = brain;
        }

        private static BlueprintAbility ConfigureSatyrAtWill32(LibraryScriptableObject library,
            IDictionary<string, BlueprintScriptableObject> bySymbol, string suffix, string nativeGuid, string title)
        {
            string symbol = Satyr32Prefix + suffix;
            var ability = Require<BlueprintAbility>(bySymbol, symbol);
            CloneMephitSpell(library, ability, nativeGuid, "native Satyr " + title);
            ability.name = InternalName(symbol); ability.Type = AbilityType.SpellLike;
            // PF1 spell-like abilities use a standard action unless specifically
            // stated otherwise. Do not inherit the donor spell's full-round flag.
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.SetIsFullRoundAction(false);
            if (suffix == "Sleep") BindFeyCelestialSleep32(ability, bySymbol);
            else if (suffix == "CharmPerson") BindFeyCelestialCharm32(ability, symbol, bySymbol, DurationRate.Hours);
            else throw new InvalidOperationException("Unexpected Satyr at-will control mapping.");
            BlueprintUnitFactAccess.Resolve().Configure(ability,
                LocalizationService.Create(symbol + ".Name", title),
                LocalizationService.Create(symbol + ".Description", title + " at will at caster level8."), ability.Icon);
            return ability;
        }

        private static BlueprintAiCastSpell ConfigureSatyrControlAi32(
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintUnit unit,
            string suffix, BlueprintAbility ability, int score)
        {
            var ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Satyr32Prefix + suffix + "Ai"), Satyr32Prefix + suffix + "Ai", ability, 1);
            var eligibility = Require<SatyrControlEligibility>(bySymbol, Satyr32Prefix + suffix + "Eligibility");
            eligibility.OwningBlueprint = unit; eligibility.Ability = ability;
            ai.BaseScore = score; ai.TargetConsiderations = new Consideration[] { eligibility };
            return ai;
        }
    }
}
