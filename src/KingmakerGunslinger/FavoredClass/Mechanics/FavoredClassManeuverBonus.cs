using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// G21 (Jon Brazer Enterprises Tiefling): the owner's earned CMB bonus for
    /// every dirty trick (all three native variants) and trip attempt,
    /// whatever delivers it. It never changes CMD or other maneuvers.
    /// </summary>
    public sealed class FavoredClassManeuverBonus : RuleInitiatorLogicComponent<RuleCalculateCMB>
    {
        public int Divisor = 2;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        public override void OnEventAboutToTrigger(RuleCalculateCMB evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null ||
                !FavoredClassRuntime.MechanicsEnabled || !Qualifies(evt.Type))
                return;
            int earned = FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
            if (earned > 0)
                evt.AddBonus(earned, fact);
        }

        public override void OnEventDidTrigger(RuleCalculateCMB evt) { }

        internal static bool Qualifies(CombatManeuver maneuver)
        {
            return maneuver == CombatManeuver.Trip ||
                maneuver == CombatManeuver.DirtyTrickBlind ||
                maneuver == CombatManeuver.DirtyTrickEntangle ||
                maneuver == CombatManeuver.DirtyTrickSickened;
        }
    }
}
