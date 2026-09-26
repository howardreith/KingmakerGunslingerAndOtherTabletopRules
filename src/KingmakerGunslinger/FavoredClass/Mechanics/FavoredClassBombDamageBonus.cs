using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// I01 (Ifrit Alchemist): the owner's earned flat bonus on bomb damage.
    /// It applies to every damage calculation made directly by an ability in
    /// the native bomb list (the Fast Bombs lineage), exactly where the
    /// bomb's own flat Intelligence bonus applies (direct hit and splash, each
    /// creature once), never to the follow-up damage of a bomb's buff (acid,
    /// explosive), never per die, and never to other Alchemist abilities.
    /// </summary>
    public sealed class FavoredClassBombDamageBonus : RuleInitiatorLogicComponent<RuleCalculateDamage>
    {
        public int Divisor = 2;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        public BlueprintAbility[] Bombs;

        public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null ||
                !FavoredClassRuntime.MechanicsEnabled || Bombs == null || evt.Reason == null)
                return;
            MechanicsContext context = evt.Reason.Context;
            var ability = context == null ? null : context.AssociatedBlueprint as BlueprintAbility;
            if (ability == null || !(Bombs.Contains(ability) ||
                    (ability.Parent != null && Bombs.Contains(ability.Parent))))
                return;
            int earned = FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
            BaseDamage first = evt.DamageBundle == null ? null : evt.DamageBundle.First;
            if (earned > 0 && first != null)
                first.AddBonus(earned);
        }

        public override void OnEventDidTrigger(RuleCalculateDamage evt) { }
    }
}
