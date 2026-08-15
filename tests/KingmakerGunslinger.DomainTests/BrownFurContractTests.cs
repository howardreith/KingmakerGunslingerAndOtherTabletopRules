using System;
using System.Collections.Generic;
using KingmakerGunslinger.BrownFur;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BrownFurContractTests
    {
        internal static void NormalProgressionIsExact()
        {
            CotwProgressionDecision decision = CotwProgressionPolicy.Resolve(
                new[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 });
            Assertions.True(decision.Compatible &&
                decision.Shape == CotwProgressionShape.Normal &&
                decision.PowerfulChangeReplacementLevel == 3 &&
                decision.ShareTransmutationReplacementLevel == 9,
                "Normal CotW progression must replace exploits at levels 3 and 9.");
        }

        internal static void BalanceProgressionIsExact()
        {
            CotwProgressionDecision decision = CotwProgressionPolicy.Resolve(
                new[] { 1, 4, 7, 10, 13, 16, 19 });
            Assertions.True(decision.Compatible &&
                decision.Shape == CotwProgressionShape.BalanceFixes &&
                decision.PowerfulChangeReplacementLevel == 4 &&
                decision.ShareTransmutationReplacementLevel == 10,
                "Balance-fixes CotW progression must replace exploits at 4 and 10.");
        }

        internal static void UnknownProgressionsFailClosed()
        {
            AssertRejected(null, "null schedule");
            AssertRejected(new int[0], "missing schedule");
            AssertRejected(new[] { 1, 3, 3, 9 }, "duplicate schedule");
            AssertRejected(new[] { 1, 5, 3, 9 }, "unordered schedule");
            AssertRejected(new[] { 0, 3, 9 }, "out-of-range schedule");
            AssertRejected(new[] { 1, 3, 5, 7, 9 }, "partial schedule");
            AssertRejected(new[] { 1, 3, 6, 9, 12, 15, 18 },
                "unknown future schedule");
        }

        internal static void AbsentCotwIsUnavailable()
        {
            CotwArcanistContractDecision decision =
                CotwArcanistContractPolicy.Evaluate(null);
            Assertions.True(decision.Availability ==
                CotwContractAvailability.Unavailable && !decision.IsCompatible,
                "Absent CotW must be unavailable rather than package-fatal.");
        }

        internal static void CompleteContractIsCompatible()
        {
            CotwArcanistContractDecision normal =
                CotwArcanistContractPolicy.Evaluate(Valid(false));
            CotwArcanistContractDecision balance =
                CotwArcanistContractPolicy.Evaluate(Valid(true));
            Assertions.True(normal.IsCompatible && balance.IsCompatible &&
                normal.Progression.Shape == CotwProgressionShape.Normal &&
                balance.Progression.Shape == CotwProgressionShape.BalanceFixes,
                "Both known structurally complete CotW contracts must be accepted.");
        }

        internal static void EveryRequiredSurfaceFailsClosed()
        {
            var checks = new Dictionary<string, Action<CotwArcanistContractCandidate>>
            {
                { "cotw-active", value => value.CotwActive = false },
                { "assembly-identity", value => value.AssemblyIdentityResolved = false },
                { "arcanist-class", value => value.ArcanistClassResolved = false },
                { "arcanist-progression", value => value.ArcanistProgressionResolved = false },
                { "casting-spellbook", value => value.CastingSpellbookResolved = false },
                { "memorization-spellbook", value => value.MemorizationSpellbookResolved = false },
                { "arcane-reservoir", value => value.ReservoirResolved = false },
                { "exploit-selection", value => value.ExploitSelectionResolved = false },
                { "magical-supremacy", value => value.MagicalSupremacyResolved = false },
                { "shared-spells-signature", value => value.SharedSpellsContractResolved = false },
                { "archetype-array", value => value.ArchetypeArrayResolved = false },
                { "transmutation-inventory", value => value.TransmutationInventoryResolved = false }
            };
            foreach (KeyValuePair<string, Action<CotwArcanistContractCandidate>> check in checks)
            {
                CotwArcanistContractCandidate candidate = Valid(false);
                check.Value(candidate);
                CotwArcanistContractDecision decision =
                    CotwArcanistContractPolicy.Evaluate(candidate);
                Assertions.True(!decision.IsCompatible &&
                    decision.Availability == CotwContractAvailability.Incompatible &&
                    decision.FailedCheck == check.Key,
                    "Missing contract surface did not fail closed: " + check.Key);
            }
        }

        internal static void AmbiguousProgressionBlocksContract()
        {
            CotwArcanistContractCandidate candidate = Valid(false);
            candidate.ExploitBearingLevels = new[] { 1, 3, 3, 9 };
            CotwArcanistContractDecision decision =
                CotwArcanistContractPolicy.Evaluate(candidate);
            Assertions.True(!decision.IsCompatible &&
                decision.FailedCheck.StartsWith("exploit-progression:",
                    StringComparison.Ordinal),
                "An ambiguous exploit schedule must block Brown-Fur publication.");
        }

        private static void AssertRejected(IEnumerable<int> levels, string label)
        {
            CotwProgressionDecision decision = CotwProgressionPolicy.Resolve(levels);
            Assertions.True(!decision.Compatible &&
                decision.Shape == CotwProgressionShape.Unknown &&
                decision.PowerfulChangeReplacementLevel == 0 &&
                decision.ShareTransmutationReplacementLevel == 0,
                label + " must fail closed without replacement levels.");
        }

        private static CotwArcanistContractCandidate Valid(bool balance)
        {
            return new CotwArcanistContractCandidate
            {
                CotwDetected = true,
                CotwActive = true,
                AssemblyIdentityResolved = true,
                ArcanistClassResolved = true,
                ArcanistProgressionResolved = true,
                CastingSpellbookResolved = true,
                MemorizationSpellbookResolved = true,
                ReservoirResolved = true,
                ExploitSelectionResolved = true,
                MagicalSupremacyResolved = true,
                SharedSpellsContractResolved = true,
                ArchetypeArrayResolved = true,
                TransmutationInventoryResolved = true,
                ExploitBearingLevels = balance
                    ? new[] { 1, 4, 7, 10, 13, 16, 19 }
                    : new[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19 }
            };
        }
    }
}
