using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// S5 T5 / POL-06: pure verdicts over one conversion popup with Call of the
    /// Wild's metamagic conversion rows. The observer derives every expectation
    /// independently of the projection under test: the conversions from Call of
    /// the Wild's own row builder, each conversion's tier from the book's native
    /// conversion lists and its quantity from its own spawn expression, and the
    /// creatures each conversion should own from the placement, native-option,
    /// third-party and Spider Summoner contracts. The product's receipts are
    /// never the expectation.
    /// </summary>
    internal static class Pol06ConversionObservationVerdict
    {
        /// <summary>One conversion row Call of the Wild built for this source,
        /// identified by its quantity spell and exact metamagic.</summary>
        internal sealed class ExpectedConversion
        {
            internal string Key;
            internal int Tier;
            internal SummonMultiplicity Quantity;
            /// <summary>The creature rows this conversion must own, by identity.</summary>
            internal string[] Children = new string[0];
            /// <summary>The prepared parent's own tier, a duplicate key or an
            /// unverifiable row: Call of the Wild's row stays raw.</summary>
            internal bool ExpectRaw;
        }

        /// <summary>One projected child in the popup, under the actual
        /// conversion object (by instance) that owns it.</summary>
        internal sealed class ProjectedChild
        {
            internal int ConversionInstance;
            internal string ConversionKey;
            internal string Child;
        }

        internal sealed class Reading
        {
            internal ExpectedConversion[] Expected = new ExpectedConversion[0];
            internal ProjectedChild[] Projected = new ProjectedChild[0];
            /// <summary>Keys of raw additional summon rows in the popup.</summary>
            internal string[] Raw = new string[0];
            /// <summary>Non-summon rows (cure, inflict, ability rows) Call of the
            /// Wild built and the popup holds; compared as multisets.</summary>
            internal string[] ExpectedRetained = new string[0], ActualRetained = new string[0];
        }

        /// <summary>Every per-conversion association problem. Empty means each
        /// expected conversion owns exactly its own creatures.</summary>
        internal static IReadOnlyList<string> AssociationProblems(Reading reading)
        {
            var problems = new List<string>();
            var expected = reading.Expected ?? new ExpectedConversion[0];
            var projected = reading.Projected ?? new ProjectedChild[0];
            var raw = reading.Raw ?? new string[0];
            foreach (var duplicate in expected.GroupBy(value => value.Key, StringComparer.Ordinal).Where(value => value.Count() != 1))
                problems.Add("expected-key-not-unique:" + duplicate.Key);
            var byKey = expected.GroupBy(value => value.Key, StringComparer.Ordinal).ToDictionary(value => value.Key, value => value.First(), StringComparer.Ordinal);
            var instances = projected.GroupBy(value => value.ConversionInstance).ToArray();
            foreach (var instance in instances)
            {
                string[] keys = instance.Select(value => value.ConversionKey).Distinct(StringComparer.Ordinal).ToArray();
                if (keys.Length != 1) { problems.Add("conversion-instance-has-several-keys:" + instance.Key); continue; }
                if (!byKey.ContainsKey(keys[0])) problems.Add("unexpected-conversion:" + keys[0]);
            }
            foreach (var key in instances.GroupBy(value => value.First().ConversionKey, StringComparer.Ordinal).Where(value => value.Count() != 1))
                problems.Add("conversion-projected-by-several-instances:" + key.Key);
            foreach (var conversion in expected)
            {
                ProjectedChild[] owned = projected.Where(value => value.ConversionKey == conversion.Key).ToArray();
                int rawCount = raw.Count(value => value == conversion.Key);
                if (conversion.ExpectRaw)
                {
                    if (owned.Length != 0) problems.Add("expected-raw-but-projected:" + conversion.Key);
                    if (rawCount != 1) problems.Add("expected-raw-count:" + conversion.Key + ":" + rawCount);
                    continue;
                }
                if (rawCount != 0) problems.Add("left-raw:" + conversion.Key);
                string[] want = conversion.Children ?? new string[0];
                if (want.Length == 0)
                {
                    // A Herald row whose every creature is forbidden is withdrawn.
                    if (owned.Length != 0) problems.Add("withdrawn-conversion-projected:" + conversion.Key);
                    continue;
                }
                string[] actual = owned.Select(value => value.Child).ToArray();
                foreach (string child in want.Where(value => !actual.Contains(value, StringComparer.Ordinal)))
                    problems.Add("missing:" + conversion.Key + ":" + child);
                foreach (string child in actual.Where(value => !want.Contains(value, StringComparer.Ordinal)).Distinct(StringComparer.Ordinal))
                    problems.Add("unexpected:" + conversion.Key + ":" + child);
                foreach (var child in actual.GroupBy(value => value, StringComparer.Ordinal).Where(value => value.Count() != 1))
                    problems.Add("duplicate:" + conversion.Key + ":" + child.Key);
            }
            foreach (string key in raw.Where(value => !byKey.ContainsKey(value)).Distinct(StringComparer.Ordinal))
                problems.Add("unexpected-raw:" + key);
            if (!SameMultiset(reading.ExpectedRetained ?? new string[0], reading.ActualRetained ?? new string[0]))
                problems.Add("retained-rows-changed");
            return problems;
        }

        /// <summary>The weaker aggregate check, kept as completeness evidence:
        /// the union of projected creatures equals the union of expected ones.</summary>
        internal static bool AggregateComplete(Reading reading)
        {
            var want = (reading.Expected ?? new ExpectedConversion[0]).Where(value => !value.ExpectRaw).SelectMany(value => value.Children ?? new string[0]).ToArray();
            var actual = (reading.Projected ?? new ProjectedChild[0]).Select(value => value.Child).ToArray();
            return SameMultiset(want, actual);
        }

        /// <summary>The count a controlled roll must summon, without Superior
        /// Summoning: one; the forced d3; the forced d4 plus one.</summary>
        internal static int ExpectedCount(SummonMultiplicity quantity, int forcedDie)
        {
            switch (quantity)
            {
                case SummonMultiplicity.One: return 1;
                case SummonMultiplicity.OneD3:
                    if (forcedDie < 1 || forcedDie > 3) throw new ArgumentOutOfRangeException("forcedDie");
                    return forcedDie;
                case SummonMultiplicity.OneD4PlusOne:
                    if (forcedDie < 1 || forcedDie > 4) throw new ArgumentOutOfRangeException("forcedDie");
                    return forcedDie + 1;
                default: throw new ArgumentOutOfRangeException("quantity");
            }
        }

        /// <summary>The quantity a single spawn's count expression states:
        /// no die (a fixed one), a d3, or a d4 (with its +1).</summary>
        internal static SummonMultiplicity? QuantityOfCountDice(string countDice)
        {
            if (countDice == "Zero" || countDice == "One") return SummonMultiplicity.One;
            if (countDice == "D3") return SummonMultiplicity.OneD3;
            if (countDice == "D4") return SummonMultiplicity.OneD4PlusOne;
            return null;
        }

        /// <summary>Rows a player cannot tell apart: the same label and the
        /// same native metamagic list, but a different creature or conversion.</summary>
        internal static IReadOnlyList<string> IndistinguishableRows(IEnumerable<Tuple<string, string, string>> labelMetamagicSemantics)
        {
            return labelMetamagicSemantics.GroupBy(value => value.Item1 + "\u0001" + value.Item2, StringComparer.Ordinal)
                .Where(value => value.Select(row => row.Item3).Distinct(StringComparer.Ordinal).Count() > 1)
                .Select(value => value.First().Item1 + " [" + value.First().Item2 + "]").ToArray();
        }

        private static bool SameMultiset(IEnumerable<string> left, IEnumerable<string> right)
        {
            var a = left.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var b = right.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            return a.SequenceEqual(b, StringComparer.Ordinal);
        }
    }
}
