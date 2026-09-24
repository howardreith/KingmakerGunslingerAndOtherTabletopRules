using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.FavoredClass
{
    internal sealed class FavoredClassIdentity
    {
        internal FavoredClassIdentity(string symbol, string guid, string plannedType)
        {
            Symbol = symbol;
            Guid = guid;
            PlannedType = plannedType;
        }

        internal string Symbol { get; private set; }
        internal string Guid { get; private set; }
        internal string PlannedType { get; private set; }
    }

    /// <summary>
    /// The exact ordered identity-manifest entries this integration appends
    /// after the Better Vendors block. The committed manifest, this catalog
    /// and the 0.0.139 validator must agree entry for entry.
    /// </summary>
    internal static class FavoredClassIdentityCatalog
    {
        internal const string Milestone = "Favored Class integration";

        private static readonly FavoredClassIdentity[] Entries =
        {
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Grit.Partial",
                "718289fb8ab945e48880961722a344fd", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Grit.Full",
                "cd8674400bea40adbd8489a08b14eeff", "BlueprintFeature"),
        };

        internal static IList<FavoredClassIdentity> All
        {
            get { return Array.AsReadOnly(Entries); }
        }

        internal static int IdentityCount
        {
            get { return Entries.Length; }
        }

        internal static FavoredClassIdentity ForSymbol(string symbol)
        {
            FavoredClassIdentity identity = Entries.FirstOrDefault(value =>
                string.Equals(value.Symbol, symbol, StringComparison.Ordinal));
            if (identity == null)
                throw new KeyNotFoundException("No committed favored-class identity for " + symbol);
            return identity;
        }
    }
}
