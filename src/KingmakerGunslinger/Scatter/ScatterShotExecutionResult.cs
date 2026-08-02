using System;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Misfires;

namespace KingmakerGunslinger.Scatter
{
    internal sealed class ScatterShotExecutionResult
    {
        internal ScatterShotExecutionResult(ScatterTargetPlan plan,
            ScatterDischargeDecision discharge,
            ScatterAttackVolleyDecision volley,
            RuleAttackWithWeapon[] attacks,
            FirearmMisfireConditionDecision condition,
            FirearmState before, FirearmState after)
        {
            Plan = plan ?? throw new ArgumentNullException("plan");
            Discharge = discharge ?? throw new ArgumentNullException("discharge");
            Volley = volley ?? throw new ArgumentNullException("volley");
            Attacks = attacks ?? throw new ArgumentNullException("attacks");
            Before = before ?? throw new ArgumentNullException("before");
            After = after ?? throw new ArgumentNullException("after");
            Condition = condition;
        }
        internal ScatterTargetPlan Plan { get; private set; }
        internal ScatterDischargeDecision Discharge { get; private set; }
        internal ScatterAttackVolleyDecision Volley { get; private set; }
        internal RuleAttackWithWeapon[] Attacks { get; private set; }
        internal FirearmMisfireConditionDecision Condition { get; private set; }
        internal FirearmState Before { get; private set; }
        internal FirearmState After { get; private set; }
    }
}
