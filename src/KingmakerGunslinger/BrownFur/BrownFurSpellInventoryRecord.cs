using System.Collections.Generic;
using Newtonsoft.Json;

namespace KingmakerGunslinger.BrownFur
{
    internal sealed class BrownFurSpellInventoryRecord
    {
        [JsonProperty("canonicalSpellGuid", Order = 1)] public string CanonicalSpellGuid { get; set; }
        [JsonProperty("blueprintName", Order = 2)] public string BlueprintName { get; set; }
        [JsonProperty("localizedName", Order = 3)] public string LocalizedName { get; set; }
        [JsonProperty("parentGuid", Order = 4)] public string ParentGuid { get; set; }
        [JsonProperty("variantGuids", Order = 5)] public List<string> VariantGuids { get; set; }
        [JsonProperty("convertedFrom", Order = 6)] public string ConvertedFrom { get; set; }
        [JsonProperty("spellLevels", Order = 7)] public List<int> SpellLevels { get; set; }
        [JsonProperty("spellbookSourceGuid", Order = 8)] public string SpellbookSourceGuid { get; set; }
        [JsonProperty("range", Order = 9)] public string Range { get; set; }
        [JsonProperty("targetAnchor", Order = 10)] public string TargetAnchor { get; set; }
        [JsonProperty("targetRestrictions", Order = 11)] public List<string> TargetRestrictions { get; set; }
        [JsonProperty("duration", Order = 12)] public string Duration { get; set; }
        [JsonProperty("metamagicSupport", Order = 13)] public string MetamagicSupport { get; set; }
        [JsonProperty("supportsExtend", Order = 14)] public bool SupportsExtend { get; set; }
        [JsonProperty("appliedBuffs", Order = 15)] public List<string> AppliedBuffs { get; set; }
        [JsonProperty("nestedActionGraph", Order = 16)] public List<string> NestedActionGraph { get; set; }
        [JsonProperty("abilityScoreBonuses", Order = 17)] public List<string> AbilityScoreBonuses { get; set; }
        [JsonProperty("abilityBonusCarrierFamilies", Order = 18)] public List<string> AbilityBonusCarrierFamilies { get; set; }
        [JsonProperty("modifierDescriptors", Order = 19)] public List<string> ModifierDescriptors { get; set; }
        [JsonProperty("valuePatterns", Order = 20)] public List<string> ValuePatterns { get; set; }
        [JsonProperty("polymorphAndSizeComponents", Order = 21)] public List<string> PolymorphAndSizeComponents { get; set; }
        [JsonProperty("hardCodedToCaster", Order = 22)] public List<string> HardCodedToCaster { get; set; }
        [JsonProperty("saveAndDispel", Order = 23)] public string SaveAndDispel { get; set; }
        [JsonProperty("shareTransmutationCompatibility", Order = 24)] public string ShareTransmutationCompatibility { get; set; }
        [JsonProperty("powerfulChangeCompatibility", Order = 25)] public string PowerfulChangeCompatibility { get; set; }
        [JsonProperty("transmutationSupremacyCompatibility", Order = 26)] public string TransmutationSupremacyCompatibility { get; set; }
        [JsonProperty("requiredAdapter", Order = 27)] public string RequiredAdapter { get; set; }
        [JsonProperty("qualificationStatus", Order = 28)] public string QualificationStatus { get; set; }
    }

    internal sealed class BrownFurSpellInventoryEvidence
    {
        [JsonProperty("schemaVersion", Order = 1)] public int SchemaVersion { get; set; }
        [JsonProperty("generatedAtUtc", Order = 2)] public string GeneratedAtUtc { get; set; }
        [JsonProperty("cotwFingerprint", Order = 3)] public string CotwFingerprint { get; set; }
        [JsonProperty("rootSpellCount", Order = 4)] public int RootSpellCount { get; set; }
        [JsonProperty("recordCountIncludingVariants", Order = 5)] public int RecordCountIncludingVariants { get; set; }
        [JsonProperty("personalSpellCount", Order = 6)] public int PersonalSpellCount { get; set; }
        [JsonProperty("abilityBonusCandidateCount", Order = 7)] public int AbilityBonusCandidateCount { get; set; }
        [JsonProperty("hardCodedToCasterCount", Order = 8)] public int HardCodedToCasterCount { get; set; }
        [JsonProperty("qualificationCounts", Order = 9)] public Dictionary<string, int> QualificationCounts { get; set; }
        [JsonProperty("records", Order = 10)] public List<BrownFurSpellInventoryRecord> Records { get; set; }
    }
}
