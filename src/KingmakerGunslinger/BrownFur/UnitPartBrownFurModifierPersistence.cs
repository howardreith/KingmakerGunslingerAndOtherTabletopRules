using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Kingmaker.UnitLogic;
using KingmakerGunslinger.Bootstrap;
using Newtonsoft.Json;

namespace KingmakerGunslinger.BrownFur
{
    public sealed class UnitPartBrownFurModifierPersistence : UnitPart
    {
        // Released member (v0.0.95+), unchanged name and element shape. Read and
        // written by a member converter so that a record the typed reader could
        // not convert never throws out of the whole save file (RISK-03, A2).
        [JsonProperty]
        [JsonConverter(typeof(BrownFurPersistedModifierListConverter))]
        private List<BrownFurPersistedModifierRecord> _records =
            new List<BrownFurPersistedModifierRecord>();

        // RISK-03 (A2): entries that are not usable active records, kept with
        // their JSON content and never used to restore an increase. Null in
        // every save that never held one; older versions ignore the member.
        [JsonProperty]
        [JsonConverter(typeof(BrownFurPersistedModifierListConverter))]
        private List<BrownFurPersistedModifierRecord> _rejectedRecords;

        private string _reportedSignature;

        internal int Count
        {
            get { lock (this) return Read().Active.Count; }
        }

        internal int RetainedCount
        {
            get { lock (this) return Read().RetainedCount; }
        }

        internal bool Remember(BrownFurPersistedModifierRecord record)
        {
            // Strict for new gameplay writes: an invalid record throws here,
            // before any persisted value changes.
            BrownFurPersistedModifierPolicy.Validate(record);
            lock (this)
            {
                Write(BrownFurPersistedModifierStore.Remember(Read(), record));
                return true;
            }
        }

        internal int ResolveIncrease(BrownFurPersistedModifierProbe probe)
        {
            lock (this)
                return BrownFurPersistedModifierPolicy.ResolveIncrease(
                    Read().Active, probe);
        }

        internal BrownFurPersistedModifierRecord ResolveOrdinaryRecast(
            BrownFurOrdinaryRecastProbe probe)
        {
            lock (this)
                return BrownFurPersistedModifierPolicy.ResolveOrdinaryRecast(
                    Read().Active, probe);
        }

        internal int Forget(string buffGuid, string spellGuid,
            string casterId)
        {
            lock (this)
            {
                int removed;
                Write(BrownFurPersistedModifierStore.Forget(Read(), buffGuid,
                    spellGuid, casterId, out removed));
                return removed;
            }
        }

        public override void PreSave()
        {
            lock (this) Write(Read());
            base.PreSave();
        }

        public override void PostLoad()
        {
            base.PostLoad();
            lock (this) Write(Read());
        }

        private BrownFurPersistedModifierState Read()
        {
            BrownFurPersistedModifierState state =
                BrownFurPersistedModifierStore.Normalize(_records,
                    _rejectedRecords);
            Report(state);
            return state;
        }

        // Both values are computed by the store before either is assigned.
        private void Write(BrownFurPersistedModifierState state)
        {
            List<BrownFurPersistedModifierRecord> active = state.Active;
            List<BrownFurPersistedModifierRecord> retained = state.Retained;
            _records = active;
            _rejectedRecords = retained;
        }

        // The instance remembers only the last reported signature (active count,
        // retained count, newly rejected count). Unchanged repeated reads are
        // silent; a change warns again; each load creates a new instance.
        private void Report(BrownFurPersistedModifierState state)
        {
            int newly = state.NewlyRejected.Count;
            if (state.RetainedCount == 0 && newly == 0) return;
            string signature = state.Active.Count.ToString(CultureInfo.InvariantCulture) +
                "|" + state.RetainedCount.ToString(CultureInfo.InvariantCulture) +
                "|" + newly.ToString(CultureInfo.InvariantCulture);
            if (string.Equals(signature, _reportedSignature, StringComparison.Ordinal))
                return;
            _reportedSignature = signature;
            ModContext context;
            if (!ModContext.TryGet(out context)) return;
            var detail = new StringBuilder();
            foreach (KeyValuePair<string, BrownFurPersistedModifierRecord> entry in
                state.NewlyRejected.Take(4))
                detail.Append(";entry=").Append(entry.Key).Append(',').Append(
                    Describe(BrownFurPersistedModifierJson.Text(entry.Value)));
            if (newly > 4) detail.Append(";moreEntries=").Append(
                (newly - 4).ToString(CultureInfo.InvariantCulture));
            BrownFurDiagnostics.Warning(context,
                "powerful-change.persistence.retained", string.Format(
                    CultureInfo.InvariantCulture,
                    "unit={0};uniqueId={1};active={2};retained={3};newlyRejected={4}{5};" +
                    "Retained Powerful Change records are kept in the save and restore no increase.",
                    Owner == null ? "unknown" : Owner.CharacterName,
                    Owner == null || Owner.Unit == null ? "unknown" : Owner.Unit.UniqueId,
                    state.Active.Count, state.RetainedCount, newly, detail));
        }

        private static string Describe(string text)
        {
            var preview = new StringBuilder();
            foreach (char value in text.Take(96))
                preview.Append(value < ' ' || value > '~' ? '?' : value);
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                return "length=" + text.Length.ToString(CultureInfo.InvariantCulture) +
                    ",sha256=" + string.Concat(hash.Take(8).Select(value =>
                        value.ToString("x2", CultureInfo.InvariantCulture)).ToArray()) +
                    ",preview=\"" + preview + (text.Length > 96 ? "...\"" : "\"");
            }
        }
    }
}
