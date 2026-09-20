using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Spells.Teleportation;

namespace KingmakerGunslinger.DomainTests
{
    // Behavioral regressions for the Favored Class variants publication
    // transaction and the per-level target resolution policy. These exercise
    // the production classes directly (the reconciler delegates to exactly
    // this code), not source tokens.
    internal static class FavoredClassVariantsTransactionTests
    {
        private sealed class FakeSpell
        {
            internal FakeSpell(string guid) { Guid = guid; }
            internal string Guid { get; private set; }
        }

        private sealed class FakeTarget
        {
            internal FakeSpell[] Field;
            internal object Cache = new object();
            internal int FieldWrites;
            internal int CacheWrites;
            internal readonly List<object> CacheValues = new List<object>();
            internal readonly List<FakeSpell[]> FieldValues = new List<FakeSpell[]>();
            // Optional fault injection: a delegate that runs after the owned
            // field/cache write, or an nth field write that throws after
            // writing.
            internal Action AfterCacheWrite;
            internal int FieldWriteThrowAfter;

            internal FakeTarget(params FakeSpell[] entries)
            { Field = entries; }

            internal FavoredClassVariantsTransaction<FakeSpell> Transaction()
            {
                return new FavoredClassVariantsTransaction<FakeSpell>(
                    () => Field, value =>
                    {
                        Field = value; FieldWrites++; FieldValues.Add(value);
                        if (FieldWrites == FieldWriteThrowAfter)
                            throw new InvalidOperationException("field writer failed after writing");
                    },
                    () => Cache, value =>
                    {
                        Cache = value; CacheWrites++; CacheValues.Add(value);
                        if (CacheWrites == 1 && AfterCacheWrite != null) AfterCacheWrite();
                    });
            }
        }

        internal static void PublishAppendsOnceAndPreservesForeignEntries()
        {
            var foreign = new FakeSpell("snapshot-a");
            var other = new FakeSpell("snapshot-b");
            var recall = new FakeSpell("word-of-recall");
            var target = new FakeTarget(foreign, other);
            var transaction = target.Transaction();
            transaction.Publish(recall, value => value.Guid);
            Assertions.True(target.Field.Length == 3 &&
                ReferenceEquals(target.Field[0], foreign) &&
                ReferenceEquals(target.Field[1], other) &&
                ReferenceEquals(target.Field[2], recall),
                "Publication must preserve every foreign snapshot entry in order and append the canonical spell exactly once.");
            Assertions.True(target.FieldWrites == 1 && target.CacheWrites == 1 &&
                target.Cache == null,
                "One publication writes the array once and invalidates the item cache once.");
        }

        internal static void SecondPassRetainsExactAssignedArray()
        {
            var recall = new FakeSpell("word-of-recall");
            var target = new FakeTarget(new FakeSpell("snapshot-a"));
            var transaction = target.Transaction();
            transaction.Publish(recall, value => value.Guid);
            var assigned = target.Field;
            int fieldWrites = target.FieldWrites;
            int cacheWrites = target.CacheWrites;
            transaction.Publish(recall, value => value.Guid);
            Assertions.True(ReferenceEquals(target.Field, assigned) &&
                target.FieldWrites == fieldWrites && target.CacheWrites == cacheWrites,
                "A repeated pass over an already-published array must retain the exact assigned array, rewrite nothing and not invalidate the cache again.");
        }

        internal static void AlreadyCorrectArrayIsNotReplaced()
        {
            var foreign = new FakeSpell("snapshot-a");
            var recall = new FakeSpell("word-of-recall");
            var array = new[] { foreign, recall };
            var target = new FakeTarget { Field = array };
            var transaction = target.Transaction();
            var published = transaction.Publish(recall, value => value.Guid);
            Assertions.True(ReferenceEquals(published, array) &&
                ReferenceEquals(target.Field, array) &&
                target.FieldWrites == 0 && target.CacheWrites == 0,
                "An already-correct variants array must be retained by instance with no replacement and no cache invalidation.");
        }

        internal static void FailureAfterAssignmentRestoresOwnedState()
        {
            var foreign = new FakeSpell("snapshot-a");
            var recall = new FakeSpell("word-of-recall");
            var before = new[] { foreign };
            var target = new FakeTarget { Field = before };
            object priorCache = target.Cache;
            var transaction = target.Transaction();
            InvalidOperationException failure = null;
            try
            {
                transaction.PublishAndValidate(recall, value => value.Guid,
                    () => { throw new InvalidOperationException("validation failed"); });
            }
            catch (InvalidOperationException exception) { failure = exception; }
            Assertions.True(failure != null && failure.Message == "validation failed",
                "The original validation failure must propagate with its information intact.");
            Assertions.True(ReferenceEquals(target.Field, before) &&
                ReferenceEquals(target.Cache, priorCache),
                "A failure after the variants assignment must restore the exact prior array and the exact prior cache value.");
            Assertions.True(target.FieldWrites == 2 && target.CacheWrites == 2 &&
                ReferenceEquals(target.CacheValues[1], priorCache),
                "Rollback performs exactly one restoring write of each owned change.");
        }

        internal static void ForeignMutationAfterAssignmentRefusesRestore()
        {
            var recall = new FakeSpell("word-of-recall");
            var before = new[] { new FakeSpell("snapshot-a") };
            var target = new FakeTarget { Field = before };
            var transaction = target.Transaction();
            transaction.Publish(recall, value => value.Guid);
            var foreignArray = new[] { new FakeSpell("foreign") };
            target.Field = foreignArray;
            InvalidOperationException failure = null;
            InvalidOperationException refusal = null;
            try
            {
                transaction.Validate(
                    () => { throw new InvalidOperationException("validation failed"); });
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("restoration refused")) refusal = exception;
                else failure = exception;
            }
            Assertions.True(failure == null && refusal != null &&
                refusal.InnerException is InvalidOperationException &&
                refusal.InnerException.Message == "validation failed",
                "A field mutated by someone else must never be overwritten, and the original failure must be preserved as the inner exception.");
            Assertions.True(ReferenceEquals(target.Field, foreignArray),
                "The genuine foreign mutation remains untouched.");
        }

        internal static void NullEntryFailsClosedBeforeAnyWrite()
        {
            var recall = new FakeSpell("word-of-recall");
            var target = new FakeTarget { Field = new FakeSpell[] { null } };
            var transaction = target.Transaction();
            Assertions.Throws<InvalidOperationException>(() =>
                transaction.Publish(recall, value => value.Guid),
                "A malformed variants array must fail closed before any mutation.");
            Assertions.True(target.FieldWrites == 0 && target.CacheWrites == 0,
                "A failed publication must not write the field or the cache.");
        }

        internal static void CacheWriterFailureRollsBackInstalledArray()
        {
            var foreign = new FakeSpell("snapshot-a");
            var recall = new FakeSpell("word-of-recall");
            var before = new[] { foreign };
            var target = new FakeTarget { Field = before };
            object priorCache = target.Cache;
            target.AfterCacheWrite = () =>
            { throw new InvalidOperationException("cache writer failed"); };
            var transaction = target.Transaction();
            InvalidOperationException failure = null;
            try
            {
                transaction.PublishAndValidate(recall, value => value.Guid,
                    () => { });
            }
            catch (InvalidOperationException exception) { failure = exception; }
            Assertions.True(failure != null && failure.Message == "cache writer failed",
                "A cache-writer failure during publication must propagate the original exception.");
            Assertions.True(ReferenceEquals(target.Field, before) &&
                ReferenceEquals(target.Cache, priorCache),
                "A cache-writer failure after the array write must still restore the exact prior array and prior cache value.");
        }

        internal static void WriterThatChangesTargetThenThrowsIsRolledBack()
        {
            var recall = new FakeSpell("word-of-recall");
            var before = new[] { new FakeSpell("snapshot-a") };
            var target = new FakeTarget { Field = before };
            object priorCache = target.Cache;
            target.FieldWriteThrowAfter = 1;
            var transaction = target.Transaction();
            InvalidOperationException failure = null;
            try
            {
                transaction.PublishAndValidate(recall, value => value.Guid,
                    () => { });
            }
            catch (InvalidOperationException exception) { failure = exception; }
            Assertions.True(failure != null && failure.Message == "field writer failed after writing",
                "A writer that changes its target and then throws must propagate its own failure.");
            Assertions.True(ReferenceEquals(target.Field, before) &&
                ReferenceEquals(target.Cache, priorCache),
                "An installed write followed by a writer exception must be rolled back with the prior cache value restored.");
        }

        internal static void RestorationFailurePreservesOriginalAndRestorationFailures()
        {
            var recall = new FakeSpell("word-of-recall");
            var before = new[] { new FakeSpell("snapshot-a") };
            var target = new FakeTarget { Field = before };
            var transaction = target.Transaction();
            transaction.Publish(recall, value => value.Guid);
            // The restoration write (the second field write) throws.
            target.FieldWriteThrowAfter = 2;
            InvalidOperationException failure = null;
            try
            {
                transaction.Validate(
                    () => { throw new InvalidOperationException("validation failed"); });
            }
            catch (InvalidOperationException exception) { failure = exception; }
            Assertions.True(failure != null &&
                failure.Message.Contains("restoration failed"),
                "A failing restoration must surface a distinct restoration failure.");
            Assertions.True(failure.InnerException is AggregateException &&
                ((AggregateException)failure.InnerException).InnerExceptions
                    .Count(exception => exception.Message == "validation failed") == 1 &&
                ((AggregateException)failure.InnerException).InnerExceptions
                    .Count(exception => exception.Message == "field writer failed after writing") == 1,
                "The restoration failure must preserve both the original failure and the restoration failure.");
        }

        internal static void RestorationRefusalDuringPublicationFailureRefuses()
        {
            var recall = new FakeSpell("word-of-recall");
            var target = new FakeTarget { Field = new[] { new FakeSpell("snapshot-a") } };
            var transaction = target.Transaction();
            // The cache writer first lets a foreign mutation replace the
            // installed array, then throws.
            var foreignArray = new[] { new FakeSpell("foreign") };
            target.AfterCacheWrite = () =>
            {
                target.Field = foreignArray;
                throw new InvalidOperationException("cache writer failed");
            };
            InvalidOperationException refusal = null;
            try
            {
                transaction.PublishAndValidate(recall, value => value.Guid,
                    () => { });
            }
            catch (InvalidOperationException exception) { refusal = exception; }
            Assertions.True(refusal != null &&
                refusal.Message.Contains("restoration refused") &&
                refusal.InnerException != null &&
                refusal.InnerException.Message == "cache writer failed",
                "A foreign mutation during a publication failure must be refused with the original failure preserved.");
            Assertions.True(ReferenceEquals(target.Field, foreignArray),
                "The genuine foreign mutation remains untouched by the refusal.");
        }

        internal static void ResolutionDistinguishesAbsentMalformedAndAmbiguous()
        {
            var classList = new object();
            var caster = new object();
            Candidate(FavoredClassTargetPolicy.Outcome.Malformed,
                FavoredClassTargetPolicy.Resolve(null, classList, caster, 6),
                "A selection without children is malformed presence, not absence.");
            Candidate(FavoredClassTargetPolicy.Outcome.Malformed,
                FavoredClassTargetPolicy.Resolve(new FavoredClassTargetPolicy.Candidate[0],
                    classList, caster, 6),
                "An empty child list is malformed presence.");
            var bound = new FavoredClassTargetPolicy.Candidate {
                Name = "level6", SpellList = classList, SpellcasterClass = caster,
                IsLearnSpellParameter = true, SpellLevel = 6,
                HasValidSelectionContract = true,
                HasValidGrantConfiguration = true };
            Candidate(FavoredClassTargetPolicy.Outcome.Resolved,
                FavoredClassTargetPolicy.Resolve(new[] { bound }, classList, caster, 6),
                "Exactly one complete candidate resolves.");
            var ambiguous = new FavoredClassTargetPolicy.Candidate {
                Name = "level6-clone", SpellList = classList, SpellcasterClass = caster,
                IsLearnSpellParameter = true, SpellLevel = 6,
                HasValidSelectionContract = true,
                HasValidGrantConfiguration = true };
            var decision = FavoredClassTargetPolicy.Resolve(
                new[] { bound, ambiguous }, classList, caster, 6);
            Candidate(FavoredClassTargetPolicy.Outcome.Ambiguous, decision,
                "Two qualifying candidates are ambiguous regardless of order; the first match is never privileged.");
            var invalidGrant = new FavoredClassTargetPolicy.Candidate {
                Name = "bad-grant", SpellList = classList, SpellcasterClass = caster,
                IsLearnSpellParameter = true, SpellLevel = 6,
                HasValidGrantConfiguration = false };
            var malformedDecision = FavoredClassTargetPolicy.Resolve(
                new[] { invalidGrant }, classList, caster, 6);
            Candidate(FavoredClassTargetPolicy.Outcome.Malformed, malformedDecision,
                "A bound candidate with an invalid grant configuration is malformed.");
            Assertions.True(malformedDecision.Detail != null &&
                malformedDecision.Detail.Contains("bad-grant"),
                "Malformed diagnostics must name the incompatible child.");
            var wrongLevel = new FavoredClassTargetPolicy.Candidate {
                Name = "level5", SpellList = classList, SpellcasterClass = caster,
                IsLearnSpellParameter = true, SpellLevel = 5,
                HasValidSelectionContract = true,
                HasValidGrantConfiguration = true };
            Candidate(FavoredClassTargetPolicy.Outcome.Malformed,
                FavoredClassTargetPolicy.Resolve(new[] { wrongLevel },
                    classList, caster, 6),
                "A child at a different spell level does not resolve.");
            var wrongList = new FavoredClassTargetPolicy.Candidate {
                Name = "other-list", SpellList = new object(), SpellcasterClass = caster,
                IsLearnSpellParameter = true, SpellLevel = 6,
                HasValidGrantConfiguration = true };
            Candidate(FavoredClassTargetPolicy.Outcome.Malformed,
                FavoredClassTargetPolicy.Resolve(
                    new FavoredClassTargetPolicy.Candidate[] { null, wrongList },
                    classList, caster, 6),
                "Null or differently-bound children are malformed, never silently ignored matches.");
        }

        internal static void MalformedSelectionContractTargetsNeverResolve()
        {
            var classList = new object();
            var caster = new object();
            var validGrant = new FavoredClassTargetPolicy.Candidate {
                Name = "level6", SpellList = classList, SpellcasterClass = caster,
                IsLearnSpellParameter = true, SpellLevel = 6,
                HasValidSelectionContract = true,
                HasValidGrantConfiguration = true };
            Candidate(FavoredClassTargetPolicy.Outcome.Resolved,
                FavoredClassTargetPolicy.Resolve(new[] { validGrant },
                    classList, caster, 6),
                "A fully contracted child resolves.");
            foreach (var malformed in new[] {
                new FavoredClassTargetPolicy.Candidate {
                    Name = "not-specific", SpellList = classList, SpellcasterClass = caster,
                    IsLearnSpellParameter = true, SpellLevel = 6,
                    HasValidSelectionContract = false,
                    HasValidGrantConfiguration = true },
                new FavoredClassTargetPolicy.Candidate {
                    Name = "with-penalty", SpellList = classList, SpellcasterClass = caster,
                    IsLearnSpellParameter = true, SpellLevel = 6,
                    HasValidSelectionContract = false,
                    HasValidGrantConfiguration = true } })
            {
                var decision = FavoredClassTargetPolicy.Resolve(
                    new[] { malformed }, classList, caster, 6);
                Candidate(FavoredClassTargetPolicy.Outcome.Malformed, decision,
                    "A child without the selection-side contract is malformed and is never resolved.");
                Assertions.True(decision.Detail != null &&
                    decision.Detail.Contains(malformed.Name) &&
                    decision.Detail.Contains("selection contract"),
                    "The malformed diagnostic must name the child and the missing selection-side contract: " + decision.Detail);
            }
            // A sibling with a valid selection contract but an invalid grant
            // configuration is malformed as well, never silently skipped.
            var badGrant = new FavoredClassTargetPolicy.Candidate {
                Name = "bad-grant", SpellList = classList, SpellcasterClass = caster,
                IsLearnSpellParameter = true, SpellLevel = 6,
                HasValidSelectionContract = true,
                HasValidGrantConfiguration = false };
            Candidate(FavoredClassTargetPolicy.Outcome.Malformed,
                FavoredClassTargetPolicy.Resolve(new[] { badGrant },
                    classList, caster, 6),
                "A valid selection contract cannot rescue an invalid grant configuration.");
        }

        private static void Candidate(FavoredClassTargetPolicy.Outcome expected,
            FavoredClassTargetPolicy.Decision decision, string message)
        {
            Assertions.True(decision.Outcome == expected, message +
                " Observed " + decision.Outcome + " (" + decision.Detail + ").");
        }
    }
}
