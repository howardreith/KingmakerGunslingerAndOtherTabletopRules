using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace KingmakerGunslinger.BrownFur
{
    public sealed class BrownFurPersistedModifierRecord
    {
        internal const int CurrentSchemaVersion = 1;

        [JsonProperty] public int SchemaVersion { get; set; } =
            CurrentSchemaVersion;
        [JsonProperty] public string BuffGuid { get; set; }
        [JsonProperty] public string SpellGuid { get; set; }
        [JsonProperty] public string CasterId { get; set; }
        [JsonProperty] internal BrownFurAbilityScore AbilityScore { get; set; }
        [JsonProperty] public int Increase { get; set; }
        [JsonProperty] public int OriginalValue { get; set; }
        [JsonProperty] public string OriginalDescriptor { get; set; }
        [JsonProperty] public string CarrierFamily { get; set; }
        [JsonProperty] public long EndTimeTicks { get; set; }

        public BrownFurPersistedModifierRecord() { }

        internal BrownFurPersistedModifierRecord(string buffGuid,
            string spellGuid, string casterId,
            BrownFurAbilityScore abilityScore, int increase,
            int originalValue, string originalDescriptor,
            string carrierFamily, long endTimeTicks)
        {
            BuffGuid = buffGuid;
            SpellGuid = spellGuid;
            CasterId = casterId;
            AbilityScore = abilityScore;
            Increase = increase;
            OriginalValue = originalValue;
            OriginalDescriptor = originalDescriptor;
            CarrierFamily = carrierFamily;
            EndTimeTicks = endTimeTicks;
        }
    }

    internal sealed class BrownFurPersistedModifierProbe
    {
        internal string BuffGuid { get; set; }
        internal string SpellGuid { get; set; }
        internal string CasterId { get; set; }
        internal BrownFurAbilityScore AbilityScore { get; set; }
        internal int OriginalValue { get; set; }
        internal string OriginalDescriptor { get; set; }
        internal string CarrierFamily { get; set; }
        internal long EndTimeTicks { get; set; }
    }

    internal static class BrownFurPersistedModifierPolicy
    {
        internal static void Validate(BrownFurPersistedModifierRecord record)
        {
            if (record == null || record.SchemaVersion !=
                BrownFurPersistedModifierRecord.CurrentSchemaVersion)
                throw new NotSupportedException(
                    "The Powerful Change persistence record is null or uses an unsupported schema.");
            RequireGuid(record.BuffGuid, "buff");
            RequireGuid(record.SpellGuid, "spell");
            if (string.IsNullOrWhiteSpace(record.CasterId))
                throw new InvalidOperationException(
                    "The Powerful Change persistence record has no caster identity.");
            if (record.AbilityScore == BrownFurAbilityScore.None ||
                (record.Increase != 2 && record.Increase != 4) ||
                record.OriginalValue <= 0 ||
                string.IsNullOrWhiteSpace(record.OriginalDescriptor) ||
                !BrownFurModifierAdjustmentPolicy.IsSupportedCarrier(
                    record.CarrierFamily) || record.EndTimeTicks < 0)
                throw new InvalidOperationException(
                    "The Powerful Change persistence record is structurally invalid.");
        }

        internal static bool Matches(BrownFurPersistedModifierRecord record,
            BrownFurPersistedModifierProbe probe)
        {
            Validate(record);
            if (probe == null) return false;
            return string.Equals(record.BuffGuid, probe.BuffGuid,
                       StringComparison.Ordinal) &&
                string.Equals(record.SpellGuid, probe.SpellGuid,
                    StringComparison.Ordinal) &&
                string.Equals(record.CasterId, probe.CasterId,
                    StringComparison.Ordinal) &&
                record.AbilityScore == probe.AbilityScore &&
                record.OriginalValue == probe.OriginalValue &&
                string.Equals(record.OriginalDescriptor,
                    probe.OriginalDescriptor, StringComparison.Ordinal) &&
                string.Equals(record.CarrierFamily, probe.CarrierFamily,
                    StringComparison.Ordinal) &&
                record.EndTimeTicks == probe.EndTimeTicks;
        }

        internal static bool SameLogicalModifier(
            BrownFurPersistedModifierRecord left,
            BrownFurPersistedModifierRecord right)
        {
            Validate(left);
            Validate(right);
            return string.Equals(left.BuffGuid, right.BuffGuid,
                       StringComparison.Ordinal) &&
                string.Equals(left.SpellGuid, right.SpellGuid,
                    StringComparison.Ordinal) &&
                string.Equals(left.CasterId, right.CasterId,
                    StringComparison.Ordinal) &&
                left.AbilityScore == right.AbilityScore &&
                string.Equals(left.CarrierFamily, right.CarrierFamily,
                    StringComparison.Ordinal);
        }

        internal static int ResolveIncrease(
            IEnumerable<BrownFurPersistedModifierRecord> records,
            BrownFurPersistedModifierProbe probe)
        {
            if (records == null || probe == null) return 0;
            BrownFurPersistedModifierRecord[] matches = records.Where(value =>
                Matches(value, probe)).Take(2).ToArray();
            return matches.Length == 1 ? matches[0].Increase : 0;
        }

        private static void RequireGuid(string value, string role)
        {
            Guid parsed;
            if (string.IsNullOrWhiteSpace(value) || value.Length != 32 ||
                !Guid.TryParseExact(value, "N", out parsed) ||
                !string.Equals(value, value.ToLowerInvariant(),
                    StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "The Powerful Change persistence " + role +
                    " identity is invalid.");
        }
    }
}
