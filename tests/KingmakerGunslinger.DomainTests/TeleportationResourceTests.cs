using System;
using System.Linq;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationResourceTests
    {
        private static TeleportPreparedSlotState Slot(bool match = true, bool available = true, bool opposition = false, params int[] links)
        { return new TeleportPreparedSlotState(match, available, opposition, links); }
        private static int[] Slots(int level = 5, int count = 2)
        { var result = new int[10]; result[level] = count; return result; }
        private static TeleportExpenditure Delta(bool[] before, bool[] after, params int[] spent)
        { return TeleportResourceDeltaPolicy.Observe(true, new int[10], new int[10], before, after, -1, spent, true); }

        internal static void PreparedSingleUsesAndReverseOrder()
        {
            var decision = TeleportPreparedPoolPolicy.Analyze(new[] { Slot(), Slot(), Slot() });
            Assertions.Equal(3, decision.Uses, "Three actual preparations remain three uses, independent of equivalent AbilityData wrappers.");
            Assertions.Equal(2, decision.SelectedOrdinal, "Native backwards scan deterministically chooses the last valid use.");
            Assertions.Equal("2", string.Join(",", decision.SpentOrdinals), "One physical slot for an ordinary preparation.");
        }
        internal static void OppositionGroupsCountOnce()
        {
            var decision = TeleportPreparedPoolPolicy.Analyze(new[] { Slot(true, true, true, 0, 1), Slot(true, true, true, 0, 1), Slot() });
            Assertions.Equal(2, decision.Uses, "Two opposition slots plus one ordinary slot are two prepared uses.");
            var opposition = TeleportPreparedPoolPolicy.Analyze(new[] { Slot(), Slot(true, true, true, 1, 2), Slot(true, true, true, 1, 2) });
            Assertions.Equal(2, opposition.Uses, "Mixed physical group sizes are counted correctly.");
            Assertions.Equal(2, opposition.SelectedOrdinal, "Native selection may be the non-main member of an opposition group.");
            Assertions.Equal("1,2", string.Join(",", opposition.SpentOrdinals), "Native Spend clears every linked member.");
        }
        internal static void SpentAndUnrelatedIgnored()
        {
            var decision = TeleportPreparedPoolPolicy.Analyze(new[] { Slot(), Slot(false), Slot(true, false), Slot(false, false) });
            Assertions.Equal(1, decision.Uses, "Known spells, other preparations and already spent slots add no use.");
            Assertions.Equal(0, decision.SelectedOrdinal, "Native candidate remains the one exact available preparation.");
            var empty = TeleportPreparedPoolPolicy.Analyze(new TeleportPreparedSlotState[0]);
            Assertions.Equal(0, empty.Uses, "Empty real book has no source.");
            Assertions.Equal(-1, empty.SelectedOrdinal, "No use means no selected physical slot.");
        }
        internal static void PartialAndBrokenGroupsFailClosed()
        {
            Assertions.True(TeleportPreparedPoolPolicy.Analyze(new[] { Slot(true, false, true, 0, 1), Slot(true, true, true, 0, 1) }) == null,
                "Partially spent opposition group cannot prove a complete use.");
            Assertions.True(TeleportPreparedPoolPolicy.Analyze(new[] { Slot(false, true, true, 0, 1), Slot(true, true, true, 0, 1) }) == null,
                "Linked unrelated spell cannot be silently consumed.");
            Assertions.True(TeleportPreparedPoolPolicy.Analyze(new[] { Slot(true, true, true, 0, 1), Slot(true, true, true, 1, 0) }) == null,
                "Contradictory native group topology fails closed.");
            Assertions.True(TeleportPreparedPoolPolicy.Analyze(new[] { Slot(true, true, false, 0, 1), Slot(true, true, false, 0, 1) }) == null,
                "Unsupported non-opposition multi-slot source is absent.");
        }
        internal static void UnknownNativeSlotsFailClosed()
        {
            Assertions.True(TeleportPreparedPoolPolicy.Analyze(null) == null, "Missing native collection.");
            Assertions.True(TeleportPreparedPoolPolicy.Analyze(new TeleportPreparedSlotState[] { null }) == null, "Missing physical slot.");
            foreach (int[] links in new[] { new[] { -1 }, new[] { 1 }, new[] { 0, 0 } })
                Assertions.True(TeleportPreparedPoolPolicy.Analyze(new[] { Slot(true, true, true, links) }) == null,
                    "Missing, foreign or duplicate linked slots are not usable.");
        }
        internal static void SpontaneousDeltaIsExact()
        {
            var prepared = new[] { true, false };
            Assertions.Equal(TeleportExpenditure.ExactlyOne, TeleportResourceDeltaPolicy.Observe(true, Slots(7, 2), Slots(7, 1),
                prepared, prepared, 7, new int[0], true), "One seventh-level slot in the selected book.");
            Assertions.Equal(TeleportExpenditure.None, TeleportResourceDeltaPolicy.Observe(true, Slots(), Slots(),
                prepared, prepared, 5, new int[0], true), "Opening, cancellation or a rejected native Spend changes nothing.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, TeleportResourceDeltaPolicy.Observe(true, Slots(7, 2), Slots(7, 0),
                prepared, prepared, 7, new int[0], true), "Two-slot expenditure cannot permit an effect.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, TeleportResourceDeltaPolicy.Observe(true, Slots(7, 2), Slots(7, 1),
                prepared, prepared, 5, new int[0], true), "Wrong-level expenditure fails closed.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, TeleportResourceDeltaPolicy.Observe(true, Slots(7, 2), Slots(7, 1),
                prepared, new[] { false, false }, 7, new int[0], true), "Unrelated prepared expenditure must also be detected.");
        }
        internal static void PreparedDeltaIsExact()
        {
            Assertions.Equal(TeleportExpenditure.ExactlyOne, Delta(new[] { true, true, true }, new[] { true, false, false }, 1, 2),
                "One opposition preparation spends both physical members.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, Delta(new[] { true, true, true }, new[] { false, false, false }, 1, 2),
                "Unrelated slot expenditure is not a one-use success.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, Delta(new[] { true, true }, new[] { true, false }, 0, 1),
                "Partial group expenditure is ambiguous.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, Delta(new[] { false, true }, new[] { true, false }, 1),
                "Simultaneous restoration elsewhere cannot hide an invalid delta.");
            Assertions.Equal(TeleportExpenditure.None, Delta(new[] { true, true }, new[] { true, true }, 1),
                "Exact compensation returns the complete captured pool to its original state.");
        }
        internal static void TopologyChangesFailClosed()
        {
            Assertions.Equal(TeleportExpenditure.Ambiguous, TeleportResourceDeltaPolicy.Observe(false, Slots(), Slots(5, 1),
                new bool[0], new bool[0], 5, new int[0], true), "Replaced book/list/slot/ability/link identity invalidates arithmetic proof.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, TeleportResourceDeltaPolicy.Observe(true, new int[9], new int[9],
                new bool[0], new bool[0], 5, new int[0], true), "Changed native level-array contract.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, Delta(new[] { true }, new[] { false }, 0, 0), "Duplicate expected members cannot be counted as one.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, Delta(new[] { true }, new[] { false }, 1), "Missing expected slot.");
        }
        internal static void OtherOperationCannotBeCompensated()
        {
            Assertions.Equal(TeleportExpenditure.None, TeleportResourceDeltaPolicy.Observe(true, Slots(), Slots(),
                new bool[0], new bool[0], 5, new int[0], false), "No native spend and no change is unspent.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, TeleportResourceDeltaPolicy.Observe(true, Slots(), Slots(5, 1),
                new bool[0], new bool[0], 5, new int[0], false), "Another operation's one-slot spend cannot be attributed or refunded.");
            Assertions.Equal(TeleportExpenditure.Ambiguous, TeleportResourceDeltaPolicy.Observe(true, new int[10], new int[10],
                new[] { true }, new[] { false }, -1, new[] { 0 }, false), "Another operation's prepared spend cannot be refunded.");
        }
        internal static void AllSingleUseDeltas()
        {
            for (int beforeMask = 1; beforeMask < 16; beforeMask++)
            {
                bool[] before = Enumerable.Range(0, 4).Select(index => (beforeMask & (1 << index)) != 0).ToArray();
                int selected = Array.FindLastIndex(before, value => value);
                for (int afterMask = 0; afterMask < 16; afterMask++)
                {
                    bool[] after = Enumerable.Range(0, 4).Select(index => (afterMask & (1 << index)) != 0).ToArray();
                    TeleportExpenditure expected = afterMask == beforeMask ? TeleportExpenditure.None :
                        afterMask == (beforeMask & ~(1 << selected)) ? TeleportExpenditure.ExactlyOne : TeleportExpenditure.Ambiguous;
                    Assertions.Equal(expected, Delta(before, after, selected), "Every four-slot transition must match the selected actual use exactly.");
                }
            }
        }
    }
}
