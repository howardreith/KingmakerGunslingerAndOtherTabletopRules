using System;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Deeds
{
    internal sealed class DeadShotExecutionResult
    {
        internal DeadShotExecutionResult(DeadShotDecision decision,
            DeadShotOutcome outcome, RuleAttackRoll[] probes,
            RuleAttackWithWeapon delivery, FirearmState before,
            FirearmState after)
        {
            Decision = decision ?? throw new ArgumentNullException("decision");
            Outcome = outcome;
            Probes = probes == null ? new RuleAttackRoll[0] :
                (RuleAttackRoll[])probes.Clone();
            Delivery = delivery;
            Before = before;
            After = after;
        }

        internal DeadShotDecision Decision { get; private set; }
        internal DeadShotOutcome Outcome { get; private set; }
        internal RuleAttackRoll[] Probes { get; private set; }
        internal RuleAttackWithWeapon Delivery { get; private set; }
        internal FirearmState Before { get; private set; }
        internal FirearmState After { get; private set; }
    }
}
