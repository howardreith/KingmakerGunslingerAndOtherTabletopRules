using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using KingmakerGunslinger.Blueprints;
using UnityEngine;

namespace KingmakerGunslinger.ElementalRaces
{
    internal sealed class ElementalBreezeKissedBlueprints
    {
        internal BlueprintAbilityResource Resource;
        internal BlueprintBuff CalmedBuff;
        internal BlueprintAbility Gust, BullRush, Trip, Calm, Renew;
        internal BlueprintScriptableObject[] Mechanics { get { return new BlueprintScriptableObject[] {
            Resource, Gust, BullRush, Trip, CalmedBuff, Calm, Renew }; } }

        internal BlueprintComponent[] ProviderComponents()
        {
            var facts = ScriptableObject.CreateInstance<AddFacts>();
            facts.Facts = new BlueprintUnitFact[] { Gust, Calm, Renew };
            facts.DoNotRestoreMissingFacts = false;
            var add = ScriptableObject.CreateInstance<AddAbilityResources>();
            add.Resource = Resource;
            add.RestoreAmount = true;
            add.RestoreOnLevelUp = false;
            var memory = ScriptableObject.CreateInstance<ElementalTraitDailyResourceState>();
            memory.Resource = Resource;
            var armor = ScriptableObject.CreateInstance<ElementalBreezeKissedArmorClass>();
            armor.Resource = Resource;
            armor.CalmedBuff = CalmedBuff;
            return new BlueprintComponent[] { facts, add, memory, armor };
        }
    }

    internal static class ElementalBreezeKissedFactory
    {
        internal const string Prefix = "KMG.ElementalRaces.Traits.Sylph.BreezeKissed";

        internal static ElementalBreezeKissedBlueprints Register(BlueprintRegistry registry,
            ElementalAlternateTraitId trait, Sprite icon)
        {
            if (trait != ElementalAlternateTraitId.BreezeKissed) return null;
            var graph = new ElementalBreezeKissedBlueprints();
            graph.Resource = registry.Register<BlueprintAbilityResource>(Prefix + ".Resource", () => {
                var value = ScriptableObject.CreateInstance<BlueprintAbilityResource>();
                value.name = (Prefix + ".Resource").Replace('.', '_');
                value.LocalizedName = LocalizationService.Create(Prefix + ".Resource.Name", "Breeze-Kissed Gusts");
                value.LocalizedDescription = LocalizationService.Create(Prefix + ".Resource.Description", ElementalBreezeKissedPolicy.Description);
                ElementalRaceAbilityFactory.ConfigureBaseAmount(value, 1);
                return value;
            });
            graph.CalmedBuff = registry.Register<BlueprintBuff>(Prefix + ".CalmedBuff", () => {
                var value = ScriptableObject.CreateInstance<BlueprintBuff>();
                value.name = (Prefix + ".CalmedBuff").Replace('.', '_');
                value.Stacking = StackingType.Replace;
                value.ComponentsArray = Array.Empty<BlueprintComponent>();
                value.FxOnStart = new PrefabLink();
                value.FxOnRemove = new PrefabLink();
                value.ResourceAssetIds = Array.Empty<string>();
                BlueprintUnitFactAccess.Resolve().Configure(value,
                    LocalizationService.Create(Prefix + ".CalmedBuff.Name", "Breeze-Kissed: Winds Calmed"),
                    LocalizationService.Create(Prefix + ".CalmedBuff.Description", "Your voluntary calm suppresses the wind defense. Renew the winds as a swift action while a daily gust remains."), icon);
                return ElementalComponentIdentity.Prepare(value);
            });
            graph.Gust = registry.Register<BlueprintAbility>(Prefix + ".Gust", () => {
                var value = Ability("Gust", "Breeze-Kissed Gust", UnitCommand.CommandType.Standard, false, icon);
                value.ComponentsArray = new BlueprintComponent[] {
                    ScriptableObject.CreateInstance<AbilityVariants>(),
                    ElementalRaceAbilityFactory.ResourceCost(graph.Resource, true)
                };
                return ElementalComponentIdentity.Prepare(value);
            });
            graph.BullRush = registry.Register<BlueprintAbility>(Prefix + ".BullRush",
                () => Gust("BullRush", "Breeze-Kissed: Bull Rush", CombatManeuver.BullRush, graph, icon));
            graph.Trip = registry.Register<BlueprintAbility>(Prefix + ".Trip",
                () => Gust("Trip", "Breeze-Kissed: Trip", CombatManeuver.Trip, graph, icon));
            graph.BullRush.Parent = graph.Trip.Parent = graph.Gust;
            graph.Gust.GetComponent<AbilityVariants>().Variants = new[] { graph.BullRush, graph.Trip };
            graph.Calm = registry.Register<BlueprintAbility>(Prefix + ".CalmWinds", () => {
                var value = Ability("CalmWinds", "Breeze-Kissed: Calm Winds", UnitCommand.CommandType.Swift, true, icon);
                var apply = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
                apply.Buff = graph.CalmedBuff;
                // Native RunAction calculates duration before checking Permanent.
                apply.DurationValue = new ContextDurationValue { Rate = DurationRate.Rounds,
                    DiceType = Kingmaker.RuleSystem.DiceType.Zero, DiceCountValue = 0, BonusValue = 1 };
                apply.Permanent = true;
                apply.ToCaster = true;
                apply.IsFromSpell = false;
                apply.IsNotDispelable = true;
                var absent = ScriptableObject.CreateInstance<AbilityCasterHasNoFacts>();
                absent.Facts = new BlueprintUnitFact[] { graph.CalmedBuff };
                value.ComponentsArray = new BlueprintComponent[] { Ready(graph.Resource), absent, Effect(apply) };
                return ElementalComponentIdentity.Prepare(value);
            });
            graph.Renew = registry.Register<BlueprintAbility>(Prefix + ".RenewWinds", () => {
                var value = Ability("RenewWinds", "Breeze-Kissed: Renew Winds", UnitCommand.CommandType.Swift, true, icon);
                var present = ScriptableObject.CreateInstance<AbilityCasterHasFacts>();
                present.Facts = new BlueprintUnitFact[] { graph.CalmedBuff };
                present.NeedsAll = true;
                value.ComponentsArray = new BlueprintComponent[] { Ready(graph.Resource), present, Effect(RemoveCalm(graph.CalmedBuff)) };
                return ElementalComponentIdentity.Prepare(value);
            });
            return graph;
        }

        private static BlueprintAbility Gust(string suffix, string name, CombatManeuver type,
            ElementalBreezeKissedBlueprints graph, Sprite icon)
        {
            var value = Ability(suffix, name, UnitCommand.CommandType.Standard, false, icon);
            var maneuver = ScriptableObject.CreateInstance<ContextActionCombatManeuver>();
            maneuver.Type = type;
            maneuver.IgnoreConcealment = false;
            maneuver.ReplaceStat = false;
            maneuver.UseCasterLevelAsBaseAttack = false;
            maneuver.UseBestMentalStat = false;
            maneuver.OnSuccess = Actions();
            // Reuse only the established idempotent one-use commitment boundary,
            // never Hydraulic Push's resource, stat replacement or spell graph.
            var commit = ScriptableObject.CreateInstance<ElementalHydraulicResourceCommit>();
            commit.Resource = graph.Resource;
            value.ComponentsArray = new BlueprintComponent[] {
                ElementalRaceAbilityFactory.ResourceCost(graph.Resource, true),
                Effect(commit, RemoveCalm(graph.CalmedBuff), maneuver)
            };
            return ElementalComponentIdentity.Prepare(value);
        }

        private static ElementalBreezeReadyRequirement Ready(BlueprintAbilityResource resource)
        {
            var value = ScriptableObject.CreateInstance<ElementalBreezeReadyRequirement>();
            value.Resource = resource;
            return value;
        }

        private static ContextActionRemoveBuff RemoveCalm(BlueprintBuff buff)
        {
            var value = ScriptableObject.CreateInstance<ContextActionRemoveBuff>();
            value.Buff = buff;
            value.ToCaster = true;
            return value;
        }

        private static AbilityEffectRunAction Effect(params GameAction[] actions)
        {
            var value = ScriptableObject.CreateInstance<AbilityEffectRunAction>();
            value.SavingThrowType = SavingThrowType.Unknown;
            value.Actions = Actions(actions);
            return value;
        }

        private static ActionList Actions(params GameAction[] actions) { return new ActionList { Actions = actions }; }

        private static BlueprintAbility Ability(string suffix, string name, UnitCommand.CommandType action,
            bool personal, Sprite icon)
        {
            string symbol = Prefix + "." + suffix;
            var value = ScriptableObject.CreateInstance<BlueprintAbility>();
            value.name = symbol.Replace('.', '_');
            value.Type = AbilityType.Supernatural;
            value.ActionType = action;
            value.SetIsFullRoundAction(false);
            value.Range = personal ? AbilityRange.Personal : AbilityRange.Custom;
            value.CustomRange = ElementalBreezeKissedPolicy.RangeFeet.Feet();
            value.CanTargetSelf = personal;
            value.CanTargetFriends = value.CanTargetEnemies = !personal;
            value.CanTargetPoint = false;
            value.SpellResistance = false;
            value.NeedEquipWeapons = false;
            value.EffectOnAlly = personal ? AbilityEffectOnUnit.Helpful : AbilityEffectOnUnit.Harmful;
            value.EffectOnEnemy = personal ? AbilityEffectOnUnit.None : AbilityEffectOnUnit.Harmful;
            value.Animation = UnitAnimationActionCastSpell.CastAnimationStyle.Immediate;
            value.MaterialComponent = new BlueprintAbility.MaterialComponentData();
            value.ResourceAssetIds = Array.Empty<string>();
            value.ComponentsArray = Array.Empty<BlueprintComponent>();
            value.LocalizedDuration = LocalizationService.Create(Prefix + ".Duration", "See description");
            value.LocalizedSavingThrow = LocalizationService.Create(Prefix + ".SavingThrow", "None");
            BlueprintUnitFactAccess.Resolve().Configure(value,
                LocalizationService.Create(symbol + ".Name", name),
                LocalizationService.Create(symbol + ".Description", ElementalBreezeKissedPolicy.Description), icon);
            return value;
        }
    }
}
