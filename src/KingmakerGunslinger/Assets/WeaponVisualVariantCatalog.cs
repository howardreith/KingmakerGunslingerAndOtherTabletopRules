using System;
using System.Collections.Generic;

namespace KingmakerGunslinger.Assets
{
    internal static class WeaponVisualVariantCatalog
    {
        internal const string SpearClassic = "ElvenBranchedSpear.ClassicBranch";
        internal const string SpearThorn = "ElvenBranchedSpear.ThornBranch";
        internal const string SpearCrown = "ElvenBranchedSpear.CrownBranch";

        private static readonly Dictionary<string, string> Variants =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "KMG.ElvenBranchedSpear.BaseItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.MasterworkItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.ColdIronItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.MasterworkColdIronItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.Plus1Item", SpearClassic },
                { "KMG.ElvenBranchedSpear.Plus1ColdIronItem", SpearClassic },
                { "KMG.ElvenBranchedSpear.Boughkeeper", SpearThorn },
                { "KMG.ElvenBranchedSpear.Thornstep", SpearThorn },
                { "KMG.ElvenBranchedSpear.MoonlitFork", SpearThorn },
                { "KMG.ElvenBranchedSpear.VipersReach", SpearCrown },
                { "KMG.ElvenBranchedSpear.BriarCrownedSpear", SpearCrown },
                { "KMG.ElvenBranchedSpear.SpearOfTheFirstBranch", SpearCrown }
            };

        internal static string Require(string blueprintSymbol)
        {
            if (string.IsNullOrEmpty(blueprintSymbol))
                throw new ArgumentException(
                    "A blueprint symbol is required.", "blueprintSymbol");
            string variant;
            if (!Variants.TryGetValue(blueprintSymbol, out variant))
                throw new KeyNotFoundException(
                    "No approved weapon visual variant is mapped for " +
                    blueprintSymbol + ".");
            return variant;
        }

        internal static bool TryGet(string blueprintSymbol, out string variant)
        { return Variants.TryGetValue(blueprintSymbol, out variant); }

        internal static KeyValuePair<string, string>[] Snapshot()
        {
            var value = new KeyValuePair<string, string>[Variants.Count];
            int index = 0;
            foreach (KeyValuePair<string, string> pair in Variants)
                value[index++] = pair;
            return value;
        }
    }
}
