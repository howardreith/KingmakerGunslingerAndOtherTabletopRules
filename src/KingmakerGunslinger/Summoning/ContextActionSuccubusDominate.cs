using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class ContextActionSuccubusDominate : ContextAction
    {
        public BlueprintBuff Domination;

        public override string GetCaption()
        {
            return "Apply bounded Succubus domination after a Will save";
        }

        public override void RunAction()
        {
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (target == null || target.Descriptor == null ||
                Domination == null || Context == null) return;
            var saving = new RuleSavingThrow(target, SavingThrowType.Will,
                Context.Params.DC);
            Rulebook.Trigger(saving);
            if (saving.IsPassed) return;
            Buff applied = target.Descriptor.Buffs.AddBuff(Domination,
                Context, TimeSpan.FromSeconds(6d *
                    ExpandedSummoningSpecialProfiles.SuccubusDominateRounds));
            if (applied != null) applied.IsNotDispelable = false;
        }
    }
}
