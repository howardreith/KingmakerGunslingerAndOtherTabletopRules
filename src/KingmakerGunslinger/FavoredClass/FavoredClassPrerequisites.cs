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

        /// <summary>The rows that open this target; empty or null means every row.</summary>
        public string[] RowIds;

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit,
            LevelUpState state)
        {
            if (unit == null || string.IsNullOrEmpty(EffectId))
                return false;
            return FavoredClassRuntime.IsEffectEligible(EffectId, unit,
                RowIds == null || RowIds.Length == 0 ? null : RowIds);
        }

        public override string GetUIText()
        {
            return "Favored class race option";
        }
    }

    /// <summary>
    /// The improved class feature is still attainable: none of the unit's
    /// archetypes of the host class removes every listed feature (O06: both
    /// paladin auras; O07: the Hunter's Bond that grants the companion).
    /// Earlier, dormant investment in a feature gained later stays legal.
    /// </summary>
    public sealed class PrerequisiteFavoredClassFeatureAvailable : Prerequisite
    {
        public BlueprintCharacterClass CharacterClass;
        public BlueprintFeature[] Features;

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit,
            LevelUpState state)
        {
            if (unit == null || CharacterClass == null || Features == null || Features.Length == 0)
                return false;
            ClassData data = unit.Progression.GetClassData(CharacterClass);
            BlueprintArchetype[] archetypes = data == null ? new BlueprintArchetype[0] :
                System.Linq.Enumerable.ToArray(data.Archetypes);
            return System.Linq.Enumerable.Any(Features, feature => feature != null &&
                !System.Linq.Enumerable.Any(archetypes, archetype => Removes(archetype, feature)));
        }

        private static bool Removes(BlueprintArchetype archetype, BlueprintFeature feature)
        {
            if (archetype == null || archetype.RemoveFeatures == null)
                return false;
            return System.Linq.Enumerable.Any(archetype.RemoveFeatures, entry => entry != null &&
                entry.Features != null && entry.Features.Contains(feature));
        }

        public override string GetUIText()
        {
            return "Can gain " + string.Join(" or ", System.Linq.Enumerable.ToArray(
                System.Linq.Enumerable.Select(Features ?? new BlueprintFeature[0],
                    feature => feature == null ? "?" : feature.Name)));
        }
    }

    /// <summary>
    /// I06/S04 and O01: the unit already has the chosen revelation (any one of
    /// its selectable features; a Dragon revelation has one per colour) or
    /// performance. Some belong to an optional provider that may create them
    /// after KMG registers, so they are matched by identity when checked.
    /// </summary>
    public sealed class PrerequisiteFavoredClassOwnsAny : Prerequisite
    {
        public string[] FeatureGuids;

        /// <summary>Player-facing name of what must be owned ("the revelation Fire Breath").</summary>
        public string Title;

        public override bool Check(FeatureSelectionState selectionState, UnitDescriptor unit,
            LevelUpState state)
        {
            if (unit == null || unit.Progression == null || FeatureGuids == null || FeatureGuids.Length == 0)
                return false;
            foreach (Feature feature in unit.Progression.Features.Enumerable)
                if (feature != null && feature.Blueprint != null &&
                    System.Array.IndexOf(FeatureGuids, feature.Blueprint.AssetGuid) >= 0)
                    return true;
            return false;
        }

        public override string GetUIText()
        {
            return "Has " + (Title ?? "?");
        }
    }
}
