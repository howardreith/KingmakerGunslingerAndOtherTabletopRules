using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Deeds
{
    public sealed class SlingersLuckSkillCheckReroll :
        RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public BlueprintAbilityResource Grit;
        public BlueprintCharacterClass GunslingerClass;
        private readonly SlingersLuckService _service = new SlingersLuckService();
        private static readonly ReferenceEventGate Events =
            new ReferenceEventGate();

        public override void OnEventAboutToTrigger(RuleSkillCheck evt) { }
        public override void OnEventDidTrigger(RuleSkillCheck evt)
        { Apply(evt); }

        private void Apply(RuleSkillCheck rule)
        {
            if (rule == null || Owner == null || Grit == null ||
                GunslingerClass == null || Fact == null) return;
            int grit = Owner.Resources.GetResourceAmount(Grit);
            bool firstEvaluation = Events.TryMark(rule);
            SlingersLuckDecision preflight = _service.Evaluate(
                new SlingersLuckRequest(Owner.Progression.GetClassLevel(
                    GunslingerClass), grit, true,
                    SlingersLuckRollKind.SkillCheck,
                    SlingersLuckRollKind.SkillCheck, firstEvaluation,
                    rule.BaseRollResult, rule.BaseRollResult));
            if (!preflight.Applied) return;
            RulebookEvent.RollEntry second = SlingersLuckRollAccess.RollNative();
            SlingersLuckDecision decision = _service.Evaluate(new SlingersLuckRequest(
                Owner.Progression.GetClassLevel(GunslingerClass), grit, true,
                SlingersLuckRollKind.SkillCheck,
                SlingersLuckRollKind.SkillCheck, true, rule.BaseRollResult,
                second.Value));
            RulebookEvent.RollEntry first = rule.D20;
            try
            {
                SlingersLuckRollAccess.Replace(rule, second);
                Owner.Resources.Spend(Grit, decision.GritCost);
                if (Owner.Resources.GetResourceAmount(Grit) !=
                    grit - decision.GritCost)
                    throw new InvalidOperationException("Fixed grit spend failed.");
                Owner.Buffs.RemoveFact(Fact);
            }
            catch
            {
                SlingersLuckRollAccess.Replace(rule, first);
                int after = Owner.Resources.GetResourceAmount(Grit);
                if (after < grit) Owner.Resources.Restore(Grit, grit - after);
            }
        }
    }
}
