using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace KingmakerGunslinger.Summoning
{
    [Serializable]
    public sealed class ContextActionApplySummonBuff : ContextAction
    {
        public BlueprintBuff Buff;
        public BlueprintBuff[] ReplacedNativeTemplateBuffs =
            Array.Empty<BlueprintBuff>();

        public override string GetCaption()
        {
            return "Apply a permanent summon-owned buff";
        }

        public override void RunAction()
        {
            UnitEntityData target = Target == null ? null : Target.Unit;
            if (target == null || target.Descriptor == null || Buff == null)
                return;
            foreach (BlueprintBuff native in ReplacedNativeTemplateBuffs ??
                Array.Empty<BlueprintBuff>())
            {
                Kingmaker.UnitLogic.Buffs.Buff existing = native == null ? null :
                    target.Descriptor.Buffs.GetBuff(native);
                if (existing != null)
                    target.Descriptor.Buffs.RemoveFact(existing);
            }
            if (target.Descriptor.Buffs.GetBuff(Buff) != null) return;
            Kingmaker.UnitLogic.Buffs.Buff applied = target.Descriptor.Buffs
                .AddBuff(Buff, Context, null);
            if (applied != null) applied.IsNotDispelable = true;
        }
    }
}
