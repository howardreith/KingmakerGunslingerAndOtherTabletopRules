using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// O05 (Oread Monk): the owner's earned bonus on the confirmation roll of
    /// its own unarmed strikes (weapon category UnarmedStrike), capped at
    /// five steps and nonstacking with Critical Focus exactly like the
    /// firearm counter. Natural attacks and manufactured weapons are not
    /// unarmed strikes. It changes only CriticalConfirmationBonus.
    /// Kingmaker marks its unarmed-strike weapon types natural as well, so
    /// the native IsUnarmed flag, not IsNatural, separates an unarmed strike
    /// from a claw, bite or other natural attack.
    /// </summary>
    public sealed class FavoredClassUnarmedConfirmationBonus : RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        public int Divisor = 3;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        public BlueprintFeature CriticalFocus;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null ||
                !FavoredClassRuntime.MechanicsEnabled || !IsUnarmedStrike(evt))
                return;
            int earned = FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
            int contribution = FavoredClassMechanicsPolicy.ConfirmationContribution(earned,
                FavoredClassCriticalFocus.Contribution(Owner, CriticalFocus, evt));
            if (contribution > 0)
                evt.CriticalConfirmationBonus += contribution;
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }

        internal static bool IsUnarmedStrike(RuleAttackRoll evt)
        {
            return evt.Weapon != null && evt.Weapon.Blueprint != null &&
                evt.Weapon.Blueprint.Category == WeaponCategory.UnarmedStrike &&
                evt.Weapon.Blueprint.IsUnarmed;
        }
    }
}
