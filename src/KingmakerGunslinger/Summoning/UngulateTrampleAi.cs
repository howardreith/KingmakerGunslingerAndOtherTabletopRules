using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Brain;
using Kingmaker.Controllers.Brain.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using TurnBased.Controllers;
using UnityEngine;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>
    /// S5 T6 / RISK-06: the summoned Mastodon's owned AI action for its
    /// existing Trample. It lives only in the Mastodon's private brain copy,
    /// after the unchanged native attack action. Its command is the ordinary
    /// native cast of the existing Trample at one validated point, so the
    /// action budget, the overrun delivery, the path checker, the contact
    /// rule, the round ledger and ordinary cancellation are all unchanged.
    /// The action holds no state: every evaluation and the command's creation
    /// plan from the live world.
    /// </summary>
    public sealed class UngulateTrampleAi : BlueprintAiCastSpell
    {
        public BlueprintUnit OwningBlueprint;
        public string CreatureKey;

        // The native default returns Self for a point-only ability, which
        // would aim the Trample at the Mastodon itself.
        public override TargetType GetTargetType(DecisionContext context) { return TargetType.Enemy; }

        public override bool IsAvailable(DecisionContext context)
        {
            UnitEntityData owner = context == null ? null : context.Unit;
            // An unavailable ability is refused here, before the native
            // selector's expended check: that check would otherwise remove
            // this action from the unit's available actions, which the game
            // restores only at creation or rest.
            return UngulateTrampleAiRuntime.ActorRefusal(owner, OwningBlueprint) ==
                MastodonTrampleAiRefusal.None && context.Ability != null &&
                context.Ability.IsAvailable && base.IsAvailable(context);
        }

        public override bool IsTargetValid(DecisionContext context)
        {
            if (context == null || context.Target == null) return false;
            UngulateTramplePlan plan;
            return UngulateTrampleAiRuntime.Plan(context.Unit, context.Target.Unit, context.Ability,
                OwningBlueprint, CreatureKey, out plan) == MastodonTrampleAiRefusal.None;
        }

        public override UnitCommand CreateCommand(DecisionContext context, UnitEntityData target)
        {
            // The plan is recomputed when the command is created. A point that
            // is no longer legal is never cast: the ordinary attack is issued
            // instead, as the native attack action would.
            UngulateTramplePlan plan;
            AbilityData ability = context.Ability;
            if (UngulateTrampleAiRuntime.ActorRefusal(context.Unit, OwningBlueprint) ==
                    MastodonTrampleAiRefusal.None &&
                UngulateTrampleAiRuntime.Plan(context.Unit, target, ability, OwningBlueprint, CreatureKey,
                    out plan) == MastodonTrampleAiRefusal.None)
                return UnitUseAbility.CreateCastCommand(ability, new TargetWrapper(plan.End));
            return UnitAttack.CreateAttackCommand(context.Unit, target);
        }
    }

    internal struct UngulateTramplePlan
    {
        internal Vector3 Start;
        internal Vector3 End;
        internal float Length;
        internal float MaxLength;
    }

    /// <summary>The native reads behind <see cref="MastodonTrampleAiPolicy"/>. Read-only.</summary>
    internal static class UngulateTrampleAiRuntime
    {
        internal static MastodonTrampleAiRefusal ActorRefusal(UnitEntityData owner, BlueprintUnit owning)
        {
            if (owner == null || owning == null || !ReferenceEquals(owner.Blueprint, owning) ||
                owner.Destroyed || !owner.IsInGame || owner.View == null || owner.Descriptor == null ||
                owner.Descriptor.State == null || owner.CombatState == null)
                return MastodonTrampleAiRefusal.NotOwner;
            var state = owner.Descriptor.State;
            bool canActAndMove = !state.IsDead && state.IsConscious && state.CanAct && state.CanMove;
            return MastodonTrampleAiPolicy.Actor(owner.IsInCombat, canActAndMove, owner.AiMovementForbidden,
                owner.CombatState.IsEngaged, CombatController.IsInTurnBasedCombat(), owner.HasFullRoundAction(),
                AnyMeleeTargetInReach(owner));
        }

        /// <summary>
        /// The native reach test (bodies, weapon reach and line of sight) for
        /// every melee weapon the Mastodon holds, against every living enemy.
        /// </summary>
        internal static bool AnyMeleeTargetInReach(UnitEntityData owner)
        {
            WeaponSlot[] slots = MeleeSlots(owner);
            if (slots.Length == 0) return false;
            foreach (UnitEntityData unit in Game.Instance.State.Units)
                if (IsLivingEnemy(owner, unit) && InReach(owner, unit, slots)) return true;
            return false;
        }

        internal static WeaponSlot[] MeleeSlots(UnitEntityData owner)
        {
            if (owner == null || owner.Body == null) return new WeaponSlot[0];
            return new[] { owner.Body.PrimaryHand, owner.Body.SecondaryHand }
                .Concat(owner.Body.AdditionalLimbs ?? Enumerable.Empty<WeaponSlot>())
                .Where(slot => slot != null && slot.HasWeapon && slot.Weapon != null &&
                    slot.Weapon.Blueprint != null && slot.Weapon.Blueprint.IsMelee).ToArray();
        }

        internal static bool InReach(UnitEntityData owner, UnitEntityData unit, WeaponSlot[] slots)
        {
            foreach (WeaponSlot slot in slots)
            {
                try { if (UnitEngagementExtension.IsReach(owner, unit, slot)) return true; }
                catch { return true; } // unknown is treated as in reach: the attack keeps precedence
            }
            return false;
        }

        private static bool IsLivingEnemy(UnitEntityData owner, UnitEntityData unit)
        {
            return unit != null && !ReferenceEquals(unit, owner) && !unit.Destroyed && unit.IsInGame &&
                unit.View != null && unit.Descriptor != null && unit.Descriptor.State != null &&
                !unit.Descriptor.State.IsDead && owner.IsEnemy(unit);
        }

        internal static MastodonTrampleAiRefusal Plan(UnitEntityData owner, UnitEntityData target,
            AbilityData ability, BlueprintUnit owning, string creatureKey, out UngulateTramplePlan plan)
        {
            plan = default(UngulateTramplePlan);
            if (owner == null || owning == null || !ReferenceEquals(owner.Blueprint, owning) ||
                owner.View == null || owner.Descriptor == null || owner.CombatState == null)
                return MastodonTrampleAiRefusal.NotOwner;
            if (!IsLivingEnemy(owner, target) || string.IsNullOrEmpty(target.UniqueId) ||
                owner.CombatState.AIData.UnreachableUnits.Contains(target))
                return MastodonTrampleAiRefusal.TargetInvalid;

            UngulateRulesProfile rules = UngulateRulesPolicy.For(creatureKey);
            int stampede = rules.Stampede ? UngulateStampedeRuntime.ActiveGroupSize(owner) : 0;
            bool smaller = rules.CanTrample((int)owner.Descriptor.State.Size,
                (int)target.Descriptor.State.Size, stampede);
            bool claimed, halted;
            ContextActionUngulateTrample.ReadLedger(owner, target.UniqueId, out claimed, out halted);
            MastodonTrampleAiRefusal refusal = MastodonTrampleAiPolicy.Target(
                InReach(owner, target, MeleeSlots(owner)), smaller, claimed, halted);
            if (refusal != MastodonTrampleAiRefusal.None) return refusal;
            if (ability == null) return MastodonTrampleAiRefusal.AbilityMissing;

            Vector3 start = owner.Position, centre = target.Position;
            float casterRadius = owner.View.Corpulence, targetRadius = target.View.Corpulence;
            float endX, endZ;
            MastodonTrampleAiPolicy.DesiredEndpoint(start.x, start.z, centre.x, centre.z, targetRadius,
                casterRadius, out endX, out endZ);
            Pathfinding.NNInfo node = Kingmaker.View.ObstacleAnalyzer.GetNearestNode(
                new Vector3(endX, centre.y, endZ));
            if (node.node == null || !node.node.Walkable) return MastodonTrampleAiRefusal.NoWalkableEndpoint;
            Vector3 end = node.clampedPosition;
            // No approach before the delivery starts: within the ability's
            // own double-move range, and within the path checker's limit.
            float maxLength = Math.Min(owner.CombatSpeedMps * 6f, owner.CombatSpeedMps * 12f);
            refusal = MastodonTrampleAiPolicy.Segment(start.x, start.z, end.x, end.z, centre.x, centre.z,
                targetRadius, casterRadius, casterRadius, maxLength);
            if (refusal != MastodonTrampleAiRefusal.None) return refusal;
            if (Vector3.Distance(Kingmaker.View.ObstacleAnalyzer.TraceAlongNavmesh(start, end), end) > 0.05f)
                return MastodonTrampleAiRefusal.PathBlocked;
            var sight = Kingmaker.Visual.FogOfWar.LineOfSightGeometry.Instance;
            if (sight == null || sight.HasObstacle(owner.EyePosition, end, 0))
                return MastodonTrampleAiRefusal.NoLineOfSight;
            foreach (UnitEntityData unit in Game.Instance.State.Units)
            {
                if (unit == null || ReferenceEquals(unit, owner) || unit.Destroyed || !unit.IsInGame ||
                    unit.View == null || unit.Descriptor == null || unit.Descriptor.State == null ||
                    unit.Descriptor.State.IsDead) continue;
                Vector2 gap = new Vector2(unit.Position.x - end.x, unit.Position.z - end.z);
                if (gap.magnitude < casterRadius + unit.View.Corpulence)
                    return MastodonTrampleAiRefusal.EndpointOccupied;
            }
            var point = new TargetWrapper(end);
            if (!ability.CanTarget(point)) return MastodonTrampleAiRefusal.NativeTargetRefused;
            if (!ability.IsAvailable) return MastodonTrampleAiRefusal.NativeUnavailable;
            plan = new UngulateTramplePlan { Start = start, End = end,
                Length = new Vector2(end.x - start.x, end.z - start.z).magnitude, MaxLength = maxLength };
            return MastodonTrampleAiRefusal.None;
        }
    }
}
