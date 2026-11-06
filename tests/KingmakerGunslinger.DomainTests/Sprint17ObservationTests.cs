using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    internal static class Sprint17ObservationTests
    {
        internal static void NativeReachRetainsTheEngineFloor()
        {
            // Sprint 4: raw 5/5, computed 2/2, actual 2/6 with the exact +4
            // tail correction (the tail is no longer a reach-weapon type).
            Assertions.True(Sprint17ObservationPolicy.NativeReach(5, 5, 5, 2, 2, 2, 2, 6, 4), "Raw5/5, computed2/2, body5 and actual2/6 with the exact tail correction retain the native floor.");
            Assertions.False(Sprint17ObservationPolicy.NativeReach(5, 5, 10, 2, 2, 6, 2, 6, 0), "The pre-Sprint-4 reach-weapon tail type is no longer accepted.");
            Assertions.False(Sprint17ObservationPolicy.NativeReach(5, 2, 2, 2, 2, 2, 2, 6, 4), "Computed getters cannot be relabeled raw profile inputs.");
            Assertions.False(Sprint17ObservationPolicy.NativeReach(5, 5, 5, 2, 5, 5, 2, 6, 4), "Raw fields cannot be relabeled computed getters.");
            Assertions.False(Sprint17ObservationPolicy.NativeReach(5, 5, 5, 2, 2, 2, 1, 6, 4), "Cannot remove the native floor to claim five feet.");
            Assertions.False(Sprint17ObservationPolicy.NativeReach(5, 5, 5, 2, 2, 2, 2, 10, 4), "The actual tail must be its type plus the exact correction, not raw reach.");
            Assertions.False(Sprint17ObservationPolicy.NativeReach(5, 5, 5, 2, 2, 2, 2, 6, 8), "A doubled correction rejects.");
            Assertions.False(Sprint17ObservationPolicy.NativeReach(10, 5, 5, 2, 2, 2, 2, 6, 4), "Body reach is independent; no inflated body reach.");
            foreach (float min in new[] { -1f, 0f, float.NaN, float.PositiveInfinity })
                Assertions.False(Sprint17ObservationPolicy.NativeReach(5, 5, 5, min, 2, 2, 2, 6, 4), "Missing/invalid native floor rejects.");
        }

        internal static void MissDoesNotResolveAPlannedDamageRule()
        {
            Assertions.True(Sprint17ObservationPolicy.ResolvedStrike(true, false, false, 0), "A planned rule object is not damage delivery.");
            Assertions.True(Sprint17ObservationPolicy.ResolvedStrike(false, true, true, 1), "One actual successful delivery.");
            Assertions.False(Sprint17ObservationPolicy.ResolvedStrike(true, false, true, 0), "Resolved miss rejects.");
            Assertions.False(Sprint17ObservationPolicy.ResolvedStrike(true, false, false, 1), "Delivered miss rejects.");
            Assertions.False(Sprint17ObservationPolicy.ResolvedStrike(false, true, false, 0), "No fabricated hit damage.");
            Assertions.False(Sprint17ObservationPolicy.ResolvedStrike(false, true, true, 2), "Duplicate delivery rejects.");
        }

        internal static void FireImmunityUsesTheDeliveredBoundary()
        {
            Assertions.True(Sprint17ObservationPolicy.FireImmunity(true, 0), "Native immune packet delivers zero, regardless of an intermediate roll.");
            Assertions.False(Sprint17ObservationPolicy.FireImmunity(false, 0), "Zero damage alone does not prove immunity.");
            Assertions.False(Sprint17ObservationPolicy.FireImmunity(true, 6), "An immune flag cannot hide real delivered damage.");
        }

        internal static void MagicReductionRequiresActualWeaponAttribution()
        {
            Assertions.True(Sprint17ObservationPolicy.MagicReduction(0, true, 0, 19, 10, 9), "Mundane native weapon is reduced.");
            Assertions.True(Sprint17ObservationPolicy.MagicReduction(1, true, 1, 20, 0, 20), "Native +1 weapon bypasses DR.");
            Assertions.False(Sprint17ObservationPolicy.MagicReduction(1, false, 1, 15, 0, 15), "Raw enhancement proxy rejects.");
            Assertions.False(Sprint17ObservationPolicy.MagicReduction(1, true, 1, 15, 10, 5), "Magic still reduced is a failure.");
            Assertions.False(Sprint17ObservationPolicy.MagicReduction(0, true, 0, 15, 0, 15), "Missing mundane reduction fails.");
            Assertions.False(Sprint17ObservationPolicy.MagicReduction(2, true, 2, 15, 0, 15), "Only the two closed probes.");
        }

        internal static void RegrabEvidenceCannotMixNativeLinkEpochs()
        {
            int[][] riders = { new[] { 1, 0 }, new[] { 2, 0 }, new[] { 3, 0 }, new[] { 3, 1 }, new[] { 3, 1 } };
            var current = riders.Where(r => SerpentineCommandReviewPolicy.SameLinkEpoch(3, r[0])).ToArray();
            Assertions.True(SerpentineCommandReviewPolicy.LaterMaintain(current.Count(r => r[1] == 0),
                1, current.Count(r => r[1] == 1), 4, 4), "A genuine later maintain remains attributable after prior escaped links.");
            Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(riders.Count(r => r[1] == 0),
                1, 2, 4, 4), "Global history is not one initial constrict.");
            Assertions.False(SerpentineCommandReviewPolicy.SameLinkEpoch(0, 0), "Missing links never correlate.");
            Assertions.False(SerpentineCommandReviewPolicy.SameLinkEpoch(3, 2), "Another link cannot contribute damage.");
            Assertions.False(SerpentineCommandReviewPolicy.LaterMaintain(1, 1, 2, 4, 5), "No extra attack relabeled maintain.");
        }

        internal static void ExpiryCannotWaiveAnAvailableNativeEffect()
        {
            Assertions.True(Sprint17ObservationPolicy.NativeExpiry(true, true, true, true, true, true), "Both native and original fade/cleanup.");
            Assertions.True(Sprint17ObservationPolicy.NativeExpiry(true, false, false, false, true, true), "Proven donor absence is explicit, not invented playback.");
            Assertions.False(Sprint17ObservationPolicy.NativeExpiry(true, true, false, false, true, true), "An available missing effect fails both.");
            Assertions.False(Sprint17ObservationPolicy.NativeExpiry(true, true, true, false, true, true), "Original failing a native effect fails.");
            Assertions.False(Sprint17ObservationPolicy.NativeExpiry(false, false, false, false, true, true), "No native control cannot excuse absence.");
            Assertions.False(Sprint17ObservationPolicy.NativeExpiry(true, false, false, false, false, true), "Normal despawn remains mandatory.");
            Assertions.False(Sprint17ObservationPolicy.NativeExpiry(true, false, false, false, true, false), "Exact resource cleanup remains mandatory.");
        }
    }
}
