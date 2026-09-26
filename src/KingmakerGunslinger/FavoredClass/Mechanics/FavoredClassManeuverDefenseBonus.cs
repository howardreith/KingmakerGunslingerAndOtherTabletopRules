using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// O04 (Oread Fighter adaptation: bull rush) and U04 (Undine Monk:
    /// grapple): +1 CMD per rank of the leaf that carries it against exactly
    /// one maneuver, when the owner is the maneuver's target. For U04 both
    /// the full and the partial leaf carry it, so the CMD equals every
    /// investment (full + partial), as the mixed-rate rule requires.
    /// </summary>
    public sealed class FavoredClassManeuverDefenseBonus : RuleTargetLogicComponent<RuleCalculateCMD>
    {
        public CombatManeuver Maneuver;

        public override void OnEventAboutToTrigger(RuleCalculateCMD evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null ||
                !FavoredClassRuntime.MechanicsEnabled || evt.Type != Maneuver)
                return;
            int rank = fact.GetRank();
            if (rank > 0)
                evt.AddBonus(rank, fact);
        }

        public override void OnEventDidTrigger(RuleCalculateCMD evt) { }
    }
}
