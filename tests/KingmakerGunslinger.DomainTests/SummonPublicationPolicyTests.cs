using System;
using System.Collections.Generic;
using System.Linq;
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

        internal static void NativeDuplicateCatalogIsExact()
        {
            SummonNativeOptionCatalog.Validate();
            SummonNativeExpansionCatalog.Validate();
            SummonVisibilityCatalog.Validate();
            Assertions.Equal(48, SummonNativeOptionCatalog.All.Count,
                "Native summon child catalog count changed.");
            Assertions.True(SummonNativeOptionCatalog.Find(
                    SummonFamily.Monster, 1,
                    "6c7915c9dc494849918e958618f61db0")
                    .IsSemanticDuplicate,
                "Native SM I preservation child must reconcile to KMG Dog.");
            Assertions.Equal(26, SummonNativeExpansionCatalog.All.Count,
                "Native individual-option expansion count changed.");
            Assertions.True(SummonNativeExpansionCatalog.Replaces(
                    SummonFamily.Monster, 8,
                    "eb6df7ddfc0669d4fb3fc9af4bd34bca"),
                "Movanic Deva/Frost Giant umbrella must be suppressed.");
            Assertions.True(SummonNativeExpansionCatalog.Replaces(
                    SummonFamily.Monster, 9,
                    "e96593e67d206ab49ad1b567327d1e75"),
                "Ghaele/Thanadaemon umbrella must be suppressed.");
            Assertions.Equal(2, SummonNativeExpansionCatalog.For(
                    SummonFamily.Monster, 8)
                    .Count(value => value.Multiplicity ==
                        SummonMultiplicity.One),
                "SM VIII distinct native singles changed.");
            Assertions.True(SummonNativeExpansionCatalog.All.Any(value =>
                    value.DisplayName == "Thanadaemon" && value.Tier == 9 &&
                    value.Multiplicity == SummonMultiplicity.One),
                "Thanadaemon individual option is missing.");
            Assertions.Equal("dire-tiger", SummonNativeOptionCatalog.Find(
                    SummonFamily.NaturesAlly, 8,
                    "86f4287572bef49449b9d06c66adf456")
                    .EquivalentCreatureKey,
                "Native SNA Smilodon reconciliation changed.");
            Assertions.Equal(667,
                ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Concat(ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly))
                    .Count(SummonVisibilityCatalog.IsPublished),
                "Visible summon placement count changed.");
            Assertions.Equal(14,
                ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Concat(ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly))
                    .Count(value => !SummonVisibilityCatalog.IsPublished(value)),
                "Dire Bat compatibility-shell placement count changed.");
        }

        internal static void DisplayOrderGroupsSinglesBeforeQuantities()
        {
            var nativeSingle = new Fake("native-single",
                SummonMultiplicity.One);
            var nativeD3 = new Fake("native-d3", SummonMultiplicity.OneD3);
            var nativeD4 = new Fake("native-d4",
                SummonMultiplicity.OneD4PlusOne);
            var thirdA = new Fake("third-a", null);
            var thirdB = new Fake("third-b", null);
            var oneA = new Fake("one-a", SummonMultiplicity.One);
            var oneB = new Fake("one-b", SummonMultiplicity.One);
            var d3 = new Fake("d3", SummonMultiplicity.OneD3);
            var d4 = new Fake("d4", SummonMultiplicity.OneD4PlusOne);
            IReadOnlyList<Fake> result = SummonDisplayOrderPolicy.Order(
                new[] { nativeD4, thirdA, nativeSingle, nativeD3, thirdB },
                new[] { d4, oneA, d3, oneB }, value => value.Kind,
                value => value.Kind.Value);
            Assertions.Equal("one-a|one-b|native-single|d3|native-d3|d4|native-d4|third-a|third-b",
                string.Join("|", result.Select(value => value.Guid)),
                "Summon display ordering changed or foreign relative order was lost.");
        }

        internal static void IconCatalogCoversEveryCreature()
        {
            SummonIconCatalog.Validate();
            SummonViewScaleCatalog.Validate();
            Assertions.Equal(77, SummonIconCatalog.All.Count,
                "Project icon concept count changed.");
            Assertions.Equal("Smilodon", SummonIconCatalog.For("dire-tiger")
                .DisplayName, "Smilodon icon identity changed.");
            Assertions.True(new[] { "air-mephit", "earth-mephit",
                    "fire-mephit", "water-mephit", "lantern-archon",
                    "pteranodon", "bralani-azata", "erinyes-devil" }
                .Select(key => SummonIconCatalog.For(key).Key)
                .Distinct(StringComparer.Ordinal).Count() == 8,
                "Unrelated creature concepts must have distinct icon keys.");
            float eagle, frog, elephant, mastodon;
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Eagle", out eagle) && eagle == 0.30f,
                "Eagle view must be reduced.");
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_PoisonousFrog", out frog) && frog < 1f,
                "Poisonous Frog view must remain reduced relative to Giant Frog.");
            Assertions.True(SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Elephant", out elephant) &&
                SummonViewScaleCatalog.TryGetMultiplier(
                    "KMG_Summoning_Unit_Mastodon", out mastodon) &&
                mastodon > elephant,
                "Mastodon must read larger than Elephant.");
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
        {
            internal Fake(string guid) : this(guid, null) { }
            internal Fake(string guid, SummonMultiplicity? kind)
            { Guid = guid; Kind = kind; }
            internal string Guid { get; private set; }
            internal SummonMultiplicity? Kind { get; private set; }
        }
    }
}
