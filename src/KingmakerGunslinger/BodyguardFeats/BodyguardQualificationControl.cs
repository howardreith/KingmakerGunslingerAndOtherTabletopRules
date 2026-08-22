using System;
using System.Collections.Generic;
using System.Reflection;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.BodyguardFeats
{
    /// <summary>
    /// Request-local deterministic dice control used only by guarded runtime
    /// qualification. No player-facing path can arm it, and both queues are
    /// thread-scoped and fail closed after one incoming attack.
    /// </summary>
    internal static class BodyguardQualificationControl
    {
        private const BindingFlags Members = BindingFlags.Instance |
            BindingFlags.NonPublic;
        private static readonly FieldInfo RollField =
            typeof(RuleAttackRoll).GetField("<Roll>k__BackingField", Members);
        [ThreadStatic] private static Queue<int> _aidRolls;
        [ThreadStatic] private static int? _incomingRoll;
        [ThreadStatic] private static int _aidConsumed;
        [ThreadStatic] private static int _incomingConsumed;

        internal static bool IsArmed
        { get { return _aidRolls != null || _incomingRoll.HasValue; } }
        internal static int AidConsumed { get { return _aidConsumed; } }
        internal static int IncomingConsumed { get { return _incomingConsumed; } }
        internal static int PendingAid
        { get { return _aidRolls == null ? 0 : _aidRolls.Count; } }
        internal static bool PendingIncoming
        { get { return _incomingRoll.HasValue; } }

        internal static void Arm(int incomingNaturalRoll,
            params int[] aidNaturalRolls)
        {
            if (IsArmed)
                throw new InvalidOperationException(
                    "Bodyguard qualification dice are already armed.");
            Validate(incomingNaturalRoll, "incomingNaturalRoll");
            _incomingRoll = incomingNaturalRoll;
            _aidRolls = new Queue<int>();
            foreach (int roll in aidNaturalRolls ?? new int[0])
            {
                Validate(roll, "aidNaturalRolls");
                _aidRolls.Enqueue(roll);
            }
            _aidConsumed = 0;
            _incomingConsumed = 0;
        }

        internal static bool TryConsumeAid(out int naturalRoll)
        {
            naturalRoll = 0;
            if (!BodyguardSyntheticAidContext.IsActive || _aidRolls == null ||
                _aidRolls.Count == 0) return false;
            naturalRoll = _aidRolls.Dequeue();
            _aidConsumed++;
            return true;
        }

        internal static bool TryApplyAidOverride(RuleRollD20 roll)
        {
            if (roll == null) throw new ArgumentNullException("roll");
            int naturalRoll;
            if (!TryConsumeAid(out naturalRoll)) return false;
            // RuleRollDice.Override is the native pre-trigger seam. It remains
            // authoritative even when an installed dice-control mod replaces
            // RuleRollD20.PreRollDice or the underlying RNG call.
            roll.Override(naturalRoll);
            return true;
        }

        internal static void BeforeSetIncomingRoll(RuleAttackRoll attack,
            ref RulebookEvent.RollEntry value)
        {
            if (attack == null || BodyguardSyntheticAidContext.IsActive ||
                !_incomingRoll.HasValue || attack.RuleAttackWithWeapon == null)
                return;
            int forced = _incomingRoll.Value;
            _incomingRoll = null;
            var history = value.RollHistory == null ? new List<int>() :
                new List<int>(value.RollHistory);
            if (history.Count == 0) history.Add(forced);
            else history[history.Count - 1] = forced;
            value.Value = forced;
            value.RollHistory = history;
            _incomingConsumed++;
        }

        internal static void BeforeEvaluateIncomingRoll(RuleAttackRoll attack,
            ref int naturalRoll)
        {
            if (attack == null || BodyguardSyntheticAidContext.IsActive ||
                !_incomingRoll.HasValue || attack.RuleAttackWithWeapon == null)
                return;
            if (RollField == null ||
                RollField.FieldType != typeof(RulebookEvent.RollEntry))
                throw new MissingFieldException(typeof(RuleAttackRoll).FullName,
                    "<Roll>k__BackingField");

            int forced = _incomingRoll.Value;
            _incomingRoll = null;
            RulebookEvent.RollEntry value = attack.Roll;
            var history = value.RollHistory == null ? new List<int>() :
                new List<int>(value.RollHistory);
            if (history.Count == 0) history.Add(forced);
            else history[history.Count - 1] = forced;
            value.Value = forced;
            value.RollHistory = history;
            RollField.SetValue(attack, value);
            naturalRoll = forced;
            _incomingConsumed++;
        }

        internal static string DescribeAndClear()
        {
            string result = "aidConsumed=" + _aidConsumed + ";aidPending=" +
                PendingAid + ";incomingConsumed=" + _incomingConsumed +
                ";incomingPending=" + PendingIncoming;
            Clear();
            return result;
        }

        internal static void Clear()
        {
            _aidRolls = null;
            _incomingRoll = null;
            _aidConsumed = 0;
            _incomingConsumed = 0;
        }

        private static void Validate(int naturalRoll, string parameter)
        {
            if (naturalRoll < 1 || naturalRoll > 20)
                throw new ArgumentOutOfRangeException(parameter);
        }
    }
}
