using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Bootstrap;

namespace KingmakerGunslinger.Summoning
{
    // Native SummonedUnitBuff.OnRemoved starts a delayed dismember/despawn
    // presentation. End this exact summon's commands at its rules deadline;
    // leave native fading, duration accounting and destruction ownership intact.
    public sealed class GriffonLifetimeBoundary : OwnedGameLogicComponent<UnitDescriptor>, IUnitBuffHandler
    {
        public void HandleBuffDidAdded(Buff buff) { }
        public void HandleBuffDidRemoved(Buff buff)
        {
            ModContext context;
            if (Owner?.Unit?.Blueprint == null || buff == null || !ReferenceEquals(buff.Owner, Owner) ||
                Owner.Unit.Blueprint.AssetGuid != GriffonVisualPolicy.BlueprintGuid ||
                !ReferenceEquals(buff.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) ||
                !ModContext.TryGet(out context) || context.FeatureModules?.Active == null ||
                !context.FeatureModules.Active.ExpandedSummoning) return;
            Owner.Unit.Commands?.InterruptAll(true);
        }
    }
}
