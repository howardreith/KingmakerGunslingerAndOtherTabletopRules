using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KingmakerGunslinger.FavoredClass;
using Newtonsoft.Json.Linq;

namespace KingmakerGunslinger.DomainTests
{
    internal static class FavoredClassLeafCatalogTests
    {
        // The Expanded Summoning Phase 1 appends (0.0.139) precede these blocks.
        private static readonly int PrecedingManifestEntries = 1913 + 43 +
            ExpandedSummoningSprint3Tests.AppendedLedgerIdentities +
            ExpandedSummoningSprint4Tests.AppendedLedgerIdentities +
            ExpandedSummoningSprint5Tests.AppendedLedgerIdentities +
            ExpandedSummoningSprint6Tests.AppendedLedgerIdentities +
            ExpandedSummoningSprint7Tests.AppendedLedgerIdentities +
            ExpandedSummoningSprint8Tests.AppendedLedgerIdentities +
            ExpandedSummoningCorrectionTests.AppendedLedgerIdentities;

        // L08: committed identities only; manifest tail == catalog, in order.
        internal static void ManifestTailIsExactlyTheCommittedIdentities()
        {
            JToken[] entries = JObject.Parse(File.ReadAllText(Path.Combine(
                Environment.CurrentDirectory, "blueprints", "blueprints.json")))["entries"].ToArray();
            IList<FavoredClassIdentity> identities = FavoredClassIdentityCatalog.All;
            // The Mostly Human companion block follows (ElementalMostlyHumanTests).
            Assertions.Equal(PrecedingManifestEntries + identities.Count +
                KingmakerGunslinger.ElementalRaces.ElementalMostlyHumanPolicy.IdentityCount,
                entries.Length, "Favored-class and Mostly Human blocks are the manifest tail.");
            JToken[] tail = entries.Skip(PrecedingManifestEntries).Take(identities.Count).ToArray();
            Assertions.Equal(identities.Count, tail.Length, "Favored-class manifest block size.");
            for (int index = 0; index < identities.Count; index++)
            {
                JToken entry = tail[index];
                FavoredClassIdentity identity = identities[index];
                Assertions.Equal(identity.Symbol, (string)entry["symbol"], "Symbol at " + index);
                Assertions.Equal(identity.Guid, (string)entry["guid"], "GUID at " + index);
                Assertions.Equal(identity.PlannedType, (string)entry["plannedType"], "Type at " + index);
                Assertions.Equal("active", (string)entry["status"], "Status at " + index);
                Assertions.Equal(FavoredClassIdentityCatalog.Milestone, (string)entry["milestone"],
                    "Milestone at " + index);
                Assertions.True(identity.Symbol.StartsWith(FavoredClassLeafCatalog.SymbolPrefix,
                    StringComparison.Ordinal), "Favored-class identities use the KMG.FavoredClass family.");
            }
        }

        // Every published leaf resolves to a committed identity and vice versa.
        internal static void EveryLeafHasACommittedIdentity()
        {
            IList<FavoredClassLeafSpec> leaves = FavoredClassLeafCatalog.AllLeaves();
            HashSet<string> identities = new HashSet<string>(
                FavoredClassIdentityCatalog.All.Select(value => value.Symbol), StringComparer.Ordinal);
            // Helper identities (aura steps property, pet features) are not leaves.
            foreach (string auxiliary in FavoredClassIdentityCatalog.AuxiliarySymbols)
                Assertions.True(identities.Remove(auxiliary), "Missing helper identity " + auxiliary);
            foreach (FavoredClassLeafSpec leaf in leaves)
            {
                Assertions.True(identities.Remove(leaf.Symbol), "Uncommitted leaf identity " + leaf.Symbol);
                Assertions.Equal("BlueprintFeature",
                    FavoredClassIdentityCatalog.ForSymbol(leaf.Symbol).PlannedType, leaf.Symbol + " type.");
            }
            Assertions.Equal(0, identities.Count, "Committed identities without a leaf: " +
                string.Join(", ", identities.ToArray()));
        }

        // Rank capacities come from the KMG ceiling policy, not the host's.
        internal static void LeafCapacitiesAndDisclosureFollowThePolicy()
        {
            foreach (string effectId in FavoredClassLeafCatalog.ImplementedEffects)
            {
                FavoredClassEffectSpec effect = FavoredClassCatalog.Effect(effectId);
                IList<FavoredClassLeafSpec> leaves = FavoredClassLeafCatalog.LeavesFor(effectId);
                foreach (string targetKey in FavoredClassLeafCatalog.TargetKeys(effectId))
                {
                    FavoredClassLeafSpec full = leaves.Single(leaf =>
                        leaf.Role == FavoredClassInvestmentRole.Full && leaf.TargetKey == targetKey);
                    Assertions.Equal(FavoredClassRankPolicy.FullCapacity(effect.Rate), full.Ranks,
                        effectId + " full capacity.");
                    Assertions.True(full.Description.Contains(effect.Rate.HasPartial
                            ? "completes" : "Each selection grants"),
                        effectId + " full leaf must say what one selection grants.");
                    FavoredClassLeafSpec partial = leaves.SingleOrDefault(leaf =>
                        leaf.Role == FavoredClassInvestmentRole.Partial && leaf.TargetKey == targetKey);
                    Assertions.Equal(effect.Rate.HasPartial, partial != null,
                        effectId + " has a partial leaf exactly when its divisor exceeds one.");
                    if (partial != null)
                    {
                        Assertions.Equal(FavoredClassRankPolicy.PartialCapacity(effect.Rate), partial.Ranks,
                            effectId + " partial capacity.");
                        Assertions.True(partial.Description.Contains(
                            FavoredClassLeafCatalog.Family(effectId).ImmediateText == null
                                ? "grants nothing by itself" : "at once"),
                            effectId + " partial leaf must disclose exactly what it grants.");
                        Assertions.True(partial.Name.EndsWith("(partial)", StringComparison.Ordinal),
                            effectId + " partial leaf name.");
                    }
                }
                foreach (FavoredClassLeafSpec leaf in leaves)
                {
                    Assertions.False(string.IsNullOrWhiteSpace(leaf.Name), leaf.Symbol + " name.");
                    // A target names the routes that open it (all of the effect's rows unless restricted).
                    foreach (string id in FavoredClassLeafCatalog.TargetRows(effectId, leaf.TargetKey))
                    {
                        FavoredClassSourceRow row = FavoredClassCatalog.Row(id);
                        if (!row.IsScheduled)
                            continue;
                        Assertions.True(leaf.Description.IndexOf(row.Ancestry,
                            StringComparison.OrdinalIgnoreCase) >= 0,
                            leaf.Symbol + " must name the " + row.Ancestry + " route.");
                        if (row.Publisher == FavoredClassPublisher.JonBrazerEnterprises)
                            Assertions.True(leaf.Description.Contains("Jon Brazer Enterprises"),
                                leaf.Symbol + " must attribute its third-party route.");
                    }
                }
            }
            FavoredClassLeafSpec grit = FavoredClassLeafCatalog.LeavesFor(FavoredClassCatalog.EffectGrit)
                .Single(leaf => leaf.Role == FavoredClassInvestmentRole.Full);
            Assertions.Equal(5, grit.Ranks, "Uncapped 1/4 grit reaches +5 over twenty investments.");
            Assertions.Equal("KMG.FavoredClass.Gunslinger.Grit.Full", grit.Symbol, "Grit full symbol.");
        }

        internal static void StatusRegistryLogsEachStateOnce()
        {
            FavoredClassIntegrationStatus absent = new FavoredClassIntegrationStatus(
                FavoredClassIntegrationAvailability.HostAbsent, "absent", 0, null);
            FavoredClassIntegrationStatusRegistry.Update(absent);
            Assertions.False(FavoredClassIntegrationStatusRegistry.Update(
                new FavoredClassIntegrationStatus(FavoredClassIntegrationAvailability.HostAbsent,
                    "absent", 0, new string[0])), "An identical status is not a change.");
            Assertions.True(FavoredClassIntegrationStatusRegistry.Update(
                new FavoredClassIntegrationStatus(FavoredClassIntegrationAvailability.Published,
                    "ready", 2, new[] { "x:profile-disabled" })), "A new status is a change.");
            Assertions.Equal(FavoredClassIntegrationAvailability.UnsupportedBinary,
                FavoredClassIntegrationStatus.FromHostState(FavoredClassHostState.UnsupportedBinary),
                "Unsupported host mapping.");
            Assertions.Equal(FavoredClassIntegrationAvailability.HostIncomplete,
                FavoredClassIntegrationStatus.FromHostState(FavoredClassHostState.IncompleteInitialization),
                "Incomplete host mapping.");
        }
    }
}
