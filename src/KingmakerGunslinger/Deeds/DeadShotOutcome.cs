namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadShotOutcome
    {
        internal DeadShotOutcome(int hitCount, int baseDamageDicePackets,
            bool misfires, int threatCount, int? confirmationPenalty)
        {
            HitCount = hitCount;
            BaseDamageDicePackets = baseDamageDicePackets;
            Misfires = misfires;
            ThreatCount = threatCount;
            ConfirmationPenalty = confirmationPenalty;
        }

        internal int HitCount { get; private set; }
        internal int BaseDamageDicePackets { get; private set; }
        internal bool Misfires { get; private set; }
        internal int ThreatCount { get; private set; }
        internal int? ConfirmationPenalty { get; private set; }
        internal bool IsHit { get { return HitCount > 0; } }
        internal int AdditionalBaseDamageDicePackets
        {
            get { return BaseDamageDicePackets > 0 ? BaseDamageDicePackets - 1 : 0; }
        }
    }
}
