using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BrownFur
{
    internal static class BrownFurInventoryQualifications
    {
        internal const string Generic = "Supported by generic contract";
        internal const string Named = "Supported by named adapter";
        internal const string Ineligible = "Intentionally ineligible";
        internal const string Blocked =
            "Blocked by an understood engine limitation";
        internal const string Unexplained = "Unexplained";
    }

    internal sealed class BrownFurInventoryClassificationInput
    {
        private readonly string[] _carrierFamilies;

        internal BrownFurInventoryClassificationInput(string spellGuid,
            string range, bool hasVariants, bool supportsExtend,
            string duration, int positiveAbilityBonusCount,
            IEnumerable<string> carrierFamilies, int hardCodedToCasterCount)
        {
            SpellGuid = spellGuid ?? string.Empty;
            Range = range ?? string.Empty;
            HasVariants = hasVariants;
            SupportsExtend = supportsExtend;
            Duration = duration ?? string.Empty;
            PositiveAbilityBonusCount = positiveAbilityBonusCount;
            _carrierFamilies = (carrierFamilies ?? new string[0])
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            HardCodedToCasterCount = hardCodedToCasterCount;
        }

        internal string SpellGuid { get; private set; }
        internal string Range { get; private set; }
        internal bool HasVariants { get; private set; }
        internal bool SupportsExtend { get; private set; }
        internal string Duration { get; private set; }
        internal int PositiveAbilityBonusCount { get; private set; }
        internal IReadOnlyList<string> CarrierFamilies { get { return
            Array.AsReadOnly((string[])_carrierFamilies.Clone()); } }
        internal int HardCodedToCasterCount { get; private set; }
    }

    internal sealed class BrownFurInventoryClassificationDecision
    {
        internal BrownFurInventoryClassificationDecision(string share,
            string powerful, string supremacy, string adapter, string status)
        {
            ShareTransmutation = share;
            PowerfulChange = powerful;
            TransmutationSupremacy = supremacy;
            RequiredAdapter = adapter;
            QualificationStatus = status;
        }

        internal string ShareTransmutation { get; private set; }
        internal string PowerfulChange { get; private set; }
        internal string TransmutationSupremacy { get; private set; }
        internal string RequiredAdapter { get; private set; }
        internal string QualificationStatus { get; private set; }
        internal bool IsExplained { get { return QualificationStatus !=
            BrownFurInventoryQualifications.Unexplained; } }
    }

    internal static class BrownFurInventoryClassificationPolicy
    {
        private static readonly HashSet<string> SupportedBonusCarriers =
            new HashSet<string>(new[] {
                "Kingmaker.Designers.Mechanics.Buffs.AddStatBonusAbilityValue",
                "Kingmaker.Designers.Mechanics.Buffs.ChangeUnitSize",
                "Kingmaker.UnitLogic.Buffs.Components.AddGenericStatBonus",
                "Kingmaker.UnitLogic.Buffs.Polymorph",
                "Kingmaker.UnitLogic.FactLogic.AddContextStatBonus",
                "Kingmaker.UnitLogic.FactLogic.AddStatBonus"
            }, StringComparer.Ordinal);

        private static readonly HashSet<string> NamedSupremacyAdapters =
            new HashSet<string>(new[] {
                "91266b6d2a4c4fd6b8e1549bc2381d12",
                "df7d13c967bce6a40bec3ba7c9f0e64c",
                "e48638596c955a74c8a32dbc90b518c1"
            }, StringComparer.Ordinal);

        private static readonly HashSet<string> ProvenNativeSupremacyPaths =
            new HashSet<string>(new[] {
                "3e4a0790fc2749bbacb1b3b1d2401148",
                "c7b52e9a09ef442f9308d9119f5877d2"
            }, StringComparer.Ordinal);

        private static readonly HashSet<string> ProvenSupremacyNoOps =
            new HashSet<string>(new[] {
                "16e23c7a8ae53cc42a93066d19766404",
                "3105d6e9febdc3f41a08d2b7dda1fe74",
                "4aa7942c3e62a164387a73184bca3fc1",
                "d752e84d9708495a93ab1237bd9c1dff",
                "e243740dfdb17a246b116b334ed0b165"
            }, StringComparer.Ordinal);

        internal static BrownFurInventoryClassificationDecision Decide(
            BrownFurInventoryClassificationInput input)
        {
            if (input == null || !ValidGuid(input.SpellGuid) ||
                input.PositiveAbilityBonusCount < 0 ||
                input.HardCodedToCasterCount < 0)
                return Unexplained("invalid inventory classification input");

            string share;
            string shareAdapter;
            if (input.Range != "Personal")
            {
                share = BrownFurInventoryQualifications.Ineligible +
                    ": original range is not Personal";
                shareAdapter = "share=none";
            }
            else if (input.HardCodedToCasterCount != 0)
            {
                return Unexplained("Personal spell contains hard-coded ToCaster routing");
            }
            else if (input.HasVariants)
            {
                share = BrownFurInventoryQualifications.Generic +
                    ": selected-variant canonicalization plus execution-scoped willing-creature targeting";
                shareAdapter = "share=selected-variant-targeting";
            }
            else
            {
                share = BrownFurInventoryQualifications.Generic +
                    ": execution-scoped willing-creature targeting";
                shareAdapter = "share=execution-scoped-targeting";
            }

            string powerful;
            string powerfulAdapter;
            if (input.PositiveAbilityBonusCount == 0)
            {
                if (input.CarrierFamilies.Count != 0)
                    return Unexplained(
                        "carrier families exist without a positive ability bonus");
                powerful = BrownFurInventoryQualifications.Ineligible +
                    ": no detected positive ability-score bonus carrier";
                powerfulAdapter = "powerful=none";
            }
            else
            {
                if (input.CarrierFamilies.Count == 0)
                    return Unexplained(
                        "positive ability bonus has no carrier-family identity");
                string unknown = input.CarrierFamilies.FirstOrDefault(value =>
                    !SupportedBonusCarriers.Contains(value));
                if (unknown != null)
                    return Unexplained("unsupported positive bonus carrier " + unknown);
                powerful = BrownFurInventoryQualifications.Generic +
                    ": descriptor-preserving modifier-registration adjustment";
                powerfulAdapter = "powerful=" + string.Join("+",
                    input.CarrierFamilies.Select(ShortName).ToArray());
            }

            string supremacy;
            string supremacyAdapter;
            bool named = false;
            if (input.SupportsExtend)
            {
                supremacy = BrownFurInventoryQualifications.Generic +
                    ": execution-scoped native Extend with non-stacking guard";
                supremacyAdapter = "supremacy=native-extend";
            }
            else if (ProvenNativeSupremacyPaths.Contains(input.SpellGuid))
            {
                supremacy = BrownFurInventoryQualifications.Generic +
                    ": installed CotW hidden-duration path honors execution-scoped Extend";
                supremacyAdapter = "supremacy=cotw-native-hidden-duration";
            }
            else if (NamedSupremacyAdapters.Contains(input.SpellGuid))
            {
                named = true;
                supremacy = BrownFurInventoryQualifications.Named +
                    ": exact installed fixed or hidden duration path";
                supremacyAdapter = "supremacy=named-duration:" +
                    input.SpellGuid;
            }
            else if (ProvenSupremacyNoOps.Contains(input.SpellGuid))
            {
                supremacy = BrownFurInventoryQualifications.Generic +
                    ": proven instantaneous, permanent, or selector no-op";
                supremacyAdapter = "supremacy=proven-no-op";
            }
            else
            {
                return Unexplained("non-Extend duration structure is unqualified");
            }

            return new BrownFurInventoryClassificationDecision(share,
                powerful, supremacy, string.Join(";", new[] { shareAdapter,
                    powerfulAdapter, supremacyAdapter }), named ?
                    BrownFurInventoryQualifications.Named :
                    BrownFurInventoryQualifications.Generic);
        }

        private static BrownFurInventoryClassificationDecision Unexplained(
            string reason)
        {
            string value = BrownFurInventoryQualifications.Unexplained +
                ": " + reason;
            return new BrownFurInventoryClassificationDecision(value, value,
                value, "none;" + reason,
                BrownFurInventoryQualifications.Unexplained);
        }

        private static bool ValidGuid(string value)
        {
            return value != null && value.Length == 32 && value.All(character =>
                (character >= '0' && character <= '9') ||
                (character >= 'a' && character <= 'f'));
        }

        private static string ShortName(string value)
        {
            int separator = value.LastIndexOf('.');
            return separator < 0 ? value : value.Substring(separator + 1);
        }
    }
}
