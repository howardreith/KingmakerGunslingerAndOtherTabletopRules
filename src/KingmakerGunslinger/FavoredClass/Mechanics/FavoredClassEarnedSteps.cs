using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// Earned whole steps of a unit's own counter, read from its full leaf
    /// rank. Used where the favored-class bonus belongs inside an existing
    /// authoritative calculation (the misfire threshold, Gunslinger's Dodge
    /// and Gunslinger Initiative) instead of a component of the leaf. Only
    /// the whole integration switch suppresses it; a disabled profile keeps
    /// earned choices working.
    /// </summary>
    internal static class FavoredClassEarnedSteps
    {
        internal static int For(UnitDescriptor unit, string effectId, string targetKey)
        {
            if (unit == null || unit.Progression == null || !FavoredClassRuntime.MechanicsEnabled)
                return 0;
            FavoredClassBlueprintSet set = BlueprintBootstrap.FavoredClassLeaves;
            FavoredClassLeafPair pair = set == null ? null : set.Pair(effectId, targetKey);
            if (pair == null)
                return 0;
            return FavoredClassRankPolicy.BenefitSteps(pair.Effect.Rate,
                unit.Progression.Features.GetRank(pair.Full));
        }

        /// <summary>G01/G18: the wielder's earned reduction for one canonical firearm type.</summary>
        internal static int MisfireReduction(UnitEntityData wielder, FirearmKind kind)
        {
            if (wielder == null || !OfficialFirearmSupport.IsOfficial(kind))
                return 0;
            return For(wielder.Descriptor, FavoredClassCatalog.EffectMisfire, kind.ToString());
        }

        /// <summary>G06 Dodge branch: added to Gunslinger's Dodge's own bonus.</summary>
        internal static int DodgeBonus(UnitDescriptor unit)
        {
            return For(unit, FavoredClassCatalog.EffectHalflingDodge, null);
        }

        /// <summary>G11/I04: added to Gunslinger Initiative's own bonus while it applies.</summary>
        internal static int InitiativeBonus(UnitDescriptor unit)
        {
            return For(unit, FavoredClassCatalog.EffectInitiative, null);
        }
    }
}
