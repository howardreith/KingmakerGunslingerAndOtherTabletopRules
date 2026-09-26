using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;

namespace KingmakerGunslinger.FavoredClass.Mechanics
{
    /// <summary>
    /// I05 (Ifrit Inquisitor adaptation) and I07 (Ifrit Rogue adaptation):
    /// the owner's earned bonus on qualifying Intimidate checks, as one
    /// untyped stackable modifier of that check only. I05 requires the
    /// action's actual current target to carry the fire subtype; I07
    /// requires the check to belong to a demoralize action. A check without
    /// a creature target (dialogue) never qualifies.
    /// </summary>
    public sealed class FavoredClassIntimidateBonus : RuleInitiatorLogicComponent<RuleSkillCheck>
    {
        public int Divisor = 2;

        /// <summary>Printed cap in steps; zero means uncapped.</summary>
        public int CapSteps;

        /// <summary>When set, the current target must have this subtype fact (I05).</summary>
        public BlueprintFeature RequiredTargetSubtype;

        /// <summary>When true, the check must belong to a demoralize action (I07).</summary>
        public bool RequireDemoralize;

        public override void OnEventAboutToTrigger(RuleSkillCheck evt)
        {
            Fact fact = Fact;
            if (evt == null || fact == null || !fact.Active || Owner == null ||
                !FavoredClassRuntime.MechanicsEnabled || evt.StatType != StatType.CheckIntimidate ||
                evt.Reason == null || evt.Reason.Context == null)
                return;
            MechanicsContext context = evt.Reason.Context;
            var data = ElementsContext.GetData<MechanicsContext.Data>();
            if (data == null || !ReferenceEquals(data.Context, context))
                return;
            UnitEntityData target = data.CurrentTarget == null ? null : data.CurrentTarget.Unit;
            if (target == null)
                return;
            if (RequiredTargetSubtype != null && !target.Descriptor.HasFact(RequiredTargetSubtype))
                return;
            if (RequireDemoralize && !FavoredClassDemoralizeScope.IsCurrent(context))
                return;
            int earned = FavoredClassRankPolicy.BenefitSteps(
                new FavoredClassRate(Divisor, CapSteps > 0 ? CapSteps : (int?)null), fact.GetRank());
            if (earned > 0)
                evt.Bonus.AddModifier(earned, this, ModifierDescriptor.UntypedStackable);
        }

        public override void OnEventDidTrigger(RuleSkillCheck evt) { }
    }
}
