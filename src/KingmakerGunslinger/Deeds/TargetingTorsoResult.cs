using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingTorsoResult
    {
        internal TargetingTorsoResult(TargetingHeadDecision decision,
            RuleAttackWithWeapon attack, RuleDealDamage damage)
        { Decision = decision; Attack = attack; Damage = damage; }
        internal TargetingHeadDecision Decision { get; private set; }
        internal RuleAttackWithWeapon Attack { get; private set; }
        internal RuleDealDamage Damage { get; private set; }
        internal bool Hit
        { get { return Attack != null && Attack.AttackRoll != null && Attack.AttackRoll.IsHit; } }
    }
}
