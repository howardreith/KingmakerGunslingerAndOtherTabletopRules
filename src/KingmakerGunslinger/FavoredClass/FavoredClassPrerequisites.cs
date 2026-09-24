using System.Globalization;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace KingmakerGunslinger.FavoredClass
{
    /// <summary>
    /// Opens exactly one leaf of a target counter at a time, using the same
    /// full/partial alternation as the Favored Class host
    /// (next pick is full iff (full + partial + 1) % d == 0), with KMG-owned
    /// ceilings instead of the host's truncating partial capacity. Like the
    /// host, a leaf already chosen in this selection chain cannot be chosen
    /// again in the same level-up.
    /// </summary>
    public sealed class PrerequisiteFavoredClassInvestment : Prerequisite
    {
        public BlueprintFeature Full;
        public BlueprintFeature Partial;
        public int Divisor = 1;

        /// <summary>Printed cap in benefit steps; zero means uncapped.</summary>
        public int CapSteps;

        public bool PartialRole;

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit,
            LevelUpState state)
        {
            if (unit == null || Full == null || Divisor < 1 || (Divisor > 1 && Partial == null))
                return false;
            if (selectionState != null)
            {
                if (selectionState.IsSelectedInChildren(Full))
                    return false;
                if (Partial != null && selectionState.IsSelectedInChildren(Partial))
                    return false;
            }
            int full = unit.Progression.Features.GetRank(Full);
            int partial = Partial == null ? 0 : unit.Progression.Features.GetRank(Partial);
            return FavoredClassRankPolicy.CanInvest(Rate, full, partial,
                PartialRole ? FavoredClassInvestmentRole.Partial : FavoredClassInvestmentRole.Full);
        }

        public override string GetUIText()
        {
            if (Divisor <= 1)
                return "Favored class investment";
            return PartialRole
                ? string.Format(CultureInfo.InvariantCulture,
                    "Favored class investment (not the {0}th of {0})", Divisor)
                : string.Format(CultureInfo.InvariantCulture,
                    "Favored class investment (completes {0} of {0})", Divisor);
        }

        internal FavoredClassRate Rate
        {
            get { return new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null); }
        }
    }

    /// <summary>
    /// Ancestry eligibility for one canonical effect: the unit's actual race
    /// identity plus verified permissions (FAQ, preserved host policy, Mostly
    /// Human), restricted to enabled profiles. Full and partial leaves carry
    /// identical instances of this prerequisite, so both halves of a counter
    /// always share the same restrictions.
    /// </summary>
    public sealed class PrerequisiteFavoredClassAncestry : Prerequisite
    {
        public string EffectId;

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit,
            LevelUpState state)
        {
            if (unit == null || string.IsNullOrEmpty(EffectId))
                return false;
            return FavoredClassRuntime.IsEffectEligible(EffectId, unit);
        }

        public override string GetUIText()
        {
            return "Favored class race option";
        }
    }
}
