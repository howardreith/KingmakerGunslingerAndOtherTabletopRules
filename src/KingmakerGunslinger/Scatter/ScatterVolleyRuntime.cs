using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;

namespace KingmakerGunslinger.Scatter
{
    /// <summary>Request-scoped bridge for independent native scatter attacks.</summary>
    internal static class ScatterVolleyRuntime
    {
        private static readonly object Gate = new object();
        private static readonly ConditionalWeakTable<RuleAttackWithWeapon, Marker>
            Markers = new ConditionalWeakTable<RuleAttackWithWeapon, Marker>();

        internal static void Register(RuleAttackWithWeapon attack, object target,
            string stableIdentity, int misfireThreshold,
            int? forcedNaturalRoll = null)
        {
            if (attack == null) throw new ArgumentNullException("attack");
            if (target == null) throw new ArgumentNullException("target");
            if (string.IsNullOrWhiteSpace(stableIdentity))
                throw new ArgumentException(
                    "A stable scatter target identity is required.",
                    "stableIdentity");
            if (misfireThreshold < 0 || misfireThreshold > 20)
                throw new ArgumentOutOfRangeException("misfireThreshold");
            if (forcedNaturalRoll.HasValue && (forcedNaturalRoll.Value < 1 ||
                forcedNaturalRoll.Value > 20))
                throw new ArgumentOutOfRangeException("forcedNaturalRoll");
            lock (Gate)
            {
                Markers.Remove(attack);
                Markers.Add(attack, new Marker(target, stableIdentity,
                    misfireThreshold, forcedNaturalRoll));
            }
        }

        internal static bool ShouldBypassOrdinaryDischarge(RuleAttackRoll roll)
        {
            Marker ignored;
            return TryGet(roll, out ignored);
        }

        internal static void SuppressPrecisionDamage(RuleDealDamage damage)
        {
            if (damage == null) return;
            Marker ignored;
            if (TryGet(damage.AttackRoll, out ignored))
                damage.DisablePrecisionDamage = true;
        }

        internal static void BeforeSetRoll(RuleAttackRoll roll,
            ref RulebookEvent.RollEntry value)
        {
            Marker marker;
            if (!TryGet(roll, out marker)) return;
            if (marker.HasNaturalRoll)
                throw new InvalidOperationException(
                    "A scatter target received more than one natural roll assignment.");
            if (marker.ForcedNaturalRoll.HasValue)
            {
                int forced = marker.ForcedNaturalRoll.Value;
                List<int> history = value.RollHistory == null ? new List<int>() :
                    new List<int>(value.RollHistory);
                if (history.Count == 0) history.Add(forced);
                else history[history.Count - 1] = forced;
                value.Value = forced;
                value.RollHistory = history;
            }
            marker.Record(value.Value);
        }

        internal static void AfterIsSuccessRoll(RuleAttackRoll roll,
            int naturalRoll, ref bool nativeResult)
        {
            Marker marker;
            if (!TryGet(roll, out marker)) return;
            marker.Verify(naturalRoll);
            if (marker.IsMisfire) nativeResult = false;
        }

        internal static ScatterAttackRollObservation Consume(
            RuleAttackWithWeapon attack)
        {
            if (attack == null) throw new ArgumentNullException("attack");
            Marker marker;
            lock (Gate)
            {
                if (!Markers.TryGetValue(attack, out marker))
                    throw new InvalidOperationException(
                        "Scatter attack marker is missing.");
                Markers.Remove(attack);
            }
            RuleAttackRoll roll = attack.AttackRoll;
            if (roll == null || !marker.HasNaturalRoll)
                throw new InvalidOperationException(
                    "Scatter attack exposed no completed native attack roll.");
            return new ScatterAttackRollObservation(marker.Target,
                marker.StableIdentity, marker.NaturalRoll, roll.IsHit,
                roll.IsCriticalRoll,
                roll.IsCriticalConfirmed);
        }

        internal static void Cancel(RuleAttackWithWeapon attack)
        {
            if (attack == null) return;
            lock (Gate) { Markers.Remove(attack); }
        }

        private static bool TryGet(RuleAttackRoll roll, out Marker marker)
        {
            marker = null;
            if (roll == null || roll.RuleAttackWithWeapon == null) return false;
            lock (Gate)
            {
                return Markers.TryGetValue(roll.RuleAttackWithWeapon, out marker);
            }
        }

        private sealed class Marker
        {
            internal Marker(object target, string stableIdentity, int threshold,
                int? forced)
            { Target = target; StableIdentity = stableIdentity.Trim();
                Threshold = threshold; ForcedNaturalRoll = forced; }
            internal object Target { get; private set; }
            internal string StableIdentity { get; private set; }
            internal int Threshold { get; private set; }
            internal int? ForcedNaturalRoll { get; private set; }
            internal int NaturalRoll { get; private set; }
            internal bool HasNaturalRoll { get { return NaturalRoll != 0; } }
            internal bool IsMisfire { get { return NaturalRoll > 0 &&
                Threshold > 0 && NaturalRoll <= Threshold; } }
            internal void Record(int value)
            {
                if (value < 1 || value > 20) throw new ArgumentOutOfRangeException("value");
                NaturalRoll = value;
            }
            internal void Verify(int value)
            {
                if (!HasNaturalRoll || NaturalRoll != value)
                    throw new InvalidOperationException(
                        "Scatter success evaluation did not match its natural roll.");
            }
        }
    }
}
