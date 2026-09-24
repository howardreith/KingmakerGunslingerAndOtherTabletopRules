using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// Adds the earned grit favored-class steps to the Gunslinger grit
    /// resource maximum. It sits on the full (step-completing) leaf, whose
    /// rank is floor(N/d). Raising a maximum never refills spent grit: the
    /// game computes the maximum on demand and only rest, the resource's
    /// first addition and the existing first-level fill restore grit. It
    /// changes neither Wisdom/Charisma nor per-event grit recovery.
    /// </summary>
    public sealed class FavoredClassGritResourceBonus : OwnedGameLogicComponent<UnitDescriptor>,
        IResourceAmountBonusHandler, IUnitSubscriber
    {
        public BlueprintAbilityResource Resource;
        public int Divisor = 4;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        public void CalculateMaxResourceAmount(BlueprintAbilityResource resource, ref int bonus)
        {
            Fact fact = Fact;
            if (fact == null || !fact.Active || Resource == null || resource != Resource ||
                Owner == null || !FavoredClassRuntime.MechanicsEnabled)
                return;
            bonus += FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
        }
    }
}
