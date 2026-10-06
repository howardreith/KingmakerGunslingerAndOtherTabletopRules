using System;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-local evidence/retry policy. Never changes production commands.
    internal static class SerpentineCommandReviewPolicy
    {
        internal static void SuspendAppearanceDriver(bool manual, Action disableManualBrain, Action deferNativeAi)
        {
            if (manual) disableManualBrain();
            else deferNativeAi(); // Never clear the AI's source action list.
        }

        internal static string[][] Cells()
        {
            return (from key in new[] { "viper", "constrictor-snake" }
                    from mode in new[] { "rtwp", "turn-based" }
                    from driver in new[] { "manual", "ai" }
                    select new[] { key, mode, driver }).ToArray();
        }

        internal static bool CanIssueManual(bool manual, bool ready, int attempts,
            bool pending, bool signature, bool held, bool targetAlive)
        {
            return manual && ready && attempts >= 0 && attempts < 4 &&
                !pending && !signature && !held && targetAlive;
        }

        internal static bool ReadyToStart(bool originalAttached, bool intact,
            bool canAct, bool manual, bool controllable, int frames)
        {
            return frames >= 60 && frames <= 600 && originalAttached && intact &&
                canAct && (!manual || controllable);
        }

        internal static bool Contact(bool owned, bool executing, bool opportunity,
            bool nativeClip, int points, float gap)
        {
            return SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(owned, executing,
                opportunity, nativeClip, points, gap) && gap <= .25f;
        }

        internal static bool SameRenderedAttack(int ruleFrame, int renderedFrame,
            bool sameCommand, bool sameHandle, bool sameAnimation, bool sameClip)
        {
            // A later animation peak is never evidence for this rule event.
            return ruleFrame >= 0 && renderedFrame == ruleFrame && sameCommand &&
                sameHandle && sameAnimation && sameClip;
        }

        internal static bool LaterMaintain(int initialRiders, int heldRound,
            int laterRiders, int attacksAtEstablishment, int currentAttacks)
        {
            return initialRiders == 1 && heldRound > 0 && laterRiders == 2 &&
                attacksAtEstablishment > 0 && attacksAtEstablishment == currentAttacks;
        }
    }
}
