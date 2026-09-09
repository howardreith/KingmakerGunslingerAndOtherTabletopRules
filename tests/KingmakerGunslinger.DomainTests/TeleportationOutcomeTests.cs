using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    internal static partial class TeleportationContextTests
    {
        private static TeleportAlternateCandidate Candidate(int index, double? graph, double? route, double? coordinate,
            TeleportDestinationFacts facts = TeleportDestinationFacts.Required, int visits = 1)
        { return new TeleportAlternateCandidate(Point(index.ToString("x32"), facts: facts, visits: visits), graph, route, coordinate); }
        internal static void AlternateDistancePreferenceAndExclusions()
        {
            var candidates = new[] { Candidate(1, 0, 0, 0), Candidate(2, 0, 0, 0), Candidate(5, 10, 90, 80),
                Candidate(6, 20, 1, 1), Candidate(7, 0, 0, 0, visits: 0), Candidate(8, 0, 0, 0),
                Candidate(9, 0, 0, 0, TeleportDestinationFacts.Required | TeleportDestinationFacts.Transient) };
            var result = TeleportAlternateDestinationPolicy.Choose(candidates, Target, Origin, Catalog(8.ToString("x32")), 0, size => 0);
            Assertions.Equal(2, result.CandidateCount, "Origin, intended target, unvisited, forbidden and transient points excluded.");
            Assertions.Equal(5.ToString("x32"), result.Id, "Native graph distance wins over route and coordinate order.");
            Assertions.Equal(TeleportDistanceBasis.Graph, result.Basis, "Graph basis.");
            var route = TeleportAlternateDestinationPolicy.Choose(new[] { Candidate(5, null, 2, 100), Candidate(6, null, 20, 1) }, Target, Origin, Catalog(), 0, size => 0);
            Assertions.Equal(TeleportDistanceBasis.Route, route.Basis, "Route before coordinates.");
            var coordinates = TeleportAlternateDestinationPolicy.Choose(new[] { Candidate(5, null, null, 50), Candidate(6, null, null, 2) }, Target, Origin, Catalog(), 0, size => 0);
            Assertions.True(coordinates.CoordinateFallback && coordinates.Id == 6.ToString("x32"), "Coordinate fallback is explicit.");
        }
        internal static void AlternateSeverityAndInjectedSelection()
        {
            var candidates = Enumerable.Range(10, 100).Reverse().Select(id => Candidate(id, id, 1000-id, 1000-id)).ToArray();
            int prior = -1;
            for (int roll = 77; roll <= 96; roll++)
            {
                double severity = TeleportFailureSeverityPolicy.Calculate(roll, 76);
                var result = TeleportAlternateDestinationPolicy.Choose(candidates, Target, Origin, Catalog(), severity, size => size-1);
                Assertions.True(result.Bucket >= prior, "Worse failure roll cannot select a nearer severity bucket.");
                Assertions.Equal((10 + result.FirstRank + result.BucketSize - 1).ToString("x32"), result.Id, "Injected RNG stays within the selected rank bucket.");
                prior = result.Bucket;
            }
            var off = TeleportAlternateDestinationPolicy.Choose(candidates, Target, Origin, Catalog(), 1.0/24, size => 0);
            var similar = TeleportAlternateDestinationPolicy.Choose(candidates, Target, Origin, Catalog(), 20.0/24, size => 0);
            Assertions.True(similar.Distance > off.Distance, "Worse similar-location tail selects farther buckets.");
            Assertions.Throws<InvalidOperationException>(() => TeleportAlternateDestinationPolicy.Choose(candidates, Target, Origin, Catalog(), 0, size => size), "Out-of-bucket RNG fails closed.");
        }
        internal static void NoAlternateDoesNotRollOrPromote()
        {
            int rolls = 0;
            var result = TeleportAlternateDestinationPolicy.Choose(new[] { Candidate(5, 1, 1, 1, visits: 0) }, Target, Origin, Catalog(), 1, size => { rolls++; return 0; });
            Assertions.False(result.Found, "No invalid candidate is promoted to an arrival.");
            Assertions.Equal(0, rolls, "No candidate-selection roll for an empty set.");
        }
        private sealed class World : ITeleportOutcomeWorld
        {
            internal readonly Queue<int> Rolls = new Queue<int>();
            internal readonly Queue<int> DamageRolls = new Queue<int>();
            internal int D100Calls, D10Calls, Relocations;
            internal string Position = Origin;
            internal bool RepeatMishap, NoAlternate;
            internal readonly Dictionary<string,int> Hp = new Dictionary<string,int> { {"caster", 2}, {"ally", 40}, {"pet", 40}, {"mount", 40}, {"dead", 0}, {"inactive", 40} };
            internal readonly List<string> Damaged = new List<string>();
            public int RollD100() { D100Calls++; return RepeatMishap ? 100 : Rolls.Dequeue(); }
            public int RollD10() { D10Calls++; return RepeatMishap ? 1 : DamageRolls.Dequeue(); }
            public IReadOnlyList<TeleportTravelerSnapshot> CaptureTravelers()
            { return Hp.Select(value => new TeleportTravelerSnapshot(value.Key, value.Value > 0, value.Key != "inactive")).ToArray(); }
            public void DamageTraveler(string id, int amount) { Hp[id] -= amount; Damaged.Add(id + ":" + amount); }
            public TeleportAlternateDecision SelectAlternate(string target, string origin, double severity)
            { return TeleportAlternateDestinationPolicy.Choose(NoAlternate ? new TeleportAlternateCandidate[0] :
                Enumerable.Range(10,20).Select(id => Candidate(id, id, id, id)), target, origin, Catalog(), severity, size => 0); }
            public void Relocate(string id, Action materialEffectStarting) { materialEffectStarting(); Relocations++; Position = id; }
        }
        private static TeleportCastTransaction Cast(World world, Execution execution, TeleportSpellKind spell = TeleportSpellKind.Teleport)
        {
            execution.Run = mark => TeleportOutcomeResolver.Resolve(spell, TeleportFamiliarity.ViewedOnce, Target, Origin, world, mark);
            var transaction = new TeleportCastTransaction(ActionRow(spell)); transaction.Confirm(execution); return transaction;
        }
        internal static void EveryRulesOutcomeConsumesOneUse()
        {
            foreach (int roll in new[] { 76, 77, 89, 97 })
            {
                var world = new World(); world.Rolls.Enqueue(roll); world.Rolls.Enqueue(1); world.DamageRolls.Enqueue(3);
                var execution = new Execution(); var transaction = Cast(world, execution);
                Assertions.Equal(TeleportTransactionState.Completed, transaction.State, "Rules result settles once.");
                Assertions.Equal(1, execution.Resource.SpendCalls, "All outcomes, including mishap reroll, spend once.");
                Assertions.Equal(1, world.Relocations, "One final relocation.");
                Assertions.Equal(0, execution.Resource.RefundCalls, "Rules result never refunded.");
            }
            var none = new World { NoAlternate = true }; none.Rolls.Enqueue(77);
            var failed = new Execution(); var outcome = Cast(none, failed);
            Assertions.Equal(TeleportExecutionStatus.NoLegalAlternate, outcome.Result.Status, "No-alternate rules failure is explicit.");
            Assertions.Equal(Origin, none.Position, "No-alternate result stays at safe origin.");
            Assertions.Equal(1, failed.Resource.Count, "Rules failure remains spent.");
            Assertions.Equal(0, none.Relocations, "No conversion to exact arrival.");
        }
        internal static void MishapsCoverLivingTravelersAndAccumulate()
        {
            var world = new World(); foreach (int roll in new[] { 97, 100, 1 }) world.Rolls.Enqueue(roll);
            world.DamageRolls.Enqueue(3); world.DamageRolls.Enqueue(7);
            var execution = new Execution(); var transaction = Cast(world, execution);
            Assertions.Equal(2, transaction.Result.Mishaps, "Two forced mishaps, then success.");
            Assertions.Equal(3, world.D100Calls, "Each mishap rerolls the same table.");
            Assertions.Equal(2, world.D10Calls, "One d10 for each mishap.");
            Assertions.Equal("ally:3,ally:7,caster:3,mount:3,mount:7,pet:3,pet:7", string.Join(",", world.Damaged.OrderBy(value => value, StringComparer.Ordinal)), "One roll damages every living traveler, including pet/mount; dead/inactive excluded.");
            Assertions.Equal(-1, world.Hp["caster"], "No protective HP floor; death is possible.");
            Assertions.Equal(30, world.Hp["pet"], "Repeated damage accumulates.");
            Assertions.Equal(1, execution.Resource.SpendCalls, "Repeated mishaps consume no additional slot.");
        }
        internal static void DefensiveMishapCapStaysSpentAtOrigin()
        {
            var world = new World { RepeatMishap = true }; var execution = new Execution();
            var transaction = Cast(world, execution);
            Assertions.Equal(TeleportExecutionStatus.DefensiveMishapLimit, transaction.Result.Status, "Defensive corruption cap is explicit.");
            Assertions.Equal(TeleportOutcomeResolver.DefensiveMishapLimit, world.D100Calls, "High finite defensive bound.");
            Assertions.Equal(Origin, world.Position, "No unsafe relocation after cap.");
            Assertions.Equal(1, execution.Resource.SpendCalls, "One committed resource remains spent.");
            Assertions.Equal(1, execution.Records, "Critical outcome reaches structured logging adapter.");
        }
        internal static void RulesFailureLoggingCannotRefund()
        {
            var world = new World { NoAlternate = true }; world.Rolls.Enqueue(77);
            var execution = new Execution { ThrowDuringRecording = true }; var transaction = Cast(world, execution);
            Assertions.Equal(TeleportExecutionStatus.NoLegalAlternate, transaction.Result.Status, "Rules failure is retained before logging.");
            Assertions.Equal(TeleportTransactionState.TechnicalFailureSpent, transaction.State, "A logging exception cannot refund a legitimate failed spell.");
            Assertions.Equal(1, execution.Resource.Count, "Rules failure remains spent.");
            Assertions.Equal(0, execution.Resource.RefundCalls, "No compensation after resolved rules outcome.");
        }
        internal static void ExactSpellsNeverRollDestinationOrDamage()
        {
            foreach (TeleportSpellKind spell in new[] { TeleportSpellKind.GreaterTeleport, TeleportSpellKind.WordOfRecall })
            {
                var world = new World(); var execution = new Execution(); var transaction = Cast(world, execution, spell);
                Assertions.Equal(Target, world.Position, "Exact selected/resolved point.");
                Assertions.Equal(0, world.D100Calls + world.D10Calls, "Exact spells have no destination or damage RNG.");
                Assertions.Equal(0, world.Damaged.Count, "No mishap damage.");
                Assertions.Equal(1, execution.Resource.SpendCalls, "One use for exact spell.");
                Assertions.Equal(TeleportTransactionState.Completed, transaction.State, "Cast completed.");
            }
        }
    }
}