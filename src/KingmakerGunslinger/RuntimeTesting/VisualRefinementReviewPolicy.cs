using System;
using System.Collections.Generic;
using System.Linq;

namespace KingmakerGunslinger.RuntimeTesting
{
    /// <summary>
    /// The creatures whose original bodies the visual-refinement lane is
    /// rebuilding, and the donors whose pre-swap bind frame the review records
    /// for the offline builders. A key outside this list reviews exactly as
    /// before.
    ///
    /// <para>It also owns what a complete refinement packet is, so the
    /// creature review cannot pass with evidence missing: every close still
    /// written, every tracking frame written (and, for a creature reviewed
    /// for both wing surfaces, every low side frame), and at least
    /// <see cref="MinimumTravelMeters"/> of native travel. Image completeness
    /// is a technical fact; whether the creature looks right stays a human
    /// judgment. Plain data and arithmetic; the domain tests compile it.</para>
    /// </summary>
    internal static class VisualRefinementReviewPolicy
    {
        internal static readonly string[] Keys =
            { "dire-rat", "rhinoceros", "tyrannosaurus",
              "woolly-rhinoceros", "aurochs", "bison",
              "hyena", "goblin-dog", "wolverine", "shadow-mastiff",
              "poisonous-frog", "deinonychus", "triceratops",
              "ankylosaurus", "crocodile", "dire-crocodile",
              "giant-ant-worker", "giant-ant-soldier", "giant-ant-drone",
              "giant-scorpion", "dire-bat", "pteranodon", "giant-wasp",
              "ape", "dire-ape", "xill", "salamander",
              "eagle", "stirge", "fire-beetle", "giant-stag-beetle",
              "girallon", "giant-crab", "stegosaurus" };

        // Single-renderer donors only; the Tyrannosaurus' two-renderer
        // Lizardfolk frame comes from its own Sprint 24 census.
        private static readonly Dictionary<string, string[]> Donors =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { "dire-rat", new[] { "dog", "Sprint 12" } },
                { "rhinoceros", new[] { "mastodon", "Sprint 11" } },
                { "woolly-rhinoceros", new[] { "mastodon", "Sprint 11" } },
                { "aurochs", new[] { "horse", "Sprint 11" } },
                { "bison", new[] { "horse", "Sprint 11" } },
                { "hyena", new[] { "wolf", "Sprint 12" } },
                { "goblin-dog", new[] { "worg", "Sprint 12" } },
                { "wolverine", new[] { "worg", "Sprint 12" } },
                { "shadow-mastiff", new[] { "worg", "Sprint 12" } },
                { "poisonous-frog", new[] { "giant-poisonous-frog", "Sprint 13" } },
                { "triceratops", new[] { "mastodon", "Sprint 11" } }
            };

        /// <summary>Close still views around the creature's own facing.</summary>
        internal static readonly string[] StillViews =
            { "front", "front-left", "left", "rear-left", "rear",
              "rear-low", "rear-right", "right", "top" };

        // Creatures whose thin wing solids are also tracked from a low side
        // view through the move legs, so both wing surfaces are seen while
        // they beat (the repaired Stirge; an offline backface-culling
        // inspection raised wing gaps as a lead).
        private static readonly string[] LowTrackingKeys = { "stirge" };

        internal const int LegFrames = 72;
        internal const int ActionFrames = 48;
        internal const int HitFrames = 24;
        internal const int Stride = 2;
        internal const float MinimumTravelMeters = 0.75f;

        internal static bool Handles(string key)
        {
            return Keys.Contains(key, StringComparer.Ordinal);
        }

        internal static bool TryGetDonor(string key, out string donor,
            out string sprintLabel)
        {
            string[] row;
            bool found = Donors.TryGetValue(key, out row);
            donor = found ? row[0] : null;
            sprintLabel = found ? row[1] : null;
            return found;
        }

        /// <summary>
        /// Whether the refinement sequence follows the Stirge's special
        /// attached-prey review (single cast only). That review restores the
        /// prey and detaches the Stirge before this runs; cleanup comes after.
        /// </summary>
        internal static bool FollowsStirgePreyReview(string key, bool quantity)
        {
            return !quantity && key == "stirge" && Handles(key);
        }

        internal static bool TracksLow(string key)
        {
            return LowTrackingKeys.Contains(key, StringComparer.Ordinal);
        }

        internal static string StillFile(string key, string view)
        {
            return key + "-refine-view-" + view + ".png";
        }

        internal static int ExpectedTrackingFrames
        {
            get { return LegFrames / Stride * 2 + ActionFrames / Stride + HitFrames / Stride; }
        }

        internal static int ExpectedLowFrames(string key)
        {
            return TracksLow(key) ? LegFrames / Stride * 2 : 0;
        }

        /// <summary>
        /// True when the packet is complete. <paramref name="missing"/> names
        /// every absent still and any short count, or is empty.
        /// </summary>
        internal static bool EvidenceComplete(string key, IEnumerable<string> writtenStills,
            int trackingWritten, int lowWritten, float travel, out string missing)
        {
            var written = new HashSet<string>(writtenStills ?? new string[0], StringComparer.Ordinal);
            var gaps = StillViews.Select(view => StillFile(key, view))
                .Where(file => !written.Contains(file)).ToList();
            if (trackingWritten != ExpectedTrackingFrames)
                gaps.Add("tracking=" + trackingWritten + "/" + ExpectedTrackingFrames);
            if (lowWritten != ExpectedLowFrames(key))
                gaps.Add("low=" + lowWritten + "/" + ExpectedLowFrames(key));
            if (float.IsNaN(travel) || travel < MinimumTravelMeters)
                gaps.Add("travel<" + MinimumTravelMeters.ToString(
                    System.Globalization.CultureInfo.InvariantCulture));
            missing = string.Join(",", gaps.ToArray());
            return gaps.Count == 0;
        }
    }
}
