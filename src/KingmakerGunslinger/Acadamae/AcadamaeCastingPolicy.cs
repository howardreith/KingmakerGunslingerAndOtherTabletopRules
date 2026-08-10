using System;

namespace KingmakerGunslinger.Acadamae
{
    internal static class AcadamaeCastingPolicy
    {
        internal static AcadamaeCastDecision Decide(AcadamaeCastRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (request.SpellLevel < 0 || request.SpellLevel > 10)
                throw new ArgumentOutOfRangeException("request.SpellLevel");
            string status = !request.HasFeat ? "no-feat" :
                !request.AccelerationModeActive ? "mode-disabled" :
                !request.IsRealSpell ? "not-spell" :
                !request.HasSpellbook ? "no-spellbook" :
                !request.IsPreparedInvocation ? "not-prepared" :
                !request.IsArcane ? "not-arcane" :
                !request.IsConjuration ? "not-conjuration" :
                !request.IsSummoning ? "not-summoning" :
                request.EffectiveCastingTime <= AcadamaeCastingTime.Standard ?
                    "already-standard-or-faster" : null;
            if (status != null)
                return new AcadamaeCastDecision(false, status,
                    request.EffectiveCastingTime, request.EffectiveRounds,
                    request.SpellLevel, 0);
            if (request.EffectiveCastingTime == AcadamaeCastingTime.FullRound)
                return new AcadamaeCastDecision(true, "full-round-to-standard",
                    AcadamaeCastingTime.Standard, 0, request.SpellLevel,
                    15 + request.SpellLevel);
            if (request.EffectiveCastingTime != AcadamaeCastingTime.MultipleRounds ||
                request.EffectiveRounds < 2)
                throw new ArgumentException("A multi-round cast must take at least two rounds.",
                    "request");
            int rounds = request.EffectiveRounds - 1;
            return new AcadamaeCastDecision(true, "one-round-reduction",
                rounds == 1 ? AcadamaeCastingTime.FullRound :
                    AcadamaeCastingTime.MultipleRounds, rounds,
                request.SpellLevel, 15 + request.SpellLevel);
        }
    }
}
