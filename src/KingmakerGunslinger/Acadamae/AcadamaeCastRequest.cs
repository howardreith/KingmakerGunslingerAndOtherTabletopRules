namespace KingmakerGunslinger.Acadamae
{
    internal sealed class AcadamaeCastRequest
    {
        internal bool HasFeat { get; set; }
        internal bool IsRealSpell { get; set; }
        internal bool HasSpellbook { get; set; }
        internal bool IsPreparedInvocation { get; set; }
        internal bool IsArcane { get; set; }
        internal bool IsConjuration { get; set; }
        internal bool IsSummoning { get; set; }
        internal AcadamaeCastingTime EffectiveCastingTime { get; set; }
        internal int EffectiveRounds { get; set; }
        internal int SpellLevel { get; set; }
    }
}
