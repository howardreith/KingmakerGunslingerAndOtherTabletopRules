using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    // Request-local evidence/retry policy. Never changes production commands.
    internal static class SerpentineCommandReviewPolicy
    {
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

        internal static bool Contact(bool owned, bool executing, bool opportunity,
            bool nativeClip, int points, float gap)
        {
            return SerpentineRigSurveyPolicy.IsMeasuredIssuedContact(owned, executing,
                opportunity, nativeClip, points, gap) && gap <= .25f;
        }

        internal static bool LaterMaintain(int initialRiders, int heldRound,
            int laterRiders, int attacksAtEstablishment, int currentAttacks)
        {
            return initialRiders == 1 && heldRound > 0 && laterRiders == 2 &&
                attacksAtEstablishment > 0 && attacksAtEstablishment == currentAttacks;
        }
    }
}
