using System;

namespace KingmakerGunslinger.AidAnotherCompatibility
{
    internal enum AidAnotherGrantSourceMode
    {
        Standalone = 0,
        CotwCanonical = 1,
        KmgKnownFallback = 2
    }

    internal enum AidAnotherHelpfulVariant
    {
        None = 0,
        Combat = 1,
        Halfling = 2,
        Both = 3
    }

    internal sealed class AidAnotherGrantRequest
    {
        internal int BaseGrant { get; set; }
        internal bool CombatHelpfulOwned { get; set; }
        internal bool HalflingHelpfulOwned { get; set; }
        internal int NonHelpfulIncrement { get; set; }
        internal AidAnotherGrantSourceMode SourceMode { get; set; }
    }

    internal sealed class AidAnotherGrantResolution
    {
        internal AidAnotherGrantResolution(bool valid, string failure,
            AidAnotherGrantSourceMode sourceMode, int baseGrant,
            AidAnotherHelpfulVariant helpfulVariant, int helpfulIncrement,
            int nonHelpfulIncrement, int finalGrant)
        {
            Valid = valid;
            Failure = failure ?? string.Empty;
            SourceMode = sourceMode;
            BaseGrant = baseGrant;
            HelpfulVariant = helpfulVariant;
            HelpfulIncrement = helpfulIncrement;
            NonHelpfulIncrement = nonHelpfulIncrement;
            FinalGrant = finalGrant;
        }

        internal bool Valid { get; private set; }
        internal string Failure { get; private set; }
        internal AidAnotherGrantSourceMode SourceMode { get; private set; }
        internal int BaseGrant { get; private set; }
        internal AidAnotherHelpfulVariant HelpfulVariant { get; private set; }
        internal int HelpfulIncrement { get; private set; }
        internal int NonHelpfulIncrement { get; private set; }
        internal int FinalGrant { get; private set; }

        internal string Describe()
        {
            return "canonicalSourceMode=" + SourceMode +
                ";baseGrant=" + BaseGrant +
                ";helpfulVariant=" + HelpfulVariant +
                ";helpfulIncrement=" + HelpfulIncrement +
                ";nonHelpfulIncrement=" + NonHelpfulIncrement +
                ";finalSuccessfulGrant=" + FinalGrant +
                (Valid ? string.Empty : ";grantFailure=" + Failure);
        }
    }

    /// <summary>
    /// One authoritative Aid Another amount policy shared by the CotW rank
    /// adapter and Bodyguard. Helpful is a replacement, so its two variants
    /// collapse to their greater increment; every unrelated canonical increment
    /// remains additive.
    /// </summary>
    internal static class AidAnotherGrantResolver
    {
        internal const int NormalBaseGrant = 2;
        internal const int CombatHelpfulIncrement = 1;
        internal const int HalflingHelpfulIncrement = 2;

        internal static AidAnotherGrantResolution Resolve(
            AidAnotherGrantRequest request)
        {
            if (request == null)
                return Invalid(AidAnotherGrantSourceMode.KmgKnownFallback,
                    "request-null", 0, 0);
            if (request.BaseGrant != NormalBaseGrant)
                return Invalid(request.SourceMode, "base-grant", request.BaseGrant,
                    request.NonHelpfulIncrement);
            if (request.NonHelpfulIncrement < 0)
                return Invalid(request.SourceMode, "non-helpful-increment",
                    request.BaseGrant, request.NonHelpfulIncrement);

            AidAnotherHelpfulVariant variant = request.CombatHelpfulOwned &&
                request.HalflingHelpfulOwned ? AidAnotherHelpfulVariant.Both :
                request.HalflingHelpfulOwned ? AidAnotherHelpfulVariant.Halfling :
                request.CombatHelpfulOwned ? AidAnotherHelpfulVariant.Combat :
                AidAnotherHelpfulVariant.None;
            int helpful = request.HalflingHelpfulOwned ?
                HalflingHelpfulIncrement : request.CombatHelpfulOwned ?
                    CombatHelpfulIncrement : 0;
            long final = (long)request.BaseGrant + helpful +
                request.NonHelpfulIncrement;
            if (final < request.BaseGrant || final > int.MaxValue)
                return Invalid(request.SourceMode, "grant-overflow",
                    request.BaseGrant, request.NonHelpfulIncrement);
            return new AidAnotherGrantResolution(true, string.Empty,
                request.SourceMode, request.BaseGrant, variant, helpful,
                request.NonHelpfulIncrement, (int)final);
        }

        internal static AidAnotherGrantResolution Standalone(
            bool combatHelpfulOwned)
        {
            return Resolve(new AidAnotherGrantRequest
            {
                BaseGrant = NormalBaseGrant,
                CombatHelpfulOwned = combatHelpfulOwned,
                HalflingHelpfulOwned = false,
                NonHelpfulIncrement = 0,
                SourceMode = AidAnotherGrantSourceMode.Standalone
            });
        }

        private static AidAnotherGrantResolution Invalid(
            AidAnotherGrantSourceMode sourceMode, string failure,
            int baseGrant, int nonHelpfulIncrement)
        {
            return new AidAnotherGrantResolution(false, failure, sourceMode,
                baseGrant, AidAnotherHelpfulVariant.None, 0,
                nonHelpfulIncrement, 0);
        }
    }
}
