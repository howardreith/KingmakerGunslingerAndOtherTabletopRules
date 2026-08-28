using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.BrownFur;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BrownFurPublicationTransactionTests
    {
        internal static void PublishesAdditivelyAndIdempotently()
        {
            var native = new Fake("native");
            var foreign = new Fake("foreign");
            var brownFur = new Fake("brown-fur");
            IList<Fake> archetypes = new List<Fake> { native, foreign };
            bool bridge = false;
            int applyCount = 0;
            int rollbackCount = 0;
            var transaction = new BrownFurPublicationTransaction()
                .Append("arcanist-archetypes", () => archetypes,
                    value => archetypes = value, new[] { brownFur },
                    value => value.Id)
                .Configure("temporary-bridge", () => bridge,
                    value => bridge = value, true, EqualityComparer<bool>.Default)
                .Step("registered-identities", () => applyCount++,
                    () => rollbackCount++);

            transaction.Commit();
            transaction.Commit();

            Assertions.True(transaction.IsCommitted && archetypes.Count == 3 &&
                ReferenceEquals(archetypes[0], native) &&
                ReferenceEquals(archetypes[1], foreign) &&
                ReferenceEquals(archetypes[2], brownFur),
                "Brown-Fur publication did not preserve existing archetype order.");
            Assertions.True(bridge && applyCount == 1 && rollbackCount == 0,
                "Repeated commit was not idempotent across owned surfaces.");
            Assertions.True(transaction.Evidence.Any(value => value.Contains(
                "surface=arcanist-archetypes;action=published")),
                "Publication evidence omitted the archetype surface.");
        }

        internal static void FailureRollsBackEveryOwnedSurface()
        {
            var native = new Fake("native");
            var brownFur = new Fake("brown-fur");
            IList<Fake> archetypes = new List<Fake> { native };
            IList<Fake> original = archetypes;
            bool bridge = false;
            bool registrationsRolledBack = false;
            var transaction = new BrownFurPublicationTransaction()
                .Step("registered-identities", () => { },
                    () => registrationsRolledBack = true)
                .Append("arcanist-archetypes", () => archetypes,
                    value => archetypes = value, new[] { brownFur },
                    value => value.Id)
                .Configure("temporary-bridge", () => bridge,
                    value => bridge = value, true, EqualityComparer<bool>.Default)
                .Step("failing-selector", () => { throw new
                    InvalidOperationException("fixture"); }, () => { });

            Assertions.Throws<InvalidOperationException>(() => transaction.Commit(),
                "Publication failure was not surfaced.");
            Assertions.True(ReferenceEquals(archetypes, original) && !bridge &&
                registrationsRolledBack,
                "Failed publication did not restore every earlier owned surface.");
        }

        internal static void InsertsBeforeCombinedArchetypeBlock()
        {
            Fake[] singles = Enumerable.Range(0, 6)
                .Select(index => new Fake("single-" + index)).ToArray();
            Fake[] combined = BrownFurArchetypeOrdering
                .KnownCombinedArchetypeGuids.Select(value =>
                    new Fake(value)).ToArray();
            var brownFur = new Fake("brown-fur");
            IList<Fake> archetypes = singles.Concat(combined)
                .Concat(new[] { brownFur }).ToList();
            var transaction = new BrownFurPublicationTransaction()
                .InsertBefore("cotw-arcanist-archetypes", () => archetypes,
                    value => archetypes = value, new[] { brownFur },
                    value => value.Id, value =>
                        BrownFurArchetypeOrdering.IsKnownCombinedArchetype(
                            value.Id));

            transaction.Commit();
            transaction.Commit();

            Assertions.True(archetypes.Count == 12 &&
                ReferenceEquals(archetypes[6], brownFur) &&
                singles.SequenceEqual(archetypes.Take(6)) &&
                combined.SequenceEqual(archetypes.Skip(7)),
                "Brown-Fur was not moved before the combined block while preserving foreign order.");
            Assertions.True(transaction.Evidence.Any(value =>
                value.Contains("boundary=True;index=6")),
                "Ordered publication evidence omitted the exact boundary index.");
        }

        internal static void OrderedPublicationAppendsWithoutBoundary()
        {
            var native = new Fake("native");
            var brownFur = new Fake("brown-fur");
            IList<Fake> archetypes = new List<Fake> { native };
            var transaction = new BrownFurPublicationTransaction()
                .InsertBefore("cotw-arcanist-archetypes", () => archetypes,
                    value => archetypes = value, new[] { brownFur },
                    value => value.Id, value =>
                        BrownFurArchetypeOrdering.IsKnownCombinedArchetype(
                            value.Id));

            transaction.Commit();

            Assertions.True(archetypes.SequenceEqual(new[] {
                    native, brownFur }),
                "Boundary-free Brown-Fur publication did not retain append behavior.");
            Assertions.True(transaction.Evidence.Any(value =>
                value.Contains("boundary=False;index=1")),
                "Boundary-free publication evidence omitted append position.");
        }

        internal static void OrderedRollbackPreservesLaterAppend()
        {
            var native = new Fake("native");
            var combined = new Fake(BrownFurArchetypeOrdering
                .KnownCombinedArchetypeGuids[0]);
            var brownFur = new Fake("brown-fur");
            var later = new Fake("later");
            IList<Fake> archetypes = new List<Fake> { native, combined };
            var transaction = new BrownFurPublicationTransaction()
                .InsertBefore("cotw-arcanist-archetypes", () => archetypes,
                    value => archetypes = value, new[] { brownFur },
                    value => value.Id, value =>
                        BrownFurArchetypeOrdering.IsKnownCombinedArchetype(
                            value.Id));
            transaction.Commit();
            archetypes = archetypes.Concat(new[] { later }).ToList();

            transaction.Rollback();

            Assertions.True(archetypes.SequenceEqual(new[] {
                    native, combined, later }),
                "Ordered rollback did not restore foreign order and preserve the proven later append.");
        }

        internal static void RollbackPreservesProvenLaterAppend()
        {
            var native = new Fake("native");
            var brownFur = new Fake("brown-fur");
            var later = new Fake("later");
            IList<Fake> archetypes = new List<Fake> { native };
            var transaction = new BrownFurPublicationTransaction().Append(
                "arcanist-archetypes", () => archetypes,
                value => archetypes = value, new[] { brownFur }, value => value.Id);
            transaction.Commit();
            archetypes = archetypes.Concat(new[] { later }).ToList();

            transaction.Rollback();
            transaction.Rollback();

            Assertions.True(!transaction.IsCommitted && archetypes.Count == 2 &&
                ReferenceEquals(archetypes[0], native) &&
                ReferenceEquals(archetypes[1], later),
                "Rollback did not remove only Brown-Fur's append.");
            Assertions.True(transaction.Evidence.Any(value => value.Contains(
                "preserved-later=1")),
                "Rollback evidence omitted the preserved foreign append.");
        }

        internal static void RollbackRefusesAmbiguousMutation()
        {
            var native = new Fake("native");
            var brownFur = new Fake("brown-fur");
            IList<Fake> archetypes = new List<Fake> { native };
            var transaction = new BrownFurPublicationTransaction().Append(
                "arcanist-archetypes", () => archetypes,
                value => archetypes = value, new[] { brownFur }, value => value.Id);
            transaction.Commit();
            archetypes = new List<Fake> { native, new Fake("interposed"), brownFur };

            Assertions.Throws<InvalidOperationException>(() => transaction.Rollback(),
                "Ambiguous external mutation must fail closed during rollback.");
            Assertions.True(archetypes.Count == 3 &&
                archetypes[1].Id == "interposed",
                "Unsafe rollback altered an unrelated publication.");
        }

        internal static void ConflictingGuidRollsBackRegisteredIdentities()
        {
            var native = new Fake("native");
            var conflicting = new Fake("brown-fur");
            var brownFur = new Fake("brown-fur");
            IList<Fake> archetypes = new List<Fake> { native, conflicting };
            bool identitiesRegistered = true;
            var transaction = new BrownFurPublicationTransaction()
                .Step("brown-fur-registered-identities", () => { },
                    () => identitiesRegistered = false)
                .Append("cotw-arcanist-archetypes", () => archetypes,
                    value => archetypes = value, new[] { brownFur },
                    value => value.Id);

            Assertions.Throws<InvalidOperationException>(() =>
                transaction.Commit(),
                "A foreign archetype with the Brown-Fur GUID must fail closed.");
            Assertions.True(!identitiesRegistered && archetypes.Count == 2 &&
                ReferenceEquals(archetypes[0], native) &&
                ReferenceEquals(archetypes[1], conflicting),
                "GUID conflict did not preserve the foreign selector and roll back owned identities.");
            Assertions.True(transaction.Evidence.Any(value => value.Contains(
                "surface=brown-fur-registered-identities;action=rolled-back")),
                "GUID-conflict rollback omitted identity-registration evidence.");
        }

        private sealed class Fake
        {
            internal Fake(string id) { Id = id; }
            internal string Id { get; private set; }
        }
    }
}
