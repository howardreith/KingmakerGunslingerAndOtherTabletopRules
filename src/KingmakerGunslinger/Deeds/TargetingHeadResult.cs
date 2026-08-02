using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class TargetingHeadResult
    {
        internal TargetingHeadResult(TargetingHeadDecision decision,
            RuleAttackWithWeapon attack, TargetingHeadRiderDecision rider,
            Buff buff)
        { Decision = decision; Attack = attack; Rider = rider; Buff = buff; }
        internal TargetingHeadDecision Decision { get; private set; }
        internal RuleAttackWithWeapon Attack { get; private set; }
        internal TargetingHeadRiderDecision Rider { get; private set; }
        internal Buff Buff { get; private set; }
        internal bool Hit { get { return Attack != null && Attack.AttackRoll != null && Attack.AttackRoll.IsHit; } }
    }
}
