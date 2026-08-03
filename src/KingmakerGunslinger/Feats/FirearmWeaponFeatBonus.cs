using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Feats
{
    internal sealed class FirearmWeaponFeatBonus :
        RuleInitiatorLogicComponent<RuleAttackRoll>,
        Kingmaker.PubSubSystem.IInitiatorRulebookHandler<RuleCalculateWeaponStats>
    {
        public FirearmKind Kind;
        public FirearmWeaponFeatEffect Effect;
        public int Bonus;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            FirearmKind actual;
            if (evt == null || !TryKind(evt.Weapon, out actual)) return;
            FirearmWeaponFeatDecision decision = FirearmWeaponFeatPolicy.Evaluate(
                Kind, actual, Effect, Bonus);
            if (decision.AttackBonus != 0)
                evt.SetAttackBonusPenalty(evt.AttackBonusPenalty - decision.AttackBonus);
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            FirearmKind actual;
            if (evt == null || !TryKind(evt.Weapon, out actual)) return;
            FirearmWeaponFeatDecision decision = FirearmWeaponFeatPolicy.Evaluate(
                Kind, actual, Effect, Bonus);
            if (decision.DamageBonus != 0)
                evt.AddBonusDamage(decision.DamageBonus);
            if (decision.DoubleCriticalEdge)
                evt.DoubleCriticalEdge = true;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        private static bool TryKind(Kingmaker.Items.ItemEntityWeapon weapon,
            out FirearmKind kind)
        {
            kind = default(FirearmKind);
            if (weapon == null || weapon.Blueprint == null ||
                weapon.Blueprint.Type == null) return false;
            FirearmDefinitionComponent[] markers = weapon.Blueprint.Type
                .ComponentsArray.OfType<FirearmDefinitionComponent>().ToArray();
            if (markers.Length != 1) return false;
            try { kind = markers[0].Definition.Kind; return true; }
            catch (Exception) { return false; }
        }
    }
}
