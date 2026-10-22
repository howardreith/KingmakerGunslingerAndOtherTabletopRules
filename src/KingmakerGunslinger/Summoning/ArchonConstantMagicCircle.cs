using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Summoning
{
    // Grant the canonical shared circle with this creature's SLA caster level.
    // Native AddFacts passes no context (CL1). Like AddFacts, retain the exact
    // applied fact across reapplication/load and remove only that instance.
    [Serializable]
    public sealed class ArchonConstantMagicCircle : OwnedGameLogicComponent<UnitDescriptor>
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintBuff Carrier;
        public int CasterLevel;
        [JsonProperty] private Buff _applied;

        public override void OnFactActivate()
        {
            if (IsReapplying) { _applied?.Recalculate(); return; }
            var unit = Owner?.Unit;
            if (unit == null || !ReferenceEquals(unit.Blueprint, OwningBlueprint)) return;
            if (Carrier == null || CasterLevel < 1)
                throw new InvalidOperationException("Archon constant circle dependency is invalid.");
            var context = new MechanicsContext(unit, Owner, Carrier,
                Fact?.MaybeContext, new TargetWrapper(unit));
            context.Params.CasterLevel = CasterLevel;
            _applied = Owner.AddFact(Carrier, context) as Buff;
            if (_applied == null)
                throw new InvalidOperationException("Archon constant circle grant failed.");
        }

        public override void OnFactDeactivate()
        {
            if (IsReapplying) return;
            _applied?.Remove();
            _applied = null;
        }
    }
}
