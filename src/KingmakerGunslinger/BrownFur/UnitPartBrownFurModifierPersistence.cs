using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.BrownFur
{
    public sealed class UnitPartBrownFurModifierPersistence : UnitPart
    {
        [JsonProperty]
        private List<BrownFurPersistedModifierRecord> _records =
            new List<BrownFurPersistedModifierRecord>();

        internal int Count
        {
            get { lock (this) { EnsureValid(); return _records.Count; } }
        }

        internal bool Remember(BrownFurPersistedModifierRecord record)
        {
            BrownFurPersistedModifierPolicy.Validate(record);
            lock (this)
            {
                EnsureValid();
                var target = _records.Where(value =>
                    !BrownFurPersistedModifierPolicy.SameLogicalModifier(
                        value, record)).ToList();
                target.Add(record);
                Validate(target);
                _records = target;
                return true;
            }
        }

        internal int ResolveIncrease(BrownFurPersistedModifierProbe probe)
        {
            lock (this)
            {
                EnsureValid();
                return BrownFurPersistedModifierPolicy.ResolveIncrease(
                    _records, probe);
            }
        }

        internal BrownFurPersistedModifierRecord ResolveOrdinaryRecast(
            BrownFurOrdinaryRecastProbe probe)
        {
            lock (this)
            {
                EnsureValid();
                return BrownFurPersistedModifierPolicy.ResolveOrdinaryRecast(
                    _records, probe);
            }
        }

        internal int Forget(string buffGuid, string spellGuid,
            string casterId)
        {
            lock (this)
            {
                EnsureValid();
                int before = _records.Count;
                _records = _records.Where(value =>
                    !string.Equals(value.BuffGuid, buffGuid,
                        StringComparison.Ordinal) ||
                    !string.Equals(value.SpellGuid, spellGuid,
                        StringComparison.Ordinal) ||
                    !string.Equals(value.CasterId, casterId,
                        StringComparison.Ordinal)).ToList();
                return before - _records.Count;
            }
        }

        public override void PreSave()
        {
            lock (this) EnsureValid();
            base.PreSave();
        }

        public override void PostLoad()
        {
            base.PostLoad();
            lock (this) EnsureValid();
        }

        private void EnsureValid()
        {
            if (_records == null)
                _records = new List<BrownFurPersistedModifierRecord>();
            Validate(_records);
        }

        private static void Validate(
            IList<BrownFurPersistedModifierRecord> records)
        {
            for (int index = 0; index < records.Count; index++)
            {
                BrownFurPersistedModifierPolicy.Validate(records[index]);
                for (int other = index + 1; other < records.Count; other++)
                    if (BrownFurPersistedModifierPolicy.SameLogicalModifier(
                        records[index], records[other]))
                        throw new InvalidOperationException(
                            "The Powerful Change persistence carrier contains duplicate logical modifiers.");
            }
        }
    }
}
