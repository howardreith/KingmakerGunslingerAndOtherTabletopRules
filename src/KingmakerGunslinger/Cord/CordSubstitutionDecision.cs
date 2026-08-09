namespace KingmakerGunslinger.Cord
{
    internal sealed class CordSubstitutionDecision
    {
        internal CordSubstitutionDecision(bool substituted, int damage,
            bool applyFatigue, string status)
        { Substituted = substituted; Damage = damage;
            ApplyFatigue = applyFatigue; Status = status; }
        internal bool Substituted { get; private set; }
        internal int Damage { get; private set; }
        internal bool ApplyFatigue { get; private set; }
        internal string Status { get; private set; }
    }
}
