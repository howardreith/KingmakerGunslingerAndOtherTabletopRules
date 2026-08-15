using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class CotwCompatibilityFingerprint
    {
        internal string AssemblyFullName { get; set; }
        internal string FileVersion { get; set; }
        internal string DllSha256 { get; set; }
        internal string DllMvid { get; set; }
        internal string ModVersion { get; set; }
        internal string SettingsSha256 { get; set; }
        internal string BalanceFixesSetting { get; set; }
        internal string ArcanistClassGuid { get; set; }
        internal string ProgressionGuid { get; set; }
        internal string CastingSpellbookGuid { get; set; }
        internal string MemorizationSpellbookGuid { get; set; }
        internal string ReservoirGuid { get; set; }
        internal string ExploitSelectionGuid { get; set; }
        internal string MagicalSupremacyGuid { get; set; }
        internal IList<int> ExploitLevels { get; set; }
        internal IList<string> SharedSpellsSignatures { get; set; }
        internal int TransmutationSpellCount { get; set; }
        internal int PersonalTransmutationSpellCount { get; set; }
        internal int AbilityBonusTransmutationSpellCount { get; set; }
        internal int SupportedComponentPatternCount { get; set; }
        internal int UnsupportedComponentPatternCount { get; set; }
        internal string PublicationStatus { get; set; }

        public override string ToString()
        {
            return "assembly=" + Value(AssemblyFullName) +
                ";fileVersion=" + Value(FileVersion) +
                ";sha256=" + Value(DllSha256) +
                ";mvid=" + Value(DllMvid) +
                ";modVersion=" + Value(ModVersion) +
                ";settingsSha256=" + Value(SettingsSha256) +
                ";balanceFixes=" + Value(BalanceFixesSetting) +
                ";class=" + Value(ArcanistClassGuid) +
                ";progression=" + Value(ProgressionGuid) +
                ";castingSpellbook=" + Value(CastingSpellbookGuid) +
                ";memorizationSpellbook=" + Value(MemorizationSpellbookGuid) +
                ";reservoir=" + Value(ReservoirGuid) +
                ";exploitSelection=" + Value(ExploitSelectionGuid) +
                ";magicalSupremacy=" + Value(MagicalSupremacyGuid) +
                ";exploitLevels=" + Join(ExploitLevels) +
                ";sharedSpells=" + Join(SharedSpellsSignatures) +
                ";transmutationSpells=" + TransmutationSpellCount +
                ";personalTransmutationSpells=" + PersonalTransmutationSpellCount +
                ";abilityBonusTransmutationSpells=" + AbilityBonusTransmutationSpellCount +
                ";supportedPatterns=" + SupportedComponentPatternCount +
                ";unsupportedPatterns=" + UnsupportedComponentPatternCount +
                ";publication=" + Value(PublicationStatus);
        }

        private static string Value(string value)
        { return string.IsNullOrWhiteSpace(value) ? "<unavailable>" : value; }

        private static string Join<T>(IEnumerable<T> values)
        { return values == null ? "<unavailable>" : string.Join(",",
            values.Select(value => Convert.ToString(value,
                System.Globalization.CultureInfo.InvariantCulture)).ToArray()); }
    }
}
