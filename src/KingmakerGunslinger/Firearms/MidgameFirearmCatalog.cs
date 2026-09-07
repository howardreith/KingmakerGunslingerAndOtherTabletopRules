using System;

namespace KingmakerGunslinger.Firearms
{
    // Authored merchant items; never mundane crafting bases or fixed-loot rewards.
    internal static class MidgameFirearmCatalog
    {
        internal const string EnhancementThreeGuid = "80bb8a737579e35498177e1e3c75899b";
        internal const int ActualEnhancement = 3;
        internal const int EquivalentBonus = 4;
        internal static readonly MidgameFirearmSpec Roadwarden = new MidgameFirearmSpec(
            "KMG.Firearms.RoadwardenItem", "KMG_Roadwarden_Item", "Roadwarden",
            FirearmKind.Musket, 33800, true, false,
            "Reliable reduces this firearm's misfire value by 1 after other increases, to a minimum of 0. A natural 1 still misses.",
            "The stock bears the mile marks of a road that no longer appears on any map. Its last keeper never missed a watch.");
        internal static readonly MidgameFirearmSpec DeadReckoning = new MidgameFirearmSpec(
            "KMG.Firearms.DeadReckoningItem", "KMG_DeadReckoning_Item", "Dead Reckoning",
            FirearmKind.Pistol, 33300, false, true,
            "Seeking ignores concealment miss chances. It does not reveal unseen creatures, allow targeting a creature you could not otherwise target, or bypass other defenses.",
            "Its maker promised that no debtor could lose themselves in the mist. The promise outlived them both.");
        internal static MidgameFirearmSpec[] Entries
        { get { return new[] { Roadwarden, DeadReckoning }; } }
    }

    internal sealed class MidgameFirearmSpec
    {
        internal MidgameFirearmSpec(string symbol, string internalName, string displayName,
            FirearmKind kind, int cost, bool reliable, bool seeking,
            string description, string flavor)
        {
            Symbol = symbol; InternalName = internalName; DisplayName = displayName;
            Kind = kind; Cost = cost; Reliable = reliable; Seeking = seeking;
            Description = description; Flavor = flavor;
        }
        internal string Symbol { get; private set; }
        internal string InternalName { get; private set; }
        internal string DisplayName { get; private set; }
        internal FirearmKind Kind { get; private set; }
        internal int Cost { get; private set; }
        internal bool Reliable { get; private set; }
        internal bool Seeking { get; private set; }
        internal string Description { get; private set; }
        internal string Flavor { get; private set; }
    }
}
