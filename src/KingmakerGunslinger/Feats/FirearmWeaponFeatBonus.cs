using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Feats
{
    internal enum FirearmWeaponFeatEffect
    {
        Attack = 0,
        Damage = 1,
        DoubleCriticalEdge = 2
    }

    internal sealed class FirearmWeaponFeatBonus :
        RuleInitiatorLogicComponent<RuleAttackRoll>,
        Kingmaker.PubSubSystem.IInitiatorRulebookHandler<RuleCalculateWeaponStats>
    {
        public FirearmKind Kind;
        public FirearmWeaponFeatEffect Effect;
        public int Bonus;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (Effect == FirearmWeaponFeatEffect.Attack && Matches(
                evt == null ? null : evt.Weapon))
                evt.SetAttackBonusPenalty(evt.AttackBonusPenalty - Bonus);
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }

        public void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt == null || !Matches(evt.Weapon)) return;
            if (Effect == FirearmWeaponFeatEffect.Damage)
                evt.AddBonusDamage(Bonus);
            else if (Effect == FirearmWeaponFeatEffect.DoubleCriticalEdge)
                evt.DoubleCriticalEdge = true;
        }

        public void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }

        private bool Matches(Kingmaker.Items.ItemEntityWeapon weapon)
        {
            if (weapon == null || weapon.Blueprint == null ||
                weapon.Blueprint.Type == null) return false;
            FirearmDefinitionComponent[] markers = weapon.Blueprint.Type
                .ComponentsArray.OfType<FirearmDefinitionComponent>().ToArray();
            if (markers.Length != 1) return false;
            try { return markers[0].Definition.Kind == Kind; }
            catch (Exception) { return false; }
        }
    }
}
