using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Kingmaker.EntitySystem.Entities;
using Harmony12;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Cord
{
    internal static class CordConditionRuntime
    {
        [System.ThreadStatic] private static UnitState _fatigueBypass;
        [System.ThreadStatic] private static UnitState _buffSubstitutionState;
        [System.ThreadStatic] private static UnitCondition _buffSubstitutionCondition;
        private static readonly ConditionalWeakTable<Buff, object> ExhaustionSources =
            new ConditionalWeakTable<Buff, object>();
        private static readonly object Marker = new object();
        private static int _lastRoll;
        private static int _lastAppliedDamage;
        private static long _publishedLogs;

        internal static int LastRoll { get { return _lastRoll; } }
        internal static int LastAppliedDamage { get { return _lastAppliedDamage; } }
        internal static long PublishedLogs { get { return Interlocked.Read(ref _publishedLogs); } }

        internal static void ResetDiagnostics()
        {
            _lastRoll = 0;
            _lastAppliedDamage = 0;
        }

        internal static bool Prefix(UnitState state, UnitCondition condition, Buff source)
        {
            if (state == null || ReferenceEquals(_fatigueBypass, state)) return true;
            if (ReferenceEquals(_buffSubstitutionState, state) &&
                _buffSubstitutionCondition == condition) return false;
            if (condition != UnitCondition.Fatigued &&
                condition != UnitCondition.Exhausted) return true;
            if (!HasExactEquippedCord(state)) return true;
            if (condition == UnitCondition.Fatigued && source != null &&
                ExhaustionSources.Remove(source)) return false;

            int damage = DealNonlethalEquivalent(state);
            PublishSubstitution(condition, damage);
            if (condition == UnitCondition.Exhausted)
            {
                if (source != null)
                {
                    try { ExhaustionSources.Add(source, Marker); }
                    catch (System.ArgumentException) { }
                }
                try
                {
                    _fatigueBypass = state;
                    state.AddCondition(UnitCondition.Fatigued, source);
                }
                finally { _fatigueBypass = null; }
            }
            return false;
        }

        internal static bool BeginBuff(BuffCollection buffs, BlueprintBuff blueprint)
        {
            if (buffs == null || buffs.Owner == null || blueprint == null ||
                _buffSubstitutionState != null) return false;
            UnitCondition condition;
            if (!TryClassifyCondition(blueprint, out condition)) return false;
            UnitState state = buffs.Owner.State;
            if (state == null || !HasExactEquippedCord(state)) return false;

            int damage = DealNonlethalEquivalent(state);
            PublishSubstitution(condition, damage);
            if (condition == UnitCondition.Exhausted)
            {
                try
                {
                    _fatigueBypass = state;
                    state.AddCondition(UnitCondition.Fatigued, null);
                }
                finally { _fatigueBypass = null; }
            }
            _buffSubstitutionState = state;
            _buffSubstitutionCondition = condition;
            return true;
        }

        internal static void EndBuff(bool owner)
        {
            if (!owner) return;
            _buffSubstitutionState = null;
            _buffSubstitutionCondition = default(UnitCondition);
        }

        private static bool TryClassifyCondition(BlueprintBuff blueprint,
            out UnitCondition condition)
        {
            UnitCondition[] conditions = (blueprint.ComponentsArray ??
                    new Kingmaker.Blueprints.BlueprintComponent[0])
                .OfType<AddCondition>()
                .Select(component => component.Condition)
                .Where(value => value == UnitCondition.Fatigued ||
                    value == UnitCondition.Exhausted)
                .Distinct().ToArray();
            if (conditions.Length == 1)
            {
                condition = conditions[0];
                return true;
            }
            condition = default(UnitCondition);
            return false;
        }

        private static bool HasExactEquippedCord(UnitState state)
        {
            return state.Owner != null && state.Owner.Unit != null &&
                state.Owner.Unit.Body != null && state.Owner.Unit.Body.Belt != null &&
                state.Owner.Unit.Body.Belt.HasItem &&
                ReferenceEquals(state.Owner.Unit.Body.Belt.Item.Blueprint,
                    BlueprintBootstrap.CordOfStubbornResolve);
        }

        private static int DealNonlethalEquivalent(UnitState state)
        {
            int roll = Rulebook.Trigger(new RuleRollDice(state.Owner.Unit,
                new DiceFormula(1, DiceType.D6))).Result;
            int amount = System.Math.Min(roll,
                System.Math.Max(0, state.Owner.Unit.HPLeft - 1));
            _lastRoll = roll;
            _lastAppliedDamage = 0;
            if (amount == 0) return 0;
            var direct = new DirectDamage(new DiceFormula(0, DiceType.D6), amount);
            var damage = new RuleDealDamage(state.Owner.Unit, state.Owner.Unit,
                new DamageBundle(direct)) {
                    DisablePrecisionDamage = true,
                    IgnoreDamageReduction = true
                };
            int damageBefore = state.Owner.Unit.Damage;
            Rulebook.Trigger(damage);
            _lastAppliedDamage = state.Owner.Unit.Damage - damageBefore;
            return _lastAppliedDamage;
        }

        private static void PublishSubstitution(UnitCondition condition, int damage)
        {
            string source = condition == UnitCondition.Exhausted ? "exhaustion" :
                "fatigue";
            string message = string.Format(CultureInfo.InvariantCulture,
                "Cord of Stubborn Resolve converts {0} into {1} nonlethal-equivalent damage{2}.",
                source, damage, damage == 0 ? " (1 HP floor)" : string.Empty);
            try
            {
                EventBus.RaiseEvent<IWarningNotificationUIHandler>(
                    handler => handler.HandleWarning(message, false));
                Interlocked.Increment(ref _publishedLogs);
            }
            catch (Exception exception)
            {
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("cord", "substitution-log.failed",
                        "Cord substitution committed, but its player-facing notification failed.",
                        exception);
            }
        }
    }

    [HarmonyPatch(typeof(UnitState), "AddCondition",
        typeof(UnitCondition), typeof(Buff))]
    internal static class CordAddConditionPatch
    {
        private static bool Prefix(UnitState __instance, UnitCondition condition,
            Buff sourceBuff)
        { return CordConditionRuntime.Prefix(__instance, condition, sourceBuff); }
    }

    [HarmonyPatch(typeof(BuffCollection), "AddBuff",
        typeof(BlueprintBuff), typeof(MechanicsContext), typeof(TimeSpan?))]
    internal static class CordAddContextBuffPatch
    {
        private static void Prefix(BuffCollection __instance, BlueprintBuff __0,
            ref bool __state)
        { __state = CordConditionRuntime.BeginBuff(__instance, __0); }

        private static void Postfix(bool __state)
        { CordConditionRuntime.EndBuff(__state); }
    }

    [HarmonyPatch(typeof(BuffCollection), "AddBuff",
        typeof(BlueprintBuff), typeof(UnitEntityData), typeof(TimeSpan?),
        typeof(AbilityParams))]
    internal static class CordAddCasterBuffPatch
    {
        private static void Prefix(BuffCollection __instance, BlueprintBuff __0,
            ref bool __state)
        { __state = CordConditionRuntime.BeginBuff(__instance, __0); }

        private static void Postfix(bool __state)
        { CordConditionRuntime.EndBuff(__state); }
    }
}
