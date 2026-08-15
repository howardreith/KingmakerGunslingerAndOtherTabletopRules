using System.Collections.Generic;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurIdentitySpec
    {
        internal BrownFurIdentitySpec(string symbol, string guid,
            string plannedType)
        {
            Symbol = symbol;
            Guid = guid;
            PlannedType = plannedType;
        }

        internal string Symbol { get; private set; }
        internal string Guid { get; private set; }
        internal string PlannedType { get; private set; }
    }

    internal static class BrownFurIdentityCatalog
    {
        internal const int IdentityCount = 19;
        internal const string Archetype = "KMG.BrownFur.Archetype";
        internal const string PowerfulChangeFeature =
            "KMG.BrownFur.PowerfulChange.Feature";
        internal const string PowerfulChangeSelection =
            "KMG.BrownFur.PowerfulChange.SelectionAbility";
        internal const string ShareFeature =
            "KMG.BrownFur.ShareTransmutation.Feature";
        internal const string ShareActivatable =
            "KMG.BrownFur.ShareTransmutation.Activatable";
        internal const string ShareBuff =
            "KMG.BrownFur.ShareTransmutation.Buff";
        internal const string SupremacyFeature =
            "KMG.BrownFur.TransmutationSupremacy.Feature";

        internal static IReadOnlyList<BrownFurIdentitySpec> All
        {
            get
            {
                return new[] {
                    Spec(Archetype, "aafa6e62241bb14582de5f587c179329",
                        "BlueprintArchetype"),
                    Spec(PowerfulChangeFeature,
                        "b3bbed7e12463e4c434cd81eda7ab2dd", "BlueprintFeature"),
                    Spec(PowerfulChangeSelection,
                        "48e76b097fc71f586d442a308eb11f87", "BlueprintAbility"),
                    Spec("KMG.BrownFur.PowerfulChange.Strength.Ability",
                        "d2cb2236a6dc31b7ed70e27dc12d5a8a", "BlueprintAbility"),
                    Spec("KMG.BrownFur.PowerfulChange.Dexterity.Ability",
                        "d16c77bcbff53fd3c1555869017bab3e", "BlueprintAbility"),
                    Spec("KMG.BrownFur.PowerfulChange.Constitution.Ability",
                        "54a2f74043e000047041f273d1e559ad", "BlueprintAbility"),
                    Spec("KMG.BrownFur.PowerfulChange.Intelligence.Ability",
                        "a6d77c07804e16d41a3c172c7f09f4ca", "BlueprintAbility"),
                    Spec("KMG.BrownFur.PowerfulChange.Wisdom.Ability",
                        "84faeefe28992744fbf19b62e2eccb08", "BlueprintAbility"),
                    Spec("KMG.BrownFur.PowerfulChange.Charisma.Ability",
                        "649b8ea4f5a155141bef5f9827675739", "BlueprintAbility"),
                    Spec("KMG.BrownFur.PowerfulChange.Strength.Buff",
                        "958e93bc70e6ae048e2e96193423915a", "BlueprintBuff"),
                    Spec("KMG.BrownFur.PowerfulChange.Dexterity.Buff",
                        "aba507d99e1b4d6c6bda9233f708eb64", "BlueprintBuff"),
                    Spec("KMG.BrownFur.PowerfulChange.Constitution.Buff",
                        "cea64eb942b294360344824a3795a351", "BlueprintBuff"),
                    Spec("KMG.BrownFur.PowerfulChange.Intelligence.Buff",
                        "5bb5dd956df4d7bc2cf03e02bbd28d5f", "BlueprintBuff"),
                    Spec("KMG.BrownFur.PowerfulChange.Wisdom.Buff",
                        "81ce31c8f868e0db5c4aa8a8e9cf1656", "BlueprintBuff"),
                    Spec("KMG.BrownFur.PowerfulChange.Charisma.Buff",
                        "9fe5998e93963fec5ae91aed6a060ef0", "BlueprintBuff"),
                    Spec(ShareFeature, "b7e929dac874cd22d173ee8f4fe0bfa4",
                        "BlueprintFeature"),
                    Spec(ShareActivatable,
                        "8641e6c39ff133ad71f669e35e1ee688",
                        "BlueprintActivatableAbility"),
                    Spec(ShareBuff, "215a03a25c8ff8b76114bf7513869d6c",
                        "BlueprintBuff"),
                    Spec(SupremacyFeature,
                        "c69cd7091219708f981272f2ac057135", "BlueprintFeature")
                };
            }
        }

        internal static string PowerfulAbility(BrownFurAbilityScore score)
        { return "KMG.BrownFur.PowerfulChange." + score + ".Ability"; }

        internal static string PowerfulBuff(BrownFurAbilityScore score)
        { return "KMG.BrownFur.PowerfulChange." + score + ".Buff"; }

        private static BrownFurIdentitySpec Spec(string symbol, string guid,
            string plannedType)
        { return new BrownFurIdentitySpec(symbol, guid, plannedType); }
    }
}
