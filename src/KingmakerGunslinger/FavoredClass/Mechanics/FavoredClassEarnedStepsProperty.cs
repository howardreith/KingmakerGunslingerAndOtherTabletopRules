using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Properties;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// O06: a unit property whose value is the unit's earned steps of one
    /// favored-class counter (zero while the whole integration is disabled).
    /// The ContextRankConfig the integration adds to the two native paladin
    /// ally buffs reads it on the aura's caster, so each ally buff folds its
    /// own paladin's increment into the native Morale modifier: allies of
    /// two paladins keep each paladin's own value, and the native
    /// non-stacking Morale rule still decides overlapping sources.
    /// </summary>
    public sealed class FavoredClassEarnedStepsProperty : PropertyValueGetter
    {
        public BlueprintFeature Feature;
        public int Divisor = 4;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        public override int GetInt(UnitEntityData unit)
        {
            if (unit == null || Feature == null || !FavoredClassRuntime.MechanicsEnabled)
                return 0;
            return FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null),
                unit.Descriptor.Progression.Features.GetRank(Feature));
        }
    }
}
