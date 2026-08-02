using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingLegsResult
    {
        internal TargetingLegsResult(TargetingHeadDecision decision,
            RuleAttackWithWeapon attack, RuleDealDamage damage,
            TargetingLegsRiderDecision rider, RuleCombatManeuver trip)
        { Decision = decision; Attack = attack; Damage = damage;
            Rider = rider; Trip = trip; }
        internal TargetingHeadDecision Decision { get; private set; }
        internal RuleAttackWithWeapon Attack { get; private set; }
        internal RuleDealDamage Damage { get; private set; }
        internal TargetingLegsRiderDecision Rider { get; private set; }
        internal RuleCombatManeuver Trip { get; private set; }
        internal bool Hit
        { get { return Attack != null && Attack.AttackRoll != null && Attack.AttackRoll.IsHit; } }
    }
}
