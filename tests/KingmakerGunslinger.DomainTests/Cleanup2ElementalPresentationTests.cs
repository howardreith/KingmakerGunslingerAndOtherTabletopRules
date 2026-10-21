using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Cleanup2ElementalPresentationTests
    {
        internal static void ObservedRanksHaveCompleteCanonicalSpeciesMetadata()
        {
            var ordinary = StandaloneElementalPresentationPolicy.All.Where(value => !value.UniqueNpc).ToArray();
            Assertions.True(ordinary.Select(value => value.Guid).Distinct(StringComparer.Ordinal).Count() ==
                ordinary.Length, "Observed standalone identities must be unique.");
            foreach (var parent in ordinary.Where(value => value.IsParent))
            {
                var children = ordinary.Where(value => value.ParentGuid == parent.Guid).ToArray();
                Assertions.True(children.SelectMany(value => value.CreatureKeys).OrderBy(value => value)
                    .SequenceEqual(parent.CreatureKeys.OrderBy(value => value)),
                    "Each observed parent must retain its exact four elemental children.");
                foreach (string key in parent.CreatureKeys)
                {
                    var creature = ExpandedSummoningCatalog.All.Single(value => value.Key == key);
                    Assertions.True(creature.MonsterTier == parent.CanonicalTier &&
                        creature.NaturesAllyTier == parent.CanonicalTier,
                        "Canonical elemental tiers must match observed rank metadata.");
                }
            }
            foreach (var child in ordinary.Where(value => !value.IsParent))
                Assertions.True(StandaloneElementalPresentationPolicy.Find(child.ParentGuid).IsParent,
                    "A child must retain its exact resolvable native parent identity.");
            Assertions.True(StandaloneElementalPresentationPolicy.Find("ElementalRaceFire") == null &&
                StandaloneElementalPresentationPolicy.Find("SummonElementalSmallBase") == null,
                "Names and unrelated elemental identities must never authorize hiding.");
        }

        internal static void CurrentIncompleteInventoryAndSavedInvocationsAlwaysPreserve()
        {
            var spec = StandaloneElementalPresentationPolicy.All.First(value => value.IsParent);
            var proof = CompleteSyntheticProof(spec);
            proof.ConsumerInventoryComplete = false;
            Assertions.Equal("preserve-consumer-inventory-incomplete",
                StandaloneElementalPresentationPolicy.Disposition(true, spec.Guid,
                    StandaloneElementalConsumerRoute.OrdinaryClassLearning, proof),
                "The actual incomplete census must not authorize any hiding.");
            proof.ConsumerInventoryComplete = true;
            foreach (var route in new[] { StandaloneElementalConsumerRoute.ExistingKnownSpell,
                StandaloneElementalConsumerRoute.PreparedSlot, StandaloneElementalConsumerRoute.PinnedHotbar })
                Assertions.Equal("compatibility-presentation-required-preserve-invocation",
                    StandaloneElementalPresentationPolicy.Disposition(true, spec.Guid, route, proof),
                    "Saved invocations need a separate tested compatibility presentation.");
            Assertions.Equal("preserve-module-off", StandaloneElementalPresentationPolicy.Disposition(
                false, spec.Guid, StandaloneElementalConsumerRoute.OrdinaryClassLearning, proof),
                "A fresh module-off process must retain original discovery.");
        }

        internal static void DomainItemNpcAndUniqueConsumersCannotBecomeRedundantByName()
        {
            var spec = StandaloneElementalPresentationPolicy.All.First(value => value.IsParent);
            var proof = CompleteSyntheticProof(spec);
            foreach (var route in new[] { StandaloneElementalConsumerRoute.DomainOrSpecialGrant,
                StandaloneElementalConsumerRoute.Item, StandaloneElementalConsumerRoute.NpcOrAi,
                StandaloneElementalConsumerRoute.Unknown })
                Assertions.False(StandaloneElementalPresentationPolicy.Disposition(true, spec.Guid, route, proof)
                    == "eligible-redundant-discovery-only",
                    "Exact ordinary ability identity does not authorize suppressing a special consumer.");
            Assertions.Equal("preserve-unique-wriggling-man",
                StandaloneElementalPresentationPolicy.Disposition(true,
                    StandaloneElementalPresentationPolicy.WrigglingManAbilityGuid,
                    StandaloneElementalConsumerRoute.OrdinaryClassLearning, proof),
                "Infested Elder Air is a unique NPC action with no claimed canonical equivalent.");
            proof.SourceSpellListGuid = proof.CanonicalSpellListGuid = "d8f30625d1b1f9d41a24446cbf7ac52e";
            Assertions.Equal("preserve-access-or-level-not-equivalent",
                StandaloneElementalPresentationPolicy.Disposition(true, spec.Guid,
                    StandaloneElementalConsumerRoute.OrdinaryClassLearning, proof),
                "A domain list cannot masquerade as an ordinary class discovery route.");
        }

        internal static void DiscoveryNeedsEveryAccessCoverageAndCompatibilityProof()
        {
            var spec = StandaloneElementalPresentationPolicy.All.First(value => value.IsParent);
            foreach (Action<StandaloneElementalEquivalenceEvidence> invalidate in
                new Action<StandaloneElementalEquivalenceEvidence>[] {
                    p => p.ConsumerInventoryComplete = false,
                    p => p.CanonicalSpellListGuid = "another-list",
                    p => p.CanonicalSpellbookGuid = "another-character-book",
                    p => p.CanonicalParentGuid = "unrelated-parent",
                    p => p.CanonicalSpellLevel++,
                    p => p.SourceSpellLevel++,
                    p => p.PublishedCanonicalCreatureKeys = spec.CreatureKeys.Skip(1).ToArray(),
                    p => p.PublishedCanonicalCreatureKeys = spec.CreatureKeys.Concat(spec.CreatureKeys).ToArray(),
                    p => p.InvocationContractEqual = false,
                    p => p.ExistingSerializedReferencesIntact = false,
                    p => p.OriginalSlotAndResourcesRetained = false,
                    p => p.LegacyInvocationStillReachable = false,
                    p => p.LearningSelectionBudgetVerified = false })
            {
                var proof = CompleteSyntheticProof(spec);
                invalidate(proof);
                Assertions.False(StandaloneElementalPresentationPolicy.Disposition(true, spec.Guid,
                    StandaloneElementalConsumerRoute.OrdinaryClassLearning, proof) ==
                        "eligible-redundant-discovery-only",
                    "A missing proof must retain the standalone discovery row.");
            }
            var complete = CompleteSyntheticProof(spec);
            Assertions.Equal("eligible-redundant-discovery-only",
                StandaloneElementalPresentationPolicy.Disposition(true, spec.Guid,
                    StandaloneElementalConsumerRoute.OrdinaryClassLearning, complete),
                "A synthetic complete ordinary proof should exercise the positive predicate.");
            Assertions.Equal("preserve-unknown-identity",
                StandaloneElementalPresentationPolicy.Disposition(true, "unknown-guid",
                    StandaloneElementalConsumerRoute.OrdinaryClassLearning, complete),
                "Unknown or third-party spell identity must remain untouched.");
        }

        internal static void LearningFilterUsesFreshCopyAndKeepsSelectedKnownAndForeignRows()
        {
            var ordinary = StandaloneElementalPresentationPolicy.All.Where(value => value.IsParent).Take(3).ToArray();
            string redundant = ordinary[0].Guid, selectedLegacy = ordinary[1].Guid, knownLegacy = ordinary[2].Guid;
            var original = new List<string> { "foreign-first", redundant, "canonical", selectedLegacy, knownLegacy, "foreign-last" };
            string[] snapshot = original.ToArray();
            var eligible = new HashSet<string>(new[] { redundant, selectedLegacy, knownLegacy }, StringComparer.Ordinal);
            string disposition;
            var result = StandaloneElementalPresentationPolicy.FilterLearningRows(original, value => value,
                eligible, Set(selectedLegacy), Set(knownLegacy), Set(), 1, 0, true, out disposition);
            Assertions.Equal("filtered-redundant-learning-rows", disposition,
                "The exact UI decision seam must actually remove an eligible unselected duplicate.");
            Assertions.True(result.SequenceEqual(new[] { "foreign-first", "canonical", selectedLegacy, knownLegacy, "foreign-last" }),
                "Foreign order, selected legacy undo and old known invocation rows must survive.");
            Assertions.True(original.SequenceEqual(snapshot) && !ReferenceEquals(original, result),
                "Filtering must return an independent collection without writing the engine cache.");
            result.Clear();
            Assertions.True(original.SequenceEqual(snapshot), "Native UI sorting or clearing its copy cannot alter raw membership.");
        }

        internal static void LearningBudgetCountsDistinctUnknownSelectableRowsAndExtraSlots()
        {
            var original = new[] { "standalone", "canonical", "known", "selected", "unique", "foreign", "foreign" };
            string disposition;
            var result = StandaloneElementalPresentationPolicy.FilterLearningRows(original, value => value,
                Set("standalone"), Set("selected"), Set("known"), Set("unique"), 2, 1, true, out disposition);
            Assertions.Equal("preserve-learning-budget-insufficient", disposition,
                "Known, selected, engine-hidden unique rows and duplicate identities cannot satisfy a free extra slot.");
            Assertions.True(result.SequenceEqual(original), "An insufficient allocation restores every original row in order.");
            result = StandaloneElementalPresentationPolicy.FilterLearningRows(original, value => value,
                Set("standalone"), Set("selected"), Set("known"), Set("unique"), 1, 1, true, out disposition);
            Assertions.Equal("filtered-redundant-learning-rows", disposition,
                "Two genuinely available distinct rows can satisfy one ordinary and one extra learning slot.");
            Assertions.False(result.Contains("standalone"), "The complete budget must exercise actual filtering.");
            result = StandaloneElementalPresentationPolicy.FilterLearningRows(new[] { "standalone", "canonical" }, value => value,
                Set("standalone"), Set(), Set("canonical"), Set(), 1, 0, true, out disposition);
            Assertions.Equal("preserve-learning-budget-insufficient", disposition,
                "An already-known canonical spell cannot make the last legacy learning choice disappear.");
            Assertions.True(result.Contains("standalone"), "The final legal learning row remains reachable.");
        }

        internal static void UnverifiedLearningContextAlwaysReturnsIndependentOriginalCopy()
        {
            var original = new[] { "foreign", "standalone", "canonical" };
            foreach (var inputs in new[] { new[] { -1, 0, 1 }, new[] { 0, -1, 1 }, new[] { 0, 0, 0 } })
            {
                string disposition;
                var result = StandaloneElementalPresentationPolicy.FilterLearningRows(original, value => value,
                    Set("standalone"), Set(), Set(), Set(), inputs[0], inputs[1], inputs[2] == 1, out disposition);
                Assertions.Equal("preserve-selection-context-unproved", disposition,
                    "Unknown native selection context and invalid allocation counts must preserve discovery.");
                Assertions.True(result.SequenceEqual(original) && !ReferenceEquals(result, original),
                    "Even a preserve decision returns an independent UI copy.");
            }
        }

        private static HashSet<string> Set(params string[] values)
        { return new HashSet<string>(values, StringComparer.Ordinal); }

        private static StandaloneElementalEquivalenceEvidence CompleteSyntheticProof(
            StandaloneElementalAbilitySpec spec)
        {
            // Positive policy fixture only. Runtime evidence is assembled
            // independently from the actual list/book/publication/graphs.
            return new StandaloneElementalEquivalenceEvidence {
                ConsumerInventoryComplete = true,
                SourceSpellListGuid = "ba0401fdeb4062f40a7aa95b6f07fe89",
                CanonicalSpellListGuid = "ba0401fdeb4062f40a7aa95b6f07fe89",
                SourceSpellbookGuid = "synthetic-character-book",
                CanonicalSpellbookGuid = "synthetic-character-book",
                SourceSpellLevel = spec.CanonicalTier,
                CanonicalSpellLevel = spec.CanonicalTier,
                CanonicalParentGuid = spec.MonsterParentGuid,
                PublishedCanonicalCreatureKeys = spec.CreatureKeys.ToArray(),
                InvocationContractEqual = true,
                ExistingSerializedReferencesIntact = true,
                OriginalSlotAndResourcesRetained = true,
                LegacyInvocationStillReachable = true,
                LearningSelectionBudgetVerified = true
            };
        }
    }
}
