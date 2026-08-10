namespace KingmakerGunslinger.Acadamae
{
    internal sealed class AcadamaeCastDecision
    {
        internal AcadamaeCastDecision(bool eligible, string status,
            AcadamaeCastingTime resultingTime, int resultingRounds,
            int spellLevel, int fortitudeDc)
        {
            Eligible = eligible; Status = status; ResultingTime = resultingTime;
            ResultingRounds = resultingRounds; SpellLevel = spellLevel;
            FortitudeDc = fortitudeDc;
        }
        internal bool Eligible { get; private set; }
        internal string Status { get; private set; }
        internal AcadamaeCastingTime ResultingTime { get; private set; }
        internal int ResultingRounds { get; private set; }
        internal int SpellLevel { get; private set; }
        internal int FortitudeDc { get; private set; }
    }
}
