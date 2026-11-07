using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// One native command's observed boundaries. Each round is the combat
    /// round current at that native event: queued at UnitCommands.Run,
    /// started at IUnitCommandStartHandler, acted at IUnitCommandActHandler
    /// (immediately after OnAction set Result), ended at
    /// IUnitCommandEndHandler. Null means the boundary was never observed.
    /// </summary>
    internal sealed class SummonCommandTiming
    {
        internal int Sequence { get; set; }
        internal int QueuedRound { get; set; }
        internal int? StartedRound { get; set; }
        internal int? ActedRound { get; set; }
        internal double? ActedGameSeconds { get; set; }
        internal string ResultAtAct { get; set; }
        internal int? EndedRound { get; set; }
        internal string ResultAtEnd { get; set; }
    }

    /// <summary>
    /// The first round in which something was observed. Round zero is a real
    /// native surprise round, so "unset" is an explicit state, never 0;
    /// <see cref="OrUnset"/> reports int.MinValue, outside every valid round.
    /// </summary>
    internal sealed class FirstRoundRecord
    {
        internal const int Unset = int.MinValue;
        internal bool HasValue { get; private set; }
        internal int Value { get; private set; }
        internal void Observe(int round)
        {
            if (HasValue) return;
            HasValue = true;
            Value = round;
        }
        internal int OrUnset { get { return HasValue ? Value : Unset; } }
        internal bool Is(int round) { return HasValue && Value == round; }
    }

    internal enum SummonFirstActionDisposition
    {
        ActedInCastRound,
        ActedLater,
        QueuedNeverActed,
        NoCommand
    }

    /// <summary>
    /// Repair R2 LIFE-01: a summon's first legal native action is the first
    /// command whose act boundary fired with a result other than Fail or
    /// Interrupt (a full attack's earlier swings report None). The queue
    /// round is never an execution round, and an eventual IsActed value read
    /// later says nothing about when it acted. A legal attack that misses is
    /// still an action; damage is not needed.
    /// </summary>
    internal static class SummonActionTimingPolicy
    {
        internal static bool IsLegalAction(SummonCommandTiming timing)
        {
            if (timing == null) throw new ArgumentNullException("timing");
            // Native UnitAttack.OnAction returns None after each attack of an
            // iterative/full attack except the last (Success only when the
            // sequence completes), so None at the act boundary is an executed
            // attack. Fail and Interrupt are not legal actions.
            return timing.ActedRound.HasValue &&
                !string.Equals(timing.ResultAtAct, "Fail", StringComparison.Ordinal) &&
                !string.Equals(timing.ResultAtAct, "Interrupt", StringComparison.Ordinal);
        }

        internal static SummonCommandTiming FirstAction(IEnumerable<SummonCommandTiming> commands)
        {
            if (commands == null) throw new ArgumentNullException("commands");
            return commands.Where(IsLegalAction)
                .OrderBy(value => value.ActedGameSeconds ?? double.MaxValue)
                .ThenBy(value => value.Sequence)
                .FirstOrDefault();
        }

        internal static SummonFirstActionDisposition Classify(
            IEnumerable<SummonCommandTiming> commands, int castRound)
        {
            if (commands == null) throw new ArgumentNullException("commands");
            SummonCommandTiming[] all = commands.ToArray();
            if (all.Length == 0) return SummonFirstActionDisposition.NoCommand;
            SummonCommandTiming first = FirstAction(all);
            if (first == null) return SummonFirstActionDisposition.QueuedNeverActed;
            return first.ActedRound.Value == castRound ?
                SummonFirstActionDisposition.ActedInCastRound :
                SummonFirstActionDisposition.ActedLater;
        }
    }
}
