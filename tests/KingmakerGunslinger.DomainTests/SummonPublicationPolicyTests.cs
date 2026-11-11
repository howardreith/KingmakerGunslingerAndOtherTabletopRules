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
            Assertions.True(SummonVariantMergePolicy
                    .PreserveNativeSemanticDuplicate(0,
                        "6c7915c9dc494849918e958618f61db0"),
                "A hidden KMG Dog must preserve the visible vanilla SM I Dog.");
            Assertions.False(SummonVariantMergePolicy
                    .PreserveNativeSemanticDuplicate(1,
                        "6c7915c9dc494849918e958618f61db0"),
                "A published exact KMG Dog replacement must collapse the vanilla duplicate.");
            Assertions.Throws<InvalidOperationException>(() =>
                SummonVariantMergePolicy.PreserveNativeSemanticDuplicate(2,
                    "6c7915c9dc494849918e958618f61db0"),
                "Ambiguous published replacements must fail closed.");
            Assertions.Equal(29, SummonNativeExpansionCatalog.All.Count,
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
            Assertions.Equal(SummoningCurrentSurface.FinalPublishedLogicalPlacements,
                ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Concat(ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly))
                    .Count(SummonVisibilityCatalog.IsPublished),
                "Visible summon placement count changed.");
            // A future creature may register ahead of qualification, but any
            // hidden placement must remain deliberate and exactly reconciled.
            Assertions.Equal(
                SummonVisibilityCatalog.SuppressedLogicalPlacementCount,
                ExpandedSummoningCatalog.GenerateVariants(SummonFamily.Monster)
                    .Concat(ExpandedSummoningCatalog.GenerateVariants(
                        SummonFamily.NaturesAlly))
                    .Count(value => !SummonVisibilityCatalog.IsPublished(value)),
                "Only placements that are unqualified or held on a proven engine barrier may remain hidden, and the catalog must hide exactly as many as it declares it hides.");
        }

        // Owner decision of 2026-11-10 supersedes the earlier name-first rule:
        // base multiplicity leads, then the family-first name, then identity.
        internal static void DisplayOrderIsQuantityThenCreatureName()
        {
            var steam = new Fake("steam", SummonMultiplicity.One);
            var air = new Fake("air", SummonMultiplicity.One);
            var airGroup = new Fake("air-d3", SummonMultiplicity.OneD3);
            var airBig = new Fake("air-d4", SummonMultiplicity.OneD4PlusOne);
            var dog = new Fake("dog", SummonMultiplicity.OneD3);
            var nativeWolf = new Fake("native-wolf", SummonMultiplicity.One);
            var unknown = new Fake("unknown-quantity", null);
            var thirdA = new Fake("third-a", null);
            var thirdB = new Fake("third-b", null);
            var names = new Dictionary<Fake, string> {
                { steam, "Mephit, Steam" }, { air, "Mephit, Air" },
                { airGroup, "Mephit, Air" }, { airBig, "MEPHIT, AIR" },
                { dog, "Dog" }, { nativeWolf, "Wolf" }, { unknown, "Aardvark" } };
            Fake[] originals = { thirdA, unknown, nativeWolf, thirdB };
            Fake[] additions = { steam, airBig, dog, airGroup, air };
            IReadOnlyList<Fake> result = SummonDisplayOrderPolicy.Order(originals, additions,
                value => names.ContainsKey(value) ?
                    SummonMenuOrderKey.Host(names[value], value.Kind, value.Guid, true) :
                    SummonMenuOrderKey.Host(value.Guid, value.Kind, value.Guid, false));
            Assertions.Equal("air|steam|native-wolf|dog|air-d3|air-d4|unknown-quantity|third-a|third-b",
                string.Join("|", result.Select(value => value.Guid)),
                "Every single summon precedes every 1d3, which precedes every 1d4+1; names order case-insensitively inside each group; an unknown base quantity follows the groups; unclassified rows stay last in relative order.");
            Assertions.True(originals.SequenceEqual(new[] { thirdA, unknown, nativeWolf, thirdB }) &&
                additions.SequenceEqual(new[] { steam, airBig, dog, airGroup, air }),
                "Ordering must not mutate its input arrays.");
            Assertions.Equal(originals.Length + additions.Length,
                result.Distinct().Count(), "Ordering lost or repeated a choice.");
            IReadOnlyList<Fake> tied = SummonDisplayOrderPolicy.Order(
                new[] { new Fake("b", SummonMultiplicity.One) },
                new[] { new Fake("a", SummonMultiplicity.One) },
                value => SummonMenuOrderKey.Host("Same", value.Kind, value.Guid, true));
            Assertions.Equal("a|b", string.Join("|", tied.Select(value => value.Guid)),
                "An exact tie must resolve through the row identity.");
            Assertions.Throws<InvalidOperationException>(() => SummonDisplayOrderPolicy.Order(
                new Fake[0], new[] { thirdA },
                value => SummonMenuOrderKey.Host("x", null, value.Guid, false)),
                "A package-owned addition cannot be an unclassified row.");
        }

        internal static void IconCatalogCoversEveryCreature()
        {
            SummonIconCatalog.Validate();
            SummonViewScaleCatalog.Validate();
            // 120 since the Brachiosaurus painting arrived; the creature
            // was already registered and already expected an icon.
            Assertions.Equal(SummoningCurrentSurface.IconConceptCount, SummonIconCatalog.All.Count,
                "Project icon concept count changed.");
            Assertions.Equal("Dire Bat", SummonIconCatalog.For("dire-bat")
                .DisplayName, "Dire Bat requires its own creature icon identity.");
            Assertions.Equal("Smilodon", SummonIconCatalog.For("dire-tiger")
                .DisplayName, "Smilodon icon identity changed.");
            Assertions.Equal("Lullaby", SummonIconCatalog.For("lullaby")
                .DisplayName, "The private Lullaby spell/effect needs its own icon concept.");
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
