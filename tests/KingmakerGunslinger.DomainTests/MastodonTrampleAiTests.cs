using System;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// S5 T6 / RISK-06: the Mastodon's owned Trample AI action. The predicate
    /// is conservative (approach maneuver only), the final straight path must
    /// cross the target and finish beyond it, planning only reads the round
    /// ledger, and the brain is the native one plus the owned action.
    /// </summary>
    internal static class MastodonTrampleAiTests
    {
        private static MastodonTrampleAiRefusal Actor(bool inCombat = true, bool canAct = true,
            bool forbidden = false, bool threatened = false, bool turnBased = false, bool fullRound = true,
            bool meleeInReach = false)
        {
            return MastodonTrampleAiPolicy.Actor(inCombat, canAct, forbidden, threatened, turnBased, fullRound,
                meleeInReach);
        }

        internal static void ActorPredicateIsConservative()
        {
            Assertions.Equal(MastodonTrampleAiRefusal.None, Actor(), "An unthreatened Mastodon with no target in reach may trample.");
            Assertions.Equal(MastodonTrampleAiRefusal.None, Actor(turnBased: true), "Turn-based with its full round available.");
            Assertions.Equal(MastodonTrampleAiRefusal.None, Actor(fullRound: false),
                "Real time has no turn budget; the native command waits for its own cooldown.");
            Assertions.Equal(MastodonTrampleAiRefusal.NotInCombat, Actor(inCombat: false), "Out of combat.");
            Assertions.Equal(MastodonTrampleAiRefusal.CannotActOrMove, Actor(canAct: false), "Cannot act or move.");
            Assertions.Equal(MastodonTrampleAiRefusal.MovementForbidden, Actor(forbidden: true), "AI movement forbidden.");
            Assertions.Equal(MastodonTrampleAiRefusal.Threatened, Actor(threatened: true), "Threatened (engaged) refuses.");
            Assertions.Equal(MastodonTrampleAiRefusal.NoFullRoundAction, Actor(turnBased: true, fullRound: false),
                "Turn-based without its full round refuses.");
            Assertions.Equal(MastodonTrampleAiRefusal.MeleeTargetInReach, Actor(meleeInReach: true),
                "Any legal ordinary melee target already in reach keeps the attack's precedence, even if that enemy does not threaten the Mastodon.");
            Assertions.Equal(MastodonTrampleAiRefusal.Threatened, Actor(threatened: true, meleeInReach: true),
                "The first failing condition is reported.");
        }

        internal static void TargetPredicateReadsTheLedger()
        {
            Assertions.Equal(MastodonTrampleAiRefusal.None, MastodonTrampleAiPolicy.Target(false, true, false, false),
                "A smaller, unclaimed target out of reach is eligible.");
            Assertions.Equal(MastodonTrampleAiRefusal.TargetInReach, MastodonTrampleAiPolicy.Target(true, true, false, false),
                "A target already in reach is attacked, not trampled.");
            Assertions.Equal(MastodonTrampleAiRefusal.TargetNotSmaller, MastodonTrampleAiPolicy.Target(false, false, false, false),
                "A target that is not smaller is ineligible under the existing size rule.");
            Assertions.Equal(MastodonTrampleAiRefusal.TargetClaimedThisRound, MastodonTrampleAiPolicy.Target(false, true, true, false),
                "A target already claimed this round is not selected again.");
            Assertions.Equal(MastodonTrampleAiRefusal.RoundHalted, MastodonTrampleAiPolicy.Target(false, true, false, true),
                "A round halted by a stopping attack of opportunity selects no movement-only cast.");
        }

        internal static void PlanningNeverClaims()
        {
            var ledger = new TrampleRoundLedger();
            for (int i = 0; i < 25; i++)
            {
                Assertions.False(ledger.HasClaim(4, "target"), "Reading does not create a claim.");
                Assertions.False(ledger.IsHalted(4), "Reading does not halt.");
            }
            Assertions.True(ledger.TryClaim(4, "target"), "After repeated reads, the contact still claims the target first.");
            Assertions.True(ledger.HasClaim(4, "target"), "The contact's claim is visible to planning in the same round.");
            Assertions.False(ledger.TryClaim(4, "target"), "Duplicate contacts in a round stay suppressed.");
            Assertions.False(ledger.HasClaim(4, "other"), "Another target stays unclaimed.");
            Assertions.False(ledger.HasClaim(5, "target"), "The claim does not carry into a later round.");
            Assertions.True(ledger.TryClaim(5, "target"), "Later-round reuse claims the same target again.");
            ledger.Halt(5);
            Assertions.True(ledger.IsHalted(5), "A stopping attack of opportunity halts the round.");
            Assertions.False(ledger.TryClaim(5, "other"), "A halted round claims nothing more.");
            Assertions.False(ledger.IsHalted(6), "The halt does not carry into the next round.");
        }

        internal static void FinalSegmentMustCrossAndPassTheTarget()
        {
            const float caster = 1.5f, target = 0.5f, max = 12f;
            float ex, ez;
            MastodonTrampleAiPolicy.DesiredEndpoint(0f, 0f, 6f, 0f, target, caster, out ex, out ez);
            Assertions.True(Math.Abs(ex - 8.5f) < 1e-4f && Math.Abs(ez) < 1e-4f,
                "The planned endpoint lies on the line, beyond the target by both bodies and the clearance.");
            Assertions.Equal(MastodonTrampleAiRefusal.None,
                MastodonTrampleAiPolicy.Segment(0f, 0f, ex, ez, 6f, 0f, target, caster, caster, max),
                "The straight path through the target is accepted.");
            Assertions.Equal(MastodonTrampleAiRefusal.MissesTarget,
                MastodonTrampleAiPolicy.Segment(0f, 0f, 8.5f, 2.5f, 6f, 0f, target, caster, caster, max),
                "An endpoint snapped beside the target misses its contact envelope.");
            Assertions.Equal(MastodonTrampleAiRefusal.EndpointNotBeyondTarget,
                MastodonTrampleAiPolicy.Segment(0f, 0f, 7f, 0f, 6f, 0f, target, caster, caster, max),
                "An endpoint snapped short leaves the Mastodon inside the target.");
            Assertions.Equal(MastodonTrampleAiRefusal.MissesTarget,
                MastodonTrampleAiPolicy.Segment(0f, 0f, -8.5f, 0f, 6f, 0f, target, caster, caster, max),
                "A path away from the target never meets it.");
            Assertions.Equal(MastodonTrampleAiRefusal.TooLong,
                MastodonTrampleAiPolicy.Segment(0f, 0f, ex, ez, 6f, 0f, target, caster, caster, 8f),
                "A path beyond the permitted distance is refused.");
            Assertions.Equal(MastodonTrampleAiRefusal.TooShort,
                MastodonTrampleAiPolicy.Segment(0f, 0f, 1f, 0f, 0.5f, 0f, target, caster, caster, max),
                "A path no longer than the Mastodon's body is refused.");
            float contact = target + caster + MastodonTrampleAiPolicy.FootMeters;
            Assertions.Equal(MastodonTrampleAiRefusal.None,
                MastodonTrampleAiPolicy.Segment(0f, 0f, 8.5f, 0f, 6f, contact * 0.49f, target, caster, caster, max),
                "A small lateral offset within half the contact envelope still meets the target.");
            Assertions.Equal(MastodonTrampleAiRefusal.MissesTarget,
                MastodonTrampleAiPolicy.Segment(0f, 0f, 8.5f, 0f, 6f, contact * 0.51f, target, caster, caster, max),
                "A lateral offset beyond half the envelope could fail the native 45-degree contact test and is refused.");
        }

        internal static void BrainIsNativePlusOwnedAction()
        {
            var attack = new object();
            var trample = new object();
            object[] actions = MastodonTrampleAiPolicy.AppendTrampleAction(new[] { attack }, trample);
            Assertions.True(actions.Length == 2 && ReferenceEquals(actions[0], attack) && ReferenceEquals(actions[1], trample),
                "The native attack action is kept first and unchanged; Trample is appended.");
            Assertions.Throws<InvalidOperationException>(() =>
                MastodonTrampleAiPolicy.AppendTrampleAction(new object[0], trample), "An empty native brain is refused.");
            Assertions.Throws<InvalidOperationException>(() =>
                MastodonTrampleAiPolicy.AppendTrampleAction(new[] { attack, trample }, trample), "A duplicate is refused.");
            Assertions.True(MastodonTrampleAiPolicy.BaseScore > 1f,
                "The owned action is evaluated before the native attack, and wins only while its predicate holds.");
            var identities = MastodonTrampleAiPolicy.NewIdentities().ToArray();
            Assertions.True(identities.Length == 2 &&
                identities.All(pair => pair.Key.StartsWith("KMG.Summoning.Special.Mastodon.", StringComparison.Ordinal)),
                "Exactly the Mastodon's own AI action and brain are appended; no other trampler changes.");
            Assertions.Equal(MastodonTrampleAiPolicy.AiPlannedType, identities[0].Value, "The owned AI action type.");
            Assertions.Equal("BlueprintBrain", identities[1].Value, "The private brain copy.");
        }
    }
}
