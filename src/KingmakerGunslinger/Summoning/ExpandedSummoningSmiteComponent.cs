using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class ExpandedSummoningSmiteComponent :
        RuleInitiatorLogicComponent<RuleAttackRoll>,
        IInitiatorRulebookHandler<RuleCalculateWeaponStats>,
        IInitiatorRulebookHandler<RuleAttackWithWeapon>
    {
        public bool SmitesEvil;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (!Eligible(evt == null ? null : evt.Target) || Owner == null) return;
            int bonus = SummonTemplateSmitePolicy.AttackBonus(
                Owner.Stats.Charisma.Bonus);
            if (bonus != 0)
                evt.SetAttackBonusPenalty(evt.AttackBonusPenalty - bonus);
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt == null || evt.AttackWithWeapon == null || Owner == null ||
                !Eligible(evt.AttackWithWeapon.Target)) return;
            int bonus = SummonTemplateSmitePolicy.DamageBonus(
                Owner.Progression.CharacterLevel);
            if (bonus != 0) evt.AddBonusDamage(bonus);
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        public void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || !evt.AttackRoll.IsHit ||
                !Eligible(evt.Target) || Owner == null || Fact == null) return;
            Owner.Buffs.RemoveFact(Fact);
        }

        private bool Eligible(UnitEntityData target)
        {
            return target != null && target.Descriptor != null &&
                target.Descriptor.Alignment != null &&
                SummonTemplateSmitePolicy.IsEligible(SmitesEvil,
                    (int)target.Descriptor.Alignment.Value);
        }
    }
}
