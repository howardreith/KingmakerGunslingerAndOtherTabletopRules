using System;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;

namespace KingmakerGunslinger.BrownFur
{
    /// <summary>
    /// Versioned optional integration boundary for callers that already own
    /// spell-source spending and need Brown-Fur's native cast transaction
    /// without constructing or queuing a UnitUseAbility command.
    /// </summary>
    public static class BrownFurDirectCastApi
    {
        public const int ContractVersion = 1;

        public static BrownFurDirectCastStatus Validate(
            AbilityData ability, TargetWrapper target)
        {
            return BrownFurCastIntentRuntime.ValidateDirect(ability, target);
        }

        public static BrownFurDirectCastHandle Begin(
            AbilityData ability, TargetWrapper target)
        {
            return BrownFurCastIntentRuntime.BeginDirect(ability, target);
        }
    }

    public sealed class BrownFurDirectCastStatus
    {
        private BrownFurDirectCastStatus(bool accepted, bool committed,
            bool complete, bool residualState, string state, string failure,
            string detail, string transactionIdentity, int reservoirCost)
        {
            Accepted = accepted;
            Committed = committed;
            Complete = complete;
            ResidualState = residualState;
            State = state ?? string.Empty;
            Failure = failure ?? string.Empty;
            Detail = detail ?? string.Empty;
            TransactionIdentity = transactionIdentity ?? string.Empty;
            ReservoirCost = reservoirCost;
        }

        public bool Accepted { get; private set; }
        public bool Committed { get; private set; }
        public bool Complete { get; private set; }
        public bool ResidualState { get; private set; }
        public string State { get; private set; }
        public string Failure { get; private set; }
        public string Detail { get; private set; }
        public string TransactionIdentity { get; private set; }
        public int ReservoirCost { get; private set; }

        internal static BrownFurDirectCastStatus PreflightAccepted(int cost)
        {
            return new BrownFurDirectCastStatus(true, false, true, false,
                "preflight-accepted", string.Empty,
                "provider preflight accepted", string.Empty, cost);
        }

        internal static BrownFurDirectCastStatus Rejected(string failure)
        {
            return new BrownFurDirectCastStatus(false, false, true, false,
                "rejected", failure, "provider request rejected",
                string.Empty, 0);
        }

        internal static BrownFurDirectCastStatus Create(bool accepted,
            bool committed, bool complete, bool residualState, string state,
            string failure, string detail, string transactionIdentity,
            int reservoirCost)
        {
            return new BrownFurDirectCastStatus(accepted, committed,
                complete, residualState, state, failure, detail,
                transactionIdentity, reservoirCost);
        }
    }

    public sealed class BrownFurDirectCastHandle : IDisposable
    {
        private readonly object _gate = new object();
        private readonly AbilityData _ability;
        private readonly TargetWrapper _target;
        private readonly string _transactionIdentity;
        private readonly int _reservoirCost;
        private bool _accepted;
        private bool _committed;
        private bool _complete;
        private bool _residualState;
        private bool _ruleReturned;
        private string _state;
        private string _failure;
        private string _detail;
        private RuleCastSpell _rule;

        private BrownFurDirectCastHandle(AbilityData ability,
            TargetWrapper target, string transactionIdentity,
            int reservoirCost, bool accepted, string failure)
        {
            _ability = ability;
            _target = target;
            _transactionIdentity = transactionIdentity ?? string.Empty;
            _reservoirCost = reservoirCost;
            _accepted = accepted;
            _complete = !accepted;
            _residualState = accepted;
            _state = accepted ? "reserved" : "rejected";
            _failure = failure ?? string.Empty;
            _detail = accepted ? "provider transaction reserved" :
                "provider request rejected";
        }

        public BrownFurDirectCastStatus Inspect()
        {
            return BrownFurCastExecutionRuntime.InspectDirect(this);
        }

        public BrownFurDirectCastStatus CompleteRule(RuleCastSpell rule)
        {
            return BrownFurCastExecutionRuntime.CompleteDirectRule(
                this, rule);
        }

        public BrownFurDirectCastStatus Cleanup()
        {
            return BrownFurCastExecutionRuntime.CleanupDirect(this);
        }

        public void Dispose()
        {
            Cleanup();
        }

        internal static BrownFurDirectCastHandle CreateAccepted(
            AbilityData ability, TargetWrapper target,
            string transactionIdentity, int reservoirCost)
        {
            return new BrownFurDirectCastHandle(ability, target,
                transactionIdentity, reservoirCost, true, string.Empty);
        }

        internal static BrownFurDirectCastHandle Rejected(string failure)
        {
            return new BrownFurDirectCastHandle(null, null, string.Empty,
                0, false, failure);
        }

        internal AbilityData Ability { get { return _ability; } }
        internal TargetWrapper Target { get { return _target; } }
        internal string TransactionIdentity
        { get { return _transactionIdentity; } }
        internal bool RuleReturned
        { get { lock (_gate) return _ruleReturned; } }

        internal bool Matches(RuleCastSpell rule)
        {
            lock (_gate) return rule != null &&
                ReferenceEquals(_rule, rule);
        }

        internal void MarkRuleAttached(RuleCastSpell rule)
        {
            lock (_gate)
            {
                _rule = rule;
                _state = "rule-attached";
                _detail = "exact rule and target attached";
            }
        }

        internal void MarkCommitted()
        {
            lock (_gate)
            {
                _committed = true;
                _state = "committed";
                _detail = "provider reservoir committed";
            }
        }

        internal void MarkRuleReturned()
        {
            lock (_gate) _ruleReturned = true;
        }

        internal void MarkBeginRejected(string failure)
        {
            lock (_gate)
            {
                _accepted = false;
                _complete = true;
                _residualState = false;
                _state = "rejected";
                _failure = failure ?? "direct-cast-reservation-rejected";
                _detail = "provider reservation rejected";
            }
        }

        internal void MarkFailure(string failure)
        {
            lock (_gate)
            {
                if (string.IsNullOrWhiteSpace(_failure))
                    _failure = failure ?? "provider-direct-cast-failed";
                _detail = _failure;
            }
        }

        internal void MarkTerminal(BrownFurCastTransaction transaction)
        {
            lock (_gate)
            {
                BrownFurCastTransactionState state = transaction == null
                    ? BrownFurCastTransactionState.Failed :
                        transaction.State;
                _complete = true;
                _residualState = false;
                _state = state.ToString();
                if (state == BrownFurCastTransactionState.Rejected &&
                    string.IsNullOrWhiteSpace(_failure))
                    _failure = "provider-commit-rejected";
                else if (state == BrownFurCastTransactionState.Cancelled &&
                    string.IsNullOrWhiteSpace(_failure))
                    _failure = "provider-direct-cast-cancelled";
                else if (state == BrownFurCastTransactionState.Failed &&
                    string.IsNullOrWhiteSpace(_failure))
                    _failure = "provider-direct-cast-failed";
                else if (state == BrownFurCastTransactionState.Interrupted &&
                    string.IsNullOrWhiteSpace(_failure))
                    _failure = "provider-direct-cast-interrupted";
                _detail = "provider transaction terminal:" + _state;
            }
        }

        internal void MarkResidualFailure(string failure)
        {
            lock (_gate)
            {
                _complete = false;
                _residualState = true;
                _state = "cleanup-failed";
                _failure = failure ?? "provider-terminal-cleanup-failed";
                _detail = _failure;
            }
        }

        internal void MarkCleanupRecovered()
        {
            lock (_gate)
            {
                _complete = true;
                _residualState = false;
                _state = "cleanup-recovered";
                _detail = "provider terminal cleanup recovered";
            }
        }

        internal BrownFurDirectCastStatus Snapshot()
        {
            lock (_gate)
                return BrownFurDirectCastStatus.Create(_accepted,
                    _committed, _complete, _residualState, _state,
                    _failure, _detail, _transactionIdentity,
                    _reservoirCost);
        }
    }
}
