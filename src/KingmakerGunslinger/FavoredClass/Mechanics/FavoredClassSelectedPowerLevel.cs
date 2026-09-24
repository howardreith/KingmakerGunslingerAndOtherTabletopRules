using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// I08/S06: the owner's earned steps raise the effective sorcerer level of
    /// exactly one chosen bloodline power's ability (and its variants), for
    /// that ability's own level-scaled values only. In the native parameter
    /// rule it adds the steps as bonus caster level, which the game also
    /// passes to every class-level rank of the ability (damage dice and
    /// damage bonus) and to its caster-level checks; where the power binds its
    /// DC to half its class level, it adds exactly the resulting difference.
    /// Other powers, spells, spell slots, general caster level, BAB and saves
    /// are unchanged, and no feature is granted early.
    /// </summary>
    public sealed class FavoredClassSelectedPowerLevel : RuleInitiatorLogicComponent<RuleCalculateAbilityParams>
    {
        public BlueprintAbility Ability;
        public BlueprintFeature PowerFeature;
        public int Divisor = 6;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        public override void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null || Ability == null ||
                !FavoredClassRuntime.MechanicsEnabled || !IsChosenAbility(evt.Spell))
                return;
            int earned = FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
            if (earned <= 0)
                return;
            evt.AddBonusCasterLevel(earned);
            BindAbilitiesToClass binding = Binding();
            if (binding != null && !binding.Cantrip)
            {
                int level = binding.GetLevel(Owner);
                int delta = FavoredClassMechanicsPolicy.HalfLevelDelta(level, earned);
                if (delta != 0)
                    evt.AddBonusDC(delta);
            }
        }

        public override void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }

        internal bool IsChosenAbility(BlueprintAbility spell)
        {
            return spell != null && (spell == Ability || (spell.Parent != null && spell.Parent == Ability));
        }

        /// <summary>The power feature's own DC binding for this ability, if any.</summary>
        internal BindAbilitiesToClass Binding()
        {
            if (PowerFeature == null || PowerFeature.ComponentsArray == null)
                return null;
            return PowerFeature.ComponentsArray.OfType<BindAbilitiesToClass>().FirstOrDefault(value =>
                value.Abilites != null && value.Abilites.Contains(Ability));
        }
    }
}
