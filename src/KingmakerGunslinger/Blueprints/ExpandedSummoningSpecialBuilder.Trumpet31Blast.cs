using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Brain.Blueprints.Considerations;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Summoning;
using UnityEngine;

namespace KingmakerGunslinger.Blueprints
{
    internal static partial class ExpandedSummoningSpecialBuilder
    {
        private static BlueprintAiCastSpell ConfigureTrumpetBlastAi31(
            IDictionary<string, BlueprintScriptableObject> bySymbol, BlueprintUnit unit,
            BlueprintItemWeapon instrument, BlueprintAbility blast)
        {
            var ai = ConfigureMephitAiAction(Require<BlueprintAiCastSpell>(bySymbol,
                Trumpet31Prefix + "TrumpetBlastAi"), Trumpet31Prefix + "TrumpetBlastAi", blast, 0);
            var safety = Require<TrumpetBlastEligibility>(bySymbol, Trumpet31Prefix + "TrumpetBlastEligibility");
            safety.SourceUnit = unit; safety.Instrument = instrument; safety.Blast = blast;
            safety.Archons = new[] { unit, Require<BlueprintUnit>(bySymbol, Hound30Unit),
                Require<BlueprintUnit>(bySymbol, LanternUnitSymbol) };
            ai.ActorConsiderations = new Consideration[] { safety };
            ai.BaseScore = 4.5f; // Heal6, Banishment5, safe blast4.5, useful Aid4, native attack.
            return ai;
        }

        private static BlueprintAbility ConfigureTrumpetBlast31(
            IDictionary<string, BlueprintScriptableObject> bySymbol,
            BlueprintUnit unit, BlueprintFeature traits,
            BlueprintItemWeapon instrument)
        {
            string symbol = Trumpet31Prefix + "TrumpetBlast";
            var ability = Require<BlueprintAbility>(bySymbol, symbol);
            var paralysis = Require<BlueprintBuff>(bySymbol,
                Trumpet31Prefix + "TrumpetParalyzed");
            paralysis.name = InternalName(Trumpet31Prefix + "TrumpetParalyzed");
            paralysis.Stacking = StackingType.Stack;
            paralysis.IsClassFeature = false;
            SetBuffFlags(paralysis, harmful: true);
            var condition = ScriptableObject.CreateInstance<AddCondition>();
            condition.Condition = UnitCondition.Paralyzed;
            var descriptor = ScriptableObject.CreateInstance<SpellDescriptorComponent>();
            descriptor.Descriptor = SpellDescriptor.Paralysis;
            paralysis.ComponentsArray = new BlueprintComponent[] { condition, descriptor };
            paralysis.ResourceAssetIds = Array.Empty<string>();

            var around = ScriptableObject.CreateInstance<AbilityTargetsAround>();
            SetField(around, "m_Radius", new Feet(TrumpetBlastRulesPolicy.RadiusFeet));
            SetField(around, "m_TargetType", TargetType.Any);
            SetField(around, "m_IncludeDead", false);
            SetField(around, "m_Condition", new ConditionsChecker {
                Operation = Operation.And, Conditions = Array.Empty<Condition>() });
            SetField(around, "m_SpreadSpeed", new Feet(0));
            var resolve = ScriptableObject.CreateInstance<ContextActionTrumpetBlast>();
            resolve.SourceUnit = unit; resolve.Instrument = instrument;
            resolve.Blast = ability; resolve.Paralyzed = paralysis;
            resolve.Archons = new[] { unit,
                Require<BlueprintUnit>(bySymbol, Hound30Unit),
                Require<BlueprintUnit>(bySymbol, LanternUnitSymbol) };
            var run = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            run.SavingThrowType = Kingmaker.EntitySystem.Stats.SavingThrowType.Unknown;
            run.Actions = new ActionList { Actions = new GameAction[] { resolve } };
            var access = ScriptableObject.CreateInstance<TrumpetBlastAccess>();
            access.SourceUnit = unit; access.Instrument = instrument;
            var parameters = ScriptableObject.CreateInstance<TrumpetBlastParameters>();
            parameters.SourceUnit = unit; parameters.Blast = ability;
            traits.ComponentsArray = traits.ComponentsArray.Concat(
                new BlueprintComponent[] { parameters }).ToArray();

            ability.name = InternalName(symbol); ability.Parent = null;
            ability.Type = AbilityType.Supernatural;
            ability.Range = AbilityRange.Personal;
            ability.CanTargetPoint = false; ability.CanTargetSelf = true;
            ability.CanTargetFriends = false; ability.CanTargetEnemies = false;
            ability.SpellResistance = false; ability.NeedEquipWeapons = true;
            ability.EffectOnEnemy = AbilityEffectOnUnit.Harmful;
            ability.EffectOnAlly = AbilityEffectOnUnit.Harmful;
            ability.ActionType = UnitCommand.CommandType.Standard;
            ability.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Omni;
            ability.Hidden = false; ability.ActionBarAutoFillIgnored = false;
            ability.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            ability.ResourceAssetIds = Array.Empty<string>();
            ability.ComponentsArray = new BlueprintComponent[] { around, run, access };

            var icon = ExpandedSummoningProjectIcons.Require("trumpet-blast");
            var facts = BlueprintUnitFactAccess.Resolve();
            var title = LocalizationService.Create(symbol + ".Name", "Trumpet Blast");
            facts.Configure(ability, title, LocalizationService.Create(symbol + ".Description",
                "At will as a standard action, sound this archon's own trumpet. All creatures except archons within 100 feet must make a Fortitude save or be paralyzed for 1d4 rounds. The live supernatural save DC is 10 + half racial Hit Dice + Charisma modifier (DC 20 at the adult profile). Spell resistance does not apply. Allies are eligible. This uses no cleric spell slot. The held trumpet and +4 sword are the same summon-owned instrument; it cannot become loot."), icon);
            facts.Configure(paralysis, title, LocalizationService.Create(
                Trumpet31Prefix + "TrumpetParalyzed.Description",
                "Paralyzed by one trumpet blast for its independently expiring 1d4-round duration. Native paralysis immunity still applies; other casters' effects retain their own ownership and duration."), icon);
            return ability;
        }
    }
}
