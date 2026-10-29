using System;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;

namespace KingmakerGunslinger.Summoning
{
    // Only private S34 wound, disease, gaze and chain-control buffs carry this.
    // Each callback removes this exact instance, never another source's fact.
    [Serializable]
    public sealed class InfernalEffectLifetime34 : BuffLogic, IUnitHandler,
        IPartyLeaveAreaHandler, IAreaLoadingStagesHandler, ITickEachRound,
        IGlobalSubscriber
    {
        internal static bool SourceIsLive(UnitEntityData source)
        {
            var summoner = source?.Get<UnitPartSummonedMonster>()?.Summoner;
            return source != null && !source.Destroyed && source.IsInState &&
                !source.Descriptor.State.IsDead &&
                source.Buffs.GetBuff(BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff) != null &&
                summoner != null && !summoner.Destroyed && summoner.IsInState &&
                !summoner.Descriptor.State.IsDead;
        }

        public void HandleUnitSpawned(UnitEntityData unit) { }
        public void HandleUnitDestroyed(UnitEntityData unit) { HandleUnitDeath(unit); }
        public void HandleUnitDeath(UnitEntityData unit)
        {
            var source = Buff?.Context?.MaybeCaster;
            if (unit != null && (ReferenceEquals(unit, source) ||
                ReferenceEquals(unit, source?.Get<UnitPartSummonedMonster>()?.Summoner)))
                Buff.Remove();
        }
        public void HandlePartyLeaveArea(BlueprintArea currentArea, BlueprintAreaEnterPoint targetArea)
        { Buff.Remove(); }
        public void OnAreaScenesLoaded() { }
        public void OnAreaLoadingComplete() { Check(); }
        public void OnNewRound() { Check(); }
        private void Check()
        {
            if (!SourceIsLive(Buff?.Context?.MaybeCaster)) Buff?.Remove();
        }

        internal static void Retire(UnitEntityData source)
        {
            if (source == null || Game.Instance?.State?.Units == null) return;
            foreach (var unit in Game.Instance.State.Units.All.ToArray())
            foreach (var buff in unit.Buffs.Enumerable.ToArray())
                if (ReferenceEquals(buff.Context?.MaybeCaster, source) &&
                    buff.SelectComponents<InfernalEffectLifetime34>().Any()) buff.Remove();
        }
    }

    // Source-side removal fires before the native summon fade/destruction.
    // The existing OutsiderLifetimeBoundary also retires native commands.
    [Serializable]
    public sealed class InfernalSourceLifetime34 : OwnedGameLogicComponent<UnitDescriptor>, IUnitBuffHandler
    {
        public BlueprintUnit OwningBlueprint;
        public void HandleBuffDidAdded(Buff buff) { }
        public void HandleBuffDidRemoved(Buff buff)
        {
            // The installed BuffCollection broadcasts IUnitBuffHandler globally.
            // Match the removed instance's owner before retiring this source.
            if (Owner?.Unit != null && ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint) &&
                ReferenceEquals(buff?.Owner, Owner) &&
                ReferenceEquals(buff?.Blueprint, BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff))
                InfernalEffectLifetime34.Retire(Owner.Unit);
        }
        public override void OnFactDeactivate()
        {
            if (Owner?.Unit != null && ReferenceEquals(Owner.Unit.Blueprint, OwningBlueprint))
                InfernalEffectLifetime34.Retire(Owner.Unit);
            base.OnFactDeactivate();
        }
    }
}
