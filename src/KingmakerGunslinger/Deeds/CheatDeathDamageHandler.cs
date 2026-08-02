using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.RuleSystem.Rules.Damage;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Deeds
{
    public sealed class CheatDeathDamageHandler :
        RuleTargetLogicComponent<RuleDealDamage>
    {
        public BlueprintAbilityResource Grit;
        public BlueprintCharacterClass GunslingerClass;
        private readonly CheatDeathService _service = new CheatDeathService();
        private static readonly ReferenceEventGate Events = new ReferenceEventGate();

        public override void OnEventAboutToTrigger(RuleDealDamage evt) { }
        public override void OnEventDidTrigger(RuleDealDamage evt) { Apply(evt); }

        private void Apply(RuleDealDamage rule)
        {
            if (rule == null || Owner == null || Owner.Unit == null || Grit == null ||
                GunslingerClass == null || Fact == null) return;
            bool ownsTarget = ReferenceEquals(rule.Target, Owner.Unit);
            int grit = Owner.Resources.GetResourceAmount(Grit);
            CheatDeathDecision decision = _service.Evaluate(new CheatDeathRequest(
                Owner.Progression.GetClassLevel(GunslingerClass), grit,
                Owner.Unit.HPLeft, rule.Damage, ownsTarget, Events.TryMark(rule)));
            if (!decision.Applied) return;
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(Owner,
                TrueGritDeed.CheatDeath, decision.GritCost, false);
            if (!trueGrit.Available) return;
            int damage = Owner.Unit.Damage;
            try
            {
                Owner.Resources.Spend(Grit, trueGrit.EffectiveCost);
                if (Owner.Resources.GetResourceAmount(Grit) !=
                    grit - trueGrit.EffectiveCost)
                    throw new InvalidOperationException(
                        "The computed Cheat Death grit cost was not spent.");
                Owner.Unit.Damage = Math.Max(0, Owner.Unit.MaxHP - 1);
                if (Owner.Unit.HPLeft != decision.FinalHitPoints)
                    throw new InvalidOperationException("Cheat Death did not leave exactly 1 HP.");
            }
            catch (Exception exception)
            {
                Owner.Unit.Damage = damage;
                int after = Owner.Resources.GetResourceAmount(Grit);
                if (after < grit) Owner.Resources.Restore(Grit, grit - after);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("cheat-death", "damage-handler.failed",
                        "Cheat Death rolled back atomically.", exception);
            }
        }
    }
}
