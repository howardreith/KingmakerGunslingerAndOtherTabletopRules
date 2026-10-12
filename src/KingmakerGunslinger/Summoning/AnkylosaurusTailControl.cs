using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using KingmakerGunslinger.Firing;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// A strike rider, not an injury effect. Native resolved hit/confirmation
    /// gates one save. The victim owns an independently expiring native buff;
    /// the source owns no cleanup callback or serialized countdown.
    /// </summary>
    [Serializable]
    public sealed class AnkylosaurusTailControl : RuleInitiatorLogicComponent<RuleAttackWithWeapon>
    {
        private static readonly ReferenceEventGate Events = new ReferenceEventGate();
        public BlueprintUnit OwningBlueprint;
        public BlueprintCharacterClass AnimalClass;
        public BlueprintItemWeapon Tail;
        public BlueprintBuff Dazed;
        public BlueprintBuff Stunned;

        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            var owner = Owner == null ? null : Owner.Unit;
            var attack = evt == null ? null : evt.AttackRoll;
            var victim = evt == null ? null : evt.Target;
            bool exactOwner = evt != null && owner != null && OwningBlueprint != null &&
                ReferenceEquals(owner.Blueprint, OwningBlueprint) && ReferenceEquals(evt.Initiator, owner);
            bool exactTail = evt != null && evt.Weapon != null && Tail != null &&
                ReferenceEquals(evt.Weapon.Blueprint, Tail);
            bool hit = attack != null && attack.IsHit;
            bool available = SummonDiseaseExposure.IsAvailable(victim);
            if (!exactOwner || !exactTail || !hit || AnimalClass == null || Dazed == null || Stunned == null) return;
            // Stamp the resolved attack roll, not a wrapper or an animation.
            // A duplicate wrapper around the same native strike is also stale.
            bool first=Events.TryMark(attack);
            if(first) AnkylosaurusTailImpactVisual.Observe(owner,victim);
            var control = AnkylosaurusRulesPolicy.Decide(exactOwner, exactTail, hit,
                available, first, attack.IsCriticalConfirmed, attack.ImmuneToCriticalHit);
            if (control == AnkylosaurusControl.None) return;
            int dc = AnkylosaurusRulesPolicy.DifficultyClass(
                Owner.Progression.GetClassLevel(AnimalClass), Owner.Stats.Strength.Bonus);
            var payload = control == AnkylosaurusControl.Stunned ? Stunned : Dazed;
            var context = new MechanicsContext(owner, victim.Descriptor, payload,
                Fact == null ? null : Fact.MaybeContext, new TargetWrapper(victim));
            context.Params.DC = dc;
            var saving = new RuleSavingThrow(victim, SavingThrowType.Fortitude, dc);
            saving.Reason = context;
            context.TriggerRule(saving);
            if (saving.IsPassed) return;
            // One native duration roll, only for failed critical saves. The
            // weapon multiplier never enters this duration calculation.
            int nativeD4 = 0;
            if (control == AnkylosaurusControl.Stunned)
            {
                var durationRoll = new RuleRollDice(owner, new DiceFormula(1, DiceType.D4)) { Reason = context };
                nativeD4 = context.TriggerRule(durationRoll).Result;
            }
            int rounds = AnkylosaurusRulesPolicy.Rounds(control, nativeD4);
            var apply = new RuleApplyBuff(victim, payload, context,
                TimeSpan.FromSeconds(rounds * 6d),
                (buff, source, duration) => victim.Descriptor.Buffs.AddBuff(buff, source, duration));
            context.TriggerRule(apply);
        }
    }
}
