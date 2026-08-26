using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Harmony12;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Fatigue;

namespace KingmakerGunslinger.Cord
{
    internal static class CordConditionRuntime
    {
        [ThreadStatic] private static UnitState _fatigueBypass;
        private static readonly ConditionalWeakTable<Buff, object>
            ExhaustionSources = new ConditionalWeakTable<Buff, object>();
        private static readonly object Marker = new object();
        private static int _lastRoll;
        private static int _lastAppliedDamage;
        private static int _resolvedSubstitutions;
        private static long _publishedLogs;

        internal static int LastRoll { get { return _lastRoll; } }
        internal static int LastAppliedDamage { get { return _lastAppliedDamage; } }
        internal static int ResolvedSubstitutions
        { get { return _resolvedSubstitutions; } }
        internal static long PublishedLogs
        { get { return Interlocked.Read(ref _publishedLogs); } }

        internal static void ResetDiagnostics()
        {
            _lastRoll = 0;
            _lastAppliedDamage = 0;
            _resolvedSubstitutions = 0;
        }

        internal static bool Prefix(UnitState state, UnitCondition condition,
            Buff source)
        {
            if (state == null || ReferenceEquals(_fatigueBypass, state))
                return true;
            if (CanonicalFatigueApplicationRuntime.IsCanonicalApplication(state))
                return true;
            if (condition != UnitCondition.Fatigued &&
                condition != UnitCondition.Exhausted)
                return true;
            if (!HasExactEquippedCord(state)) return true;
            if (condition == UnitCondition.Fatigued && source != null &&
                ExhaustionSources.Remove(source))
                return false;

            ResolveCanonical(state,
                condition == UnitCondition.Exhausted
                    ? CordConditionKind.Exhaustion
                    : CordConditionKind.Fatigue);
            if (condition == UnitCondition.Exhausted)
            {
                if (source != null)
                {
                    try { ExhaustionSources.Add(source, Marker); }
                    catch (ArgumentException) { }
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

        internal static void ResolveCanonical(UnitState state,
            CordConditionKind condition)
        {
            if (state == null) throw new ArgumentNullException("state");
            if (!Enum.IsDefined(typeof(CordConditionKind), condition))
                throw new ArgumentOutOfRangeException("condition");
            if (!HasExactEquippedCord(state))
                throw new InvalidOperationException(
                    "Canonical Cord substitution requires the exact equipped Cord.");
            _resolvedSubstitutions++;
            int damage = DealNonlethalEquivalent(state);
            PublishSubstitution(condition == CordConditionKind.Exhaustion
                ? UnitCondition.Exhausted : UnitCondition.Fatigued, damage);
        }

        internal static bool HasExactEquippedCord(UnitState state)
        {
            return state != null && state.Owner != null &&
                state.Owner.Unit != null && state.Owner.Unit.Body != null &&
                state.Owner.Unit.Body.Belt != null &&
                state.Owner.Unit.Body.Belt.HasItem &&
                ReferenceEquals(state.Owner.Unit.Body.Belt.Item.Blueprint,
                    BlueprintBootstrap.CordOfStubbornResolve);
        }

        private static int DealNonlethalEquivalent(UnitState state)
        {
            int roll = Rulebook.Trigger(new RuleRollDice(state.Owner.Unit,
                new DiceFormula(1, DiceType.D6))).Result;
            int amount = Math.Min(roll,
                Math.Max(0, state.Owner.Unit.HPLeft - 1));
            _lastRoll = roll;
            _lastAppliedDamage = 0;
            if (amount == 0) return 0;
            var direct = new DirectDamage(new DiceFormula(0, DiceType.D6),
                amount);
            var damage = new RuleDealDamage(state.Owner.Unit,
                state.Owner.Unit, new DamageBundle(direct))
            {
                DisablePrecisionDamage = true,
                IgnoreDamageReduction = true
            };
            int damageBefore = state.Owner.Unit.Damage;
            Rulebook.Trigger(damage);
            _lastAppliedDamage = state.Owner.Unit.Damage - damageBefore;
            return _lastAppliedDamage;
        }

        private static void PublishSubstitution(UnitCondition condition,
            int damage)
        {
            string source = condition == UnitCondition.Exhausted ?
                "exhaustion" : "fatigue";
            string message = string.Format(CultureInfo.InvariantCulture,
                "Cord of Stubborn Resolve: {0} becomes {1} nonlethal damage{2}.",
                source, damage, damage == 0 ? " (1 HP floor)" : string.Empty);
            if (NativeCombatLog.Publish("cord", "substitution-log.failed",
                    message,
                    "Cord substitution committed, but its native combat-log entry failed."))
                Interlocked.Increment(ref _publishedLogs);
        }
    }

    [HarmonyPatch(typeof(UnitState), "AddCondition",
        typeof(UnitCondition), typeof(Buff))]
    internal static class CordAddConditionPatch
    {
        private static bool Prefix(UnitState __instance,
            UnitCondition condition, Buff sourceBuff)
        {
            return CordConditionRuntime.Prefix(__instance, condition,
                sourceBuff);
        }
    }
}
