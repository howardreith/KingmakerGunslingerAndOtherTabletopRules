using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Deeds
{
    public sealed class StunningShotAttackHandler :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintAbilityResource Grit;
        public BlueprintCharacterClass GunslingerClass;
        public BlueprintBuff StunnedBuff;
        private readonly StunningShotService _service = new StunningShotService();
        private static readonly ReferenceEventGate Events = new ReferenceEventGate();

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        { Apply(evt); }

        private void Apply(RuleAttackWithWeapon rule)
        {
            if (rule == null || Owner == null || Owner.Unit == null || Fact == null ||
                Grit == null || GunslingerClass == null || StunnedBuff == null) return;
            RuleAttackRoll attack = rule.AttackRoll;
            bool owned = ReferenceEquals(rule.Initiator, Owner.Unit);
            bool exact = attack != null &&
                FirearmMarkerLookup.ReadFromRuleEvent(attack).IsExactFirearm;
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(Owner,
                TrueGritDeed.StunningShot, 2, false);
            int currentGrit = Owner.Resources.GetResourceAmount(Grit);
            int evaluationGrit = trueGrit.Available ?
                Math.Max(currentGrit, 2) : currentGrit;
            StunningShotDecision decision = _service.Evaluate(new StunningShotRequest(
                Owner.Progression.GetClassLevel(GunslingerClass),
                Owner.Stats.Wisdom.Bonus, evaluationGrit,
                exact, owned, attack != null && attack.IsHit,
                attack != null && attack.ImmuneToCriticalHit,
                Events.TryMark(rule)));
            if (!decision.ConsumeMarker) return;
            BlueprintBuff marker = Fact.Blueprint as BlueprintBuff;
            MechanicsContext markerContext = Fact.MaybeContext;
            int gritBefore = Owner.Resources.GetResourceAmount(Grit);
            Buff applied = null;
            try
            {
                Owner.Buffs.RemoveFact(Fact);
                if (!decision.ShouldSave) return;
                Owner.Resources.Spend(Grit, trueGrit.EffectiveCost);
                var saving = new RuleSavingThrow(rule.Target,
                    SavingThrowType.Fortitude, decision.DifficultyClass);
                Rulebook.Trigger(saving);
                if (!saving.IsPassed)
                {
                    applied = rule.Target.Descriptor.Buffs.AddBuff(StunnedBuff,
                        markerContext, TimeSpan.FromSeconds(6d));
                    if (applied == null)
                        throw new InvalidOperationException(
                            "Native Stunned buff was rejected.");
                }
            }
            catch (Exception exception)
            {
                if (applied != null) rule.Target.Descriptor.Buffs.RemoveFact(applied);
                int after = Owner.Resources.GetResourceAmount(Grit);
                if (after < gritBefore) Owner.Resources.Restore(Grit, gritBefore - after);
                if (marker != null) Owner.Buffs.AddBuff(marker, markerContext, null);
                ModContext context;
                if (ModContext.TryGet(out context))
                    context.Logger.Failure("stunning-shot", "attack-handler.failed",
                        "Stunning Shot rolled back atomically.", exception);
            }
        }
    }
}
