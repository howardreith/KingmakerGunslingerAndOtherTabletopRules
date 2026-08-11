using System;
using System.Collections.Generic;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class SummonPublicationPolicyTests
    {
        internal static void MergePreservesOrderAndIsIdempotent()
        {
            var vanilla = new Fake("vanilla"); var foreign = new Fake("foreign");
            var kmg = new Fake("kmg");
            IList<Fake> original = new List<Fake> { vanilla, foreign };
            IList<Fake> merged = SummonVariantMergePolicy.Merge(original,
                new[] { kmg }, value => value.Guid);
            Assertions.True(merged.Count == 3 && ReferenceEquals(merged[0], vanilla) &&
                ReferenceEquals(merged[1], foreign) && ReferenceEquals(merged[2], kmg),
                "Merge changed preexisting order or references.");
            IList<Fake> second = SummonVariantMergePolicy.Merge(merged,
                new[] { kmg }, value => value.Guid);
            Assertions.True(ReferenceEquals(merged, second), "Repeated merge was not idempotent.");
        }

        internal static void MergeDeduplicatesExistingAndRejectsConflicts()
        {
            var first = new Fake("same"); var duplicateGuid = new Fake("same");
            IList<Fake> merged = SummonVariantMergePolicy.Merge(
                new List<Fake> { first, first, duplicateGuid }, new Fake[0], v => v.Guid);
            Assertions.True(merged.Count == 1 && ReferenceEquals(merged[0], first),
                "Existing duplicate references/GUIDs were not singularized.");
            Assertions.Throws<InvalidOperationException>(() =>
                SummonVariantMergePolicy.Merge(new List<Fake> { first },
                    new[] { duplicateGuid }, v => v.Guid),
                "A conflicting KMG addition must fail closed.");
            Assertions.Throws<InvalidOperationException>(() =>
                SummonVariantMergePolicy.Merge(new List<Fake> { null },
                    new Fake[0], v => v.Guid), "Null variants must fail closed.");
        }

        internal static void TransactionRollsBackExactReferences()
        {
            IList<Fake> first = new List<Fake> { new Fake("a") };
            IList<Fake> second = new List<Fake> { new Fake("b") };
            IList<Fake> firstBefore = first; IList<Fake> secondBefore = second;
            bool failSecondWrite = true;
            var targets = new[] {
                new SummonPublicationTarget<Fake>("first", () => first,
                    value => first = value, new[] { new Fake("k1") }),
                new SummonPublicationTarget<Fake>("second", () => second,
                    value => { second = value; if (failSecondWrite) {
                        failSecondWrite = false;
                        throw new InvalidOperationException("fixture"); } },
                    new[] { new Fake("k2") }) };
            Assertions.Throws<InvalidOperationException>(() =>
                SummonPublicationTransaction.Publish(targets, v => v.Guid),
                "Transaction failure was not surfaced.");
            Assertions.True(ReferenceEquals(first, firstBefore) &&
                ReferenceEquals(second, secondBefore),
                "Rollback did not restore exact original collections.");
        }

        internal static void TransactionRefusesUnsafeRollback()
        {
            IList<Fake> first = new List<Fake> { new Fake("a") };
            IList<Fake> second = new List<Fake> { new Fake("b") };
            var unrelated = new List<Fake> { new Fake("foreign-later") };
            var targets = new[] {
                new SummonPublicationTarget<Fake>("first", () => first,
                    value => first = value, new[] { new Fake("k1") }),
                new SummonPublicationTarget<Fake>("second", () => second,
                    value => { first = unrelated; throw new InvalidOperationException("fixture"); },
                    new[] { new Fake("k2") }) };
            try { SummonPublicationTransaction.Publish(targets, v => v.Guid); }
            catch (InvalidOperationException exception) {
                Assertions.True(exception.Message.Contains("Rollback refused"),
                    "Unsafe rollback did not report refusal."); return;
            }
            throw new InvalidOperationException("Unsafe rollback was accepted.");
        }

        private sealed class Fake
        { internal Fake(string guid) { Guid = guid; } internal string Guid { get; private set; } }
    }
}
