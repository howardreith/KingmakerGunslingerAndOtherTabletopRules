using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.Summoning
{
    /// <summary>Why the owned Mastodon Trample AI action declines to act.</summary>
    internal enum MastodonTrampleAiRefusal
    {
        None,
        NotOwner,
        NotInCombat,
        CannotActOrMove,
        MovementForbidden,
        Threatened,
        NoFullRoundAction,
        MeleeTargetInReach,
        TargetInvalid,
        TargetInReach,
        TargetNotSmaller,
        TargetClaimedThisRound,
        RoundHalted,
        AbilityMissing,
        NoWalkableEndpoint,
        TooShort,
        TooLong,
        MissesTarget,
        EndpointNotBeyondTarget,
        PathBlocked,
        NoLineOfSight,
        EndpointOccupied,
        NativeTargetRefused,
        NativeUnavailable
    }

    /// <summary>
    /// S5 T6 / RISK-06 (owner decision, 2026-11-20): the summoned Mastodon may
    /// use its existing Trample as an approach maneuver, chosen by its own AI.
    /// The policy is deliberately conservative: Trample is offered only when
    /// the Mastodon is in combat, unthreatened, able to spend its full round,
    /// and no legal ordinary melee target can be attacked without moving. The
    /// target must be smaller than the Mastodon and not already claimed in
    /// the current round, and the final straight path must legally cross the
    /// target's contact envelope and finish beyond it. Pure decisions only;
    /// the native reads live in <c>UngulateTrampleAiRuntime</c>.
    /// </summary>
    internal static class MastodonTrampleAiPolicy
    {
        internal const string CreatureKey = "mastodon";
        internal const string UnitSymbol = "KMG.Summoning.Unit.Mastodon";
        internal const string AiSymbol = "KMG.Summoning.Special.Mastodon.TrampleAi";
        internal const string BrainSymbol = "KMG.Summoning.Special.Mastodon.Brain";
        internal const string AiPlannedType = "UngulateTrampleAi";

        /// <summary>The native "Dumb Monster Brain" the Mastodon copies (unchanged).</summary>
        internal const string NativeBrainGuid = "5abc8884c6f15204c8604cb01a2efbab";

        /// <summary>
        /// Above the native attack action, so the native selector evaluates
        /// it first. It can only win while the predicate holds; otherwise
        /// the action is unavailable and the ordinary attack decides.
        /// </summary>
        internal const float BaseScore = 5f;

        internal const float FootMeters = 0.3048f;

        /// <summary>Clearance planned beyond the target's body, before navmesh adjustment.</summary>
        internal const float PlannedClearanceMeters = 0.5f;

        /// <summary>
        /// The native overrun delivery contacts a unit within the sum of the
        /// two bodies plus one foot, and only while it lies within 45 degrees
        /// of the direction of travel. A path whose closest approach is at
        /// most half that envelope meets the target at about 30 degrees.
        /// </summary>
        internal const float CrossingFraction = 0.5f;

        internal static IEnumerable<KeyValuePair<string, string>> NewIdentities()
        {
            yield return new KeyValuePair<string, string>(AiSymbol, AiPlannedType);
            yield return new KeyValuePair<string, string>(BrainSymbol, "BlueprintBrain");
        }

        /// <summary>Actor conditions, in the order they are reported.</summary>
        internal static MastodonTrampleAiRefusal Actor(bool inCombat, bool canActAndMove,
            bool movementForbidden, bool threatened, bool turnBased, bool hasFullRoundAction,
            bool anyMeleeTargetInReach)
        {
            if (!inCombat) return MastodonTrampleAiRefusal.NotInCombat;
            if (!canActAndMove) return MastodonTrampleAiRefusal.CannotActOrMove;
            if (movementForbidden) return MastodonTrampleAiRefusal.MovementForbidden;
            if (threatened) return MastodonTrampleAiRefusal.Threatened;
            if (turnBased && !hasFullRoundAction) return MastodonTrampleAiRefusal.NoFullRoundAction;
            if (anyMeleeTargetInReach) return MastodonTrampleAiRefusal.MeleeTargetInReach;
            return MastodonTrampleAiRefusal.None;
        }

        /// <summary>
        /// Target conditions. The ledger state is read, never claimed: the
        /// contact action alone claims a target when the delivery reaches it.
        /// </summary>
        internal static MastodonTrampleAiRefusal Target(bool targetInReach, bool smaller,
            bool claimedThisRound, bool roundHalted)
        {
            if (targetInReach) return MastodonTrampleAiRefusal.TargetInReach;
            if (!smaller) return MastodonTrampleAiRefusal.TargetNotSmaller;
            if (roundHalted) return MastodonTrampleAiRefusal.RoundHalted;
            if (claimedThisRound) return MastodonTrampleAiRefusal.TargetClaimedThisRound;
            return MastodonTrampleAiRefusal.None;
        }

        /// <summary>
        /// The planned endpoint on the line from the Mastodon through the
        /// target's centre, beyond the target by both bodies and a clearance.
        /// </summary>
        internal static void DesiredEndpoint(float startX, float startZ, float targetX, float targetZ,
            float targetRadius, float casterRadius, out float endX, out float endZ)
        {
            float dx = targetX - startX, dz = targetZ - startZ;
            float length = (float)Math.Sqrt(dx * dx + dz * dz);
            if (length < 1e-4f) { endX = targetX; endZ = targetZ; return; }
            float beyond = Math.Max(0f, targetRadius) + Math.Max(0f, casterRadius) + PlannedClearanceMeters;
            endX = targetX + dx / length * beyond;
            endZ = targetZ + dz / length * beyond;
        }

        /// <summary>
        /// Validates the final straight segment the native delivery will
        /// follow (after any navmesh adjustment): its length, that it crosses
        /// the target's contact envelope ahead of the start, and that it ends
        /// beyond the target with the Mastodon's body clear of it.
        /// </summary>
        internal static MastodonTrampleAiRefusal Segment(float startX, float startZ, float endX, float endZ,
            float targetX, float targetZ, float targetRadius, float casterRadius, float minLength,
            float maxLength)
        {
            float dx = endX - startX, dz = endZ - startZ;
            float length = (float)Math.Sqrt(dx * dx + dz * dz);
            if (length <= Math.Max(minLength, 1e-3f)) return MastodonTrampleAiRefusal.TooShort;
            if (length > maxLength) return MastodonTrampleAiRefusal.TooLong;
            float ux = dx / length, uz = dz / length;
            float tx = targetX - startX, tz = targetZ - startZ;
            float along = tx * ux + tz * uz;
            float offset = Math.Abs(tx * uz - tz * ux);
            float contact = Math.Max(0f, targetRadius) + Math.Max(0f, casterRadius) + FootMeters;
            if (along <= 0f || offset > contact * CrossingFraction)
                return MastodonTrampleAiRefusal.MissesTarget;
            if (length - along < Math.Max(0f, targetRadius) + Math.Max(0f, casterRadius))
                return MastodonTrampleAiRefusal.EndpointNotBeyondTarget;
            return MastodonTrampleAiRefusal.None;
        }

        /// <summary>The native brain's actions, unchanged and in order, then the Trample action.</summary>
        internal static T[] AppendTrampleAction<T>(T[] nativeActions, T trample) where T : class
        {
            if (trample == null) throw new ArgumentNullException("trample");
            T[] native = nativeActions ?? Array.Empty<T>();
            if (native.Length == 0 || native.Any(action => action == null || ReferenceEquals(action, trample)))
                throw new InvalidOperationException("The Mastodon Trample brain requires the native attack actions, once each.");
            return native.Concat(new[] { trample }).ToArray();
        }
    }
}
