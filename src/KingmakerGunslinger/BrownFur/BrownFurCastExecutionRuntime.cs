using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using KingmakerGunslinger.Blueprints;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// Inert-until-armed runtime boundary for one already validated Brown-Fur
    /// cast. Player intent discovery remains a separate publication gate.
    /// </summary>
    internal static class BrownFurCastExecutionRuntime
    {
        private sealed class Binding
        {
            internal UnitDescriptor Owner;
            internal UnitUseAbility Command;
            internal AbilityData Ability;
            internal TargetWrapper Target;
            internal CotwArcanistContract Contract;
            internal BrownFurBonusAdapterPlan BonusPlan;
            internal BrownFurDirectCastHandle DirectHandle;
            internal RuleCastSpell Rule;
            internal AbilityExecutionProcess Process;
            internal bool IntentConsumed;
        }

        private static readonly object Gate = new object();
        private static readonly Dictionary<string, Binding> Bindings =
            new Dictionary<string, Binding>(StringComparer.Ordinal);
        private static readonly HashSet<AbilityData> SuppressedSpends =
            new HashSet<AbilityData>(ReferenceComparer.Instance);
        private static readonly Dictionary<UnitUseAbility, AbilityData>
            SuppressedSpendCommands =
                new Dictionary<UnitUseAbility, AbilityData>(
                    CommandReferenceComparer.Instance);
        private static readonly Dictionary<UnitUseAbility, string>
            RejectedCommands = new Dictionary<UnitUseAbility, string>(
                CommandReferenceComparer.Instance);
        private static string _lastFailure;
        private static string _lastTerminalState = string.Empty;
        private static readonly BrownFurCastCommitCoordinator<UnitDescriptor,
            UnitUseAbility, AbilityData, RuleCastSpell,
            AbilityExecutionContext, AbilityExecutionProcess> Coordinator =
                new BrownFurCastCommitCoordinator<UnitDescriptor,
                    UnitUseAbility, AbilityData, RuleCastSpell,
                    AbilityExecutionContext, AbilityExecutionProcess>(Release);

        internal static int ActiveTransactionCount
        { get { return Coordinator.ActiveTransactionCount; } }
        internal static int ReservationCount
        { get { return Coordinator.ReservationCount; } }
        internal static int SuppressedSpendCount
        { get { lock (Gate) return SuppressedSpends.Count; } }
        internal static int RejectedCommandCount
        { get { lock (Gate) return RejectedCommands.Count; } }
        internal static string LastFailure
        { get { lock (Gate) return _lastFailure; } }
        internal static string LastTerminalState
        { get { lock (Gate) return _lastTerminalState; } }

        internal static bool Begin(CotwArcanistContract contract,
            UnitUseAbility command, AbilityData ability, TargetWrapper target,
            BrownFurCastTransaction transaction,
            BrownFurBonusAdapterPlan bonusPlan)
        {
            if (contract == null || contract.Reservoir == null ||
                command == null || ability == null || ability.Caster == null ||
                target == null || transaction == null ||
                transaction.State != BrownFurCastTransactionState.Validated)
                return false;
            lock (Gate)
                if (SuppressedSpends.Contains(ability)) return false;
            UnitDescriptor owner = ability.Caster;
            int available = owner.Resources.ContainsResource(contract.Reservoir) ?
                owner.Resources.GetResourceAmount(contract.Reservoir) : 0;
            if (!Coordinator.Begin(owner, command, ability, transaction,
                    available)) return false;
            string identity = transaction.Intent.TransactionIdentity;
            try
            {
                var binding = new Binding { Owner = owner,
                    Command = command, Ability = ability, Target = target,
                    Contract = contract, BonusPlan = bonusPlan };
                lock (Gate) Bindings.Add(identity, binding);
                OpenPreCommitScopes(binding, transaction);
                return true;
            }
            catch (Exception exception)
            {
                RecordFailure("begin", exception);
                Coordinator.EndCommand(command, true);
                return false;
            }
        }

        internal static bool BeginDirect(CotwArcanistContract contract,
            AbilityData ability, TargetWrapper target,
            BrownFurCastTransaction transaction,
            BrownFurBonusAdapterPlan bonusPlan,
            BrownFurDirectCastHandle handle)
        {
            if (contract == null || contract.Reservoir == null ||
                ability == null || ability.Caster == null || target == null ||
                target.Unit == null || transaction == null || handle == null ||
                transaction.State != BrownFurCastTransactionState.Validated ||
                !ReferenceEquals(handle.Ability, ability) ||
                !ReferenceEquals(handle.Target, target) ||
                !string.Equals(handle.TransactionIdentity,
                    transaction.Intent.TransactionIdentity,
                    StringComparison.Ordinal)) return false;
            lock (Gate)
                if (SuppressedSpends.Contains(ability)) return false;
            UnitDescriptor owner = ability.Caster;
            int available = owner.Resources.ContainsResource(contract.Reservoir) ?
                owner.Resources.GetResourceAmount(contract.Reservoir) : 0;
            if (!Coordinator.BeginDirect(owner, ability, transaction,
                    available)) return false;
            string identity = transaction.Intent.TransactionIdentity;
            try
            {
                var binding = new Binding { Owner = owner, Ability = ability,
                    Target = target, Contract = contract,
                    BonusPlan = bonusPlan, DirectHandle = handle };
                lock (Gate) Bindings.Add(identity, binding);
                OpenPreCommitScopes(binding, transaction);
                return true;
            }
            catch (Exception exception)
            {
                RecordFailure("begin-direct", exception);
                Coordinator.CancelDirect(ability);
                return false;
            }
        }

        internal static bool AttachRule(RuleCastSpell rule)
        {
            try
            {
                if (rule == null || rule.Spell == null ||
                    rule.Context == null) return false;
                BrownFurCastTransaction transaction;
                if (!Coordinator.TryGetByAbility(rule.Spell,
                        out transaction)) return false;
                Binding binding = Get(
                    transaction.Intent.TransactionIdentity);
                if (binding == null) return false;
                if (binding.DirectHandle != null &&
                    (rule.SpellTarget == null ||
                     rule.SpellTarget.Unit == null ||
                     binding.Target == null ||
                     !ReferenceEquals(rule.SpellTarget.Unit,
                        binding.Target.Unit)))
                {
                    binding.DirectHandle.MarkFailure(
                        "direct-rule-target-mismatch");
                    return false;
                }
                bool attached = Coordinator.AttachRule(
                    rule.Spell, rule, rule.Context);
                if (attached)
                {
                    binding.Rule = rule;
                    if (binding.DirectHandle != null)
                        binding.DirectHandle.MarkRuleAttached(rule);
                }
                return attached;
            }
            catch (Exception exception)
            {
                RecordFailure("attach-rule", exception);
                return false;
            }
        }

        internal static bool TryCommit(RuleCastSpell rule, out bool proceed)
        {
            proceed = true;
            BrownFurCastTransaction transaction = null;
            try
            {
                if (rule == null || rule.Spell == null) return false;
                if (!Coordinator.TryGetByRule(rule, out transaction))
                {
                    BrownFurCastTransaction pending;
                    if (!Coordinator.TryGetByAbility(rule.Spell,
                            out pending)) return false;
                    Binding unmatched = Get(
                        pending.Intent.TransactionIdentity);
                    if (unmatched == null ||
                        unmatched.DirectHandle == null) return false;
                    proceed = false;
                    unmatched.DirectHandle.MarkFailure(
                        "direct-rule-binding-rejected");
                    Coordinator.CancelDirect(rule.Spell);
                    return true;
                }
                Binding binding = Get(transaction.Intent.TransactionIdentity);
                if (binding == null)
                {
                    proceed = false;
                    Coordinator.FailRule(rule);
                    return true;
                }
                bool committed = Coordinator.Commit(binding.Owner,
                    binding.Ability, cost => DebitAndOpenModifier(binding,
                        rule.Context, transaction, cost));
                proceed = committed;
                if (committed && binding.DirectHandle != null)
                    binding.DirectHandle.MarkCommitted();
                if (!committed && binding.DirectHandle != null)
                    binding.DirectHandle.MarkFailure(
                        "provider-direct-commit-rejected");
                else if (!committed)
                    lock (Gate)
                    {
                        SuppressedSpends.Add(binding.Ability);
                        SuppressedSpendCommands[binding.Command] =
                            binding.Ability;
                    }
                return true;
            }
            catch (Exception exception)
            {
                proceed = false;
                RecordFailure("commit", exception);
                if (transaction != null) RuleFailed(rule);
                return transaction != null;
            }
        }

        internal static void AttachProcess(RuleCastSpell rule)
        {
            try
            {
                if (rule == null || rule.ExecutionProcess == null) return;
                BrownFurCastTransaction transaction;
                if (!Coordinator.TryGetByRule(rule,
                        out transaction)) return;
                if (!Coordinator.AttachProcess(rule,
                        rule.ExecutionProcess)) return;
                Binding binding = Get(
                    transaction.Intent.TransactionIdentity);
                if (binding != null)
                    binding.Process = rule.ExecutionProcess;
            }
            catch (Exception exception)
            { RecordFailure("attach-process", exception); }
        }

        internal static void ConsumeCommittedIntent(RuleCastSpell rule)
        {
            BrownFurCastTransaction transaction;
            if (rule == null || !Coordinator.TryGetByRule(rule,
                    out transaction) || transaction == null ||
                transaction.State != BrownFurCastTransactionState.Committed)
                return;
            Binding binding = Get(transaction.Intent.TransactionIdentity);
            if (binding == null) return;
            lock (Gate)
            {
                if (binding.IntentConsumed) return;
                binding.IntentConsumed = true;
            }
            try
            {
                BrownFurBlueprintSet blueprints =
                    BrownFurOptionalExtensionCoordinator.Blueprints;
                BrownFurPlayerIntentRuntime.Consume(binding.Owner, blueprints,
                    transaction.Decision);
            }
            catch
            {
                lock (Gate) binding.IntentConsumed = false;
                throw;
            }
        }

        internal static void RuleFailed(RuleCastSpell rule)
        {
            BrownFurCastTransaction transaction;
            if (rule == null || !Coordinator.TryGetByRule(rule,
                    out transaction)) return;
            Binding binding = Get(transaction.Intent.TransactionIdentity);
            try
            {
                if (binding != null && transaction.State ==
                    BrownFurCastTransactionState.Committed &&
                    transaction.DebitedReservoirPoints > 0)
                    RestoreExact(binding, transaction.DebitedReservoirPoints);
            }
            catch (Exception exception)
            { RecordFailure("reservoir-rollback", exception); }
            finally
            {
                try { Coordinator.FailRule(rule); }
                catch (Exception exception)
                { RecordFailure("fail-rule", exception); }
            }
        }

        internal static bool ConsumeSpendSuppression(AbilityData ability)
        {
            if (ability == null) return false;
            lock (Gate)
            {
                bool removed = SuppressedSpends.Remove(ability);
                if (removed)
                {
                    UnitUseAbility command = null;
                    foreach (KeyValuePair<UnitUseAbility, AbilityData> pair in
                        SuppressedSpendCommands)
                        if (ReferenceEquals(pair.Value, ability))
                        {
                            command = pair.Key;
                            break;
                        }
                    if (command != null)
                        SuppressedSpendCommands.Remove(command);
                }
                return removed;
            }
        }

        internal static void RejectCommand(UnitUseAbility command,
            string failure)
        {
            if (command == null) return;
            lock (Gate) RejectedCommands[command] = failure ?? string.Empty;
        }

        internal static bool ConsumeCommandRejection(UnitUseAbility command,
            out string failure)
        {
            failure = string.Empty;
            if (command == null) return false;
            lock (Gate)
            {
                if (!RejectedCommands.TryGetValue(command, out failure))
                    return false;
                RejectedCommands.Remove(command);
                return true;
            }
        }

        internal static void EndCommand(UnitUseAbility command)
        {
            try
            {
                lock (Gate)
                {
                    if (command != null) RejectedCommands.Remove(command);
                    AbilityData ability;
                    if (command != null && SuppressedSpendCommands.TryGetValue(
                            command, out ability))
                    {
                        SuppressedSpendCommands.Remove(command);
                        SuppressedSpends.Remove(ability);
                    }
                }
                bool interrupted = command != null &&
                    command.Result != UnitCommand.ResultType.Success;
                Coordinator.EndCommand(command, interrupted);
            }
            catch (Exception exception)
            { RecordFailure("end-command", exception); }
        }

        internal static void ProcessTick(AbilityExecutionProcess process)
        {
            try
            {
                if (process != null && process.IsEnded)
                    Coordinator.ProcessTerminal(process, false);
            }
            catch (Exception exception)
            { RecordFailure("process-terminal", exception); }
        }

        internal static BrownFurDirectCastStatus CompleteDirectRule(
            BrownFurDirectCastHandle handle, RuleCastSpell rule)
        {
            if (handle == null)
                return BrownFurDirectCastStatus.Rejected(
                    "direct-handle-missing");
            handle.MarkRuleReturned();
            if (!handle.Matches(rule))
            {
                handle.MarkFailure("direct-rule-identity-mismatch");
                CleanupDirect(handle);
                return handle.Snapshot();
            }
            BrownFurDirectCastStatus status = handle.Snapshot();
            if (!status.Accepted || status.Complete) return status;
            if (!status.Committed)
            {
                handle.MarkFailure("direct-rule-not-committed");
                CleanupDirect(handle);
                return handle.Snapshot();
            }
            if (rule.ExecutionProcess == null)
            {
                if (!Coordinator.CompleteDirect(handle.Ability) &&
                    !handle.Snapshot().Complete)
                    handle.MarkFailure(
                        "direct-rule-without-process-not-completed");
                return handle.Snapshot();
            }
            Binding directBinding = GetDirectBinding(handle);
            if (directBinding == null)
            {
                handle.MarkResidualFailure(
                    "direct-transaction-binding-missing");
                return handle.Snapshot();
            }
            // Retain the process even when the lifecycle hook cannot attach it.
            // A failed attachment must never make Cleanup treat a still-running
            // native effect process as a process-free synchronous cast.
            directBinding.Process = rule.ExecutionProcess;
            if (!Coordinator.DirectProcessAttached(handle.Ability))
            {
                BrownFurCastTransaction transaction;
                if (!Coordinator.TryGetByAbility(handle.Ability,
                        out transaction) ||
                    !Coordinator.AttachProcess(rule,
                        rule.ExecutionProcess))
                {
                    handle.MarkResidualFailure(
                        "direct-execution-process-not-attached");
                    return handle.Snapshot();
                }
            }
            if (rule.ExecutionProcess.IsEnded)
                Coordinator.ProcessTerminal(rule.ExecutionProcess, false);
            return handle.Snapshot();
        }

        internal static BrownFurDirectCastStatus InspectDirect(
            BrownFurDirectCastHandle handle)
        {
            if (handle == null)
                return BrownFurDirectCastStatus.Rejected(
                    "direct-handle-missing");
            BrownFurDirectCastStatus status = handle.Snapshot();
            if (!status.Accepted || status.Complete) return status;
            Binding binding = GetDirectBinding(handle);
            if (binding == null)
            {
                handle.MarkResidualFailure(
                    "direct-transaction-binding-missing");
                return handle.Snapshot();
            }
            if (binding.Process != null && binding.Process.IsEnded)
            {
                if (!Coordinator.ProcessTerminal(binding.Process, false) &&
                    !handle.Snapshot().Complete &&
                    !Coordinator.CompleteDirect(handle.Ability))
                    handle.MarkResidualFailure(
                        "direct-ended-process-not-completed");
            }
            else if (binding.Process == null && handle.RuleReturned &&
                status.Committed)
                Coordinator.CompleteDirect(handle.Ability);
            return handle.Snapshot();
        }

        internal static BrownFurDirectCastStatus CleanupDirect(
            BrownFurDirectCastHandle handle)
        {
            if (handle == null)
                return BrownFurDirectCastStatus.Rejected(
                    "direct-handle-missing");
            BrownFurDirectCastStatus status = handle.Snapshot();
            if (!status.Accepted || status.Complete) return status;
            Binding binding = GetDirectBinding(handle);
            if (binding == null)
            {
                if (RetryReleasedCleanup(handle.TransactionIdentity))
                    handle.MarkCleanupRecovered();
                else
                    handle.MarkResidualFailure(
                        "provider-terminal-cleanup-retry-failed");
                return handle.Snapshot();
            }
            BrownFurCastTransaction transaction;
            if (!Coordinator.TryGetByAbility(handle.Ability,
                    out transaction))
            {
                handle.MarkResidualFailure(
                    "direct-transaction-lifecycle-missing");
                return handle.Snapshot();
            }
            if (transaction.State ==
                BrownFurCastTransactionState.Validated)
            {
                Coordinator.CancelDirect(handle.Ability);
                return handle.Snapshot();
            }
            if (transaction.State ==
                BrownFurCastTransactionState.Committed)
            {
                if (binding.Process != null)
                {
                    if (binding.Process.IsEnded)
                    {
                        if (!Coordinator.ProcessTerminal(
                                binding.Process, false) &&
                            !handle.Snapshot().Complete &&
                            !Coordinator.CompleteDirect(handle.Ability))
                            handle.MarkResidualFailure(
                                "direct-ended-process-not-completed");
                    }
                    return handle.Snapshot();
                }
                if (handle.RuleReturned)
                {
                    Coordinator.CompleteDirect(handle.Ability);
                    return handle.Snapshot();
                }
                try
                {
                    if (transaction.DebitedReservoirPoints > 0)
                        RestoreExact(binding,
                            transaction.DebitedReservoirPoints);
                }
                catch (Exception exception)
                {
                    RecordFailure("direct-reservoir-rollback", exception);
                    handle.MarkResidualFailure(
                        "direct-reservoir-rollback-failed");
                    return handle.Snapshot();
                }
                handle.MarkFailure(
                    "direct-cast-aborted-before-rule-return");
                Coordinator.FailDirect(handle.Ability);
            }
            return handle.Snapshot();
        }

        internal static void Clear()
        {
            lock (Gate)
            {
                _lastFailure = null;
                _lastTerminalState = string.Empty;
            }
            try { Coordinator.Clear(); }
            catch (Exception exception)
            { RecordFailure("clear", exception); }
            finally
            {
                lock (Gate)
                {
                    Bindings.Clear();
                    SuppressedSpends.Clear();
                    SuppressedSpendCommands.Clear();
                    RejectedCommands.Clear();
                }
                SafeCleanup("clear-intent", BrownFurCastIntentRuntime.Clear);
                SafeCleanup("clear-share",
                    BrownFurShareTargetingRuntime.Clear);
                SafeCleanup("clear-supremacy",
                    BrownFurSupremacyRuntime.Clear);
                SafeCleanup("clear-modifier",
                    BrownFurModifierAdjustmentRuntime.Clear);
            }
        }

        private static bool DebitAndOpenModifier(Binding binding,
            AbilityExecutionContext context, BrownFurCastTransaction transaction,
            int cost)
        {
            BrownFurReservoirDebitResult result =
                BrownFurReservoirDebit.TryDebitExact(binding.Owner,
                    binding.Contract.Reservoir, cost);
            if (!result.Success) return false;
            if (!transaction.Decision.PowerfulChange) return true;
            bool opened = binding.BonusPlan != null &&
                binding.BonusPlan.Status ==
                    BrownFurBonusAdapterPlanStatus.Supported &&
                BrownFurModifierAdjustmentRuntime.Begin(
                    transaction.Intent.TransactionIdentity, context,
                    binding.Owner.Unit, binding.Ability.Blueprint,
                    transaction.Intent.SelectedAbilityScore,
                    transaction.Decision.PowerfulChangeIncrease,
                    binding.BonusPlan.AppliedBuffGuids,
                    binding.BonusPlan.CarrierFamilies);
            if (opened) return true;
            RestoreExact(binding, cost);
            return false;
        }

        private static void OpenPreCommitScopes(Binding binding,
            BrownFurCastTransaction transaction)
        {
            string identity = transaction.Intent.TransactionIdentity;
            BrownFurCastDecision decision = transaction.Decision;
            if (decision.ShareTransmutation &&
                !BrownFurShareTargetingRuntime.Begin(identity,
                    binding.Ability, binding.Target.Unit,
                    decision.ShareDelivery))
                throw new InvalidOperationException(
                    "Share Transmutation scope could not be retained.");
            if (decision.TransmutationSupremacy &&
                !BrownFurSupremacyRuntime.Begin(identity, binding.Ability))
                throw new InvalidOperationException(
                    "Transmutation Supremacy scope could not be retained.");
        }

        private static void RestoreExact(Binding binding, int cost)
        {
            int before = binding.Owner.Resources.GetResourceAmount(
                binding.Contract.Reservoir);
            binding.Owner.Resources.Restore(binding.Contract.Reservoir, cost);
            int after = binding.Owner.Resources.GetResourceAmount(
                binding.Contract.Reservoir);
            if (after != before + cost) throw new InvalidOperationException(
                "Brown-Fur reservoir rollback did not restore the exact debit.");
        }

        private static Binding Get(string identity)
        {
            lock (Gate)
            {
                Binding binding;
                return Bindings.TryGetValue(identity, out binding) ?
                    binding : null;
            }
        }

        private static Binding GetDirectBinding(
            BrownFurDirectCastHandle handle)
        {
            if (handle == null) return null;
            BrownFurCastTransaction transaction;
            if (!Coordinator.TryGetByAbility(handle.Ability,
                    out transaction)) return null;
            Binding binding = Get(transaction.Intent.TransactionIdentity);
            return binding != null &&
                ReferenceEquals(binding.DirectHandle, handle) &&
                string.Equals(transaction.Intent.TransactionIdentity,
                    handle.TransactionIdentity, StringComparison.Ordinal)
                ? binding : null;
        }

        internal static void RecordPatchFailure(string operation,
            Exception exception)
        { RecordFailure("patch-" + operation, exception); }

        private static void RecordFailure(string operation,
            Exception exception)
        {
            lock (Gate) _lastFailure = operation + ":" +
                (exception == null ? "unknown" :
                    exception.GetType().FullName + ":" + exception.Message);
        }

        private static void Release(BrownFurCastTransaction transaction)
        {
            if (transaction == null) return;
            string identity = transaction.Intent.TransactionIdentity;
            Binding binding = Get(identity);
            bool cleanup = SafeCleanup("release-share", () =>
                BrownFurShareTargetingRuntime.Release(identity));
            cleanup = SafeCleanup("release-supremacy", () =>
                BrownFurSupremacyRuntime.Release(identity)) && cleanup;
            cleanup = SafeCleanup("release-modifier", () =>
                BrownFurModifierAdjustmentRuntime.Release(identity)) &&
                cleanup;
            lock (Gate)
            {
                _lastTerminalState = identity + ":" + transaction.State;
                Bindings.Remove(identity);
            }
            if (binding != null && binding.DirectHandle != null)
            {
                binding.DirectHandle.MarkTerminal(transaction);
                if (!cleanup)
                    binding.DirectHandle.MarkResidualFailure(
                        "provider-terminal-cleanup-failed:" +
                        LastFailure);
            }
        }

        private static bool RetryReleasedCleanup(string identity)
        {
            bool cleanup = SafeCleanup("retry-release-share", () =>
                BrownFurShareTargetingRuntime.Release(identity));
            cleanup = SafeCleanup("retry-release-supremacy", () =>
                BrownFurSupremacyRuntime.Release(identity)) && cleanup;
            cleanup = SafeCleanup("retry-release-modifier", () =>
                BrownFurModifierAdjustmentRuntime.Release(identity)) &&
                cleanup;
            return cleanup;
        }

        private static bool SafeCleanup(string operation, Action cleanup)
        {
            try
            {
                cleanup();
                return true;
            }
            catch (Exception exception)
            {
                RecordFailure(operation, exception);
                return false;
            }
        }

        private sealed class ReferenceComparer : IEqualityComparer<AbilityData>
        {
            internal static readonly ReferenceComparer Instance =
                new ReferenceComparer();
            public bool Equals(AbilityData left, AbilityData right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(AbilityData value)
            { return RuntimeHelpers.GetHashCode(value); }
        }

        private sealed class CommandReferenceComparer :
            IEqualityComparer<UnitUseAbility>
        {
            internal static readonly CommandReferenceComparer Instance =
                new CommandReferenceComparer();
            public bool Equals(UnitUseAbility left, UnitUseAbility right)
            { return ReferenceEquals(left, right); }
            public int GetHashCode(UnitUseAbility value)
            { return RuntimeHelpers.GetHashCode(value); }
        }
    }
}
