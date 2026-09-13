using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.Spells.Teleportation
{
    // Read-only adapter for the installed Kingmaker/CotW native scroll contract.
    // Never constructs a rule event, invokes a subscriber, rolls, attaches a fact,
    // or opens the activation gate. Unknown applicable behavior is not scored.
    internal static class TeleportScrollReaderAdapter
    {
        private const StatType Umd = StatType.SkillUseMagicDevice;
        private static readonly MethodInfo Suitable = typeof(ItemEntity).GetMethod("IsSuitableUnitForUseAbility",
            BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(UnitEntityData) }, null);

        internal static bool Eligible(UnitEntityData unit, ItemEntity item)
        {
            if (unit == null || item == null || item.Count <= 0 || item.Collection == null ||
                unit.Descriptor == null || unit.Descriptor.State.IsDead || unit.Descriptor.State.IsUnconscious ||
                Suitable == null || !(bool)Suitable.Invoke(null, new object[] { unit })) return false;
            var scroll = item.Blueprint as BlueprintItemEquipmentUsable;
            if (scroll == null || scroll.Type != UsableItemType.Scroll || scroll.Ability == null ||
                item.IsSpendCharges && item.Charges <= 0) return false;
            var ability = new AbilityData(scroll.Ability, unit.Descriptor);
            KingmakerGunslinger.Blueprints.TeleportationSpellBlueprints.Validate(scroll.Ability);
            // Canonical strategic abilities have only SpellComponent, the
            // read-only caster checker and spell-list metadata: no availability
            // providers, material cost, parameter, sticky-touch or active logic.
            // Mirror native item-specific availability without constructing or
            // activating a temporary Ability fact just to inspect it.
            if (scroll.Ability.CasterCheckers.Any(check => !check.CorrectCaster(unit)) ||
                ability.RequireFullRoundAction && unit.Descriptor.State.HasCondition(Kingmaker.UnitLogic.UnitCondition.Staggered) ||
                scroll.RequireUMDIfCasterHasNoSpellInSpellList &&
                    (unit.Descriptor.State.SpellCastingForbidden || unit.Descriptor.Body.IsPolymorphed) ||
                !ability.HasRequiredParams || scroll.IsUnitNeedUMDForUse(unit.Descriptor) && !unit.Descriptor.HasUMDSkill)
                return false;
            return ability.CanTarget(new Kingmaker.Utility.TargetWrapper(unit));
        }

        internal static TeleportScrollActivationChance Read(UnitEntityData unit, ItemEntity item)
        {
            try
            {
                var scroll = (BlueprintItemEquipmentUsable)item.Blueprint;
                var ability = new AbilityData(scroll.Ability, unit.Descriptor);
                if (ability.IsAffectedByArcaneSpellFailure)
                    return Unknown("scroll unexpectedly requires an arcane spell failure check");
                if (unit.Blueprint.IsCheater)
                    return TeleportScrollActivationChance.Native(false, 0, 0, 0, false, 0, "native forced activation");
                bool needsUmd = scroll.IsUnitNeedUMDForUse(unit.Descriptor); // Includes CotW FamiliarFreeItemUse postfix.
                int failure = 0;
                foreach (var handler in Handlers(unit, typeof(RuleCastSpell), true))
                {
                    string name = ContractName(handler);
                    if (name == "native:AddSpellFailureChance") failure = Math.Max(failure, Int(handler, "Chance"));
                    else if (name == "cotw:ItemUseFailure") failure = Math.Max(failure, Int(handler, "chance"));
                    else if (!CastObservers.Contains(name)) return Unknown("cast subscriber " + handler.GetType().FullName);
                }
                if (!needsUmd)
                    return TeleportScrollActivationChance.Native(false, 0, 0, failure, false, 0, "native list/item route; failure=" + failure);
                if (!SkillCheatsInactive()) return Unknown("active or unrecognized skill/dice cheat settings");
                int dc = 20 + scroll.SpellLevel + BlueprintRoot.Instance.DifficultyList
                    .GetAdjustmentPreset(Game.Instance.Player.Difficulty.StatsAdjustmentsType).SkillCheckDCBonus;
                // Exactly the detached Bonus value used by RuleSkillCheck. It has
                // no owner, dependents or subscribers; native descriptor stacking
                // applies without changing the reader's actual stat or modifiers.
                var bonus = new ModifiableValue(StatType.Unknown);
                bool takeTen = false; int successBonus = 0;
                foreach (var handler in Handlers(unit, typeof(RuleSkillCheck), false))
                {
                    string name = ContractName(handler);
                    if (name == "native:Take10ForSuccess")
                    {
                        if (Stat(handler, "Skill") == Umd && (UsableItemType)Field(handler, "MagicDeviceType") == UsableItemType.Scroll) takeTen = true;
                    }
                    else if (name == "native:SkillSuccessIfBonus")
                    {
                        var component = (GameLogicComponent)handler;
                        if (component.Fact.MaybeContext != null) successBonus = Value(handler, "Value");
                    }
                    else if (name == "cotw:ReplaceSkillRankWithClassLevel" || name == "cotw:SkillBonusInCombat")
                    {
                        if (Stat(handler, "skill") != Umd) continue;
                        if (Field(handler, "reason") != null) return Unknown("reason-dependent UMD bonus " + name);
                        int amount = name == "cotw:SkillBonusInCombat" ? Value(handler, "value") :
                            Math.Max(0, unit.Descriptor.Progression.GetClassLevel((BlueprintCharacterClass)Field(handler, "character_class")) - unit.Stats.GetStat(Umd).BaseValue);
                        bonus.AddModifier(amount, (GameLogicComponent)null, name == "cotw:SkillBonusInCombat" ? (ModifierDescriptor)Field(handler, "descriptor") : (ModifierDescriptor)25);
                    }
                    else if (name == "cotw:AddBonusToSkillCheckIfNoClassSkill")
                    {
                        if (Stat(handler, "check") == Umd && !unit.Stats.GetStat<ModifiableValueSkill>(Stat(handler, "skill")).ClassSkill)
                            bonus.AddModifier(3, (GameLogicComponent)null, (ModifierDescriptor)21);
                    }
                    else if (name == "cotw:AddRandomBonusOnSkillCheckAndConsumeResource")
                    {
                        if (HasStat(handler, "stats", Umd)) return Unknown("armed random UMD bonus/resource effect");
                    }
                    else if (name == "kmg:SlingersLuckSkillCheckReroll")
                    {
                        var luck = (KingmakerGunslinger.Deeds.SlingersLuckSkillCheckReroll)handler;
                        if (luck.Grit != null && luck.GunslingerClass != null && unit.Descriptor.Progression.GetClassLevel(luck.GunslingerClass) >= 15 &&
                            unit.Descriptor.Resources.GetResourceAmount(luck.Grit) > 0) return Unknown("armed Slinger's Luck skill reroll");
                    }
                    else if (!SkillObservers.Contains(name)) return Unknown("skill subscriber " + handler.GetType().FullName);
                }
                int modifier = unit.Stats.GetStat(Umd).ModifiedValue + bonus.ModifiedValue;
                // Native take-ten bypasses the d20 event entirely when it can
                // meet the adjusted DC. Inactive roll effects do not lower it.
                foreach (var handler in takeTen && 10 + modifier >= dc ? Enumerable.Empty<object>() : Handlers(unit, typeof(RuleRollD20), false))
                {
                    string name = ContractName(handler);
                    if (name == "native:ModifyD20" || name == "cotw:ModifyD20WithActions")
                    {
                        if ((Int(handler, "Rule") & 2) != 0 && (!(bool)Field(handler, "SpecificSkill") || HasStat(handler, "Skill", Umd)))
                            return Unknown("active UMD d20 replacement/reroll " + name);
                    }
                    else if (name == "native:AbilityScoreCheckBonus")
                    {
                        if (Stat(handler, "Stat") == Umd) return Unknown("ability-score component configured for UMD");
                    }
                    else if (name == "cotw:AddRandomBonusOnSkillCheckAndConsumeResource")
                    {
                        if (HasStat(handler, "stats", Umd)) return Unknown("armed random UMD bonus/resource effect");
                    }
                    else if (!D20Observers.Contains(name)) return Unknown("d20 subscriber " + handler.GetType().FullName);
                }
                return TeleportScrollActivationChance.Native(true, modifier, dc, failure, takeTen, successBonus,
                    "UMD=" + modifier + ";DC=" + dc + ";take10=" + takeTen + ";successBonus=" + successBonus + ";failure=" + failure);
            }
            catch (Exception exception) { return Unknown("native forecast unavailable: " + exception.GetType().Name + ": " + exception.Message); }
        }

        private static TeleportScrollActivationChance Unknown(string reason) { return TeleportScrollActivationChance.Unsupported(reason); }
        private static string ContractName(object value)
        {
            string assembly = value.GetType().Assembly.GetName().Name;
            return (assembly == "Assembly-CSharp" ? "native" : assembly == "CallOfTheWild" ? "cotw" :
                assembly == "KingmakerGunslinger" ? "kmg" : assembly) + ":" + value.GetType().Name;
        }
        private static object Field(object owner, string name)
        {
            for (var type = owner.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(owner);
            }
            throw new MissingFieldException(owner.GetType().FullName, name);
        }
        private static int Int(object owner, string name) { return Convert.ToInt32(Field(owner, name)); }
        private static StatType Stat(object owner, string name) { return (StatType)Field(owner, name); }
        private static bool HasStat(object owner, string name, StatType stat) { return ((IEnumerable)Field(owner, name)).Cast<object>().Any(value => (StatType)value == stat); }
        private static int Value(object owner, string name) { return ((ContextValue)Field(owner, name)).Calculate(((GameLogicComponent)owner).Fact.MaybeContext); }
        private static IEnumerable<object> Handlers(UnitEntityData unit, Type rule, bool selfTarget)
        {
            var managers = new object[] { RulebookEventBus.GlobalRulebookSubscribers,
                selfTarget ? RulebookEventBus.TargetRulebookSubscribers.Get(unit) : null,
                RulebookEventBus.InitiatorRulebookSubscribers.Get(unit) };
            foreach (var manager in managers)
            {
                if (manager == null) continue;
                var listeners = (IDictionary)Field(manager, "m_Listeners");
                for (var type = rule; type != null && type != typeof(RulebookEvent); type = type.BaseType)
                {
                    if (!listeners.Contains(type)) continue;
                    var list = (IEnumerable)Field(listeners[type], "List");
                    foreach (var handler in list) if (handler != null) yield return handler;
                }
            }
        }
        private static bool SkillCheatsInactive()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(value => value.GetName().Name == "BagOfTricks");
            if (assembly == null) return true;
            var type = assembly.GetType("BagOfTricks.HarmonyPatches", true);
            var settings = type.GetField("settings", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            var storage = assembly.GetType("BagOfTricks.Storage", true);
            var disabled = (string)storage.GetField("isFalseString", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
            return new[] { "togglePartyAlwaysRoll20", "toggleEnemiesAlwaysRoll1", "togglePassSkillChecksIndividual" }
                .All(name => (string)Field(settings, name) == disabled);
        }
        // Installed native/CotW handlers whose verified predicates or effects do
        // not change scroll activation success. Unknown handlers fail closed.
        private static readonly HashSet<string> CastObservers = new HashSet<string>(StringComparer.Ordinal) {
            "native:ChangeSpellElementalDamage", "native:KineticistController", "native:ActivatableAbility", "native:BattleLogManager",
            "native:CombatTextManager", "native:OvertipsVM", "native:CombatLogVM", "native:MetamagicRodMechanics", "native:SpendChargesOnSpellCast",
            "native:SpellCastTrigger", "native:MagusController", "native:TouchSpellsController", "native:AchievementLogicMetamagister",
            "cotw:SpendLowerLevelSpellSlot", "cotw:MagicalSupremacy", "cotw:SpendResourceOnExtraArcanistSpellCast", "cotw:UseAbilitiesWithSameOrLowerActionCostForFree",
            "cotw:SpellSynthesis", "cotw:FreeTouchOrPersonalSpellUseFromSpellbook", "cotw:FamiliarFreeItemUse", "cotw:SpellFailureChance",
            "cotw:RunActionAfterAbilityUse", "cotw:RunActionAfterSpellCastBasedOnLevel", "cotw:HealAfterSpellCast", "cotw:ApplyBuffAfterSpellCast",
            "cotw:RageCasting", "cotw:ConduitSurge", "cotw:MetamagicAdept", "cotw:SpendResourceOnSpellCast", "cotw:SpendResourceOnSpecificSpellCast",
            "cotw:SpellCastTrigger", "cotw:MetamagicOnPersonalSpell", "cotw:MetaRage", "cotw:MetamagicOnSchool", "cotw:MetamagicOnSpellType",
            "cotw:MetamagicOnSpellDescriptor", "cotw:MetamagicOnSpellList", "cotw:MetamagicIfHasParametrizedFeature", "cotw:ChangeSpellElementalDamage",
            "cotw:ApplyMetamagicToPersonalSpell"
        };
        private static readonly HashSet<string> SkillObservers = new HashSet<string>(StringComparer.Ordinal) {
            "native:NegativeLevelComponent", "native:BuffAbilityRollsBonus", "native:TrapPerceptionBonus", "native:CheatsStats",
            "cotw:DependentAbilityScoreCheckStatReplacement"
        };
        private static readonly HashSet<string> D20Observers = new HashSet<string>(StringComparer.Ordinal) {
            "cotw:RerollOnStandardSingleAttack", "cotw:RerollOnWeaponCategoryAndSpendResource",
            "cotw:AddRandomBonusOnAttackRollAndConsumeResource", "cotw:AddRandomBonusOnSavingThrowAndConsumeResource"
        };
    }
}
