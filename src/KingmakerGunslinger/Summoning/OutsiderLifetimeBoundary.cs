using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Summoning
{
    // BuffCollection raises this event before Buff.OnRemove starts native
    // dismember/fade. Retire exact-owner executable work while its view exists.
    // Native duration, destruction and independently owned effects are untouched.
    public sealed class OutsiderLifetimeBoundary : OwnedGameLogicComponent<UnitDescriptor>, IUnitBuffHandler
    {
        public BlueprintUnit OwningBlueprint;
        [JsonIgnore] private bool _retiring;
        [JsonIgnore] private bool _retired;
        public void HandleBuffDidAdded(Buff buff) { }
        public void HandleBuffDidRemoved(Buff buff)
        {
            var unit = Owner?.Unit;
            ModContext context;
            if (_retiring || _retired || unit == null || unit.Destroyed || !unit.IsInState || unit.View == null ||
                OwningBlueprint == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint) ||
                buff == null || !ReferenceEquals(buff.Owner, Owner) ||
                !ReferenceEquals(buff.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) ||
                !ModContext.TryGet(out context) || context.FeatureModules?.Active == null ||
                !context.FeatureModules.Active.ExpandedSummoning) return;
            _retiring = true;
            try {
                // The predicate overload interrupts AND removes active and queued
                // commands; the boolean overload can merely clear queued entries.
                unit.Commands.InterruptAll(command => true);
                _retired = true;
            } finally { _retiring = false; }
        }
    }
}
