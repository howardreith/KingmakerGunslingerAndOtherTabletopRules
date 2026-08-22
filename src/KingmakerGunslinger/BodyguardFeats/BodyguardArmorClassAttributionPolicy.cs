using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal sealed class BodyguardArmorClassContribution
    {
        internal BodyguardArmorClassContribution(string protectorId, int bonus)
        {
            if (string.IsNullOrWhiteSpace(protectorId))
                throw new ArgumentException("A protector identity is required.",
                    "protectorId");
            if (bonus != BodyguardAidPolicy.SuccessArmorClassBonus)
                throw new ArgumentOutOfRangeException("bonus");
            ProtectorId = protectorId;
            Bonus = bonus;
        }

        internal string ProtectorId { get; private set; }
        internal int Bonus { get; private set; }
    }

    internal sealed class BodyguardArmorClassAttributionPlan
    {
        internal BodyguardArmorClassAttributionPlan(int nativeArmorClass,
            BodyguardArmorClassContribution[] contributions)
        {
            if (contributions == null)
                throw new ArgumentNullException("contributions");
            NativeArmorClass = nativeArmorClass;
            Contributions = Array.AsReadOnly(contributions);
            TotalBonus = contributions.Sum(value => value.Bonus);
            FinalArmorClass = checked(nativeArmorClass + TotalBonus);
        }

        internal int NativeArmorClass { get; private set; }
        internal IReadOnlyList<BodyguardArmorClassContribution> Contributions
        { get; private set; }
        internal int TotalBonus { get; private set; }
        internal int FinalArmorClass { get; private set; }
    }

    /// <summary>
    /// Builds the exact attack-scoped AC total and one truthful native
    /// presentation contribution for every successful Bodyguard protector.
    /// Runtime code maps each contribution to that protector's Bodyguard fact.
    /// </summary>
    internal static class BodyguardArmorClassAttributionPolicy
    {
        internal static BodyguardArmorClassAttributionPlan Create(
            int nativeArmorClass, IEnumerable<BodyguardAidResult> attempts)
        {
            if (attempts == null) throw new ArgumentNullException("attempts");
            BodyguardAidResult[] values = attempts.ToArray();
            int expected = BodyguardAidPolicy.StackArmorClassBonus(values);
            BodyguardArmorClassContribution[] contributions = values
                .Where(value => value.Success)
                .Select(value => new BodyguardArmorClassContribution(
                    value.ProtectorId,
                    BodyguardAidPolicy.SuccessArmorClassBonus))
                .ToArray();
            var result = new BodyguardArmorClassAttributionPlan(
                nativeArmorClass, contributions);
            if (result.TotalBonus != expected)
                throw new InvalidOperationException(
                    "Bodyguard AC attribution did not conserve the Aid Another bonus.");
            return result;
        }
    }
}
