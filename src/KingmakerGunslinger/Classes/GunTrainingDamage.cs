using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.RuleSystem.Rules;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Classes
{
    internal sealed class GunTrainingDamage :
        RuleInitiatorLogicComponent<RuleCalculateWeaponStats>
    {
        public FirearmKind Kind;

        public override void OnEventAboutToTrigger(RuleCalculateWeaponStats evt)
        {
            if (evt == null || evt.Initiator == null || evt.Weapon == null ||
                evt.Weapon.Blueprint == null || evt.Weapon.Blueprint.Type == null)
                return;
            FirearmDefinitionComponent[] markers = evt.Weapon.Blueprint.Type
                .ComponentsArray.OfType<FirearmDefinitionComponent>().ToArray();
            if (markers.Length != 1)
                return;
            FirearmDefinition definition;
            try { definition = markers[0].Definition; }
            catch (Exception) { return; }
            int bonus;
            try
            {
                bonus = GunTrainingPolicy.DamageBonus(Kind, definition.Kind,
                    evt.Initiator.Stats.Dexterity.Bonus);
            }
            catch (ArgumentException) { return; }
            if (bonus != 0)
                evt.AddBonusDamage(bonus);
        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }
}
