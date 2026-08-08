using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using KingmakerGunslinger.Diagnostics;
using KingmakerGunslinger.Firearms;
using KingmakerGunslinger.Rules;
using Kingmaker.Utility;

namespace KingmakerGunslinger.Archetypes
{
    public sealed class TwinShotHitTracker :
        RuleInitiatorLogicComponent<RuleAttackWithWeapon>, ITickEachRound
    {
        public override void OnEventAboutToTrigger(RuleAttackWithWeapon evt) { }

        public override void OnEventDidTrigger(RuleAttackWithWeapon evt)
        {
            if (evt == null || evt.AttackRoll == null || evt.Target == null ||
                Owner == null) return;
            FirearmMarkerSnapshot marker =
                FirearmMarkerLookup.ReadFromRuleEvent(evt.AttackRoll);
            if (TwinShotKnockdownPolicy.IsQualifyingHit(
                    TwinShotKnockdownRuntime.IsOwnersTurn(evt.Initiator),
                    marker.IsExactFirearm, marker.MarkerCount,
                    marker.Definition == null ? FirearmKind.Unknown :
                        marker.Definition.Kind, evt.AttackRoll.IsHit))
                TwinShotKnockdownRuntime.Record(Owner.Unit, evt.Target, evt);
        }

        public void OnNewRound()
        { if (Owner != null) TwinShotKnockdownRuntime.Clear(Owner.Unit); }
    }

    [Serializable]
    public sealed class TwinShotKnockdownAbilityLogic : AbilityCustomLogic
    {
        public BlueprintAbilityResource Grit;

        public override IEnumerator<AbilityDeliveryTarget> Deliver(
            AbilityExecutionContext context, TargetWrapper target)
        {
            if (context == null || context.Caster == null || target == null ||
                target.Unit == null || Grit == null)
                throw new InvalidOperationException(
                    "Twin Shot Knockdown requires an exact caster and target.");
            TwinShotKnockdownRuntime.Execute(context.Caster, target.Unit, Grit);
            yield return new AbilityDeliveryTarget(target);
        }

        public override void Cleanup(AbilityExecutionContext context) { }
    }

    internal static class TwinShotKnockdownRuntime
    {
        private static readonly ConditionalWeakTable<UnitEntityData, OwnerState>
            States = new ConditionalWeakTable<UnitEntityData, OwnerState>();

        internal static bool IsOwnersTurn(UnitEntityData owner)
        {
            try
            {
                if (!TurnBased.Controllers.CombatController.IsInTurnBasedCombat())
                    return true;
                var controller = Game.Instance == null ? null :
                    Game.Instance.TurnBasedCombatController;
                return controller != null && controller.CurrentTurn != null &&
                    ReferenceEquals(controller.CurrentTurn.Unit, owner);
            }
            catch { return false; }
        }

        internal static void Record(UnitEntityData owner, UnitEntityData target,
            RuleAttackWithWeapon attack)
        {
            if (owner == null || target == null || attack == null) return;
            lock (States)
            {
                OwnerState state = States.GetOrCreateValue(owner);
                state.Record(target, attack);
            }
        }

        internal static void Clear(UnitEntityData owner)
        {
            if (owner == null) return;
            lock (States) { States.Remove(owner); }
        }

        internal static int HitCount(UnitEntityData owner, UnitEntityData target)
        {
            if (owner == null || target == null) return 0;
            lock (States)
            {
                OwnerState state;
                return States.TryGetValue(owner, out state) ?
                    state.Count(target) : 0;
            }
        }

        internal static void Execute(UnitEntityData owner, UnitEntityData target,
            BlueprintAbilityResource grit)
        {
            bool immune = target.Descriptor.State.HasCondition(
                UnitCondition.ImmuneToCombatManeuvers);
            bool prone = target.Descriptor.State.HasCondition(UnitCondition.Prone);
            int current = owner.Descriptor.Resources.GetResourceAmount(grit);
            lock (States)
            {
                OwnerState state;
                int hits = States.TryGetValue(owner, out state) ?
                    state.Count(target) : 0;
                bool used = state != null && state.WasUsed(target);
                if (!IsOwnersTurn(owner) || !TwinShotKnockdownPolicy.CanExecute(
                        hits, used, prone, immune, current))
                    throw new InvalidOperationException(
                        "Twin Shot Knockdown target is not currently eligible.");
                owner.Descriptor.Resources.Spend(grit,
                    TwinShotKnockdownPolicy.OrdinaryGritCost);
                try
                {
                    target.Descriptor.State.AddCondition(UnitCondition.Prone,
                        null);
                    if (!target.Descriptor.State.HasCondition(UnitCondition.Prone))
                        throw new InvalidOperationException(
                            "The native Prone condition was rejected.");
                    state.MarkUsed(target);
                }
                catch
                {
                    owner.Descriptor.Resources.Restore(grit,
                        TwinShotKnockdownPolicy.OrdinaryGritCost);
                    throw;
                }
            }
        }

        private sealed class OwnerState
        {
            private readonly Dictionary<UnitEntityData, TargetState> _targets =
                new Dictionary<UnitEntityData, TargetState>(
                    ReferenceIdentityComparer<UnitEntityData>.Instance);

            internal void Record(UnitEntityData target, RuleAttackWithWeapon attack)
            {
                TargetState state;
                if (!_targets.TryGetValue(target, out state))
                    _targets.Add(target, state = new TargetState());
                state.Attacks.Add(attack);
            }
            internal int Count(UnitEntityData target)
            { TargetState state; return _targets.TryGetValue(target, out state) ? state.Attacks.Count : 0; }
            internal bool WasUsed(UnitEntityData target)
            { TargetState state; return _targets.TryGetValue(target, out state) && state.Used; }
            internal void MarkUsed(UnitEntityData target)
            { _targets[target].Used = true; }
        }

        private sealed class TargetState
        {
            internal readonly HashSet<RuleAttackWithWeapon> Attacks =
                new HashSet<RuleAttackWithWeapon>(
                    ReferenceIdentityComparer<RuleAttackWithWeapon>.Instance);
            internal bool Used;
        }

        private sealed class ReferenceIdentityComparer<T> : IEqualityComparer<T>
            where T : class
        {
            internal static readonly ReferenceIdentityComparer<T> Instance =
                new ReferenceIdentityComparer<T>();
            public bool Equals(T left, T right) { return ReferenceEquals(left, right); }
            public int GetHashCode(T value) { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
