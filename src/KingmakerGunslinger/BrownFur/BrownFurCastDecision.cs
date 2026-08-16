namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurCastDecision
    {
        internal BrownFurCastDecision(bool eligible, string failure,
            int reservoirCost, bool powerfulChange, bool shareTransmutation,
            bool transmutationSupremacy, int powerfulChangeIncrease,
            BrownFurShareDelivery shareDelivery,
            BrownFurAbilityScore selectedAbilityScore =
                BrownFurAbilityScore.None)
        {
            Eligible = eligible;
            Failure = failure ?? string.Empty;
            ReservoirCost = reservoirCost;
            PowerfulChange = powerfulChange;
            ShareTransmutation = shareTransmutation;
            TransmutationSupremacy = transmutationSupremacy;
            PowerfulChangeIncrease = powerfulChangeIncrease;
            ShareDelivery = shareDelivery;
            SelectedAbilityScore = selectedAbilityScore;
        }

        internal bool Eligible { get; private set; }
        internal string Failure { get; private set; }
        internal int ReservoirCost { get; private set; }
        internal bool PowerfulChange { get; private set; }
        internal bool ShareTransmutation { get; private set; }
        internal bool TransmutationSupremacy { get; private set; }
        internal int PowerfulChangeIncrease { get; private set; }
        internal BrownFurShareDelivery ShareDelivery { get; private set; }
        internal BrownFurAbilityScore SelectedAbilityScore { get; private set; }
    }
}
