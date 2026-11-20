using System;
using KingmakerGunslinger.Firearms;

namespace KingmakerGunslinger.Rules
{
    internal static class SteadyAimPolicy
    {
        internal const int RangeBonusFeet = 10;

        internal static bool IsQualifyingShot(bool exactFirearm,
            int markerCount, FirearmKind kind, bool scatterDelivery)
        {
            if (markerCount < 0) throw new ArgumentOutOfRangeException("markerCount");
            if (!exactFirearm || markerCount != 1 || scatterDelivery) return false;
            return FirearmHandednessPolicy.Matches(kind,
                FirearmHandedness.TwoHanded);
        }

        /// <summary>
        /// What one observed attack roll does to the armed state. Only a roll
        /// that discharged a loaded round is a shot: probes, synthetic rolls,
        /// scatter deliveries and empty chambers leave the state alone. A
        /// qualifying shot always consumes it, and brings the range bonus only
        /// while the deed's grit requirement still holds.
        /// </summary>
        internal static SteadyAimShotDecision ResolveShot(bool armed,
            bool dischargedRound, bool exactFirearm, int markerCount,
            FirearmKind kind, bool scatterDelivery, bool requiredGrit)
        {
            bool qualifying = IsQualifyingShot(exactFirearm, markerCount, kind,
                scatterDelivery);
            if (!armed || !dischargedRound || !qualifying)
                return new SteadyAimShotDecision(false, 0);
            return new SteadyAimShotDecision(true,
                requiredGrit ? RangeBonusFeet : 0);
        }
    }

    internal struct SteadyAimShotDecision
    {
        private readonly bool _consumesArmedState;
        private readonly int _rangeBonusFeet;

        internal SteadyAimShotDecision(bool consumesArmedState,
            int rangeBonusFeet)
        {
            if (rangeBonusFeet < 0 || (!consumesArmedState && rangeBonusFeet != 0))
                throw new ArgumentOutOfRangeException("rangeBonusFeet");
            _consumesArmedState = consumesArmedState;
            _rangeBonusFeet = rangeBonusFeet;
        }

        internal bool ConsumesArmedState { get { return _consumesArmedState; } }
        internal int RangeBonusFeet { get { return _rangeBonusFeet; } }
    }
}
