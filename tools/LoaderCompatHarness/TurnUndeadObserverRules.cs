using System;
using System.Collections.Generic;
using System.Linq;

namespace KmgLoaderCompatHarness
{
    /// <summary>One recorded Turn Undead saving throw (from the rule trace).</summary>
    internal sealed class TurnSaveRecord
    {
        internal TurnSaveRecord(string type, int dc, bool passed, int? natural)
        {
            Type = type;
            Dc = dc;
            Passed = passed;
            Natural = natural;
        }

        internal string Type { get; private set; }
        internal int Dc { get; private set; }
        internal bool Passed { get; private set; }
        internal int? Natural { get; private set; }

        public override string ToString()
        {
            return Type + " DC " + Dc + " " + (Passed ? "passed" : "failed") + (Natural.HasValue ? " (natural " + Natural.Value + ")" : "");
        }
    }

    /// <summary>
    /// Engine-free judgments the Turn Undead probes apply to what they recorded. Shared with the
    /// domain suite (linked source) so the judgments themselves are tested against constructed
    /// cases and retained traces. Fixture choices (lowered or raised Will totals) only make an
    /// outcome likely; the judgments follow the save that was actually rolled.
    /// </summary>
    internal static class TurnUndeadObserverRules
    {
        internal const string CoveragePrefix = "coverage incomplete: ";

        /// <summary>
        /// The initial burst for one unit. An excluded unit rolls nothing and gains nothing. An
        /// included unit rolls exactly one Will save at the expected DC, and has the effect exactly
        /// when that save failed. <paramref name="outcome"/> is "excluded", "failed", "passed" or
        /// null when the roll could not be judged.
        /// </summary>
        internal static List<string> JudgeInitial(string label, bool included, IList<TurnSaveRecord> saves,
            bool hasEffect, int expectedDc, out string outcome)
        {
            var problems = new List<string>();
            outcome = null;
            saves = saves ?? new TurnSaveRecord[0];
            if (!included)
            {
                if (saves.Count != 0) problems.Add(label + " was excluded but rolled " + saves.Count + " Turn save(s)");
                if (hasEffect) problems.Add(label + " was excluded but has the Turn effect");
                if (problems.Count == 0) outcome = "excluded";
                return problems;
            }
            if (saves.Count != 1)
            {
                problems.Add(label + " made " + saves.Count + " initial Turn saves, expected exactly one");
                return problems;
            }
            TurnSaveRecord save = saves[0];
            if (!string.Equals(save.Type, "Will", StringComparison.Ordinal) || save.Dc != expectedDc)
                problems.Add(label + " save was " + save.Type + " DC " + save.Dc + ", expected Will DC " + expectedDc);
            if (save.Passed && hasEffect) problems.Add(label + " has the Turn effect after a successful save (" + save + ")");
            if (!save.Passed && !hasEffect) problems.Add(label + " failed its save (" + save + ") but has no Turn effect");
            outcome = save.Passed ? "passed" : "failed";
            return problems;
        }

        /// <summary>
        /// A required initial outcome the fixture aimed for but the dice did not produce: missing
        /// scenario coverage, reported apart from product behavior.
        /// </summary>
        internal static string CoverageGap(string label, string required, string observed)
        {
            return observed == null || observed == required ? null :
                CoveragePrefix + label + " needed a " + required + " initial save to observe the next behavior; the roll " + observed;
        }

        /// <summary>
        /// Repeat saves after an initial failure. A mindless unit makes none and keeps the effect.
        /// An intelligent unit keeps the effect while every repeat save fails and loses it at its
        /// first success, which must be its last Turn save: an early success (a natural 20 against a
        /// lowered total included) is recovery, not a defect.
        /// </summary>
        internal static List<string> JudgeRepeats(string label, bool intelligent, IList<TurnSaveRecord> repeats,
            bool effectNow, out bool recovered)
        {
            var problems = new List<string>();
            repeats = repeats ?? new TurnSaveRecord[0];
            recovered = false;
            if (!intelligent)
            {
                if (repeats.Count != 0) problems.Add(label + " is mindless but made " + repeats.Count + " repeat save(s)");
                if (!effectNow) problems.Add(label + " lost the effect without a repeat save");
                return problems;
            }
            int first = -1;
            for (int index = 0; index < repeats.Count; index++)
                if (repeats[index].Passed) { first = index; break; }
            if (first < 0)
            {
                if (!effectNow) problems.Add(label + " lost the effect although every repeat save failed (" + Describe(repeats) + ")");
                return problems;
            }
            if (first != repeats.Count - 1)
                problems.Add(label + " kept rolling repeat saves after succeeding (" + Describe(repeats) + ")");
            if (effectNow) problems.Add(label + " kept the effect after a successful repeat save (" + Describe(repeats) + ")");
            recovered = problems.Count == 0;
            return problems;
        }

        /// <summary>
        /// Exact membership of the Turn identities on one modifier list: every expected identity
        /// exactly once, and no other Turn identity. A duplicate standing in for a missing identity
        /// therefore fails even though the total count matches.
        /// </summary>
        internal static List<string> MembershipProblems(string list, IList<string> expected,
            IEnumerable<string> observedTurnIdentities)
        {
            var problems = new List<string>();
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string guid in observedTurnIdentities ?? new string[0])
            {
                int count;
                counts.TryGetValue(guid, out count);
                counts[guid] = count + 1;
            }
            foreach (string guid in expected)
            {
                int count;
                counts.TryGetValue(guid, out count);
                if (count != 1) problems.Add(list + ": Turn identity " + guid + " appears " + count + " time(s), expected once");
            }
            foreach (string guid in counts.Keys.Where(value => !expected.Contains(value)).OrderBy(value => value, StringComparer.Ordinal))
                problems.Add(list + ": unexpected Turn identity " + guid);
            return problems;
        }

        /// <summary>Cleanup failures that are only the harness's own injected test failure.</summary>
        internal static bool OnlyInjected(IEnumerable<string> failures, string marker)
        {
            string[] all = (failures ?? new string[0]).ToArray();
            return all.Length > 0 && all.All(value => value != null && value.IndexOf(marker, StringComparison.Ordinal) >= 0);
        }

        private static string Describe(IEnumerable<TurnSaveRecord> saves)
        {
            return string.Join(", ", saves.Select(value => value.ToString()).ToArray());
        }
    }
}
