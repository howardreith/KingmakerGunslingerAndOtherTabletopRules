using System.Collections.Generic;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.Acquisition.BetterVendors
{
    /// <summary>
    /// Save-local ledger of authorized progression entries whose one-time
    /// progression grant has already been applied in this campaign. It lives on
    /// the main character, is serialized with the save by the game's native
    /// unit-part persistence, and therefore loads, reloads and switches with
    /// its own campaign. It is independent of Better Vendors' own save flag,
    /// which it never reads, writes or repurposes. Disabling the integration or
    /// a content module leaves it untouched.
    ///
    /// This type name and its JSON members are a persisted save contract and
    /// must never be renamed.
    /// </summary>
    public sealed class UnitPartBetterVendorsProgressionGrants : UnitPart
    {
        internal const int CurrentSchemaVersion = 1;

        [JsonProperty]
        private int m_SchemaVersion = CurrentSchemaVersion;

        [JsonProperty]
        private readonly List<string> m_GrantedEntries = new List<string>();

        internal int SchemaVersion { get { return m_SchemaVersion; } }

        internal ProgressionGrantLedger Ledger
        {
            get { return new ProgressionGrantLedger(m_GrantedEntries); }
        }
    }
}
