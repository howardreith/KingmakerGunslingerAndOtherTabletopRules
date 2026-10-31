using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;
using Pathfinding;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    internal sealed class InfernalWallPlacement35
    {
        internal Vector3 Center;
        internal Vector3 Forward;
        internal Vector3 Along;
        internal GridGraph Grid;
        internal int GridIndex;
        internal bool LocalBypassOpen;
    }

    /// <summary>
    /// Wall-only legal-placement and AI geometry. It reads the installed
    /// GridGraph but never mutates it; the view adapter remains the only
    /// component allowed to claim or restore nodes.
    /// </summary>
    internal static class InfernalWallPlacementRuntime35
    {
        private const float ProjectionToleranceMeters = 1f;

        internal static bool TryResolve(UnitEntityData caster,
            TargetWrapper target, BlueprintUnit wallSection,
            out InfernalWallPlacement35 placement)
        {
            placement = null;
            if (caster == null || target == null || target.Unit != null ||
                wallSection == null || caster.Destroyed || !caster.IsInState ||
                caster.Descriptor.State.IsDead || AstarPath.active == null ||
                AstarPath.active.graphs == null) return false;
            Vector3 requested = target.Point;
            if (!Finite(requested)) return false;
            NNInfo projected = ObstacleAnalyzer.GetNearestNode(requested);
            if (projected.node == null || !projected.node.Walkable ||
                Vector3.Distance(requested, projected.clampedPosition) >
                    ProjectionToleranceMeters) return false;
            Vector3 center = projected.clampedPosition;
            Vector3 forward = center - caster.Position;
            forward.y = 0f;
            if (forward.sqrMagnitude < .0625f)
            {
                forward = caster.OrientationDirection;
                forward.y = 0f;
            }
            if (forward.sqrMagnitude < .0625f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 along = Vector3.Cross(Vector3.up, forward).normalized;

            GridGraph grid = null;
            int gridIndex = -1;
            float nearestDistance = float.PositiveInfinity;
            for (int index = 0; index < AstarPath.active.graphs.Length;
                index++)
            {
                var candidate = AstarPath.active.graphs[index] as GridGraph;
                if (candidate == null || candidate.nodes == null) continue;
                NNInfo nearest = candidate.GetNearest(center,
                    NNConstraint.None);
                if (nearest.node == null) continue;
                float distance = Vector3.Distance(center,
                    nearest.clampedPosition);
                if (distance >= nearestDistance) continue;
                grid = candidate;
                gridIndex = index;
                nearestDistance = distance;
            }
            if (grid == null || nearestDistance >
                    Math.Max(1.5f, grid.nodeSize * 2f)) return false;

            float halfLength = InfernalRules35
                .WallOfIceSectionLengthMeters * .5f;
            float halfThickness = InfernalRules35
                .WallOfIceSectionThicknessMeters * .5f;
            var bounds = new Bounds(center, new Vector3(
                InfernalRules35.WallOfIceSectionLengthMeters +
                    grid.nodeSize * 2f,
                4f,
                InfernalRules35.WallOfIceSectionLengthMeters +
                    grid.nodeSize * 2f));
            List<GraphNode> nodes = null;
            int footprintNodes = 0;
            try
            {
                nodes = grid.GetNodesInArea(bounds);
                if (nodes == null) return false;
                foreach (GraphNode node in nodes)
                {
                    if (node == null || node.GraphIndex != (uint)gridIndex)
                        continue;
                    Vector3 delta = (Vector3)node.position - center;
                    if (!InfernalRules35.WallNodeIntersects(
                            Vector3.Dot(delta, along),
                            Vector3.Dot(delta, forward), halfLength,
                            halfThickness, grid.nodeSize)) continue;
                    footprintNodes++;
                    if (!node.Walkable) return false;
                }
            }
            finally
            {
                if (nodes != null)
                    Pathfinding.Util.ListPool<GraphNode>.Release(nodes);
            }
            if (footprintNodes == 0) return false;

            if (Game.Instance?.State?.Units?.All == null) return false;
            foreach (UnitEntityData unit in Game.Instance.State.Units.All)
            {
                if (unit == null || unit.Destroyed || !unit.IsInState ||
                    unit.Descriptor.State.IsDead) continue;
                float distance = DistanceToFootprint(unit.Position, center,
                    along, forward);
                if (InfernalRules35.WallPlacementOccupied(distance,
                        Math.Max(.05f, unit.Corpulence))) return false;
            }

            placement = new InfernalWallPlacement35 {
                Center = center,
                Forward = forward,
                Along = along,
                Grid = grid,
                GridIndex = gridIndex,
                LocalBypassOpen = HasLocalBypass(grid, center, along,
                    halfLength)
            };
            return true;
        }

        internal static bool TryPlanAi(UnitEntityData caster,
            UnitEntityData enemy, BlueprintUnit wallSection,
            out InfernalWallPlacement35 placement)
        {
            placement = null;
            if (caster == null || enemy == null ||
                ReferenceEquals(caster, enemy) || !caster.IsEnemy(enemy) ||
                enemy.Destroyed || !enemy.IsInState ||
                enemy.Descriptor.State.IsDead ||
                !caster.Memory.ContainsVisible(enemy) || !caster.HasLOS(enemy))
                return false;
            Vector3 direction = enemy.Position - caster.Position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 2.25f) return false;
            direction.Normalize();
            float clearance = Math.Max(.05f, enemy.Corpulence) +
                InfernalRules35.WallOfIceSectionThicknessMeters * .5f + .6f;
            Vector3 requested = enemy.Position - direction * clearance;
            if (!TryResolve(caster, new TargetWrapper(requested), wallSection,
                    out placement) || !placement.LocalBypassOpen) return false;
            float actorSide = Vector3.Dot(caster.Position - placement.Center,
                placement.Forward);
            float enemySide = Vector3.Dot(enemy.Position - placement.Center,
                placement.Forward);
            if (actorSide * enemySide >= 0f) return false;

            // This is an AI redundancy filter, not a player-visible one-wall
            // rule. Independent or meaningfully separated sections remain legal.
            Vector3 plannedCenter = placement.Center;
            if (Game.Instance?.State?.Units?.All.Any(unit =>
                unit != null && !unit.Destroyed && unit.IsInState &&
                ReferenceEquals(unit.Blueprint, wallSection) &&
                ReferenceEquals(unit.Get<UnitPartSummonedMonster>()?.Summoner,
                    caster) && Vector3.Distance(unit.Position,
                        plannedCenter) <=
                    InfernalRules35.WallOfIceSectionLengthMeters * 2f) == true)
                return false;
            return true;
        }

        internal static IEnumerable<UnitEntityData> AdjacentUnits(
            InfernalWallPlacement35 placement)
        {
            if (placement == null || Game.Instance?.State?.Units?.All == null)
                return Enumerable.Empty<UnitEntityData>();
            return Game.Instance.State.Units.All.Where(unit =>
                unit != null && !unit.Destroyed && unit.IsInState &&
                !unit.Descriptor.State.IsDead &&
                InfernalRules35.WallFormationAdjacent(
                    DistanceToFootprint(unit.Position, placement.Center,
                        placement.Along, placement.Forward),
                    Math.Max(.05f, unit.Corpulence))).ToArray();
        }

        internal static float DistanceToFootprint(Vector3 point,
            Vector3 center, Vector3 along, Vector3 forward)
        {
            Vector3 delta = point - center;
            delta.y = 0f;
            return InfernalRules35.WallDistanceToFootprint(
                Vector3.Dot(delta, along), Vector3.Dot(delta, forward),
                InfernalRules35.WallOfIceSectionLengthMeters * .5f,
                InfernalRules35.WallOfIceSectionThicknessMeters * .5f);
        }

        private static bool HasLocalBypass(GridGraph grid, Vector3 center,
            Vector3 along, float halfLength)
        {
            float clearance = Math.Max(1f, grid.nodeSize * 2f);
            return Open(grid, center + along * (halfLength + clearance)) &&
                Open(grid, center - along * (halfLength + clearance));
        }

        private static bool Open(GridGraph grid, Vector3 point)
        {
            NNInfo nearest = grid.GetNearest(point, NNConstraint.None);
            return nearest.node != null && nearest.node.Walkable &&
                Vector3.Distance(nearest.clampedPosition, point) <=
                    Math.Max(1.5f, grid.nodeSize * 2f);
        }

        private static bool Finite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsNaN(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.x) &&
                !float.IsInfinity(value.y) && !float.IsInfinity(value.z);
        }
    }

    [Serializable]
    public sealed class InfernalWallTarget35 : BlueprintComponent,
        IAbilityTargetChecker
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintUnit WallSection;

        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            InfernalWallPlacement35 placement;
            return caster?.Blueprint != null && OwningBlueprint != null &&
                ReferenceEquals(caster.Blueprint, OwningBlueprint) &&
                InfernalWallPlacementRuntime35.TryResolve(caster, target,
                    WallSection, out placement);
        }
    }

    /// <summary>
    /// Formation owns occupied-space revalidation, per-adjacent-creature SR
    /// and Reflex disruption, then delegates lifetime/linking to native
    /// RuleSummonUnit. It never publishes a temporary route or bypasses action
    /// economy.
    /// </summary>
    [Serializable]
    public sealed class ContextActionFormInfernalWall35 : ContextAction
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintUnit WallSection;

        public override string GetCaption()
        { return "Form one grounded Wall of Ice section"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context?.MaybeCaster;
            InfernalWallPlacement35 placement;
            if (caster?.Blueprint == null || OwningBlueprint == null ||
                !ReferenceEquals(caster.Blueprint, OwningBlueprint) ||
                !InfernalWallPlacementRuntime35.TryResolve(caster, Target,
                    WallSection, out placement)) return;
            int dc = InfernalRules35.WallOfIceFormationDc(
                caster.Stats.Charisma.ModifiedValue);
            Context.Params.CasterLevel = InfernalRules35.SpellLikeCasterLevel;
            Context.Params.SpellLevel = InfernalRules35.WallOfIceSpellLevel;
            Context.Params.DC = dc;
            foreach (UnitEntityData unit in
                InfernalWallPlacementRuntime35.AdjacentUnits(placement))
            {
                var resistance = Context.TriggerRule(
                    new RuleSpellResistanceCheck(Context, unit));
                if (resistance.TargetIsImmune || resistance.IsSpellResisted)
                    return;
                var save = new RuleSavingThrow(unit, SavingThrowType.Reflex,
                    dc) { Reason = Context };
                Context.TriggerRule(save);
                if (save.IsPassed) return;
            }

            var summon = new RuleSummonUnit(caster, WallSection,
                placement.Center,
                InfernalRules35.WallOfIceDurationRounds.Rounds(), 0) {
                    Context = Context,
                    Reason = Context,
                    DoNotLinkToCaster = false,
                    IsDirectlyControllable = false
                };
            Context.TriggerRule(summon);
            UnitEntityData section = summon.SummonedUnit;
            if (section == null) return;
            float orientation = Quaternion.LookRotation(placement.Forward,
                Vector3.up).eulerAngles.y;
            section.Orientation = orientation;
            section.DesiredOrientation = orientation;
            var binding = section.View == null ? null : section.View
                .GetComponent<InfernalWallNavmesh35>();
            if (binding != null) binding.Reorient(orientation);
        }
    }

    /// <summary>
    /// The section is an attackable native unit solely to obtain engine HP,
    /// damage, saves and serialization. This component supplies the printed
    /// object rules and the breach transition without supplying an attack.
    /// </summary>
    [Serializable]
    public sealed class InfernalWallSection35 :
        OwnedGameLogicComponent<UnitDescriptor>,
        ITargetRulebookHandler<RuleAttackRoll>,
        ITargetRulebookHandler<RuleDealDamage>, IUnitHandler,
        IUnitBuffHandler, IPartyLeaveAreaHandler,
        IAreaLoadingStagesHandler, ITickEachRound, IGlobalSubscriber
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintUnit SourceBlueprint;
        public BlueprintAbility WallAbility;
        public BlueprintAbilityAreaEffect ResidualArea;
        public BlueprintAbilityAreaEffect FogArea;

        [JsonProperty] private bool _breached;
        [JsonIgnore] private bool _retiring;
        [JsonIgnore] private RuleDealDamage _currentDamage;
        [JsonIgnore] private bool _currentDamageHasFire;

        public void OnEventAboutToTrigger(RuleAttackRoll evt)
        {
            if (!ExactOwner(evt?.Target)) return;
            evt.AutoMiss = false;
            evt.AutoHit = true;
        }

        public void OnEventDidTrigger(RuleAttackRoll evt) { }

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (!ExactOwner(evt?.Target) || evt.DamageBundle == null) return;
            _currentDamage = evt;
            _currentDamageHasFire = false;
            foreach (BaseDamage packet in evt.DamageBundle)
            {
                if (packet == null) continue;
                var energy = packet as EnergyDamage;
                bool fire = energy != null &&
                    energy.EnergyType == DamageEnergyType.Fire;
                if (fire) _currentDamageHasFire = true;
                else if (InfernalRules35.WallDamageUsesObjectHalf(false))
                    packet.Half = true;
            }
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
            if (!ReferenceEquals(evt, _currentDamage)) return;
            UnitEntityData unit = Owner?.Unit;
            if (ExactOwner(unit) && (unit.Descriptor.State.IsDead ||
                unit.Damage >= unit.Stats.HitPoints.ModifiedValue))
                Breach(unit, _currentDamageHasFire, null);
            _currentDamage = null;
            _currentDamageHasFire = false;
        }

        public void HandleUnitSpawned(UnitEntityData unit) { }
        public void HandleUnitDestroyed(UnitEntityData unit)
        { HandleUnitDeath(unit); }

        public void HandleUnitDeath(UnitEntityData unit)
        {
            Breach(unit, _currentDamageHasFire, null);
        }

        private void Breach(UnitEntityData unit, bool fire, Buff lifetime)
        {
            if (!ExactOwner(unit) || _retiring || _breached) return;
            _breached = true;
            if (lifetime == null)
                lifetime = Owner.Buffs.GetBuff(
                    BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            MechanicsContext context = lifetime?.Context;
            TimeSpan remaining = lifetime == null ? TimeSpan.Zero :
                lifetime.TimeLeft;
            ReleaseNavigation();
            if (context?.MaybeCaster == null ||
                !ReferenceEquals(context.SourceAbility, WallAbility) ||
                remaining <= TimeSpan.Zero ||
                !InfernalEffectLifetime34.SourceIsLive(context.MaybeCaster))
                return;
            if (ResidualArea != null)
                AreaEffectsController.Spawn(context, ResidualArea,
                    new TargetWrapper(unit.Position), remaining);
            if (fire && FogArea != null)
                AreaEffectsController.Spawn(context, FogArea,
                    new TargetWrapper(unit.Position), TimeSpan.FromMinutes(10));
        }

        public void HandleBuffDidAdded(Buff buff) { }
        public void HandleBuffDidRemoved(Buff buff)
        {
            if (!ExactOwner(Owner?.Unit) || buff == null ||
                !ReferenceEquals(buff.Owner, Owner) ||
                !ReferenceEquals(buff.Blueprint,
                    BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff))
                return;
            if (Owner.Unit.Descriptor.State.IsDead || Owner.Unit.Damage >=
                    Owner.Unit.Stats.HitPoints.ModifiedValue)
            {
                Breach(Owner.Unit, _currentDamageHasFire, buff);
                return;
            }
            _retiring = true;
            ReleaseNavigation();
        }

        public void HandlePartyLeaveArea(BlueprintArea currentArea,
            BlueprintAreaEnterPoint targetArea)
        { RetireWithoutBreach(); }
        public void OnAreaScenesLoaded() { }
        public void OnAreaLoadingComplete() { CheckLifetime(); }
        public void OnNewRound() { CheckLifetime(); }

        public override void OnFactDeactivate()
        {
            UnitEntityData unit = Owner?.Unit;
            if (ExactOwner(unit) && (unit.Descriptor.State.IsDead ||
                    unit.Damage >= unit.Stats.HitPoints.ModifiedValue))
                Breach(unit, _currentDamageHasFire, null);
            else RetireWithoutBreach();
            base.OnFactDeactivate();
        }

        internal void RetireWithoutBreach()
        {
            if (_retiring) return;
            _retiring = true;
            ReleaseNavigation();
            Buff lifetime = Owner?.Buffs?.GetBuff(
                BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            if (lifetime != null && !lifetime.IsDisposed) lifetime.Remove();
        }

        internal static void Retire(UnitEntityData section)
        {
            InfernalWallSection35 component = Find(section);
            if (component != null) { component.RetireWithoutBreach(); return; }
            Buff lifetime = section?.Buffs?.GetBuff(
                BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff);
            if (lifetime != null && !lifetime.IsDisposed) lifetime.Remove();
        }

        private void CheckLifetime()
        {
            UnitEntityData unit = Owner?.Unit;
            if (!ExactOwner(unit)) return;
            UnitEntityData source = unit.Get<UnitPartSummonedMonster>()?
                .Summoner;
            ModContext mod;
            if (!ModContext.TryGet(out mod) ||
                mod.FeatureModules?.Active?.ExpandedSummoning != true ||
                source?.Blueprint == null || SourceBlueprint == null ||
                !ReferenceEquals(source.Blueprint, SourceBlueprint) ||
                !InfernalEffectLifetime34.SourceIsLive(source))
                RetireWithoutBreach();
        }

        private bool ExactOwner(UnitEntityData unit)
        {
            return unit != null && ReferenceEquals(unit, Owner?.Unit) &&
                OwningBlueprint != null &&
                ReferenceEquals(unit.Blueprint, OwningBlueprint);
        }

        private void ReleaseNavigation()
        {
            var binding = Owner?.Unit?.View == null ? null :
                Owner.Unit.View.GetComponent<InfernalWallNavmesh35>();
            if (binding != null) binding.Release();
        }

        private static InfernalWallSection35 Find(UnitEntityData unit)
        {
            return unit?.Descriptor?.Progression?.Features?.Enumerable
                .SelectMany(feature => feature.SelectComponents<
                    InfernalWallSection35>())
                .SingleOrDefault(component => component.Fact?.Active == true &&
                    ReferenceEquals(component.Owner?.Unit, unit));
        }
    }

    /// <summary>The breach damages each actual entry, with no save. Native
    /// AreaEffectEntityData owns per-creature spell resistance admission.</summary>
    [Serializable]
    public sealed class InfernalWallResidual35 : AbilityAreaEffectLogic
    {
        public BlueprintUnit SourceBlueprint;

        protected override void OnUnitEnter(MechanicsContext context,
            AreaEffectEntityData area, UnitEntityData unit)
        {
            UnitEntityData source = context?.MaybeCaster;
            if (source?.Blueprint == null || SourceBlueprint == null ||
                !ReferenceEquals(source.Blueprint, SourceBlueprint) ||
                unit == null || unit.Destroyed || !unit.IsInState ||
                unit.Descriptor.State.IsDead || area == null || area.IsEnded ||
                !InfernalEffectLifetime34.SourceIsLive(source)) return;
            var cold = new EnergyDamage(new DiceFormula(
                InfernalRules35.WallOfIceResidualDiceCount, DiceType.D6),
                DamageEnergyType.Cold);
            cold.AddBonus(InfernalRules35.WallOfIceResidualBonus);
            var rule = new RuleDealDamage(source, unit,
                new DamageBundle(cold)) {
                    Reason = context,
                    SourceAbility = context.SourceAbility
                };
            context.TriggerRule(rule);
        }

        protected override void OnUnitMove(MechanicsContext context,
            AreaEffectEntityData area, UnitEntityData unit) { }
        protected override void OnUnitExit(MechanicsContext context,
            AreaEffectEntityData area, UnitEntityData unit) { }
        protected override void OnTick(MechanicsContext context,
            AreaEffectEntityData area)
        {
            if (area != null && !area.IsEnded &&
                !InfernalEffectLifetime34.SourceIsLive(context?.MaybeCaster))
                area.ForceEnd();
        }
    }

    /// <summary>Ends only this Bone Devil's still-active sections and Wall
    /// areas. It never erases already dealt hit-point or ability damage.</summary>
    [Serializable]
    public sealed class InfernalWallSourceLifetime35 :
        OwnedGameLogicComponent<UnitDescriptor>, IUnitBuffHandler,
        IUnitHandler, IPartyLeaveAreaHandler, IAreaLoadingStagesHandler,
        ITickEachRound, IGlobalSubscriber
    {
        public BlueprintUnit OwningBlueprint;
        public BlueprintUnit WallSection;
        public BlueprintAbility WallAbility;
        public BlueprintAbilityAreaEffect ResidualArea;
        public BlueprintAbilityAreaEffect FogArea;
        [JsonIgnore] private bool _retiring;

        public void HandleBuffDidAdded(Buff buff) { }
        public void HandleBuffDidRemoved(Buff buff)
        {
            if (!ExactSource(Owner?.Unit) || buff == null ||
                !ReferenceEquals(buff.Owner, Owner) ||
                !ReferenceEquals(buff.Blueprint,
                    BlueprintRoot.Instance.SystemMechanics.SummonedUnitBuff))
                return;
            RetireOwned();
        }

        public void HandleUnitSpawned(UnitEntityData unit) { }
        public void HandleUnitDestroyed(UnitEntityData unit)
        { HandleUnitDeath(unit); }
        public void HandleUnitDeath(UnitEntityData unit)
        {
            UnitEntityData source = Owner?.Unit;
            if (source != null && (ReferenceEquals(unit, source) ||
                ReferenceEquals(unit, source.Get<UnitPartSummonedMonster>()?
                    .Summoner))) RetireOwned();
        }

        public void HandlePartyLeaveArea(BlueprintArea currentArea,
            BlueprintAreaEnterPoint targetArea)
        { RetireOwned(); }
        public void OnAreaScenesLoaded() { }
        public void OnAreaLoadingComplete() { Check(); }
        public void OnNewRound() { Check(); }

        public override void OnFactDeactivate()
        {
            RetireOwned();
            base.OnFactDeactivate();
        }

        private void Check()
        {
            if (ExactSource(Owner?.Unit) &&
                !InfernalEffectLifetime34.SourceIsLive(Owner.Unit))
                RetireOwned();
        }

        private bool ExactSource(UnitEntityData source)
        {
            return source != null && OwningBlueprint != null &&
                ReferenceEquals(source.Blueprint, OwningBlueprint);
        }

        private void RetireOwned()
        {
            UnitEntityData source = Owner?.Unit;
            if (_retiring || !ExactSource(source) ||
                Game.Instance?.State == null) return;
            _retiring = true;
            try
            {
                if (Game.Instance.State.Units?.All != null)
                    foreach (UnitEntityData section in Game.Instance.State
                        .Units.All.Where(unit => unit != null &&
                            !unit.Destroyed && unit.IsInState &&
                            ReferenceEquals(unit.Blueprint, WallSection) &&
                            ReferenceEquals(unit.Get<
                                UnitPartSummonedMonster>()?.Summoner,
                                source)).ToArray())
                        InfernalWallSection35.Retire(section);
                if (Game.Instance.State.AreaEffects?.All != null)
                    foreach (AreaEffectEntityData area in Game.Instance.State
                        .AreaEffects.All.Where(area => area != null &&
                            !area.IsEnded &&
                            (ReferenceEquals(area.Blueprint, ResidualArea) ||
                             ReferenceEquals(area.Blueprint, FogArea)) &&
                            ReferenceEquals(area.Context?.MaybeCaster,
                                source) &&
                            ReferenceEquals(area.Context?.SourceAbility,
                                WallAbility)).ToArray())
                        area.ForceEnd();
            }
            finally { _retiring = false; }
        }
    }
}
