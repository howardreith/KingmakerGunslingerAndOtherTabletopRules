using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Spells.Teleportation
{
    internal sealed class TeleportPreparedSlotState
    {
        internal TeleportPreparedSlotState(bool matchesSource, bool available, bool opposition, IEnumerable<int> links)
        { MatchesSource = matchesSource; Available = available; Opposition = opposition;
            Links = links == null ? null : links.ToArray(); }
        internal bool MatchesSource { get; private set; }
        internal bool Available { get; private set; }
        internal bool Opposition { get; private set; }
        internal int[] Links { get; private set; }
    }
    internal sealed class TeleportPreparedPoolDecision
    {
        internal TeleportPreparedPoolDecision(int uses, int selected, int[] spent)
        { Uses = uses; SelectedOrdinal = selected; SpentOrdinals = Array.AsReadOnly(spent); }
        internal int Uses { get; private set; }
        internal int SelectedOrdinal { get; private set; }
        internal IReadOnlyList<int> SpentOrdinals { get; private set; }
    }
    internal static class TeleportPreparedPoolPolicy
    {
        // Ordinals are positions in this invocation's actual native slot list,
        // never persisted identities. Native SpendInternal scans that list backwards.
        internal static TeleportPreparedPoolDecision Analyze(IReadOnlyList<TeleportPreparedSlotState> slots)
        {
            if (slots == null || slots.Any(value => value == null)) return null;
            var counted = new HashSet<int>();
            int uses = 0, selected = -1;
            int[] selectedGroup = new int[0];
            for (int index = slots.Count - 1; index >= 0; index--)
            {
                TeleportPreparedSlotState slot = slots[index];
                if (!slot.MatchesSource || !slot.Available || counted.Contains(index)) continue;
                int[] group = slot.Opposition && slot.Links != null && slot.Links.Length != 0 ?
                    slot.Links : new[] { index };
                if (group.Length == 0 || group.Distinct().Count() != group.Length || !group.Contains(index) ||
                    group.Any(value => value < 0 || value >= slots.Count)) return null;
                // Multi-slot non-opposition groups are not a proven source for these
                // unmodified base spells. Reject overlapping or partially spent groups.
                if (!slot.Opposition && slot.Links != null &&
                    (slot.Links.Length > 1 || (slot.Links.Length == 1 && slot.Links[0] != index))) return null;
                foreach (int member in group)
                {
                    TeleportPreparedSlotState other = slots[member];
                    if (counted.Contains(member) || !other.MatchesSource || !other.Available || other.Opposition != slot.Opposition)
                        return null;
                    int[] otherGroup = other.Opposition && other.Links != null && other.Links.Length != 0 ? other.Links : new[] { member };
                    if (!otherGroup.SequenceEqual(group)) return null;
                }
                if (selected < 0) { selected = index; selectedGroup = group.ToArray(); }
                foreach (int member in group) counted.Add(member);
                uses++;
            }
            return new TeleportPreparedPoolDecision(uses, selected, selectedGroup);
        }
    }

    internal static class TeleportResourceDeltaPolicy
    {
        internal static TeleportExpenditure Observe(bool exactTopology, int[] beforeSpontaneous, int[] afterSpontaneous,
            bool[] beforePrepared, bool[] afterPrepared, int spontaneousLevel, IReadOnlyList<int> expectedPrepared, bool requestInvokedNativeSpend)
        {
            if (!exactTopology || beforeSpontaneous == null || afterSpontaneous == null ||
                beforeSpontaneous.Length != 10 || afterSpontaneous.Length != 10 ||
                beforeSpontaneous.Any(value => value < 0) || afterSpontaneous.Any(value => value < 0) ||
                beforePrepared == null || afterPrepared == null || beforePrepared.Length != afterPrepared.Length)
                return TeleportExpenditure.Ambiguous;
            if (beforeSpontaneous.SequenceEqual(afterSpontaneous) && beforePrepared.SequenceEqual(afterPrepared))
                return TeleportExpenditure.None;
            // An equal-sized change made by another operation is not this request's
            // expenditure and must never be compensated by this request.
            if (!requestInvokedNativeSpend) return TeleportExpenditure.Ambiguous;
            if (spontaneousLevel >= 1 && spontaneousLevel <= 9)
            {
                if (!beforePrepared.SequenceEqual(afterPrepared)) return TeleportExpenditure.Ambiguous;
                for (int level = 0; level < 10; level++)
                    if (afterSpontaneous[level] != beforeSpontaneous[level] - (level == spontaneousLevel ? 1 : 0))
                        return TeleportExpenditure.Ambiguous;
                return TeleportExpenditure.ExactlyOne;
            }
            if (spontaneousLevel != -1 || !beforeSpontaneous.SequenceEqual(afterSpontaneous) || expectedPrepared == null ||
                expectedPrepared.Count == 0 || expectedPrepared.Distinct().Count() != expectedPrepared.Count ||
                expectedPrepared.Any(value => value < 0 || value >= beforePrepared.Length || !beforePrepared[value]))
                return TeleportExpenditure.Ambiguous;
            for (int index = 0; index < beforePrepared.Length; index++)
                if (afterPrepared[index] != (beforePrepared[index] && !expectedPrepared.Contains(index)))
                    return TeleportExpenditure.Ambiguous;
            return TeleportExpenditure.ExactlyOne;
        }
    }
}
