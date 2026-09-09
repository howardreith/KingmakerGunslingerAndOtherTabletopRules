using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static class TeleportationArrivalTests
    {
        private const string A = "11111111111111111111111111111111";
        private const string B = "22222222222222222222222222222222";
        private const string C = "33333333333333333333333333333333";
        private static TeleportRouteBoundary[] Route()
        { return new[] { new TeleportRouteBoundary(A, 10, true), new TeleportRouteBoundary(B, 20, true), new TeleportRouteBoundary(C, 30, true) }; }

        internal static void CrossedBoundariesOnly()
        {
            Assertions.Equal(0, new TeleportOrdinaryArrivalObservation(0, Route()).Complete(9.99, false, null).Length, "Departure is not arrival.");
            Assertions.Equal(A, string.Join(",", new TeleportOrdinaryArrivalObservation(9, Route()).Complete(10, false, null)), "Inclusive arrival boundary.");
            Assertions.Equal(B, string.Join(",", new TeleportOrdinaryArrivalObservation(10, Route()).Complete(20, false, null)), "A prior exact boundary is not counted twice.");
            Assertions.Equal(A + "," + B + "," + C, string.Join(",", new TeleportOrdinaryArrivalObservation(0, Route()).Complete(30, false, null)), "Large frame records all actual route crossings.");
        }

        internal static void PartialStartAndRevisits()
        {
            Assertions.Equal(B, string.Join(",", new TeleportOrdinaryArrivalObservation(15, Route()).Complete(22, false, null)), "Native initialized partial-edge distance excludes earlier endpoints.");
            var loop = new[] { new TeleportRouteBoundary(A, 10, true), new TeleportRouteBoundary(B, 20, true), new TeleportRouteBoundary(A, 30, true) };
            var state = new TeleportFamiliarityState();
            foreach (string id in new TeleportOrdinaryArrivalObservation(0, loop).Complete(30, false, null)) state.RecordOrdinaryArrival(id);
            Assertions.Equal(2, state.Count(A), "Returning after traveling away is another arrival, independent of path reveal.");
            Assertions.Equal(1, state.Count(B), "Intermediate crossroads count.");
        }

        internal static void NativeStopsDoNotCountBeyondArrival()
        {
            Assertions.Equal(A, string.Join(",", new TeleportOrdinaryArrivalObservation(8, Route()).Complete(25, true, A)), "Native reveal stop caps a frame's nominal progress.");
            Assertions.Equal(0, new TeleportOrdinaryArrivalObservation(8, Route()).Complete(25, true, C).Length, "An unproven later stop fails closed.");
            Assertions.Equal(0, new TeleportOrdinaryArrivalObservation(8, Route()).Complete(25, true, null).Length, "Unanchored replacement cannot invent arrivals.");
            Assertions.Equal(C, string.Join(",", new TeleportOrdinaryArrivalObservation(29, Route()).Complete(30, true, C)), "Final arrival survives native TravelData removal.");
        }

        internal static void NonPointEndsAndInactivePointsExcluded()
        {
            var route = new[] { new TeleportRouteBoundary(A, 10, true), new TeleportRouteBoundary(B, 20, false), new TeleportRouteBoundary(C, 30, false) };
            Assertions.Equal(A, string.Join(",", new TeleportOrdinaryArrivalObservation(0, route).Complete(30, false, null)), "Inactive points and a final partial-edge destination are not arrivals.");
        }

        internal static void CompletionIsIdempotentAndRequiresProgress()
        {
            var observation = new TeleportOrdinaryArrivalObservation(0, Route());
            Assertions.Equal(3, observation.Complete(30, false, null).Length, "One completed native step.");
            Assertions.Equal(0, observation.Complete(30, false, null).Length, "Duplicate callback records nothing.");
            foreach (double after in new[] { 10.0, 0, -1, double.NaN, double.PositiveInfinity })
                Assertions.Equal(0, new TeleportOrdinaryArrivalObservation(10, Route()).Complete(after, false, null).Length, "No positive proven movement means no arrival.");
            // Merely constructing an observation is read-only; cancellation has no completion.
            var ledger = new TeleportFamiliarityState();
            new TeleportOrdinaryArrivalObservation(0, Route());
            Assertions.Equal("1|0", ledger.Serialize(), "No observer construction, menu, load or relocation changes ledger state.");
        }

        internal static void InvalidGeometryFailsClosed()
        {
            foreach (double distance in new[] { 0, -1, double.NaN, double.PositiveInfinity })
                Assertions.Throws<ArgumentException>(() => new TeleportOrdinaryArrivalObservation(0,
                    new[] { new TeleportRouteBoundary(A, distance, true) }), "Reject unproven geometry.");
            Assertions.Throws<ArgumentException>(() => new TeleportOrdinaryArrivalObservation(0,
                new[] { new TeleportRouteBoundary(A, 10, true), new TeleportRouteBoundary(B, 10, true) }), "Ambiguous equal-distance boundary.");
            Assertions.Throws<ArgumentException>(() => new TeleportOrdinaryArrivalObservation(0,
                new[] { new TeleportRouteBoundary("Localized crossroads", 10, true) }), "Stable IDs only.");
        }

        internal static void SaveOwnedObserverContract()
        {
            VerifyReadOnlyNativeLoadCounter();
            VerifyDisposablePersistenceIdentity();
            string part = File.ReadAllText("src/KingmakerGunslinger/Spells/Teleportation/UnitPartTeleportFamiliarity.cs");
            string patches = File.ReadAllText("src/KingmakerGunslinger/Spells/Teleportation/TeleportFamiliarityPatches.cs");
            string runtime = File.ReadAllText("src/KingmakerGunslinger/Spells/Teleportation/TeleportFamiliarityRuntime.cs");
            Assertions.True(part.Contains("[JsonProperty]\n        private string _state;") &&
                !part.Contains("override void PreSave") && !part.Contains("override void PostLoad"), "Save-owned bytes survive load/save and disablement without rewriting.");
            Assertions.True(!patches.Contains("[HarmonyPatch") && patches.Contains("!context.FeatureModules.Active.TeleportationSpells"), "Disabled module installs no hooks.");
            Assertions.True(runtime.Contains("edge.Spline.WorldLength") && !runtime.Contains("path.RevealPath") &&
                !runtime.Contains("SetCurrentPosition(") && !runtime.Contains("AdvanceGameTime(") &&
                !runtime.Contains("FindPath("), "Observe actual existing path geometry independently of reveal without planning or moving.");
            Assertions.True(patches.Contains("source[index].opcode == OpCodes.Stloc_1") &&
                patches.Contains("load.labels.AddRange(instruction.labels)"), "Capture native initialized baseline and preserve branch targets at returns.");
        }
        private static void VerifyDisposablePersistenceIdentity()
        {
            const string tx = "20260908T2130001234567Z_0123456789abcdef0123456789abcdef";
            Assertions.True(KingmakerGunslinger.RuntimeTesting.TeleportPersistenceIdentity.ValidTransaction(tx), "Unique UTC/GUID transaction format.");
            foreach (string bad in new[] { "", "../../working", tx.ToUpperInvariant(), tx + "_A" })
                Assertions.False(KingmakerGunslinger.RuntimeTesting.TeleportPersistenceIdentity.ValidTransaction(bad), "Reject ambiguous transaction identity.");
            string a = KingmakerGunslinger.RuntimeTesting.TeleportPersistenceIdentity.Name(tx, "A");
            Assertions.Equal("KMG_AUTOMATION_WORKING", KingmakerGunslinger.RuntimeTesting.TeleportPersistenceIdentity.InputName(tx, "A"), "Phase A has one protected input.");
            Assertions.Equal(a, KingmakerGunslinger.RuntimeTesting.TeleportPersistenceIdentity.InputName(tx, "B"), "B loads only its transaction A.");
            Assertions.True(KingmakerGunslinger.RuntimeTesting.TeleportPersistenceIdentity.MatchesFile(a, "Manual_301_" + a + ".zks"), "Only the exact native manual filename is allowed.");
            foreach (string bad in new[] { "Manual_299_KMG_AUTOMATION_WORKING.zks", "../Manual_301_" + a + ".zks", "Manual_301_" + a + ".zks.bak", "Auto_301_" + a + ".zks" })
                Assertions.False(KingmakerGunslinger.RuntimeTesting.TeleportPersistenceIdentity.MatchesFile(a, bad), "No overwrite, unrelated path or broad prefix ownership.");
            Assertions.Throws<ArgumentException>(() => KingmakerGunslinger.RuntimeTesting.TeleportPersistenceIdentity.Name(tx, "D"), "The final verification phase cannot create a save.");
        }
        private static void VerifyReadOnlyNativeLoadCounter()
        {
            const string before = "{\"Name\":\"working\",\"LoadedTimes\":8,\"GameId\":\"campaign\"}";
            const string after = "{\"Name\":\"working\",\"LoadedTimes\":9,\"GameId\":\"campaign\"}";
            var counter = new KingmakerGunslinger.RuntimeTesting.GuardedSaveLoadCounter(before);
            Assertions.False(counter.Complete, "Opening a save grants no commit.");
            Assertions.Throws<InvalidOperationException>(() => counter.SuppressCommit(), "An uncorrelated commit is rejected.");
            foreach (string wrong in new[] { before, after.Replace("campaign", "another"), after.Replace(":9", ":10") })
                Assertions.Throws<InvalidOperationException>(() => counter.SuppressHeader("header", wrong),
                    "Only one exact native LoadedTimes increment may be suppressed.");
            Assertions.Throws<InvalidOperationException>(() => counter.SuppressHeader("player", after), "Campaign writes cannot masquerade as bookkeeping.");
            counter.SuppressHeader("header", after);
            Assertions.False(counter.Complete, "Header observation requires its matching native commit boundary.");
            Assertions.Throws<InvalidOperationException>(() => counter.SuppressHeader("header", after), "Duplicate header callback rejected.");
            counter.SuppressCommit();
            Assertions.True(counter.Complete, "Exactly one correlated header/commit pair completes without a disk write.");
            Assertions.Throws<InvalidOperationException>(() => counter.SuppressCommit(), "Duplicate commit rejected.");
            foreach (string invalid in new[] { "{}", "{\"LoadedTimes\":-1}", "{\"LoadedTimes\":2147483647}", "{\"LoadedTimes\":\"8\"}" })
                Assertions.Throws<InvalidOperationException>(() => new KingmakerGunslinger.RuntimeTesting.GuardedSaveLoadCounter(invalid),
                    "Ambiguous or overflowed native bookkeeping cannot authorize a load write.");
        }
    }
}
