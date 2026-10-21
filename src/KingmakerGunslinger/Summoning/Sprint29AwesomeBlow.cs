using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>A private standard action for the two printed owners. Native
    /// CMB/CMD, damage resolution and forced movement remain the consumers.
    /// The feature does not change any campaign actor's Bull Rush or slam.</summary>
    [Serializable]
    public sealed class Sprint29AwesomeBlow : ContextAction
    {
        public BlueprintUnit Cloud;
        public BlueprintUnit Storm;
        public BlueprintFeature Incorporeal;
        public Kingmaker.ElementsSystem.ActionList Knockdown;

        public override string GetCaption()
        { return "Awesome Blow (owned Cloud/Storm giant)"; }

        public override void RunAction()
        {
            UnitEntityData caster = Context == null ? null :
                Context.MaybeCaster;
            UnitEntityData victim = Target == null ? null : Target.Unit;
            if (caster == null || victim == null ||
                (!ReferenceEquals(caster.Blueprint, Cloud) &&
                 !ReferenceEquals(caster.Blueprint, Storm)) ||
                caster.Descriptor == null || victim.Descriptor == null ||
                caster.Destroyed || victim.Destroyed ||
                victim.Descriptor.State.IsDead ||
                !caster.IsEnemy(victim) ||
                (int)victim.Descriptor.State.Size >=
                    (int)caster.Descriptor.State.Size ||
                (Incorporeal != null &&
                    victim.Descriptor.HasFact(Incorporeal)) ||
                victim.Descriptor.State.HasCondition(
                    UnitCondition.ImmuneToCombatManeuvers)) return;

            Vector3 direction = victim.Position - caster.Position;
            direction.y = 0f;
            if (direction.sqrMagnitude < .0001f) return;
            direction.Normalize();
            UnitPartForceMove beforePart = victim.Get<UnitPartForceMove>();
            UnitPartForceMove.Chunk[] before = beforePart == null ?
                Array.Empty<UnitPartForceMove.Chunk>() :
                beforePart.Chunks.ToArray();
            Vector3 start = victim.Position;
            var maneuver = new RuleCombatManeuver(caster, victim,
                CombatManeuver.BullRush) {
                // This is an Awesome Blow check, not a Bull Rush use.
                // Both owners have Improved Bull Rush, whose native +2
                // would otherwise attach to this substitute rule type.
                AdditionalBonus = -2
            };
            Context.TriggerRule(maneuver);
            if (maneuver.AutoFailure || !maneuver.Success) return;

            var impact = new PhysicalDamage(
                new DiceFormula(2, DiceType.D6),
                PhysicalDamageForm.Bludgeoning);
            impact.AddBonus(caster.Stats.Strength.Bonus);
            Context.TriggerRule(new RuleDealDamage(caster, victim, impact));
            UnitPartForceMove force = victim.Get<UnitPartForceMove>();
            UnitPartForceMove.Chunk[] added = force == null ?
                Array.Empty<UnitPartForceMove.Chunk>() :
                force.Chunks.Where(value => !before.Contains(value))
                    .ToArray();
            if (victim.Descriptor.State.IsDead)
            {
                foreach (UnitPartForceMove.Chunk chunk in added)
                    force.StopMove(chunk);
                return;
            }
            if (!victim.Descriptor.State.IsDead)
            {
                float distance = new Feet(10).Meters;
                // Native Bull Rush already queues a push whose distance grows
                // with the check margin. Awesome Blow has a fixed 10-foot
                // distance. Reuse exactly one fresh native chunk and cap it;
                // do not stack a second private push on top of it.
                UnitPartForceMove.Chunk push;
                if (added.Length == 1)
                    push = added[0];
                else
                {
                    foreach (UnitPartForceMove.Chunk chunk in added)
                        force.StopMove(chunk);
                    push = victim.Ensure<UnitPartForceMove>().Push(
                        direction, distance, false);
                }
                push.MaxDistance = distance;
                Sprint29AwesomeBlowCollision.Register(caster, victim,
                    push, start, distance);
                if (Knockdown != null) Knockdown.Run();
            }
        }
    }

    /// <summary>The installed force-move controller clamps at navmesh
    /// obstacles but has no Awesome Blow damage callback. Observe only the
    /// chunk returned by the exact Cloud/Storm action and apply the printed
    /// 1d6 bludgeoning to its victim when movement ends short. Unrelated
    /// pushes and interrupted chunks never enter this ledger.</summary>
    [HarmonyPatch(typeof(UnitForceMoveController), "TickOnUnit")]
    internal static class Sprint29AwesomeBlowCollision
    {
        private sealed class Pending
        {
            internal UnitEntityData Caster;
            internal UnitPartForceMove.Chunk Chunk;
            internal Vector3 Start;
            internal Vector3 Direction;
            internal float Distance;
            internal float NativeSpeed;
        }

        private sealed class Bucket
        {
            internal readonly List<Pending> Entries = new List<Pending>();
        }

        private static readonly ConditionalWeakTable<UnitEntityData, Bucket>
            PendingByVictim = new ConditionalWeakTable<UnitEntityData,
                Bucket>();

        internal static void Register(UnitEntityData caster,
            UnitEntityData victim, UnitPartForceMove.Chunk chunk,
            Vector3 start, float distance)
        {
            if (caster == null || victim == null || chunk == null ||
                distance <= 0f) return;
            Vector3 direction = chunk.Dir.normalized;
            if (direction == Vector3.zero) return;
            PendingByVictim.GetOrCreateValue(victim).Entries.Add(
                new Pending { Caster = caster, Chunk = chunk,
                    Start = start, Direction = direction,
                    Distance = distance, NativeSpeed = chunk.Speed });
        }

        private static void Prefix(UnitEntityData unit)
        {
            Bucket bucket;
            if (unit == null || !PendingByVictim.TryGetValue(unit,
                out bucket)) return;
            UnitPartForceMove part = unit.Get<UnitPartForceMove>();
            float delta = Kingmaker.Game.Instance.TimeController.DeltaTime;
            if (part == null || delta <= 0f) return;
            foreach (Pending pending in bucket.Entries)
            {
                if (!part.Chunks.Contains(pending.Chunk)) continue;
                // Installed TickOnUnit advances at Speed * DeltaTime and
                // checks MaxDistance only after moving. Bound this exact
                // Awesome Blow chunk before the native navmesh trace.
                float traveled = Vector3.Dot(unit.Position -
                    pending.Start, pending.Direction);
                float remaining = Mathf.Max(0f,
                    pending.Distance - traveled);
                pending.Chunk.Speed = Mathf.Min(pending.NativeSpeed,
                    remaining / delta);
                if (remaining <= .001f)
                    pending.Chunk.PassedDistance =
                        pending.Chunk.MaxDistance.Value;
            }
        }

        private static void Postfix(UnitEntityData unit)
        {
            Bucket bucket;
            if (unit == null || !PendingByVictim.TryGetValue(unit,
                out bucket)) return;
            UnitPartForceMove part = unit.Get<UnitPartForceMove>();
            foreach (Pending pending in bucket.Entries.ToArray())
            {
                if (unit.Destroyed)
                {
                    bucket.Entries.Remove(pending);
                    continue;
                }
                if (pending.Chunk.PassedDistance + .001f <
                    pending.Distance)
                {
                    if (part == null ||
                        !part.Chunks.Contains(pending.Chunk))
                        bucket.Entries.Remove(pending);
                    continue;
                }
                bucket.Entries.Remove(pending);
                float traveled = Vector3.Dot(unit.Position -
                    pending.Start, pending.Direction);
                if (traveled + .15f >= pending.Distance ||
                    pending.Caster == null || pending.Caster.Destroyed ||
                    unit.Descriptor == null ||
                    unit.Descriptor.State.IsDead) continue;
                var collision = new PhysicalDamage(
                    new DiceFormula(1, DiceType.D6),
                    PhysicalDamageForm.Bludgeoning);
                Rulebook.Trigger(new RuleDealDamage(pending.Caster,
                    unit, collision));
            }
            if (bucket.Entries.Count == 0)
                PendingByVictim.Remove(unit);
        }
    }
}
