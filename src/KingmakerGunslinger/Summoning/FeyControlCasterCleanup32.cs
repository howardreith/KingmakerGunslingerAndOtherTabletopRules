using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic.Buffs.Components;

namespace KingmakerGunslinger.Summoning
{
    // Used by private timed control effects, including Succubus. Native destruction
    // raises IUnitHandler before removing the destroyed caster from the entity
    // pool, so RemoveBuffIfCasterIsMissing can still resolve that caster. Match
    // the exact event identity at this existing boundary; keep the native
    // component alongside this one for its missing-source area-load handling.
    // No extra owner state, polling, registry or serialized lifecycle is added.
    [Serializable]
    public sealed class FeyControlCasterCleanup32 : BuffLogic, IUnitHandler, IGlobalSubscriber
    {
        public void HandleUnitSpawned(UnitEntityData unit) { }
        public void HandleUnitDeath(UnitEntityData unit) { }
        public void HandleUnitDestroyed(UnitEntityData unit)
        {
            if (unit != null && ReferenceEquals(Buff?.Context?.MaybeCaster, unit))
                Buff.Remove();
        }
    }
}
