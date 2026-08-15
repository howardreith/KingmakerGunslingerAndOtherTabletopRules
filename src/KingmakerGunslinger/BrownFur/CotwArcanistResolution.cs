namespace KingmakerGunslinger.BrownFur
{
    internal sealed class CotwArcanistResolution
    {
        internal CotwArcanistResolution(CotwArcanistContractDecision decision,
            CotwArcanistContract contract)
        {
            Decision = decision;
            Contract = contract;
        }

        internal CotwArcanistContractDecision Decision { get; private set; }
        internal CotwArcanistContract Contract { get; private set; }
    }
}
