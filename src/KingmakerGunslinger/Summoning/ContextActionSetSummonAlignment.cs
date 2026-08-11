using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class ContextActionSetSummonAlignment : ContextAction
    {
        public SummonAlignmentMode Mode;

        public override string GetCaption()
        {
            return "Set summoned creature alignment from the summon context";
        }

        public override void RunAction()
        {
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (target == null || target.Descriptor == null ||
                target.Descriptor.Alignment == null) return;
            UnitEntityData caster = Context == null ? null : Context.MaybeCaster;
            int? casterAlignment = caster == null || caster.Descriptor == null ||
                caster.Descriptor.Alignment == null ? (int?)null :
                (int)caster.Descriptor.Alignment.Value;
            int resolved;
            if (!SummonAlignmentRuntimePolicy.TryResolve(Mode,
                (int)target.Descriptor.Alignment.Value, casterAlignment,
                out resolved)) return;
            target.Descriptor.Alignment.Set((Alignment)resolved);
        }
    }
}
