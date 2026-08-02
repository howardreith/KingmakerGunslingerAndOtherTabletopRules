using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Deeds
{
    public sealed class DeathsShotAttackHandler :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        public BlueprintAbilityResource Grit;
        public BlueprintCharacterClass GunslingerClass;
        public BlueprintBuff DeathEffect;
        private readonly DeathsShotService _service = new DeathsShotService();
        private static readonly ReferenceEventGate Events = new ReferenceEventGate();
        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }
        public override void OnEventDidTrigger(RuleAttackWithWeapon rule)
        {
            if (rule == null || Owner == null || Owner.Unit == null || Fact == null)
                return;
            RuleAttackRoll attack = rule.AttackRoll;
            TrueGritDecision trueGrit = TrueGritRuntime.Evaluate(Owner,
                TrueGritDeed.DeathsShot, 1, false);
            int current = Owner.Resources.GetResourceAmount(Grit);
            DeathsShotDecision decision = _service.Evaluate(
                Owner.Progression.GetClassLevel(GunslingerClass),
                Owner.Stats.Dexterity.Bonus,
                trueGrit.Available ? Math.Max(1, current) : current,
                ReferenceEquals(rule.Initiator, Owner.Unit), attack != null &&
                FirearmMarkerLookup.ReadFromRuleEvent(attack).IsExactFirearm,
                attack != null && attack.IsHit,
                attack != null && attack.IsCriticalConfirmed,
                attack != null && attack.ImmuneToCriticalHit,
                Events.TryMark(rule));
            if (!decision.ConsumeMarker) return;
            Owner.Buffs.RemoveFact(Fact);
            if (!decision.ShouldSave) return;
            Owner.Resources.Spend(Grit, trueGrit.EffectiveCost);
            var saving = new RuleSavingThrow(rule.Target,
                SavingThrowType.Fortitude, decision.DifficultyClass);
            Rulebook.Trigger(saving);
            if (!saving.IsPassed)
                rule.Target.Descriptor.Buffs.AddBuff(DeathEffect,
                    Fact.MaybeContext, TimeSpan.FromSeconds(1d));
        }
    }
}
