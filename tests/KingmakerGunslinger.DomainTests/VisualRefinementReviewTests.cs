using System;
using System.IO;
using System.Linq;
using KingmakerGunslinger.RuntimeTesting;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// The visual-refinement creature review: the Stirge reaches its
    /// refinement sequence after its attached-prey review, and a refinement
    /// packet passes only when every close still, every tracking frame and the
    /// native travel are present.
    /// </summary>
    internal static class VisualRefinementReviewTests
    {
        private static string Runner()
        {
            return File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "src",
                "KingmakerGunslinger", "RuntimeTesting",
                "RuntimeTestRunner.ExpandedSummoningCreatureReview.cs"));
        }

        internal static void StirgeRefinementFollowsItsPreyReview()
        {
            Assertions.True(VisualRefinementReviewPolicy.FollowsStirgePreyReview("stirge", false),
                "A single Stirge goes on to its refinement sequence.");
            Assertions.False(VisualRefinementReviewPolicy.FollowsStirgePreyReview("stirge", true),
                "A Stirge crowd review keeps its own route.");
            Assertions.False(VisualRefinementReviewPolicy.FollowsStirgePreyReview("dire-rat", false),
                "Only the Stirge has the attached-prey branch.");
            string source = Runner();
            int prey = source.IndexOf("if (!StepExpandedSummoningStirgePreyMovement()) return;",
                StringComparison.Ordinal);
            Assertions.True(prey > 0, "The attached-prey phase is still reviewed.");
            int refine = source.IndexOf("VisualRefinementReviewPolicy.FollowsStirgePreyReview(key,",
                prey, StringComparison.Ordinal);
            int cleanup = source.IndexOf("CleanupExpandedSummoningUnit(unit);", prey,
                StringComparison.Ordinal);
            int next = source.IndexOf("_creatureReviewPhase = 7;", prey, StringComparison.Ordinal);
            Assertions.True(refine > prey && next > refine && cleanup > next,
                "After the prey review, the Stirge enters the refinement phase before cleanup.");
        }

        internal static void RefinementPacketRequiresEveryStill()
        {
            string[] stills = VisualRefinementReviewPolicy.StillViews
                .Select(view => VisualRefinementReviewPolicy.StillFile("giant-crab", view)).ToArray();
            int tracking = VisualRefinementReviewPolicy.ExpectedTrackingFrames;
            string missing;
            Assertions.Equal(108, tracking, "Two 36-frame legs, 24 attack and 12 hit frames.");
            Assertions.True(VisualRefinementReviewPolicy.EvidenceComplete("giant-crab", stills,
                    tracking, 0, 1.5f, out missing) && missing.Length == 0,
                "A full packet passes.");
            Assertions.False(VisualRefinementReviewPolicy.EvidenceComplete("giant-crab",
                    stills.Where(file => !file.EndsWith("-rear-low.png", StringComparison.Ordinal)),
                    tracking, 0, 1.5f, out missing),
                "A failed or missing close still fails the packet.");
            Assertions.True(missing.Contains("giant-crab-refine-view-rear-low.png"),
                "The missing still is named.");
            Assertions.False(VisualRefinementReviewPolicy.EvidenceComplete("giant-crab", stills,
                    tracking - 1, 0, 1.5f, out missing), "A missing tracking frame fails.");
            Assertions.False(VisualRefinementReviewPolicy.EvidenceComplete("giant-crab", stills,
                    tracking, 0, 0.5f, out missing), "Too little native travel fails.");
            Assertions.False(VisualRefinementReviewPolicy.EvidenceComplete("giant-crab", stills,
                    tracking, 0, float.NaN, out missing), "Unmeasured travel fails.");

            string[] stirge = VisualRefinementReviewPolicy.StillViews
                .Select(view => VisualRefinementReviewPolicy.StillFile("stirge", view)).ToArray();
            int low = VisualRefinementReviewPolicy.ExpectedLowFrames("stirge");
            Assertions.Equal(72, low, "The Stirge's two move legs each give 36 low side frames.");
            Assertions.False(VisualRefinementReviewPolicy.EvidenceComplete("stirge", stirge,
                    tracking, 0, 1.5f, out missing),
                "The Stirge's packet needs its low side frames for both wing surfaces.");
            Assertions.True(VisualRefinementReviewPolicy.EvidenceComplete("stirge", stirge,
                    tracking, low, 1.5f, out missing), "A full Stirge packet passes.");

            string source = Runner();
            Assertions.True(source.Contains("VisualRefinementReviewPolicy.EvidenceComplete(_refinementKey,") &&
                source.Contains("if (result.StartsWith(\"png=\" + file + \";\", StringComparison.Ordinal))") &&
                source.Contains("_refinementStills.Add(file);"),
                "The runner records each still only when it was written, and completes on the policy.");
        }
    }
}
