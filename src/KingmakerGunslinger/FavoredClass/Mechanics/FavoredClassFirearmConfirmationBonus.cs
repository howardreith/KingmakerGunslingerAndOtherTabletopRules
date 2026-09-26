using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// G02/G08/G16: the owner's earned bonus on the confirmation roll of its
    /// own firearm shots. It sits on the full leaf (rank = earned steps, at
    /// most five) and changes only RuleAttackRoll.CriticalConfirmationBonus:
    /// never the threat range, multiplier or ordinary attack bonus. A melee
    /// attack (including the Pistol-Whip surrogate) is not a firearm shot.
    /// Critical Focus does not stack with it: Critical Focus's own native
    /// contribution to this roll is computed exactly as the native component
    /// computes it, and only the excess is added, so the better bonus wins.
    /// </summary>
    public sealed class FavoredClassFirearmConfirmationBonus : RuleInitiatorLogicComponent<RuleAttackRoll>
    {
        public int Divisor = 3;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        public BlueprintFeature CriticalFocus;

        public override void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null ||
                !FavoredClassRuntime.MechanicsEnabled || !IsFirearmShot(evt))
                return;
            int earned = FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
            int contribution = FavoredClassMechanicsPolicy.ConfirmationContribution(earned,
                FavoredClassCriticalFocus.Contribution(Owner, CriticalFocus, evt));
            if (contribution > 0)
                evt.CriticalConfirmationBonus += contribution;
        }

        public override void OnEventDidTrigger(RuleAttackRoll evt) { }

        internal static bool IsFirearmShot(RuleAttackRoll evt)
        {
            if (evt.AttackType != AttackType.Ranged && evt.AttackType != AttackType.RangedTouch)
                return false;
            ItemEntityWeapon weapon = evt.Weapon;
            if (weapon == null || weapon.Blueprint == null || weapon.Blueprint.Type == null ||
                !weapon.Blueprint.IsRanged)
                return false;
            return weapon.Blueprint.Type.ComponentsArray
                .OfType<FirearmDefinitionComponent>().Count() == 1;
        }
    }
}
