using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// Read-only structural census of the shipped Expanded Summoning surface.
    /// Sprint 0 of the Expanded Summoning charter uses it to freeze a baseline
    /// that later sprints are measured against. It observes the frozen catalogs
    /// and never registers, publishes, mutates, or caches production state, so
    /// repeated calls are pure and order-stable.
    /// </summary>
    internal static class ExpandedSummoningBaselineInventory
    {
        internal const string BaselineSchema = "expanded-summoning-baseline/1";

        internal sealed class ParentCensus
        {
            internal ParentCensus(SummonFamily family, int tier, int one,
                int oneD3, int oneD4PlusOne, int suppressed, int nativeWrappers)
            {
                Family = family; Tier = tier; One = one; OneD3 = oneD3;
                OneD4PlusOne = oneD4PlusOne; Suppressed = suppressed;
                NativeWrappers = nativeWrappers;
            }

            internal SummonFamily Family { get; private set; }
            internal int Tier { get; private set; }
            internal int One { get; private set; }
            internal int OneD3 { get; private set; }
            internal int OneD4PlusOne { get; private set; }
            internal int Suppressed { get; private set; }
            internal int NativeWrappers { get; private set; }

            /// <summary>Generated placements registered for this parent spell.</summary>
            internal int Registered { get { return One + OneD3 + OneD4PlusOne; } }

            /// <summary>Generated placements the visibility catalog publishes.</summary>
            internal int PublishedGenerated { get { return Registered - Suppressed; } }

            /// <summary>Total choices a player sees under this one parent spell.</summary>
            internal int VisibleChoices { get { return PublishedGenerated + NativeWrappers; } }
        }

        /// <summary>
        /// Per-parent census for one family, ordered by ascending spell tier.
        /// </summary>
        internal static IReadOnlyList<ParentCensus> Census(SummonFamily family)
        {
            IReadOnlyList<SummonVariantSpec> variants =
                ExpandedSummoningCatalog.GenerateVariants(family);
            var result = new List<ParentCensus>();
            for (int tier = 1; tier <= 9; tier++)
            {
                int scopedTier = tier;
                SummonVariantSpec[] scoped = variants
                    .Where(value => value.ParentTier == scopedTier).ToArray();
                result.Add(new ParentCensus(family, scopedTier,
                    scoped.Count(value => value.Multiplicity == SummonMultiplicity.One),
                    scoped.Count(value => value.Multiplicity == SummonMultiplicity.OneD3),
                    scoped.Count(value => value.Multiplicity == SummonMultiplicity.OneD4PlusOne),
                    scoped.Count(value => !SummonVisibilityCatalog.IsPublished(value)),
                    SummonNativeExpansionCatalog.For(family, scopedTier).Count));
            }

            return result.AsReadOnly();
        }

        internal static int UniqueCreatures
        { get { return ExpandedSummoningCatalog.All.Count; } }

        internal static int RosterEntries(SummonFamily family)
        {
            return ExpandedSummoningCatalog.All.Count(value =>
                family == SummonFamily.Monster
                    ? value.MonsterTier.HasValue
                    : value.NaturesAllyTier.HasValue);
        }

        internal static int VisibleChoices(SummonFamily family)
        { return Census(family).Sum(value => value.VisibleChoices); }

        internal static int RegisteredPlacements(SummonFamily family)
        { return Census(family).Sum(value => value.Registered); }

        /// <summary>
        /// Creature keys that own registered identities but are withheld from
        /// every menu, so old saves keep deserializing while players see nothing.
        /// </summary>
        internal static IReadOnlyList<string> RegisteredButHiddenCreatures
        {
            get
            {
                return ExpandedSummoningCatalog.All
                    .Where(creature => !ExpandedSummoningCatalog
                        .GenerateVariants(SummonFamily.Monster)
                        .Concat(ExpandedSummoningCatalog.GenerateVariants(
                            SummonFamily.NaturesAlly))
                        .Where(value => value.Creature.Key == creature.Key)
                        .Any(SummonVisibilityCatalog.IsPublished))
                    .Select(creature => creature.Key)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Creatures whose shipped appearance borrows another creature's body.
        /// A view policy naming the creature itself is an exact visual, not a
        /// proxy, so only a differing name counts. The charter forbids counting
        /// any entry listed here as an ideal visual (decision D-03).
        /// </summary>
        internal static IReadOnlyList<string> ProxyVisualCreatures
        {
            get
            {
                return ExpandedSummoningCatalog.All
                    .Where(value => !string.IsNullOrEmpty(value.Visual) &&
                        !string.Equals(value.Visual, value.DisplayName,
                            StringComparison.Ordinal))
                    .Select(value => value.Key + "<" + value.Visual)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Deterministic JSON census. The text is byte-stable for one source
        /// revision so two observations can be compared by hash alone.
        /// </summary>
        internal static string Emit()
        {
            var text = new StringBuilder();
            text.Append("{\n  \"schema\": \"").Append(BaselineSchema)
                .Append("\",\n");
            text.Append("  \"uniqueCreatures\": ").Append(Number(UniqueCreatures))
                .Append(",\n");
            text.Append("  \"registeredLogicalPlacements\": ")
                .Append(Number(SummonVisibilityCatalog.RegisteredLogicalPlacementCount))
                .Append(",\n");
            text.Append("  \"suppressedLogicalPlacements\": ")
                .Append(Number(SummonVisibilityCatalog.SuppressedLogicalPlacementCount))
                .Append(",\n");
            text.Append("  \"publishedLogicalPlacements\": ")
                .Append(Number(SummonVisibilityCatalog.PublishedLogicalPlacementCount))
                .Append(",\n");
            text.Append("  \"nativeExpansionWrappers\": ")
                .Append(Number(SummonNativeExpansionCatalog.All.Count))
                .Append(",\n");
            text.Append("  \"nativeOptionsInspected\": ")
                .Append(Number(SummonNativeOptionCatalog.All.Count))
                .Append(",\n");
            text.Append("  \"totalVisibleChoices\": ")
                .Append(Number(VisibleChoices(SummonFamily.Monster) +
                    VisibleChoices(SummonFamily.NaturesAlly)))
                .Append(",\n");
            text.Append("  \"families\": {\n");
            AppendFamily(text, SummonFamily.Monster, "summonMonster", true);
            AppendFamily(text, SummonFamily.NaturesAlly, "summonNaturesAlly", false);
            text.Append("  },\n");
            AppendStrings(text, "registeredButHidden", RegisteredButHiddenCreatures, true);
            AppendStrings(text, "proxyVisuals", ProxyVisualCreatures, false);
            text.Append("}\n");
            return text.ToString();
        }

        private static void AppendFamily(StringBuilder text, SummonFamily family,
            string name, bool trailingComma)
        {
            text.Append("    \"").Append(name).Append("\": {\n");
            text.Append("      \"rosterEntries\": ")
                .Append(Number(RosterEntries(family))).Append(",\n");
            text.Append("      \"registeredPlacements\": ")
                .Append(Number(RegisteredPlacements(family))).Append(",\n");
            text.Append("      \"visibleChoices\": ")
                .Append(Number(VisibleChoices(family))).Append(",\n");
            text.Append("      \"parents\": [\n");
            IReadOnlyList<ParentCensus> census = Census(family);
            for (int index = 0; index < census.Count; index++)
            {
                ParentCensus value = census[index];
                text.Append("        { \"tier\": ").Append(Number(value.Tier))
                    .Append(", \"one\": ").Append(Number(value.One))
                    .Append(", \"oneD3\": ").Append(Number(value.OneD3))
                    .Append(", \"oneD4PlusOne\": ").Append(Number(value.OneD4PlusOne))
                    .Append(", \"registered\": ").Append(Number(value.Registered))
                    .Append(", \"suppressed\": ").Append(Number(value.Suppressed))
                    .Append(", \"nativeWrappers\": ").Append(Number(value.NativeWrappers))
                    .Append(", \"visibleChoices\": ").Append(Number(value.VisibleChoices))
                    .Append(" }").Append(index == census.Count - 1 ? "\n" : ",\n");
            }

            text.Append("      ]\n    }").Append(trailingComma ? ",\n" : "\n");
        }

        private static void AppendStrings(StringBuilder text, string name,
            IReadOnlyList<string> values, bool trailingComma)
        {
            text.Append("  \"").Append(name).Append("\": [");
            for (int index = 0; index < values.Count; index++)
            {
                text.Append(index == 0 ? "" : ", ").Append('"')
                    .Append(values[index]).Append('"');
            }

            text.Append(']').Append(trailingComma ? ",\n" : "\n");
        }

        private static string Number(int value)
        { return value.ToString(CultureInfo.InvariantCulture); }
    }
}
