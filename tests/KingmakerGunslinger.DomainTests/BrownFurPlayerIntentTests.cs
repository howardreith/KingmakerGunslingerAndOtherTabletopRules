using System.Linq;
using KingmakerGunslinger.BrownFur;

namespace KingmakerGunslinger.DomainTests
{
    internal static class BrownFurPlayerIntentTests
    {
        internal static void EmptyStateIsUnrequested()
        {
            BrownFurPlayerIntentDecision decision = Decide(false, false,
                false, new BrownFurAbilityScore[0], false);
            Assertions.True(decision.Valid && !decision.CasterOwnsBrownFur &&
                !decision.PowerfulChangeRequested &&
                !decision.ShareTransmutationRequested &&
                decision.SelectedAbilityScore == BrownFurAbilityScore.None,
                "Empty ownership state unexpectedly armed Brown-Fur intent.");
        }

        internal static void EachAbilityScoreIsExclusive()
        {
            foreach (BrownFurAbilityScore score in System.Enum.GetValues(
                typeof(BrownFurAbilityScore)).Cast<BrownFurAbilityScore>()
                .Where(value => value != BrownFurAbilityScore.None))
            {
                BrownFurPlayerIntentDecision decision = Decide(true, false,
                    false, new[] { score, score }, false);
                Assertions.True(decision.Valid &&
                    decision.CasterOwnsBrownFur &&
                    decision.HasPowerfulChange &&
                    decision.PowerfulChangeRequested &&
                    decision.SelectedAbilityScore == score,
                    "One-shot Powerful Change intent changed for " + score + ".");
            }
        }

        internal static void AmbiguousScoresFailClosed()
        {
            BrownFurPlayerIntentDecision decision = Decide(true, true, false,
                new[] { BrownFurAbilityScore.Strength,
                    BrownFurAbilityScore.Dexterity }, true);
            Assertions.True(!decision.Valid && decision.Failure ==
                "powerful-selection-ambiguous" &&
                !decision.PowerfulChangeRequested &&
                decision.SelectedAbilityScore == BrownFurAbilityScore.None,
                "Multiple pending ability scores did not fail closed.");
        }

        internal static void ShareAndCapstoneOwnershipAreIndependent()
        {
            BrownFurPlayerIntentDecision decision = Decide(false, true, true,
                new BrownFurAbilityScore[0], true);
            Assertions.True(decision.Valid &&
                decision.CasterOwnsBrownFur &&
                !decision.PowerfulChangeRequested &&
                decision.ShareTransmutationRequested &&
                decision.HasShareTransmutation &&
                decision.HasTransmutationSupremacy,
                "Share or capstone ownership was inferred from the wrong fact.");
        }

        internal static void PendingMarkersRequireTheirFeatures()
        {
            BrownFurPlayerIntentDecision powerful = Decide(false, false,
                true, new[] { BrownFurAbilityScore.Strength }, false);
            BrownFurPlayerIntentDecision share = Decide(true, false, false,
                new BrownFurAbilityScore[0], true);
            Assertions.True(!powerful.Valid && powerful.Failure ==
                "powerful-feature-missing" &&
                !powerful.PowerfulChangeRequested,
                "A pending score marker armed Powerful Change without ownership.");
            Assertions.True(!share.Valid && share.Failure ==
                "share-feature-missing" &&
                !share.ShareTransmutationRequested,
                "A pending Share marker armed Share Transmutation without ownership.");
        }

        private static BrownFurPlayerIntentDecision Decide(bool powerful,
            bool share, bool supremacy, BrownFurAbilityScore[] scores,
            bool sharePending)
        {
            return BrownFurPlayerIntentPolicy.Decide(
                new BrownFurPlayerIntentInput {
                    HasPowerfulChange = powerful,
                    HasShareTransmutation = share,
                    HasTransmutationSupremacy = supremacy,
                    PendingAbilityScores = scores,
                    ShareTransmutationPending = sharePending
                });
        }
    }
}
