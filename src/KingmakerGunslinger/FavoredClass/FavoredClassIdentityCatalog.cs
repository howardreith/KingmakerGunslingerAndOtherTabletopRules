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
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Misfire.Pistol.Partial",
                "115ab4b2b0174a708cb93adac7826079", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Misfire.Pistol.Full",
                "6d125884d3f94f83a59ba57112a31ec6", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Misfire.Musket.Partial",
                "103ecf217c6b4e93865a846a2c584f03", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Misfire.Musket.Full",
                "a603feebd92b45c683be87fe17b084c2", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Misfire.Blunderbuss.Partial",
                "7627279fc5ae4239b6ccaa51d3f55032", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Misfire.Blunderbuss.Full",
                "8bf4dee4bb164d93bf22a3bbff946dd7", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.FirearmConfirmation.Partial",
                "b07889d80b5f48b285e364885a512815", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.FirearmConfirmation.Full",
                "9d8e1f609b2b47d2b9c80a3525022e69", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.PistolWhip.Partial",
                "e24c74437b88443298f1861c1eb2043e", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.PistolWhip.Full",
                "84639afbb1d14aba83dcf6630eb626c0", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.HalflingNimble.Partial",
                "40269699043e4fe3a512e5ee28dd93bd", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.HalflingNimble.Full",
                "29a320ea1af0499d9185fe8321d60cae", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.HalflingDodge.Partial",
                "b62476699d734b10bb1c124b549f634f", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.HalflingDodge.Full",
                "5400219fafad4780b440dee91788eb68", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.DrowNimble.Partial",
                "49359d21155e4741b6a1f438bf0db7ed", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.DrowNimble.Full",
                "efb83ae0fde04abb96dd1e7cce0211e7", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Initiative.Partial",
                "385a62ab48214b32accaa2f5e7c86d66", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.Initiative.Full",
                "403489a552e5470bb720a8148c2b09ed", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.DirtyTrickTrip.Partial",
                "422ad9a4bd294b84b2f6230856ecdd10", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Gunslinger.DirtyTrickTrip.Full",
                "ece977845f1c4b40a6df5183b6caa7e4", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Alchemist.BombDamage.Partial",
                "6d101f4776294ec78ac07b2bf8b1ace7", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Alchemist.BombDamage.Full",
                "c98af1a0630a452e8af968684e285183", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Inquisitor.FireIntimidate.Partial",
                "96c9cb2bc89941b2a37f126aafccf3ca", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Inquisitor.FireIntimidate.Full",
                "98847fc06c1c4f53a1da0ce0df78f266", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Rogue.Demoralize.Partial",
                "a9f9782987db4b518ccc6726039f56fc", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Rogue.Demoralize.Full",
                "bf08007130b24072b50fa165e640ffa1", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Fighter.BullRushDefense.Full",
                "429de527d5dc46039e9d5a2311901374", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Monk.UnarmedConfirmation.Partial",
                "8b3453ed61fe489a9f120c773215b68e", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Monk.UnarmedConfirmation.Full",
                "c76759e1e881420a9248ce8fe74b5004", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Cleric.AquaticPenetration.Full",
                "189538fd7b67433e8d0cea5e691dd8eb", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Monk.GrappleStunning.Partial",
                "9c56579186ef4fdba0f07d956b1cb9a5", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Monk.GrappleStunning.Full",
                "ca51bd6d19b241e48f05e6acf892a5f1", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Paladin.AuraAllyBonus.Partial",
                "076fb613ebba4c54b3bcd3edbf841fa6", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Paladin.AuraAllyBonus.Full",
                "4c88771d0d5b4348843d1011d7ae242e", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Paladin.AuraAllyBonus.StepsProperty",
                "604f0f1bae3041618aa08b6348a088e6", "BlueprintUnitProperty"),
            new FavoredClassIdentity("KMG.FavoredClass.Ranger.CompanionNaturalArmor.Partial",
                "a2fa12fa25a848c7b297a515e745865c", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Ranger.CompanionNaturalArmor.Full",
                "4dda167346da47d7b8ee82244229df75", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Ranger.CompanionNaturalArmor.PetFeature",
                "5c47cb4deec546289e7302295b4efae2", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Summoner.EidolonNaturalArmor.Partial",
                "c8d050714c6c4def9e15f49dc9ea52b4", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Summoner.EidolonNaturalArmor.Full",
                "02c00d3cf652431a93f0248c9a5d9c96", "BlueprintFeature"),
            new FavoredClassIdentity("KMG.FavoredClass.Summoner.EidolonNaturalArmor.PetFeature",
                "563c6a25ff824d14a6bfc8012d62e46a", "BlueprintFeature"),
        };

        /// <summary>Owned helper identities that are not menu leaves.</summary>
        internal static readonly string[] AuxiliarySymbols =
        {
            "KMG.FavoredClass.Paladin.AuraAllyBonus.StepsProperty",
            "KMG.FavoredClass.Ranger.CompanionNaturalArmor.PetFeature",
            "KMG.FavoredClass.Summoner.EidolonNaturalArmor.PetFeature"
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
