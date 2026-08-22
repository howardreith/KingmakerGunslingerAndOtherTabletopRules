using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.BodyguardFeats
{
    internal sealed class BodyguardInterceptorCandidate
    {
        internal BodyguardInterceptorCandidate(string persistentId, int partyOrder,
            bool bodyguardAttempted, bool bodyguardSucceeded,
            bool hasInHarmsWay, bool modeActive, bool immediateActionAvailable)
        {
            if (string.IsNullOrWhiteSpace(persistentId))
                throw new ArgumentException("A persistent identity is required.",
                    "persistentId");
            PersistentId = persistentId;
            PartyOrder = partyOrder < 0 ? int.MaxValue : partyOrder;
            BodyguardAttempted = bodyguardAttempted;
            BodyguardSucceeded = bodyguardSucceeded;
            HasInHarmsWay = hasInHarmsWay;
            ModeActive = modeActive;
            ImmediateActionAvailable = immediateActionAvailable;
        }

        internal string PersistentId { get; private set; }
        internal int PartyOrder { get; private set; }
        internal bool BodyguardAttempted { get; private set; }
        internal bool BodyguardSucceeded { get; private set; }
        internal bool HasInHarmsWay { get; private set; }
        internal bool ModeActive { get; private set; }
        internal bool ImmediateActionAvailable { get; private set; }
    }

    internal static class BodyguardInterceptionPolicy
    {
        internal static BodyguardInterceptorCandidate[] OrderEligible(
            bool moduleEnabled, bool attackHit, bool alreadyIntercepted,
            IEnumerable<BodyguardInterceptorCandidate> candidates)
        {
            if (candidates == null) throw new ArgumentNullException("candidates");
            BodyguardInterceptorCandidate[] values = candidates.ToArray();
            if (values.Any(value => value == null))
                throw new ArgumentException("An interceptor candidate is null.",
                    "candidates");
            if (!moduleEnabled || !attackHit || alreadyIntercepted)
                return Array.Empty<BodyguardInterceptorCandidate>();
            return values.Where(value => value.BodyguardAttempted &&
                    value.BodyguardSucceeded && value.HasInHarmsWay &&
                    value.ModeActive && value.ImmediateActionAvailable)
                .OrderBy(value => value.PartyOrder)
                .ThenBy(value => value.PersistentId, StringComparer.Ordinal)
                .ToArray();
        }
    }
}
