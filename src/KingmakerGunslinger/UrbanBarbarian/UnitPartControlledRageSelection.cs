using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.UrbanBarbarian
{
    public sealed class UnitPartControlledRageSelection : UnitPart
    {
        // Released field (v0.0.95+): the active tier selections in the
        // released grammar. Name and type are unchanged.
        [JsonProperty]
        private string _state;

        // RISK-03: persisted inputs that failed validation, each kept verbatim
        // and never interpreted as a grant. Absent (null) in every save that
        // never held rejected input. Older versions ignore the member.
        [JsonProperty]
        private List<string> _rejectedStates;

        private string _reportedSignature;

        internal ControlledRageAllocation SelectionFor(ControlledRageTier tier)
        {
            return Read().SelectionFor(tier);
        }

        internal bool IsBlocked(ControlledRageTier tier)
        {
            return Read().IsBlocked(tier);
        }

        internal void Unlock(ControlledRageTier tier)
        {
            ControlledRageSelectionState state = Read();
            state.Unlock(tier);
            Write(state);
        }

        internal bool TrySelect(ControlledRageTier tier,
            ControlledRageAllocation allocation, bool rageActive)
        {
            ControlledRageSelectionState state = Read();
            bool selected = state.TrySelectExact(tier, allocation, rageActive);
            if (selected) Write(state);
            return selected;
        }

        internal object CaptureSnapshot()
        {
            return new Snapshot(_state, _rejectedStates);
        }

        internal void RestoreSnapshot(object snapshot)
        {
            var value = snapshot as Snapshot;
            if (value == null)
                throw new ArgumentException(
                    "A Controlled Rage persistence snapshot is required.",
                    "snapshot");
            _state = value.State;
            _rejectedStates = value.Rejected == null ? null :
                new List<string>(value.Rejected);
        }

        public override void PreSave()
        {
            Write(Read());
            base.PreSave();
        }

        public override void PostLoad()
        {
            base.PostLoad();
            Write(Read());
        }

        private ControlledRageSelectionState Read()
        {
            ControlledRageSelectionState state =
                ControlledRageSelectionState.Restore(_state, _rejectedStates);
            Report(state);
            return state;
        }

        // Every value is computed before either field is assigned, so a
        // failure leaves the previous persisted state untouched.
        private void Write(ControlledRageSelectionState state)
        {
            string serialized = state.Serialize();
            List<string> rejected = state.MergeRetained(_rejectedStates);
            _state = serialized;
            _rejectedStates = rejected;
        }

        // The part instance remembers only the last reported condition
        // signature (blocked tiers and retained-payload count). Repeated reads
        // in an unchanged condition are silent; each transition to a different
        // signature (for example a recovery, then a new rejection) warns again,
        // and each load creates a new instance that reports its first read.
        private void Report(ControlledRageSelectionState state)
        {
            string blocked = string.Join(",", state.BlockedTiers.Select(value =>
                ((int)value).ToString(CultureInfo.InvariantCulture)).ToArray());
            List<string> merged = state.MergeRetained(_rejectedStates);
            int retained = merged == null ? 0 : merged.Count;
            if (retained == 0 && blocked.Length == 0)
                return;
            // The count includes a pending payload, so moving it into the
            // retained list on the following write does not re-report.
            string signature = blocked + "|" +
                retained.ToString(CultureInfo.InvariantCulture);
            if (string.Equals(signature, _reportedSignature,
                    StringComparison.Ordinal))
                return;
            _reportedSignature = signature;
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            string active = string.Join(",", state.ActiveTiers.Select(value =>
                ((int)value).ToString(CultureInfo.InvariantCulture)).ToArray());
            context.Logger.Warning("urban-barbarian",
                "controlled-rage.persistence.retained", string.Format(
                    CultureInfo.InvariantCulture,
                    "unit={0};uniqueId={1};activeTiers={2};blockedTiers={3};retainedPayloads={4};newlyRejected={5};" +
                    "Rejected Controlled Rage data is kept verbatim in the save and grants nothing. A blocked tier has no allocation " +
                    "until the player chooses one with that tier's selector.",
                    Owner == null ? "unknown" : Owner.CharacterName,
                    Owner == null || Owner.Unit == null ? "unknown" :
                        Owner.Unit.UniqueId,
                    active.Length == 0 ? "none" : active,
                    blocked.Length == 0 ? "none" : blocked, retained,
                    state.PendingRejected == null ? "none" : Describe(
                        state.PendingRejected)));
        }

        private static string Describe(string payload)
        {
            var preview = new StringBuilder();
            foreach (char value in payload.Take(96))
                preview.Append(value < ' ' || value > '~' ? '?' : value);
            return "length=" + payload.Length.ToString(
                CultureInfo.InvariantCulture) + ",sha256=" +
                Fingerprint(payload) + ",preview=\"" + preview +
                (payload.Length > 96 ? "...\"" : "\"");
        }

        private static string Fingerprint(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload));
                return string.Concat(hash.Take(8).Select(value =>
                    value.ToString("x2", CultureInfo.InvariantCulture))
                    .ToArray());
            }
        }

        private sealed class Snapshot
        {
            internal Snapshot(string state, List<string> rejected)
            {
                State = state;
                Rejected = rejected == null ? null : rejected.ToArray();
            }

            internal string State { get; private set; }
            internal string[] Rejected { get; private set; }
        }
    }

    public sealed class ControlledRageSelectionController :
        OwnedGameLogicComponent<UnitDescriptor>
    {
        public int Tier;

        public override void OnTurnOn()
        {
            ControlledRageRuntime.UnlockTier(Owner, (ControlledRageTier)Tier);
        }

        public override void OnTurnOff()
        {
            ControlledRageRuntime.RemoveTier(Owner,
                (ControlledRageTier)Tier);
        }
    }
}
