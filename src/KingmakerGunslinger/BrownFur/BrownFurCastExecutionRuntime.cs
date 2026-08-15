using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kingmaker.Controllers;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;

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
            internal CotwArcanistContract Contract;
            internal BrownFurBonusAdapterPlan BonusPlan;
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
        private static string _lastFailure;
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
        internal static string LastFailure
        { get { lock (Gate) return _lastFailure; } }

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
                lock (Gate) Bindings.Add(identity, new Binding { Owner = owner,
                    Command = command, Ability = ability, Contract = contract,
                    BonusPlan = bonusPlan });
                BrownFurCastDecision decision = transaction.Decision;
                if (decision.ShareTransmutation &&
                    !BrownFurShareTargetingRuntime.Begin(identity, ability,
                        target.Unit, decision.ShareDelivery))
                    throw new InvalidOperationException(
                        "Share Transmutation scope could not be retained.");
                if (decision.TransmutationSupremacy &&
                    !BrownFurSupremacyRuntime.Begin(identity, ability))
                    throw new InvalidOperationException(
                        "Transmutation Supremacy scope could not be retained.");
                return true;
            }
            catch (Exception exception)
            {
                RecordFailure("begin", exception);
                Coordinator.EndCommand(command, true);
                return false;
            }
        }

        internal static bool AttachRule(RuleCastSpell rule)
        {
            try
            {
                return rule != null && rule.Spell != null &&
                    rule.Context != null && Coordinator.AttachRule(
                        rule.Spell, rule, rule.Context);
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
                if (rule == null || !Coordinator.TryGetByRule(rule,
                        out transaction)) return false;
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
                if (!committed)
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
                if (rule != null && rule.ExecutionProcess != null)
                    Coordinator.AttachProcess(rule, rule.ExecutionProcess);
            }
            catch (Exception exception)
            { RecordFailure("attach-process", exception); }
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

        internal static void EndCommand(UnitUseAbility command)
        {
            try
            {
                lock (Gate)
                {
                    AbilityData ability;
                    if (command != null && SuppressedSpendCommands.TryGetValue(
                            command, out ability))
                    {
                        SuppressedSpendCommands.Remove(command);
                        SuppressedSpends.Remove(ability);
                    }
                }
                Coordinator.EndCommand(command, false);
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

        internal static void Clear()
        {
            lock (Gate) _lastFailure = null;
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
                }
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
            SafeCleanup("release-share", () =>
                BrownFurShareTargetingRuntime.Release(identity));
            SafeCleanup("release-supremacy", () =>
                BrownFurSupremacyRuntime.Release(identity));
            SafeCleanup("release-modifier", () =>
                BrownFurModifierAdjustmentRuntime.Release(identity));
            lock (Gate) Bindings.Remove(identity);
        }

        private static void SafeCleanup(string operation, Action cleanup)
        {
            try { cleanup(); }
            catch (Exception exception)
            { RecordFailure(operation, exception); }
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
