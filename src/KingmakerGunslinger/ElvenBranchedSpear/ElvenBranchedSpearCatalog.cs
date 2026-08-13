using System;

namespace KingmakerGunslinger.ElvenBranchedSpear
{
    internal enum ElvenBranchedSpearItemKind
    {
        Mundane = 0,
        Masterwork = 1,
        ColdIron = 2,
        MasterworkColdIron = 3,
        PlusOne = 4,
        PlusOneColdIron = 5
    }

    internal sealed class ElvenBranchedSpearItemSpec
    {
        internal ElvenBranchedSpearItemSpec(ElvenBranchedSpearItemKind kind,
            string symbol, string internalName, string displayName, int cost,
            bool masterwork, bool coldIron, int enhancement)
        {
            if (string.IsNullOrWhiteSpace(symbol) ||
                string.IsNullOrWhiteSpace(internalName) ||
                string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Spear item identity is incomplete.");
            if (cost < 0 || enhancement < 0 || enhancement > 5)
                throw new ArgumentOutOfRangeException("cost");
            if (enhancement > 0 && !masterwork)
                throw new ArgumentException("Every magic weapon must be masterwork.");
            Kind = kind;
            Symbol = symbol;
            InternalName = internalName;
            DisplayName = displayName;
            Cost = cost;
            Masterwork = masterwork;
            ColdIron = coldIron;
            Enhancement = enhancement;
        }

        internal ElvenBranchedSpearItemKind Kind { get; private set; }
        internal string Symbol { get; private set; }
        internal string InternalName { get; private set; }
        internal string DisplayName { get; private set; }
        internal int Cost { get; private set; }
        internal bool Masterwork { get; private set; }
        internal bool ColdIron { get; private set; }
        internal int Enhancement { get; private set; }
    }

    internal static class ElvenBranchedSpearCatalog
    {
        // A namespaced, non-native integer avoids claiming the likely first extension
        // value while remaining stable in BlueprintWeaponType and FeatureParam saves.
        internal const int WeaponCategoryValue = 0x004b4d47;
        internal const int DamageDieCount = 1;
        internal const int DamageDieSides = 8;
        internal const int CriticalThreatMinimum = 20;
        internal const int CriticalMultiplier = 3;
        internal const int WeightPounds = 10;
        internal const int MovementAttackOfOpportunityBonus = 2;
        internal const string WeaponTypeSymbol =
            "KMG.ElvenBranchedSpear.WeaponType";

        private static readonly ElvenBranchedSpearItemSpec[] Items =
        {
            new ElvenBranchedSpearItemSpec(ElvenBranchedSpearItemKind.Mundane,
                "KMG.ElvenBranchedSpear.BaseItem", "KMG_ElvenBranchedSpear_Item",
                "Elven Branched Spear", 20, false, false, 0),
            new ElvenBranchedSpearItemSpec(ElvenBranchedSpearItemKind.Masterwork,
                "KMG.ElvenBranchedSpear.MasterworkItem",
                "KMG_MasterworkElvenBranchedSpear_Item",
                "Masterwork Elven Branched Spear", 320, true, false, 0),
            new ElvenBranchedSpearItemSpec(ElvenBranchedSpearItemKind.ColdIron,
                "KMG.ElvenBranchedSpear.ColdIronItem",
                "KMG_ColdIronElvenBranchedSpear_Item",
                "Cold Iron Elven Branched Spear", 40, false, true, 0),
            new ElvenBranchedSpearItemSpec(
                ElvenBranchedSpearItemKind.MasterworkColdIron,
                "KMG.ElvenBranchedSpear.MasterworkColdIronItem",
                "KMG_MasterworkColdIronElvenBranchedSpear_Item",
                "Masterwork Cold Iron Elven Branched Spear", 340, true, true, 0),
            new ElvenBranchedSpearItemSpec(ElvenBranchedSpearItemKind.PlusOne,
                "KMG.ElvenBranchedSpear.Plus1Item",
                "KMG_ElvenBranchedSpearPlus1_Item",
                "+1 Elven Branched Spear", 2320, true, false, 1),
            new ElvenBranchedSpearItemSpec(ElvenBranchedSpearItemKind.PlusOneColdIron,
                "KMG.ElvenBranchedSpear.Plus1ColdIronItem",
                "KMG_ColdIronElvenBranchedSpearPlus1_Item",
                "+1 Cold Iron Elven Branched Spear", 4340, true, true, 1)
        };

        internal static ElvenBranchedSpearItemSpec[] All
        { get { return (ElvenBranchedSpearItemSpec[])Items.Clone(); } }

        internal static ElvenBranchedSpearItemSpec Require(
            ElvenBranchedSpearItemKind kind)
        {
            for (int index = 0; index < Items.Length; index++)
                if (Items[index].Kind == kind) return Items[index];
            throw new ArgumentOutOfRangeException("kind");
        }
    }
}
