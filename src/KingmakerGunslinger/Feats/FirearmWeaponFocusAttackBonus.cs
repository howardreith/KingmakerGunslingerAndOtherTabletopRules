using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Feats
{
    internal sealed class FirearmWeaponFocusAttackBonus :
        RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        public FirearmKind Kind;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (evt == null || evt.Weapon == null || evt.Weapon.Blueprint == null ||
                evt.Weapon.Blueprint.Type == null) return;
            FirearmDefinitionComponent[] markers = evt.Weapon.Blueprint.Type
                .ComponentsArray.OfType<FirearmDefinitionComponent>().ToArray();
            if (markers.Length != 1) return;
            FirearmDefinition definition;
            try { definition = markers[0].Definition; }
            catch (Exception) { return; }
            if (definition.Kind == Kind)
                evt.SetAttackBonusPenalty(evt.AttackBonusPenalty - 1);
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }
    }
}
