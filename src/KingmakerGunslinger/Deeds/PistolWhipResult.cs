using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class PistolWhipResult
    {
        internal PistolWhipResult(PistolWhipDecision decision,
            RuleAttackWithWeapon attack, RuleCombatManeuver trip,
            int enhancement)
        {
            Decision = decision;
            Attack = attack;
            Trip = trip;
            Enhancement = enhancement;
        }
        internal PistolWhipDecision Decision { get; private set; }
        internal RuleAttackWithWeapon Attack { get; private set; }
        internal RuleCombatManeuver Trip { get; private set; }
        internal int Enhancement { get; private set; }
        internal bool Hit { get { return Attack != null && Attack.AttackRoll != null && Attack.AttackRoll.IsHit; } }
    }
}
