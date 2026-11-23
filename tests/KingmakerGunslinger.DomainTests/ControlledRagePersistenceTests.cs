using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KingmakerGunslinger.UrbanBarbarian;

namespace KingmakerGunslinger.DomainTests
{
    // S6 T4 / RISK-03 (A1): the total reader and the persisted pair
    // (_state, _rejectedStates) of UnitPartControlledRageSelection, driven
    // through the production Restore / Serialize / MergeRetained cycle the
    // part's PostLoad and PreSave perform. Native PreSave/PostLoad and the
    // real save serializer are exercised by the loader-compat session
    // risk03-controlled-rage-persistence, not here.
    internal static class ControlledRagePersistenceTests
    {
        private static readonly ControlledRageTier[] Tiers = {
            ControlledRageTier.Ordinary, ControlledRageTier.Greater,
            ControlledRageTier.Mighty };

        internal static void EveryReleasedStateRestoresExactlyAsTheStrictParser()
        {
            int count = 0;
            foreach (ControlledRageAllocation ordinary in
                ControlledRageAllocationPolicy.Generate(ControlledRageTier.Ordinary))
            foreach (ControlledRageAllocation greater in WithAbsent(
                ControlledRageTier.Greater))
            foreach (ControlledRageAllocation mighty in WithAbsent(
                ControlledRageTier.Mighty))
            {
                var state = new ControlledRageSelectionState();
                Assertions.True(state.TrySelectExact(ControlledRageTier.Ordinary,
                    ordinary, false), "Released ordinary selection was refused.");
                if (greater != null)
                {
                    state.Unlock(ControlledRageTier.Greater);
                    state.TrySelectExact(ControlledRageTier.Greater, greater, false);
                }
                if (mighty != null)
                {
                    state.Unlock(ControlledRageTier.Mighty);
                    state.TrySelectExact(ControlledRageTier.Mighty, mighty, false);
                }
                string persisted = state.Serialize();
                ControlledRageSelectionState strict =
                    ControlledRageSelectionState.Parse(persisted);
                ControlledRageSelectionState restored =
                    ControlledRageSelectionState.Restore(persisted, null);
                Assertions.Equal(persisted, restored.Serialize(),
                    "A released Controlled Rage state did not restore exactly.");
                AssertSameSelections(strict, restored, persisted);
                Assertions.True(restored.PendingRejected == null &&
                    !restored.BlockedTiers.Any(),
                    "A released Controlled Rage state was treated as rejected: " + persisted);
                Assertions.True(ReferenceEquals(null, restored.MergeRetained(null)),
                    "A released state created a retained-payload list: " + persisted);
                count++;
            }
            Assertions.Equal(6 * 11 * 16, count,
                "The released Controlled Rage state space was not fully enumerated.");

            // Forms the released strict reader also accepts (unordered tiers,
            // leading zeros). No separate legacy schema was released; these are
            // the only other inputs the v0.0.95+ reader admitted.
            foreach (string accepted in new[] {
                "6:2,2,2;4:0,0,4", "8:2,2,4;4:0,4,0;6:0,6,0",
                "04:00,00,04", "4:0,0,4;06:2,02,2" })
            {
                ControlledRageSelectionState restored =
                    ControlledRageSelectionState.Restore(accepted, null);
                AssertSameSelections(ControlledRageSelectionState.Parse(accepted),
                    restored, accepted);
                Assertions.True(restored.PendingRejected == null &&
                    !restored.BlockedTiers.Any(),
                    "Strictly accepted released input was rejected: " + accepted);
            }
        }

        internal static void RestoreIsTotalAndAgreesWithTheStrictParser()
        {
            var random = new Random(20261122);
            const string alphabet = "0123456789468:;,,;:: v-";
            var inputs = new List<string>();
            for (int index = 0; index < 4000; index++)
            {
                int length = random.Next(0, 28);
                var text = new StringBuilder();
                for (int position = 0; position < length; position++)
                    text.Append(alphabet[random.Next(alphabet.Length)]);
                inputs.Add(text.ToString());
            }
            string[] seeds = { "4:0,0,4", "4:2,2,0;6:4,2,0", "4:0,4,0;6:2,2,2;8:4,2,2" };
            for (int index = 0; index < 4000; index++)
            {
                char[] text = seeds[random.Next(seeds.Length)].ToCharArray();
                int edits = 1 + random.Next(3);
                for (int edit = 0; edit < edits; edit++)
                    text[random.Next(text.Length)] = alphabet[random.Next(alphabet.Length)];
                inputs.Add(new string(text));
            }

            int accepted = 0, rejected = 0;
            foreach (string input in inputs)
            {
                ControlledRageSelectionState strict = null;
                try { strict = ControlledRageSelectionState.Parse(input); }
                catch (FormatException) { }
                catch (ArgumentException) { }
                ControlledRageSelectionState restored =
                    ControlledRageSelectionState.Restore(input, null);
                foreach (ControlledRageTier tier in Tiers)
                {
                    ControlledRageAllocation value = restored.SelectionFor(tier);
                    Assertions.True(value == null ||
                        ControlledRageAllocationPolicy.IsLegalForTier(tier, value),
                        "Restore activated an illegal allocation from \"" + input + "\".");
                    Assertions.False(value != null && restored.IsBlocked(tier),
                        "A tier was both active and blocked for \"" + input + "\".");
                }
                if (strict != null)
                {
                    accepted++;
                    AssertSameSelections(strict, restored, input);
                    Assertions.True(restored.PendingRejected == null &&
                        !restored.BlockedTiers.Any(),
                        "Strictly valid input was rejected: \"" + input + "\".");
                }
                else
                {
                    rejected++;
                    Assertions.Equal(input, restored.PendingRejected,
                        "Rejected input was not kept verbatim.");
                    List<string> retained = restored.MergeRetained(null);
                    Assertions.True(retained != null && retained.Count == 1 &&
                        string.Equals(retained[0], input, StringComparison.Ordinal),
                        "Rejected input was not added to the retained payloads.");
                    ControlledRageSelectionState reloaded =
                        ControlledRageSelectionState.Restore(restored.Serialize(),
                            retained);
                    AssertSameSelections(restored, reloaded, input);
                    Assertions.True(reloaded.PendingRejected == null &&
                        reloaded.BlockedTiers.SequenceEqual(restored.BlockedTiers),
                        "A migrated state did not reload identically for \"" + input + "\".");
                }
            }
            Assertions.True(accepted > 50 && rejected > 1000,
                "The fuzz corpus did not exercise both outcomes: accepted=" +
                accepted + ";rejected=" + rejected);
        }

        internal static void MalformedDuplicateAndUnknownInputKeepOnlyUnambiguousTiers()
        {
            // input, active tiers "tier=S,D,C" in tier order, blocked tiers
            var cases = new[]
            {
                Case("4:0,0,4;6:6,2,0", "4=0,0,4", "6"),
                Case("4:0,0,4;6:2,2,2;8:9,9,9", "4=0,0,4 6=2,2,2", "8"),
                Case("4:0,0,4;6:1,3,2", "4=0,0,4", "6"),
                Case("4:0,0,4;6:2147483646,2,2", "4=0,0,4", "6"),
                Case("4:0,0,4;6:2,2,2:extra", "4=0,0,4", "6"),
                Case("4:4,0,0;6:0,0,4", "4=4,0,0", "6"),
                // Conflicting duplicate group: neither record is chosen.
                Case("4:0,0,4;4:4,0,0;6:2,2,2", "6=2,2,2", "4"),
                Case("4:2,2,0;6:2,2,2;6:0,6,0;8:2,2,4", "4=2,2,0 8=2,2,4", "6"),
                // Identical duplicates have one meaning.
                Case("4:0,0,4;4:0,0,4", "4=0,0,4", ""),
                // The released writer always records ordinary.
                Case("6:2,2,2", "6=2,2,2", "4"),
                // Unknown tier key: retained, grants nothing, no taint.
                Case("4:0,0,4;10:4,4,2", "4=0,0,4", ""),
                // Unattributable content: the whole input's meaning is unknown.
                Case("v2{\"tiers\":[4,6]}", "", "4 6 8"),
                Case("4:0,0,4;garbage", "", "4 6 8"),
                Case("4:0,0,4;", "", "4 6 8"),
                Case("", "", "4 6 8"),
                Case("   ", "", "4 6 8"),
            };
            foreach (string[] entry in cases)
            {
                string input = entry[0];
                ControlledRageSelectionState state =
                    ControlledRageSelectionState.Restore(input, null);
                Assertions.Equal(entry[1], Describe(state),
                    "Active tiers differ for \"" + input + "\".");
                Assertions.Equal(entry[2], string.Join(" ", state.BlockedTiers
                    .Select(value => ((int)value).ToString(CultureInfo.InvariantCulture))
                    .ToArray()), "Blocked tiers differ for \"" + input + "\".");
                Assertions.Equal(input, state.PendingRejected,
                    "Rejected input was not retained verbatim.");
                foreach (ControlledRageTier tier in state.BlockedTiers.ToArray())
                {
                    Assertions.False(state.Unlock(tier),
                        "A blocked tier received a default unlock for \"" + input + "\".");
                    Assertions.True(state.SelectionFor(tier) == null,
                        "A blocked tier granted a selection for \"" + input + "\".");
                }
            }

            // A legitimately absent tier beside a blocked one still receives
            // its released first-unlock default.
            ControlledRageSelectionState sibling = ControlledRageSelectionState
                .Restore("4:0,0,4;6:6,2,0", null);
            Assertions.True(sibling.Unlock(ControlledRageTier.Mighty),
                "A legitimately absent tier was not unlocked.");
            Assertions.Equal("STR +8", sibling.SelectionFor(
                ControlledRageTier.Mighty).Name,
                "First unlock beside a blocked tier lost its default.");
            Assertions.Equal("4:0,0,4;8:8,0,0", sibling.Serialize(),
                "Active tiers did not persist in the released grammar.");
        }

        internal static void RetainedInputSurvivesOperationsAndResaves()
        {
            const string original = "4:0,0,4;4:4,0,0;6:2,2,2";
            Persisted pair = new Persisted(original, null);

            // First native PostLoad, then repeated PreSave/PostLoad cycles.
            pair = Cycle(pair);
            Assertions.Equal("6:2,2,2", pair.State,
                "Migration changed the unambiguous sibling.");
            AssertRetained(pair, original);
            for (int index = 0; index < 5; index++)
            {
                Persisted next = Cycle(pair);
                Assertions.Equal(pair.State, next.State,
                    "A repeated callback changed the active state.");
                Assertions.True(ReferenceEquals(pair.Rejected, next.Rejected),
                    "A repeated callback rewrote the retained payload list.");
                pair = next;
            }

            ControlledRageSelectionState state = pair.Read();
            Assertions.True(state.IsBlocked(ControlledRageTier.Ordinary),
                "The conflicting ordinary group did not stay blocked across reloads.");
            Assertions.False(state.Unlock(ControlledRageTier.Ordinary),
                "A reload turned the conflicting ordinary group into a default.");

            // Legitimate operations: first unlock, a valid selection, refusals.
            Assertions.True(state.Unlock(ControlledRageTier.Mighty),
                "A legitimate first unlock was refused.");
            ControlledRageAllocation greater = Allocation(ControlledRageTier.Greater, 4, 2, 0);
            Assertions.True(state.TrySelectExact(ControlledRageTier.Greater, greater, false),
                "A valid selection on an unambiguous tier was refused.");
            pair = pair.Write(state);
            Assertions.Equal("6:4,2,0;8:8,0,0", pair.State,
                "Legitimate operations did not persist exactly.");
            AssertRetained(pair, original);

            state = pair.Read();
            string before = pair.State;
            Assertions.False(state.TrySelectExact(ControlledRageTier.Greater,
                Allocation(ControlledRageTier.Greater, 2, 2, 2), true),
                "A selection was accepted while raging.");
            Assertions.False(state.TrySelectExact(ControlledRageTier.Greater,
                Allocation(ControlledRageTier.Ordinary, 2, 2, 0), false),
                "A wrong-tier allocation was accepted.");
            Assertions.Equal(before, state.Serialize(),
                "A refused selection changed the persisted state.");

            // An explicit player choice replaces the block; the rejected input
            // it replaces stays retained.
            ControlledRageAllocation chosen = Allocation(ControlledRageTier.Ordinary, 0, 2, 2);
            Assertions.True(state.TrySelectExact(ControlledRageTier.Ordinary, chosen, false),
                "An explicit selection for a blocked tier was refused.");
            pair = pair.Write(state);
            Assertions.Equal("4:0,2,2;6:4,2,0;8:8,0,0", pair.State,
                "The explicit selection did not persist exactly.");
            AssertRetained(pair, original);
            state = pair.Read();
            Assertions.True(!state.BlockedTiers.Any() && Equals(chosen,
                state.SelectionFor(ControlledRageTier.Ordinary)),
                "The explicit selection did not survive a reload.");

            // A second, different rejected input is appended, never merged.
            pair = Cycle(new Persisted("garbage", pair.Rejected));
            Assertions.True(pair.State == null && pair.Rejected.Count == 2 &&
                pair.Rejected[0] == original && pair.Rejected[1] == "garbage",
                "A second rejected input did not append verbatim.");
            state = pair.Read();
            Assertions.True(Tiers.All(state.IsBlocked) && !state.ActiveTiers.Any(),
                "Unattributable input did not block every tier.");
            Assertions.False(state.Unlock(ControlledRageTier.Ordinary),
                "Null state beside retained input was treated as legitimate absence.");

            // Legitimate absence keeps the released first-unlock default.
            foreach (List<string> none in new[] { null, new List<string>() })
            {
                ControlledRageSelectionState fresh =
                    ControlledRageSelectionState.Restore(null, none);
                Assertions.Equal("4:4,0,0", fresh.Serialize(),
                    "Legitimate absence lost the released ordinary default.");
                Assertions.True(fresh.PendingRejected == null,
                    "Legitimate absence was treated as rejected input.");
            }
        }

        // A1 review repair 1: a null active state beside retained history is
        // an already-used part whose current allocation data is missing, not
        // a fresh part. Ordinary and every tier the history claims or taints
        // stay blocked through reads, unlocks and resaves; the history is
        // never replayed; an explicit legal selection recovers a tier.
        internal static void NullStateBesideRetainedHistoryIsNeverANewAllocation()
        {
            var cases = new[]
            {
                // retained payload, blocked tiers, explicit recoveries in order
                Case("4:0,4,0;4:0,4,0", "4", "4"),
                Case("4:0,4,0;10:4,4,2", "4", "4"),
                Case("4:0,4,0;6:bad", "4 6", "4 6"),
                Case("4:0,4,0;6:2,2,2;8:2,2,4", "4 6 8", "4 6 8"),
            };
            // Every case runs, so one report names each failing input.
            var failures = new List<string>();
            foreach (string[] entry in cases)
            {
                try { NullStateBesideHistoryCase(entry); }
                catch (InvalidOperationException failure) { failures.Add(failure.Message); }
            }
            Assertions.True(failures.Count == 0, string.Join(" | ", failures.ToArray()));

            // Controls. Genuinely fresh absence keeps the first-unlock default.
            foreach (List<string> none in new[] { null, new List<string>() })
                Assertions.Equal("4:4,0,0", ControlledRageSelectionState.Restore(
                    null, none).Serialize(), "Fresh absence lost the released default.");

            // The ordinary migration of "4:bad" leaves no active tier; that alone
            // does not establish corruption of the higher tiers, which keep their
            // first-unlock defaults through later reloads.
            Persisted migrated = Cycle(new Persisted("4:bad", null));
            Assertions.True(migrated.State == null && migrated.Rejected.Count == 1,
                "\"4:bad\" did not migrate to retained history.");
            for (int cycle = 0; cycle < 3; cycle++)
            {
                ControlledRageSelectionState state = migrated.Read();
                Assertions.Equal("4", Blocked(state),
                    "Migrated \"4:bad\" blocked a legitimately absent higher tier.");
                Assertions.False(state.Unlock(ControlledRageTier.Ordinary),
                    "Migrated \"4:bad\" received an ordinary default.");
                state.Unlock(ControlledRageTier.Greater);
                state.Unlock(ControlledRageTier.Mighty);
                migrated = migrated.Write(state);
                Assertions.Equal("6:6,0,0;8:8,0,0", migrated.State,
                    "Legitimately absent higher tiers lost their defaults.");
            }
        }

        private static void NullStateBesideHistoryCase(string[] entry)
        {
            string payload = entry[0];
            var retained = new List<string> { payload };
            Persisted pair = new Persisted(null, retained);
            for (int cycle = 0; cycle < 4; cycle++)
            {
                ControlledRageSelectionState state = pair.Read();
                Assertions.True(!state.ActiveTiers.Any() &&
                    state.PendingRejected == null, "Null state beside \"" + payload +
                    "\" restored as a new allocation: " + state.Serialize() + ".");
                Assertions.False(state.Unlock(ControlledRageTier.Ordinary),
                    "Ordinary unlock beside \"" + payload + "\" created " +
                    state.Serialize() + ".");
                Assertions.Equal(entry[1], Blocked(state),
                    "Blocked tiers differ for null state beside \"" + payload + "\".");
                foreach (ControlledRageTier tier in state.BlockedTiers.ToArray())
                    Assertions.False(state.Unlock(tier),
                        "A blocked tier was defaulted beside \"" + payload + "\".");
                pair = pair.Write(state);
                Assertions.True(pair.State == null &&
                    ReferenceEquals(pair.Rejected, retained) &&
                    retained.Count == 1 && retained[0] == payload,
                    "A resave changed null state or retained history for \"" + payload + "\".");
            }

            var expected = new List<string>();
            foreach (string symbol in entry[2].Split(' '))
            {
                var tier = (ControlledRageTier)int.Parse(symbol,
                    CultureInfo.InvariantCulture);
                ControlledRageAllocation chosen =
                    ControlledRageAllocationPolicy.Generate(tier).Last();
                ControlledRageSelectionState state = pair.Read();
                Assertions.True(state.TrySelectExact(tier, chosen, false),
                    "An explicit legal selection did not recover tier " + symbol +
                    " beside \"" + payload + "\".");
                pair = pair.Write(state);
                expected.Add(string.Format(CultureInfo.InvariantCulture,
                    "{0}:{1},{2},{3}", (int)tier, chosen.Strength,
                    chosen.Dexterity, chosen.Constitution));
                for (int cycle = 0; cycle < 3; cycle++)
                {
                    ControlledRageSelectionState reread = pair.Read();
                    // The ordinary runtime read unlocks the unit's tier.
                    Assertions.False(reread.Unlock(ControlledRageTier.Ordinary),
                        "Ordinary unlock changed a recovered state.");
                    Persisted next = pair.Write(reread);
                    Assertions.Equal(string.Join(";", expected.ToArray()), next.State,
                        "An explicit recovery did not survive resaves beside \"" +
                        payload + "\".");
                    Assertions.True(ReferenceEquals(next.Rejected, retained) &&
                        retained.Count == 1 && retained[0] == payload,
                        "Recovery changed retained history for \"" + payload + "\".");
                    pair = next;
                }
            }
        }

        private static string Blocked(ControlledRageSelectionState state)
        {
            return string.Join(" ", state.BlockedTiers.Select(value =>
                ((int)value).ToString(CultureInfo.InvariantCulture)).ToArray());
        }

        private static Persisted Cycle(Persisted pair)
        {
            return pair.Write(pair.Read());
        }

        private static void AssertRetained(Persisted pair, string original)
        {
            Assertions.True(pair.Rejected != null && pair.Rejected.Count == 1 &&
                string.Equals(pair.Rejected[0], original, StringComparison.Ordinal),
                "The rejected input was not retained verbatim.");
        }

        private static ControlledRageAllocation Allocation(ControlledRageTier tier,
            int strength, int dexterity, int constitution)
        {
            return ControlledRageAllocationPolicy.Generate(tier).SingleOrDefault(
                value => value.Strength == strength && value.Dexterity == dexterity &&
                    value.Constitution == constitution) ??
                new ControlledRageAllocation(strength, dexterity, constitution);
        }

        private static IEnumerable<ControlledRageAllocation> WithAbsent(
            ControlledRageTier tier)
        {
            yield return null;
            foreach (ControlledRageAllocation value in
                ControlledRageAllocationPolicy.Generate(tier))
                yield return value;
        }

        private static void AssertSameSelections(ControlledRageSelectionState expected,
            ControlledRageSelectionState actual, string input)
        {
            foreach (ControlledRageTier tier in Tiers)
                Assertions.True(Equals(expected.SelectionFor(tier),
                    actual.SelectionFor(tier)),
                    "Tier " + (int)tier + " differs for \"" + input + "\".");
        }

        private static string Describe(ControlledRageSelectionState state)
        {
            return string.Join(" ", state.ActiveTiers.Select(tier =>
            {
                ControlledRageAllocation value = state.SelectionFor(tier);
                return string.Format(CultureInfo.InvariantCulture, "{0}={1},{2},{3}",
                    (int)tier, value.Strength, value.Dexterity, value.Constitution);
            }).ToArray());
        }

        private static string[] Case(string input, string active, string blocked)
        {
            return new[] { input, active, blocked };
        }

        private sealed class Persisted
        {
            internal Persisted(string state, List<string> rejected)
            {
                State = state;
                Rejected = rejected;
            }

            internal string State { get; private set; }
            internal List<string> Rejected { get; private set; }

            internal ControlledRageSelectionState Read()
            {
                return ControlledRageSelectionState.Restore(State, Rejected);
            }

            internal Persisted Write(ControlledRageSelectionState state)
            {
                return new Persisted(state.Serialize(), state.MergeRetained(Rejected));
            }
        }
    }
}
