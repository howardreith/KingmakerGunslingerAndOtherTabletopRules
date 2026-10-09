using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Acquisition
{
    // Persisted native unit-part contract. Never rename this type or its JSON members.
    // No load hook grants anything or refreshes any container.
    public sealed class UnitPartCampaignWeaponRecovery : UnitPart
    {
        [JsonProperty] private int m_SchemaVersion = 1;
        [JsonProperty] private readonly List<CampaignWeaponRecoveryRecord> m_Records =
            new List<CampaignWeaponRecoveryRecord>();

        internal bool HasRecord(string itemGuid)
        {
            if (m_SchemaVersion != 1 || m_Records == null)
                throw new InvalidOperationException("Unsupported campaign weapon recovery ledger; refusing recovery.");
            return m_Records.Any(value => value == null || value.ItemGuid == itemGuid);
        }

        internal CampaignWeaponRecoveryRecord Reserve(string itemGuid, string targetGuid, int chapter)
        {
            if (HasRecord(itemGuid)) throw new InvalidOperationException("Weapon recovery is already recorded.");
            var record = new CampaignWeaponRecoveryRecord {
                ItemGuid = itemGuid, TargetGuid = targetGuid, Chapter = chapter,
                Status = "Reserved", InvokedUtcTicks = DateTime.UtcNow.Ticks };
            m_Records.Add(record);
            return record;
        }
        internal CampaignWeaponRecoveryRecord[] Records { get { return m_Records.ToArray(); } }
    }

    public sealed class CampaignWeaponRecoveryRecord
    {
        [JsonProperty] public string ItemGuid { get; internal set; }
        [JsonProperty] public string TargetGuid { get; internal set; }
        [JsonProperty] public int Chapter { get; internal set; }
        [JsonProperty] public string Status { get; internal set; }
        [JsonProperty] public long InvokedUtcTicks { get; internal set; }
        [JsonProperty] public int GrantedCount { get; internal set; }
    }
}
