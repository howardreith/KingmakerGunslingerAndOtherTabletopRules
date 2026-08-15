using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace KingmakerGunslinger.BrownFur
{
    internal enum BrownFurBonusAdapterPlanStatus
    {
        Ineligible = 0,
        Supported = 1,
        Blocked = 2
    }

    internal sealed class BrownFurBonusAdapterPlan
    {
        internal BrownFurBonusAdapterPlan(BrownFurBonusAdapterPlanStatus status,
            string failure, IEnumerable<BrownFurAbilityScore> scores,
            IEnumerable<string> buffGuids, IEnumerable<string> carrierFamilies)
        {
            Status = status;
            Failure = failure ?? string.Empty;
            AbilityScores = (scores ?? Enumerable.Empty<BrownFurAbilityScore>())
                .Distinct().OrderBy(value => (int)value).ToArray();
            AppliedBuffGuids = (buffGuids ?? Enumerable.Empty<string>())
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            CarrierFamilies = (carrierFamilies ?? Enumerable.Empty<string>())
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
        }

        internal BrownFurBonusAdapterPlanStatus Status { get; private set; }
        internal string Failure { get; private set; }
        internal BrownFurAbilityScore[] AbilityScores { get; private set; }
        internal string[] AppliedBuffGuids { get; private set; }
        internal string[] CarrierFamilies { get; private set; }
        internal bool Supports(BrownFurAbilityScore score)
        { return Status == BrownFurBonusAdapterPlanStatus.Supported &&
            AbilityScores.Contains(score); }
    }

    internal static class BrownFurBonusAdapterPlanPolicy
    {
        private static readonly HashSet<string> DirectFamilies =
            new HashSet<string>(StringComparer.Ordinal) {
                "AddStatBonus", "AddContextStatBonus", "AddGenericStatBonus",
                "AddStatBonusAbilityValue"
            };

        internal static BrownFurBonusAdapterPlan Create(
            IEnumerable<string> abilityBonusCarriers,
            IEnumerable<string> appliedBuffs)
        {
            string[] carriers = (abilityBonusCarriers ??
                Enumerable.Empty<string>()).Where(value =>
                    !string.IsNullOrWhiteSpace(value)).ToArray();
            if (carriers.Length == 0) return Plan(
                BrownFurBonusAdapterPlanStatus.Ineligible,
                "no-positive-ability-bonus-carrier", null, null, null);

            var scores = new HashSet<BrownFurAbilityScore>();
            var families = new HashSet<string>(StringComparer.Ordinal);
            foreach (string carrier in carriers)
            {
                string family;
                Dictionary<string, string> fields;
                if (!TryParseCarrier(carrier, out family, out fields))
                    return Block("bonus-carrier-malformed", scores, null,
                        families);
                families.Add(family);
                if (family == "ChangeUnitSize") continue;
                if (DirectFamilies.Contains(family))
                {
                    BrownFurAbilityScore stat;
                    int value;
                    if (!fields.ContainsKey("Stat") ||
                        !TryScore(fields["Stat"], out stat) ||
                        !fields.ContainsKey("Value") ||
                        !int.TryParse(fields["Value"], NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out value))
                        return Block("bonus-carrier-fields-invalid", scores,
                            null, families);
                    if (value > 0) scores.Add(stat);
                    continue;
                }
                if (family == "Polymorph")
                {
                    if (!TryAddPositive(fields, "StrengthBonus",
                            BrownFurAbilityScore.Strength, scores) ||
                        !TryAddPositive(fields, "DexterityBonus",
                            BrownFurAbilityScore.Dexterity, scores) ||
                        !TryAddPositive(fields, "ConstitutionBonus",
                            BrownFurAbilityScore.Constitution, scores))
                        return Block("bonus-carrier-fields-invalid", scores,
                            null, families);
                    continue;
                }
                return Block("bonus-carrier-unsupported", scores, null,
                    families);
            }
            if (scores.Count == 0)
                return Block("bonus-carrier-no-positive-stat", scores, null,
                    families);

            var buffs = new HashSet<string>(StringComparer.Ordinal);
            foreach (string applied in appliedBuffs ?? Enumerable.Empty<string>())
            {
                string guid;
                if (!TryAppliedBuffGuid(applied, out guid))
                    return Block("bonus-applied-buff-malformed", scores, buffs,
                        families);
                buffs.Add(guid);
            }
            if (buffs.Count == 0)
                return Block("bonus-applied-buff-missing", scores, buffs,
                    families);
            return Plan(BrownFurBonusAdapterPlanStatus.Supported, string.Empty,
                scores, buffs, families);
        }

        private static bool TryAddPositive(IDictionary<string, string> fields,
            string name, BrownFurAbilityScore score,
            ISet<BrownFurAbilityScore> scores)
        {
            string text;
            int value;
            if (!fields.TryGetValue(name, out text)) return true;
            if (!int.TryParse(text, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out value)) return false;
            if (value > 0) scores.Add(score);
            return true;
        }

        private static bool TryParseCarrier(string value, out string family,
            out Dictionary<string, string> fields)
        {
            family = string.Empty;
            fields = new Dictionary<string, string>(StringComparer.Ordinal);
            int equals = value.IndexOf('=');
            int open = value.IndexOf('{', equals + 1);
            int close = value.LastIndexOf('}');
            if (equals <= 0 || open <= equals + 1 || close <= open ||
                close != value.Length - 1) return false;
            string type = value.Substring(equals + 1, open - equals - 1);
            int dot = type.LastIndexOf('.');
            family = dot < 0 ? type : type.Substring(dot + 1);
            if (string.IsNullOrWhiteSpace(family)) return false;
            string body = value.Substring(open + 1, close - open - 1);
            if (body.Length == 0) return family == "ChangeUnitSize";
            foreach (string pair in body.Split(','))
            {
                int separator = pair.IndexOf('=');
                if (separator <= 0 || separator == pair.Length - 1) return false;
                string name = pair.Substring(0, separator);
                string text = pair.Substring(separator + 1);
                if (fields.ContainsKey(name)) return false;
                fields.Add(name, text);
            }
            return true;
        }

        private static bool TryAppliedBuffGuid(string value, out string guid)
        {
            guid = string.Empty;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            int equals = value.IndexOf('=');
            int slash = value.IndexOf('/', equals + 1);
            if (equals <= 0 || slash - equals != 33) return false;
            string candidate = value.Substring(equals + 1, 32);
            Guid parsed;
            if (!Guid.TryParseExact(candidate, "N", out parsed) ||
                candidate.Any(character => character >= 'A' && character <= 'F'))
                return false;
            guid = candidate;
            return true;
        }

        private static bool TryScore(string value,
            out BrownFurAbilityScore score)
        {
            return Enum.TryParse(value, false, out score) &&
                score != BrownFurAbilityScore.None;
        }

        private static BrownFurBonusAdapterPlan Block(string failure,
            IEnumerable<BrownFurAbilityScore> scores,
            IEnumerable<string> buffs, IEnumerable<string> families)
        { return Plan(BrownFurBonusAdapterPlanStatus.Blocked, failure, scores,
            buffs, families); }

        private static BrownFurBonusAdapterPlan Plan(
            BrownFurBonusAdapterPlanStatus status, string failure,
            IEnumerable<BrownFurAbilityScore> scores,
            IEnumerable<string> buffs, IEnumerable<string> families)
        { return new BrownFurBonusAdapterPlan(status, failure, scores, buffs,
            families); }
    }
}
