namespace KingmakerGunslinger.Acquisition
{
    internal static class SkeletalSalesmanStockCatalog
    {
        internal const string UnitGuid = "b8b362de19b0a8340ad050586f1162d1";
        internal const string UnitName = "RE_Trader";
        internal const string DialogueGuid = "0dd69ac03c55bc14292a9f9c887885ff";
        internal const int FirstKingdomDay = 491;
        internal const int CopiesPerStock = 1;
        // Both random variants at days 491..690, and both later variants (691+).
        // C2, other merchants, and fixed campaign loot are deliberately absent.
        internal static SkeletalSalesmanStockTarget[] Targets
        {
            get { return new[] {
                new SkeletalSalesmanStockTarget("b3bc1bb9f4a59f3438edc505e0f3b407", "C3_VendorTableLarge"),
                new SkeletalSalesmanStockTarget("9126c670f0743b647b4e9ba850214d8d", "C3_VendorTableSmall"),
                new SkeletalSalesmanStockTarget("fc01b45fee3606749a21d9612c5629a6", "C4_VendorTableLarge"),
                new SkeletalSalesmanStockTarget("4b1bb03a5d19a534bad2aa5cd766af92", "C4_VendorTableSmall") }; }
        }
    }

    internal sealed class SkeletalSalesmanStockTarget
    {
        internal SkeletalSalesmanStockTarget(string guid, string name)
        { Guid = guid; Name = name; }
        internal string Guid { get; private set; }
        internal string Name { get; private set; }
    }
}
