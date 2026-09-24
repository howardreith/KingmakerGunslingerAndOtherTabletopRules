using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// U04 (Undine Monk): the owner's earned whole steps as extra maximum uses
    /// of an existing resource (Stunning Fist). The game queries maximums only
    /// for resources the owner already has, so a character without the
    /// resource (for example Call of the Wild's Zen Archer) gains no resource
    /// or feat; raising a maximum never refills spent uses.
    /// </summary>
    public sealed class FavoredClassResourceBonus : OwnedGameLogicComponent<UnitDescriptor>,
        IResourceAmountBonusHandler, IUnitSubscriber
    {
        public BlueprintAbilityResource Resource;
        public int Divisor = 3;

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
