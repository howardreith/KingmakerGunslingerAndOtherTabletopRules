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
            FirearmTrainingRuntime.ApplyDamageOnce(evt);
        }

        public override void OnEventDidTrigger(RuleCalculateWeaponStats evt) { }
    }
}
