using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// One published menu leaf: the full (step-completing) or partial
    /// (investment-only) half of one canonical effect's target counter.
    /// </summary>
    internal sealed class FavoredClassLeafSpec
    {
        internal FavoredClassLeafSpec(string symbol, string effectId, string targetKey,
            FavoredClassInvestmentRole role, int ranks, string name, string description)
        {
            Symbol = symbol;
            EffectId = effectId;
            TargetKey = targetKey;
            Role = role;
            Ranks = ranks;
            Name = name;
            Description = description;
        }

        internal string Symbol { get; private set; }
        internal string EffectId { get; private set; }

        /// <summary>Null for an effect with one counter.</summary>
        internal string TargetKey { get; private set; }

        internal FavoredClassInvestmentRole Role { get; private set; }

        /// <summary>
        /// The leaf's rank capacity: floor(T/d) for the full leaf and
        /// T - floor(T/d) for the partial leaf.
        /// </summary>
        internal int Ranks { get; private set; }

        internal string Name { get; private set; }
        internal string Description { get; private set; }
    }

    /// <summary>
    /// Stable menu-leaf identities and player-facing text. Every symbol here
    /// has a committed identity-manifest entry; nothing is generated at
    /// runtime. Partial leaves disclose that they are investment toward a
    /// later whole benefit rather than a benefit of their own.
    /// </summary>
    internal static class FavoredClassLeafCatalog
    {
        internal const string SymbolPrefix = "KMG.FavoredClass.";

        private static readonly Dictionary<string, string> Titles =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { FavoredClassCatalog.EffectGrit, "Grit" },
            };

        private static readonly Dictionary<string, string> SymbolKeys =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { FavoredClassCatalog.EffectGrit, "Gunslinger.Grit" },
            };

        private static readonly Dictionary<string, string> StepText =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { FavoredClassCatalog.EffectGrit, "+1 maximum grit" },
            };

        /// <summary>Effects whose leaves this candidate registers.</summary>
        internal static IList<string> ImplementedEffects
        {
            get { return SymbolKeys.Keys.OrderBy(key => key, StringComparer.Ordinal).ToList(); }
        }

        internal static bool IsImplemented(string effectId)
        {
            return SymbolKeys.ContainsKey(effectId);
        }

        internal static IList<FavoredClassLeafSpec> LeavesFor(string effectId)
        {
            string key;
            if (!SymbolKeys.TryGetValue(effectId, out key))
                throw new KeyNotFoundException("No registered leaves for effect " + effectId);
            FavoredClassEffectSpec effect = FavoredClassCatalog.Effect(effectId);
            if (effect.TargetKind != FavoredClassTargetKind.None)
                throw new InvalidOperationException(effectId + " needs a target manifest.");
            List<FavoredClassLeafSpec> leaves = new List<FavoredClassLeafSpec>
            {
                Leaf(effect, key, null, FavoredClassInvestmentRole.Full)
            };
            if (effect.Rate.HasPartial)
                leaves.Add(Leaf(effect, key, null, FavoredClassInvestmentRole.Partial));
            return leaves.AsReadOnly();
        }

        internal static IList<FavoredClassLeafSpec> AllLeaves()
        {
            return ImplementedEffects.SelectMany(LeavesFor).ToList().AsReadOnly();
        }

        internal static string Symbol(string symbolKey, string targetKey, FavoredClassInvestmentRole role)
        {
            return SymbolPrefix + symbolKey + (targetKey == null ? string.Empty : "." + targetKey) +
                (role == FavoredClassInvestmentRole.Full ? ".Full" : ".Partial");
        }

        private static FavoredClassLeafSpec Leaf(FavoredClassEffectSpec effect, string key,
            string targetKey, FavoredClassInvestmentRole role)
        {
            FavoredClassRate rate = effect.Rate;
            int ranks = role == FavoredClassInvestmentRole.Full
                ? FavoredClassRankPolicy.FullCapacity(rate)
                : FavoredClassRankPolicy.PartialCapacity(rate);
            string title = Titles[effect.Id];
            string name = "Favored Class: " + title +
                (role == FavoredClassInvestmentRole.Partial ? " (partial)" : string.Empty);
            return new FavoredClassLeafSpec(Symbol(key, targetKey, role), effect.Id, targetKey,
                role, ranks, name, Describe(effect, role));
        }

        /// <summary>Tooltip text: routes, rate, cap and what this pick does.</summary>
        internal static string Describe(FavoredClassEffectSpec effect, FavoredClassInvestmentRole role)
        {
            FavoredClassRate rate = effect.Rate;
            string step = StepText[effect.Id];
            string routes = RouteText(effect);
            string cap = rate.CapSteps.HasValue
                ? string.Format(CultureInfo.InvariantCulture,
                    " The bonus is limited to {0} steps; this choice closes when the limit is reached.",
                    rate.CapSteps.Value)
                : string.Empty;
            string pick;
            if (!rate.HasPartial)
                pick = "Each selection grants " + step + ".";
            else if (role == FavoredClassInvestmentRole.Full)
                pick = string.Format(CultureInfo.InvariantCulture,
                    "This selection completes {0} investments and grants {1}. The number of completed steps equals this feature's rank.",
                    rate.Divisor, step);
            else
                pick = string.Format(CultureInfo.InvariantCulture,
                    "This selection is one investment toward the next {0}; every {1} investments complete a step. A partial investment grants nothing by itself, and its rank counts every partial investment made.",
                    step, OrdinalWord(rate.Divisor));
            string adaptation = effect.OmittedPortion == null
                ? string.Empty
                : " CRPG adaptation: " + effect.OmittedPortion;
            return "Favored class bonus (" + routes + "): " + effect.Summary + " " + pick + cap +
                adaptation;
        }

        private static string RouteText(FavoredClassEffectSpec effect)
        {
            IEnumerable<FavoredClassSourceRow> rows = effect.Rows.Select(FavoredClassCatalog.Row)
                .Where(row => row.IsScheduled);
            List<string> parts = new List<string>();
            foreach (FavoredClassSourceRow row in rows)
            {
                string ancestry = char.ToUpperInvariant(row.Ancestry[0]) + row.Ancestry.Substring(1);
                parts.Add(row.Publisher == FavoredClassPublisher.JonBrazerEnterprises
                    ? ancestry + " (Jon Brazer Enterprises)"
                    : ancestry);
            }
            return string.Join(", ", parts.ToArray());
        }

        private static string OrdinalWord(int divisor)
        {
            switch (divisor)
            {
                case 2: return "two";
                case 3: return "three";
                case 4: return "four";
                case 6: return "six";
                default: return divisor.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
