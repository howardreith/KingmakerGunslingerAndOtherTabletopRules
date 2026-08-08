using System;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Class.LevelUp;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Feats
{
    [Serializable]
    public sealed class PrerequisiteFirearmProficiency : Prerequisite
    {
        public BlueprintFeature FullProficiency;
        public BlueprintFeature OneHandedProficiency;
        public BlueprintFeature TwoHandedProficiency;
        public FirearmKind Kind;

        public override bool Check(FeatureSelectionState selectionState,
            UnitDescriptor unit, LevelUpState state)
        {
            return unit != null && unit.Progression != null &&
                FullProficiency != null && OneHandedProficiency != null &&
                TwoHandedProficiency != null &&
                FirearmProficiencyPolicy.CanUse(1, Kind,
                    unit.Progression.Features.GetRank(FullProficiency) > 0,
                    unit.Progression.Features.GetRank(OneHandedProficiency) > 0,
                    unit.Progression.Features.GetRank(TwoHandedProficiency) > 0);
        }

        public override string GetUIText()
        {
            return "Proficiency with " + Kind + " firearms";
        }
    }
}
