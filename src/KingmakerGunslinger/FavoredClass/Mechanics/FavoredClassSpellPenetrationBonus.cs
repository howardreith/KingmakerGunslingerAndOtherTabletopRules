using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// U02 (Undine Cleric): +1 per rank on the owner's checks to overcome the
    /// spell resistance of a target that actually has one of the listed
    /// subtype facts (aquatic or water). It changes only the spell
    /// penetration of that check: never caster level, DCs or other targets.
    /// </summary>
    public sealed class FavoredClassSpellPenetrationBonus : RuleInitiatorLogicComponent<RuleSpellResistanceCheck>
    {
        public BlueprintFeature[] TargetSubtypes;

        public override void OnEventAboutToTrigger(RuleSpellResistanceCheck evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null ||
                !FavoredClassRuntime.MechanicsEnabled || evt.Target == null ||
                TargetSubtypes == null ||
                !TargetSubtypes.Any(subtype => subtype != null && evt.Target.Descriptor.HasFact(subtype)))
                return;
            int rank = fact.GetRank();
            if (rank > 0)
                evt.AdditionalSpellPenetration += rank;
        }

        public override void OnEventDidTrigger(RuleSpellResistanceCheck evt) { }
    }
}
