using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Actions;
using KingmakerGunslinger.Bootstrap;
using KingmakerGunslinger.Firearms;
using UnityEngine;

namespace KingmakerGunslinger.Firing
{
    internal static class NativeFirearmAttackOrder
    {
        internal sealed class Binding
        {
            internal UnitEntityData Actor;
            internal ItemEntityWeapon Weapon;
            internal UnitEntityData Target;
            internal int Epoch;
            internal long Submission;
            internal FirearmAttackOrderLedger.Order Order;
            internal bool Proposal, Preview;
            internal UnitCommand MergedInto;
        }
        // A local receipt for one native handler invocation. It never grants
        // consent; it revokes only orders that this handler submitted if a
        // later native input operation throws. Nothing survives on a stack,
        // thread, handler instance, or frame after the invocation finishes.
        internal sealed class InputInvocation
        {
            internal bool Completed, Simulated;
            internal readonly List<KeyValuePair<UnitCommand, Binding>> Submitted =
                new List<KeyValuePair<UnitCommand, Binding>>();
        }
        internal static InputInvocation BeginInput() { return new InputInvocation(); }
        internal static void SetInputSimulation(InputInvocation invocation, bool simulated)
        { invocation.Simulated = simulated; }
        internal static void CompleteInput(InputInvocation invocation)
        { invocation.Completed = true; }
        internal static void EndInput(InputInvocation invocation)
        {
            if (invocation.Completed) return;
            foreach (var submitted in invocation.Submitted)
            {
                Binding binding = submitted.Value;
                Ledger.Cancel(binding.Order);
                UnitCommand command = binding.MergedInto ?? submitted.Key;
                // A reentrant newer order owns itself; a stale cleanup cannot
                // interrupt it even if native merging reused a command object.
                if (ReferenceEquals(Get(command), binding) &&
                    ReferenceEquals(command.Executor, binding.Actor) &&
                    (binding.Actor.Commands.Raw.Contains(command) ||
                     binding.Actor.Commands.Queue.Contains(command)))
                    command.Interrupt(true);
            }
        }
        private static readonly ConditionalWeakTable<UnitCommand, Binding> Commands =
            new ConditionalWeakTable<UnitCommand, Binding>();
        [ThreadStatic] private static FirearmAttackOrderLedger.Order constructing;
        [ThreadStatic] private static bool constructionClaimed;
        private static FirearmAttackOrderLedger Ledger
        { get { return BrokenSequenceSuppressionRuntime.Orders; } }
        internal static Binding Get(UnitCommand command)
        {
            Binding binding;
            return command != null && Commands.TryGetValue(command, out binding)
                ? binding : null;
        }
        internal static bool ClaimConstruction(UnitEntityData actor,
            UnitEntityData target, ItemEntityWeapon weapon)
        {
            if (constructing == null || constructionClaimed ||
                !ReferenceEquals(constructing.Actor, actor) ||
                !ReferenceEquals(constructing.Target, target) ||
                !ReferenceEquals(constructing.Weapon, weapon)) return false;
            constructionClaimed = true;
            return true;
        }
        private static bool Resolve(UnitEntityData actor, UnitEntityData target,
            out ExactEquippedFirearmContext firearm)
        {
            firearm = null;
            string reason;
            return actor != null && actor.Descriptor != null && target != null &&
                target.Descriptor != null && !target.Descriptor.State.IsDead &&
                actor.Descriptor.State.CanAct && actor.CanAttack(target) &&
                ExactEquippedFirearmResolver.TryResolve(actor.Descriptor,
                    out firearm, out reason);
        }
        internal static UnitCommand CreateAttack(UnitEntityData actor, UnitEntityData target)
        {
            ExactEquippedFirearmContext firearm;
            if (!Resolve(actor, target, out firearm))
                return UnitAttack.CreateAttackCommand(actor, target);
            var proposal = Ledger.Propose(actor, firearm.Weapon, target);
            var previous = constructing;
            bool previousClaimed = constructionClaimed;
            UnitCommand command;
            constructing = proposal;
            constructionClaimed = false;
            try { command = UnitAttack.CreateAttackCommand(actor, target); }
            finally { constructing = previous; constructionClaimed = previousClaimed; }
            Track(command, actor, firearm.Weapon, proposal, true);
            return command;
        }
        internal static UnitCommand CreateControllerAttack(UnitEntityData target,
            UnitEntityData actor)
        {
            ExactEquippedFirearmContext firearm;
            return Resolve(actor, target, out firearm)
                ? CreateAttack(actor, target) : new UnitAttack(target);
        }
        internal static UnitCommand CreateAutoUse(AbilityData ability,
            TargetWrapper target, GameObject clickedObject)
        {
            if (!ReferenceEquals(ability.Blueprint, BlueprintBootstrap.ReloadTestMusketAbility))
                return UnitUseAbility.CreateCastCommand(ability, target);
            var view = clickedObject == null ? null : clickedObject.GetComponent<Kingmaker.View.UnitEntityView>();
            UnitEntityData actor = ability.Caster.Unit;
            ExactEquippedFirearmContext firearm;
            if (view == null || !Resolve(actor, view.EntityData, out firearm) ||
                firearm.EffectiveCondition == FirearmCondition.Wrecked ||
                !ReferenceEquals(actor.GetAvailableAutoUseAbility(), ability)) return null;
            UnitCommand command = UnitUseAbility.CreateCastCommand(ability, target);
            Track(command, actor, firearm.Weapon,
                Ledger.Propose(actor, firearm.Weapon, view.EntityData), true);
            var reload = command as UnitUseAbility;
            if (reload != null)
                EmptyFirearmAttackCommandPatch.CaptureReload(reload, actor,
                    view.EntityData, firearm.Weapon);
            return command;
        }
        private static Binding Track(UnitCommand command, UnitEntityData actor,
            ItemEntityWeapon weapon, FirearmAttackOrderLedger.Order order, bool proposal)
        {
            if (command == null) return null;
            var binding = new Binding { Actor = actor, Weapon = weapon,
                Epoch = Ledger.Epoch(actor, weapon), Order = order, Proposal = proposal,
                Target = order == null ? (command as UnitAttack)?.Target : order.Target as UnitEntityData };
            Commands.Remove(command);
            Commands.Add(command, binding);
            return binding;
        }
        internal static void Submit(UnitCommands commands, UnitCommand command, InputInvocation invocation)
        {
            if (command == null) return;
            Binding binding = Get(command);
            if (invocation.Simulated)
            {
                // Native turn previews use Temporary commands and simulate=true.
                // They need the ordinary reload/attack shape for predictions,
                // but must neither accept nor cancel a live player order.
                if (binding == null)
                {
                    binding = new Binding();
                    Commands.Add(command, binding);
                }
                binding.Preview = true;
                binding.Proposal = false;
                binding.Order = null;
                commands.Run(command);
                return;
            }
            if (binding == null || !binding.Proposal) { commands.Run(command); return; }
            ExactEquippedFirearmContext firearm;
            var actor = binding.Actor;
            if (!ReferenceEquals(commands, actor.Commands) ||
                !Resolve(actor, binding.Order.Target as UnitEntityData, out firearm) ||
                !ReferenceEquals(firearm.Weapon, binding.Weapon) ||
                firearm.EffectiveCondition == FirearmCondition.Wrecked ||
                binding.Epoch != Ledger.Epoch(actor, binding.Weapon))
            { Ledger.Cancel(binding.Order); return; }
            var ability = command as UnitUseAbility;
            if (ability != null && !ability.Spell.IsAvailable)
            { Ledger.Cancel(binding.Order); return; }
            invocation.Submitted.Add(new KeyValuePair<UnitCommand, Binding>(command, binding));
            try
            {
                commands.Run(command);
                // Native paused retargeting can retain the running command.
                // Accept ownership of that actual survivor, preserving its
                // already-spent actions and attack progress.
                UnitCommand accepted = binding.MergedInto ?? command;
                bool owned = ReferenceEquals(accepted.Executor, actor) &&
                    (commands.Raw.Contains(accepted) || commands.Queue.Contains(accepted));
                if (owned && !accepted.IsFinished && Ledger.Accept(binding.Order, binding.Submission))
                {
                    binding.Proposal = false;
                    Ledger.Own(binding.Order, accepted, commands);
                    if (!ReferenceEquals(accepted, command))
                    {
                        Commands.Remove(accepted);
                        Commands.Add(accepted, binding);
                        EmptyFirearmAttackCommandPatch.TransferPendingReload(
                            command as UnitUseAbility, accepted as UnitUseAbility);
                    }
                    return;
                }
                Ledger.Cancel(binding.Order);
            }
            catch
            {
                Ledger.Cancel(binding.Order);
                if (ReferenceEquals(command.Executor, actor) &&
                    (commands.Raw.Contains(command) || commands.Queue.Contains(command)))
                    command.Interrupt(true);
                throw;
            }
        }
        internal static void Initialized(UnitCommand command, UnitEntityData actor)
        {
            if (Get(command) != null || !(command is UnitAttack)) return;
            ExactEquippedFirearmContext firearm;
            string reason;
            if (actor != null && ExactEquippedFirearmResolver.TryResolve(
                actor.Descriptor, out firearm, out reason))
                Track(command, actor, firearm.Weapon, Ledger.Current(actor), false);
        }
        internal static void CreatedAutomatic(UnitCommand command, UnitEntityData actor, UnitEntityData target)
        {
            Initialized(command, actor);
            var reload = command as UnitUseAbility;
            if (reload == null || !ReferenceEquals(reload.Spell.Blueprint, BlueprintBootstrap.ReloadTestMusketAbility)) return;
            ExactEquippedFirearmContext firearm;
            string reason;
            if (!ExactEquippedFirearmResolver.TryResolve(actor.Descriptor, out firearm, out reason)) return;
            CaptureReloadBinding(reload, actor, firearm.Weapon);
            Get(reload).Target = target;
            var current = Ledger.Current(actor);
            if (current != null && ReferenceEquals(current.Target, target))
                EmptyFirearmAttackCommandPatch.CaptureReload(reload, actor, target, firearm.Weapon);
        }
        internal static void CaptureReloadBinding(UnitUseAbility command,
            UnitEntityData actor, ItemEntityWeapon weapon)
        {
            if (Get(command) == null)
                Track(command, actor, weapon, constructing ?? Ledger.Current(actor),
                    constructing != null);
        }
        internal static bool Submitting(UnitEntityData actor, UnitCommand command,
            bool fromQueue)
        {
            if (actor == null || command == null) return false;
            Initialized(command, actor);
            var current = Ledger.Current(actor);
            Binding binding = Get(command);
            if (binding != null && binding.Preview) return true;
            var reload = command as UnitUseAbility;
            if (binding == null && reload != null &&
                command.AiAction != null && ReferenceEquals(reload.Spell.Blueprint,
                    BlueprintBootstrap.ReloadTestMusketAbility))
            {
                ExactEquippedFirearmContext firearm;
                string reason;
                if (ExactEquippedFirearmResolver.TryResolve(actor.Descriptor, out firearm, out reason))
                {
                    // An AI reload may be the very first stale operation after
                    // the break. Bind it even when there is no accepted order,
                    // so suppression rejects it before native action spend.
                    Track(command, actor, firearm.Weapon, current, false);
                    if (current != null)
                        EmptyFirearmAttackCommandPatch.CaptureReload(reload, actor,
                            current.Target as UnitEntityData, firearm.Weapon);
                    binding = Get(command);
                }
            }
            if (binding != null && !ReferenceEquals(binding.Actor, actor)) return false;
            if (binding != null && !binding.Proposal &&
                (binding.Epoch != Ledger.Epoch(actor, binding.Weapon) ||
                 Ledger.IsSuppressed(actor, binding.Weapon) ||
                 (binding.Epoch != 0 && binding.Order == null) ||
                 (binding.Order != null && (!Ledger.IsCurrent(binding.Order) ||
                    !ReferenceEquals(binding.Target, binding.Order.Target) ||
                    (command is UnitAttack && !ReferenceEquals(((UnitAttack)command).Target, binding.Order.Target)))))) return false;
            if (binding != null && binding.Proposal && binding.Order.Cancelled) return false;
            return true;
        }
        private static bool IsOwned(UnitEntityData actor, UnitCommand command)
        {
            return command != null && ReferenceEquals(command.Executor, actor) &&
                (actor.Commands.Raw.Contains(command) || actor.Commands.Queue.Contains(command) ||
                 ReferenceEquals(actor.Commands.PreviousCommand, command));
        }
        internal static bool IsOrderContainer(UnitCommands commands, UnitEntityData actor)
        {
            var order = Ledger.Current(actor);
            // Native Temporary replaces actor.Commands while predicting input.
            // Retain the container of the accepted order, not that temporary
            // property value, including the finished-reload callback gap.
            return actor != null && ReferenceEquals(commands, actor.Commands) &&
                (order == null || ReferenceEquals(order.CommandContainer, commands));
        }
        internal static void Submitted(UnitEntityData actor, UnitCommand command)
        {
            Binding binding = Get(command);
            if (binding != null && binding.Preview) return;
            UnitCommand retained = binding == null ? command : binding.MergedInto ?? command;
            if (binding != null && !retained.IsFinished && IsOwned(actor, retained))
            {
                // Native Run may return without accepting, or may enqueue by
                // calling AddToQueueInternal. Count the retained command once,
                // after that outcome, never an unrelated observed submission.
                if (binding.Submission == 0) binding.Submission = Ledger.Submit(actor);
                if (!binding.Proposal) Ledger.Own(binding.Order, retained, actor.Commands);
            }
            ReconcileOwnership(actor);
        }
        internal static void ReconcileOwnership(UnitEntityData actor)
        {
            var current = Ledger.Current(actor);
            var owner = current == null ? null : current.Owner as UnitCommand;
            // Native queue clearing does not call OnEnded on removed commands.
            // Successful completed reloads keep their one scheduled continuation.
            if (owner != null && (current.OwnerSlotRemoved || !owner.IsFinished) && !IsOwned(actor, owner))
                Ledger.Cancel(current);
        }
        internal static void RemovedSlot(UnitEntityData actor, UnitCommand.CommandType type)
        {
            var current = Ledger.Current(actor);
            var owner = current == null ? null : current.Owner as UnitCommand;
            // Observe the native slot-removal operation (including its native
            // Standard/Move pairing), not a guess about an incoming ability.
            // This also revokes a just-completed reload's pending callback when
            // a real replacement clears the slot before the callback runs.
            if (owner != null && owner.Type == type && !IsOwned(actor, owner))
                current.OwnerSlotRemoved = true;
            // Run is the only native caller (apart from this method's paired
            // recursion). Decide after it retains the incoming command: a
            // legitimate reload-to-attack transfer clears this marker in Own.

        }
        internal static void InterruptedAll(UnitEntityData actor)
        {
            var current = Ledger.Current(actor);
            var owner = current == null ? null : current.Owner as UnitCommand;
            // InterruptAll can retain a genuinely uninterruptible live command.
            if (owner != null && (owner.IsFinished || !IsOwned(actor, owner)))
                Ledger.Cancel(current);
        }
        internal sealed class TargetResolution
        {
            internal Binding Binding;
            internal UnitEntityData PreviousTarget;
        }
        internal static TargetResolution ResolvingTarget(UnitAttack command)
        {
            Binding binding = Get(command);
            return binding != null && !command.IsFinished && binding.Order != null &&
                ReferenceEquals(binding.Order.Owner, command) && IsOwned(binding.Actor, command) && MayExecute(command)
                ? new TargetResolution { Binding = binding, PreviousTarget = command.Target } : null;
        }
        internal static void ResolvedTarget(UnitAttack command, TargetResolution resolution, bool resolved)
        {
            // Only the actual surviving command's native UpdateTarget may move
            // its order target. Construction, AI target equality and callbacks
            // cannot supply this provenance or acquire a different owner.
            if (!resolved || resolution == null || !ReferenceEquals(Get(command), resolution.Binding) ||
                ReferenceEquals(command.Target, resolution.PreviousTarget)) return;
            Binding binding = resolution.Binding;
            if (IsOwned(binding.Actor, command) && Ledger.Retarget(binding.Order, command,
                resolution.PreviousTarget, command.Target)) binding.Target = command.Target;
        }
        internal static bool MayExecute(UnitAttack command)
        {
            Binding binding = Get(command);
            if (binding == null) return true;
            if (binding.Preview) return false;
            ExactEquippedFirearmContext firearm;
            string reason;
            bool valid = !binding.Proposal &&
                binding.Epoch == Ledger.Epoch(binding.Actor, binding.Weapon) &&
                !Ledger.IsSuppressed(binding.Actor, binding.Weapon) &&
                (binding.Epoch == 0 || binding.Order != null) &&
                (binding.Order == null || (Ledger.IsCurrent(binding.Order) &&
                    ReferenceEquals(binding.Target, binding.Order.Target) &&
                    ReferenceEquals(command.Target, binding.Order.Target))) &&
                ExactEquippedFirearmResolver.TryResolve(binding.Actor.Descriptor,
                    out firearm, out reason) && ReferenceEquals(firearm.Weapon, binding.Weapon) &&
                firearm.EffectiveCondition != FirearmCondition.Wrecked;
            if (!valid && !binding.Proposal) Invalidate(binding);
            return valid;
        }
        internal static void Invalidate(Binding binding)
        { if (binding != null) Ledger.Cancel(binding.Order); }
        internal static bool MayMerge(UnitAttack proposed, UnitCommand previous)
        {
            Binding binding = Get(proposed);
            if (binding == null) return true;
            var attack = previous as UnitAttack;
            Binding survivor = Get(previous);
            if (binding.Preview || (survivor != null && survivor.Preview)) return false;
            // Preserve native retargeting only within the same still-valid
            // generation. Never inherit the shot/progress of the broken order.
            return attack != null && survivor != null &&
                ReferenceEquals(binding.Actor, survivor.Actor) &&
                ReferenceEquals(binding.Weapon, survivor.Weapon) &&
                binding.Epoch == survivor.Epoch && MayExecute(attack);
        }
        internal static bool MayMergeReload(UnitUseAbility proposed, UnitCommand previous)
        {
            Binding binding = Get(proposed), survivor = Get(previous);
            if (binding == null) return true;
            if (binding.Preview || (survivor != null && survivor.Preview)) return false;
            // A manual reload has no attack authority to inherit. Let native
            // spell/range rules decide its merge. A bound reload must still
            // belong to the same live firearm generation and accepted order.
            return survivor == null || (ReferenceEquals(binding.Actor, survivor.Actor) &&
                ReferenceEquals(binding.Weapon, survivor.Weapon) &&
                binding.Epoch == survivor.Epoch && MayResume(survivor));
        }
        internal static void Merged(UnitCommand proposed, UnitCommand previous, bool merged)
        {
            Binding binding = Get(proposed);
            if (merged && binding != null && binding.Proposal)
                binding.MergedInto = previous;
        }
        internal static void Interrupted(UnitCommand command)
        {
            Binding binding = Get(command);
            if (binding != null && !binding.Preview) Ledger.Cancel(binding.Order);
        }
        internal static bool MayResume(Binding binding)
        {
            return binding != null && !binding.Proposal && !binding.Preview &&
                (binding.Epoch == 0 || binding.Order != null) &&
                (binding.Order == null || ReferenceEquals(binding.Target, binding.Order.Target)) && Ledger.MayResume(
                binding.Actor, binding.Weapon, binding.Epoch,
                binding.Submission, binding.Order);
        }
    }
}
