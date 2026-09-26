using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.RuleSystem.Rules;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// G05/G20: the owner's earned bonus on the Pistol-Whip deed's attack roll.
    /// The deed attacks with its own surrogate weapon items, which exist for
    /// no other attack, and marks that attack while it resolves, so the bonus
    /// is added only to the attack bonus of that exact attack (and appears in
    /// its combat-log breakdown). It never reaches firearm shots, damage or
    /// the following trip attempt, including a maneuver bonus another mod
    /// derives from the surrogate's attack bonus while the trip resolves.
    /// </summary>
    public sealed class FavoredClassPistolWhipAttackBonus :
        RuleInitiatorLogicComponent<RuleCalculateAttackBonusWithoutTarget>
    {
        public int Divisor = 3;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        public BlueprintItemWeapon OneHandedSurrogate;
        public BlueprintItemWeapon TwoHandedSurrogate;

        public override void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null ||
                !FavoredClassRuntime.MechanicsEnabled || evt.Weapon == null ||
                evt.Weapon.Blueprint == null)
                return;
            BlueprintItemWeapon blueprint = evt.Weapon.Blueprint;
            if (!ReferenceEquals(blueprint, OneHandedSurrogate) &&
                !ReferenceEquals(blueprint, TwoHandedSurrogate))
                return;
            RuleAttackWithWeapon deed = Deeds.PistolWhipAttackScope.Current;
            if (deed == null || !ReferenceEquals(deed.Weapon, evt.Weapon))
                return;
            int earned = FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
            if (earned > 0)
                evt.AddBonus(earned, fact);
        }

        public override void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt) { }
    }
}
