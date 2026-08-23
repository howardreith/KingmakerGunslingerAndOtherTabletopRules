using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.AidAnotherCompatibility;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal sealed class BodyguardAidResult
    {
        internal BodyguardAidResult(string protectorId, int naturalRoll,
            int attackBonus)
            : this(protectorId, naturalRoll, attackBonus,
                AidAnotherGrantResolver.Standalone(false))
        { }

        internal BodyguardAidResult(string protectorId, int naturalRoll,
            int attackBonus, AidAnotherGrantResolution grant)
        {
            if (string.IsNullOrWhiteSpace(protectorId))
                throw new ArgumentException("A protector identity is required.",
                    "protectorId");
            if (naturalRoll < 1 || naturalRoll > 20)
                throw new ArgumentOutOfRangeException("naturalRoll");
            if (grant == null || !grant.Valid || grant.FinalGrant <
                    AidAnotherGrantResolver.NormalBaseGrant)
                throw new ArgumentException(
                    "A valid Aid Another successful-grant resolution is required.",
                    "grant");
            ProtectorId = protectorId;
            NaturalRoll = naturalRoll;
            AttackBonus = attackBonus;
            Total = naturalRoll + attackBonus;
            Success = naturalRoll == 20 || naturalRoll != 1 && Total >= 10;
            Grant = grant;
        }

        internal string ProtectorId { get; private set; }
        internal int NaturalRoll { get; private set; }
        internal int AttackBonus { get; private set; }
        internal int Total { get; private set; }
        internal bool Success { get; private set; }
        internal AidAnotherGrantResolution Grant { get; private set; }
        internal int ResolvedSuccessfulGrant { get { return Grant.FinalGrant; } }
        internal int ActualArmorClassContribution
        { get { return Success ? ResolvedSuccessfulGrant : 0; } }
        internal int ArmorClassBonus
        { get { return ActualArmorClassContribution; } }
    }

    internal static class BodyguardAidPolicy
    {
        internal const int TargetArmorClass = 10;
        internal const int SuccessArmorClassBonus =
            AidAnotherGrantResolver.NormalBaseGrant;

        internal static int StackArmorClassBonus(
            IEnumerable<BodyguardAidResult> attempts)
        {
            if (attempts == null) throw new ArgumentNullException("attempts");
            BodyguardAidResult[] values = attempts.ToArray();
            if (values.Any(value => value == null))
                throw new ArgumentException("An Aid Another attempt is null.",
                    "attempts");
            if (values.Select(value => value.ProtectorId).Distinct(
                    StringComparer.Ordinal).Count() != values.Length)
                throw new InvalidOperationException(
                    "A protector may contribute at most once to one attack.");
            return values.Aggregate(0, (total, value) => checked(total +
                value.ActualArmorClassContribution));
        }
    }
}
