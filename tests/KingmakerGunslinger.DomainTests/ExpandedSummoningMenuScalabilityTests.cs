using System;
using System.Collections.Generic;
using System.Linq;
using KingmakerGunslinger.Summoning;

namespace KingmakerGunslinger.DomainTests
{
    /// <summary>
    /// Sprint 1 scalability gate for the Expanded Summoning charter.
    ///
    /// The charter's 1,232 figure is an aggregate over eighteen parent spells,
    /// never a single list. The real question is whether the shipped bounded
    /// presentation still behaves when the worst single parent spell grows from
    /// today's 69 visible choices to the projected 120 - the deduplicated plan,
    /// not 120 plus wrappers that the plan already contains.
    ///
    /// Acceptance rubric, fixed before measuring, relative to the baseline:
    ///   R1 the menu never escapes the canvas-safe rectangle;
    ///   R2 when content exceeds the viewport, scrolling is offered;
    ///   R3 every option stays reachable - the scroll offset spans exactly the
    ///      overflow, with no unreachable head or tail;
    ///   R4 the viewport never collapses below the baseline viewport, so growth
    ///      costs scroll extent and never usable height;
    ///   R5 behaviour at projected scale is the same shape as at baseline scale,
    ///      so nothing new is required of the player beyond scrolling further.
    /// A rubric failure is a real regression and must not be waived.
    /// </summary>
    internal static class ExpandedSummoningMenuScalabilityTests
    {
        // Worst observed parent today: Summon Monster VIII and IX, 69 choices.
        private const int BaselineWorstParent = 69;

        // Projected worst parent: the deduplicated Summon Monster IX plan.
        //
        // An earlier revision computed this as 120 plus the four native
        // wrappers preserved at that tier, giving 124. That double-counted:
        // Bogeyman, Frost Giant, Movanic Deva and Thanadaemon are creatures of
        // the ideal roster and are already inside the 120. One creature is one
        // option, whichever catalog supplies its unit.
        private const int ProjectedWorstParent = 120;
        private const int ProjectedWorstAllyParent = 110;

        // Stress sample, not a target roster. A count above the plan only
        // probes headroom; it authorises no later-sprint content.
        private const int StressSampleWorstParent = 154;

        private const float RowHeight = 34f;
        private const float MenuWidth = 320f;

        /// <summary>
        /// The projection must be a semantic union - one option per creature,
        /// family, parent tier and multiplicity - not a sum of overlapping
        /// catalogs. This is the regression for the 124/112 double-count.
        /// </summary>
        internal static void ProjectedScaleIsDeduplicated()
        {
            Assertions.Equal(69, BaselineWorstParent,
                "The baseline worst parent must match the Sprint 0 census.");

            foreach (SummonFamily family in new[] {
                SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                int expected = family == SummonFamily.Monster
                    ? ProjectedWorstParent : ProjectedWorstAllyParent;
                int planned = ExpandedSummoningIdealRosterCatalog
                    .Placements(family, 9).Count;
                Assertions.Equal(expected, planned,
                    "Projected ninth-level choices drifted from the manifest for " + family);

                // Every wrapper creature at this tier is already a roster
                // creature, so adding wrappers would count it twice.
                foreach (SummonNativeExpansionSpec wrapper in
                    SummonNativeExpansionCatalog.For(family, 9))
                {
                    string key = ExpandedSummoningCoveragePolicy
                        .CanonicalKey(wrapper.CreatureKey);
                    Assertions.True(ExpandedSummoningIdealRosterCatalog
                        .Placements(family, 9)
                        .Any(value => string.Equals(value.Key, key,
                            StringComparison.Ordinal)),
                        "A native wrapper at tier 9 is missing from the plan: " + key);
                }
            }

            // Growth is well under 2x on the only axis a player experiences.
            Assertions.True(ProjectedWorstParent < BaselineWorstParent * 2,
                "The worst single menu must not double against the baseline.");
        }

        /// <summary>
        /// Every tier of the forecast, and the core aggregate, derive from the
        /// same deduplicated plan rather than from separate arithmetic.
        /// </summary>
        internal static void EveryTierDerivesFromOneDeduplicatedPlan()
        {
            int monster = 0;
            int allies = 0;
            for (int parent = 1; parent <= 9; parent++)
            {
                var monsterKeys = ExpandedSummoningIdealRosterCatalog
                    .Placements(SummonFamily.Monster, parent)
                    .Select(value => value.Key).ToArray();
                var allyKeys = ExpandedSummoningIdealRosterCatalog
                    .Placements(SummonFamily.NaturesAlly, parent)
                    .Select(value => value.Key).ToArray();

                Assertions.Equal(monsterKeys.Length,
                    monsterKeys.Distinct(StringComparer.Ordinal).Count(),
                    "Summon Monster " + parent + " repeated a creature.");
                Assertions.Equal(allyKeys.Length,
                    allyKeys.Distinct(StringComparer.Ordinal).Count(),
                    "Summon Nature's Ally " + parent + " repeated a creature.");

                monster += monsterKeys.Length;
                allies += allyKeys.Length;
            }

            Assertions.Equal(624, monster, "Summon Monster forecast changed.");
            Assertions.Equal(608, allies, "Summon Nature's Ally forecast changed.");
            Assertions.Equal(1232, monster + allies,
                "The core aggregate must stay 624 + 608 = 1,232.");
        }

        /// <summary>
        /// Production-compatible ordering, proved through the shipped
        /// SummonDisplayOrderPolicy rather than asserted about a planning list.
        /// The alphabetical order the manifest returns is a stable enumeration,
        /// not the menu contract; the menu contract is singles, then 1d3, then
        /// 1d4+1, with unrelated foreign children preserved at the end.
        /// </summary>
        internal static void ProjectedOrderMatchesTheShippedContract()
        {
            foreach (SummonFamily family in new[] {
                SummonFamily.Monster, SummonFamily.NaturesAlly })
            {
                for (int parent = 1; parent <= 9; parent++)
                {
                    int scopedParent = parent;
                    SummonFamily scopedFamily = family;

                    // Two unrelated third-party children that must survive in
                    // place: the policy identifies them by having no quantity.
                    string[] foreign = { "third-party-alpha", "third-party-omega" };

                    IReadOnlyList<string> ordered = SummonDisplayOrderPolicy.Order(
                        foreign,
                        ExpandedSummoningIdealRosterCatalog
                            .Placements(scopedFamily, scopedParent)
                            .Select(value => value.Key).ToArray(),
                        value => (SummonMultiplicity?)null,
                        value => ExpandedSummoningIdealRosterCatalog.Multiplicity(
                            ExpandedSummoningIdealRosterCatalog.Find(value)
                                .Tier(scopedFamily).Value, scopedParent));

                    int[] rank = ordered
                        .Where(value => !foreign.Contains(value, StringComparer.Ordinal))
                        .Select(value => ExpandedSummoningIdealRosterCatalog.Multiplicity(
                            ExpandedSummoningIdealRosterCatalog.Find(value)
                                .Tier(scopedFamily).Value, scopedParent))
                        .Select(value => value == SummonMultiplicity.One ? 0
                            : value == SummonMultiplicity.OneD3 ? 1 : 2)
                        .ToArray();
                    Assertions.True(rank.SequenceEqual(rank.OrderBy(value => value)),
                        "Quantity groups are out of shipped order for " +
                        scopedFamily + " " + scopedParent + ".");

                    // Unrelated children are preserved, and left at the end.
                    Assertions.Equal(foreign.Length,
                        ordered.Count(value => foreign.Contains(value, StringComparer.Ordinal)),
                        "Unrelated third-party children were dropped at " +
                        scopedFamily + " " + scopedParent + ".");
                    Assertions.True(
                        foreign.SequenceEqual(ordered.Skip(ordered.Count - foreign.Length)),
                        "Unrelated third-party children moved or reordered at " +
                        scopedFamily + " " + scopedParent + ".");

                    // Ordering is stable: same input, same output.
                    Assertions.True(ordered.SequenceEqual(
                        SummonDisplayOrderPolicy.Order(foreign,
                            ExpandedSummoningIdealRosterCatalog
                                .Placements(scopedFamily, scopedParent)
                                .Select(value => value.Key).ToArray(),
                            value => (SummonMultiplicity?)null,
                            value => ExpandedSummoningIdealRosterCatalog.Multiplicity(
                                ExpandedSummoningIdealRosterCatalog.Find(value)
                                    .Tier(scopedFamily).Value, scopedParent))),
                        "Projected ordering is not stable for " +
                        scopedFamily + " " + scopedParent + ".");
                }
            }
        }

        internal static void BaselineAndProjectedScalesSatisfyTheRubric()
        {
            foreach (Viewport viewport in Viewports())
            {
                Outcome baseline = Evaluate(viewport, BaselineWorstParent);
                Outcome projected = Evaluate(viewport, ProjectedWorstParent);
                Outcome projectedAlly = Evaluate(viewport, ProjectedWorstAllyParent);
                AssertRubric(projectedAlly, viewport, "projected Nature's Ally");

                // R1 / R2 / R3 hold independently at both scales.
                AssertRubric(baseline, viewport, "baseline");
                AssertRubric(projected, viewport, "projected");

                // R4: growth must cost scroll extent, never viewport height.
                Assertions.True(projected.ViewportHeight >= baseline.ViewportHeight - 0.01f,
                    "Projected scale shrank the usable viewport on " + viewport.Name + ".");

                // R5: same shape. Once the baseline already scrolls, the larger
                // list may only scroll further; it may not change behaviour.
                if (baseline.RequiresScrolling)
                {
                    Assertions.True(projected.RequiresScrolling,
                        "Projected scale stopped scrolling on " + viewport.Name + ".");
                    Assertions.True(projected.ScrollExtent > baseline.ScrollExtent,
                        "A larger list must scroll further on " + viewport.Name + ".");
                }
            }
        }

        /// <summary>
        /// Headroom probe using a deliberately oversized stress sample. It is
        /// not the target roster and authorises no later-sprint content.
        /// </summary>
        internal static void StressSampleStaysBounded()
        {
            Assertions.True(StressSampleWorstParent > ProjectedWorstParent,
                "A stress sample must exceed the planned roster to probe headroom.");
            foreach (Viewport viewport in Viewports())
            {
                AssertRubric(Evaluate(viewport, StressSampleWorstParent),
                    viewport, "stress sample");
            }
        }

        /// <summary>
        /// The presentation must stay option-count agnostic. A list one row
        /// longer may never flip a clamped menu outside the safe rectangle or
        /// strand a row, at any count from one to well past the projection.
        ///
        /// An earlier revision ran this loop against a single viewport while the
        /// evidence claimed the whole resolution grid. It now executes every
        /// count against every viewport, so the claim and the run agree.
        /// </summary>
        internal static void EveryOptionCountRemainsBounded()
        {
            int executed = 0;
            foreach (Viewport viewport in Viewports())
            {
                for (int options = 1; options <= 200; options++)
                {
                    AssertRubric(Evaluate(viewport, options), viewport,
                        "option count " + options);
                    executed++;
                }
            }

            Assertions.Equal(Viewports().Length * 200, executed,
                "The claimed resolution-by-option-count grid was not fully executed.");
        }

        private static void AssertRubric(Outcome outcome, Viewport viewport,
            string label)
        {
            // R1: never escapes the canvas-safe rectangle.
            Assertions.True(outcome.WithinSafeRect,
                "R1 failed: the menu escaped the safe rectangle at " + label +
                " on " + viewport.Name + ".");

            // R2: overflow always offers scrolling.
            Assertions.Equal(outcome.Overflows, outcome.RequiresScrolling,
                "R2 failed: overflow and scrolling disagree at " + label +
                " on " + viewport.Name + ".");

            // R3: the reachable range is exactly the overflow - no stranded rows.
            Assertions.True(Math.Abs(outcome.OffsetAtTop) < 0.01f,
                "R3 failed: the list head is unreachable at " + label +
                " on " + viewport.Name + ".");
            Assertions.True(
                Math.Abs(outcome.OffsetAtBottom - outcome.ScrollExtent) < 0.01f,
                "R3 failed: the list tail is unreachable at " + label +
                " on " + viewport.Name + ".");
            Assertions.True(outcome.ScrollExtent >= 0f,
                "R3 failed: negative scroll extent at " + label + ".");
        }

        private sealed class Viewport
        {
            internal Viewport(string name, float width, float height, float margin,
                SummonVariantMenuRect anchor,
                SummonVariantMenuOpeningDirection direction)
            {
                Name = name; Width = width; Height = height; Margin = margin;
                Anchor = anchor; Direction = direction;
            }

            internal string Name { get; private set; }
            internal float Width { get; private set; }
            internal float Height { get; private set; }
            internal float Margin { get; private set; }
            internal SummonVariantMenuRect Anchor { get; private set; }
            internal SummonVariantMenuOpeningDirection Direction { get; private set; }
        }

        private sealed class Outcome
        {
            internal bool WithinSafeRect;
            internal bool Overflows;
            internal bool RequiresScrolling;
            internal float ScrollExtent;
            internal float ViewportHeight;
            internal float OffsetAtTop;
            internal float OffsetAtBottom;
        }

        private static Viewport[] Viewports()
        {
            // The resolutions the shipped menu-layout suite already covers, at
            // the action-bar slot positions the popup actually opens from.
            return new[] {
                new Viewport("1920x1080 bottom-left slot", 1920f, 1080f, 20f,
                    new SummonVariantMenuRect(20f, 30f, 64f, 64f),
                    SummonVariantMenuOpeningDirection.Up),
                new Viewport("1600x900 first sidebar slot", 1600f, 900f, 9f,
                    new SummonVariantMenuRect(20f, 820f, 64f, 64f),
                    SummonVariantMenuOpeningDirection.Up),
                new Viewport("1280x720 bottom-left slot", 1280f, 720f, 10f,
                    new SummonVariantMenuRect(18f, 26f, 48f, 48f),
                    SummonVariantMenuOpeningDirection.Up),
                new Viewport("3440x1440 ultrawide", 3440f, 1440f, 24f,
                    new SummonVariantMenuRect(40f, 40f, 64f, 64f),
                    SummonVariantMenuOpeningDirection.Up),
            };
        }

        private static Outcome Evaluate(Viewport viewport, int optionCount)
        {
            float desiredHeight = optionCount * RowHeight;
            SummonVariantMenuLayoutDecision decision =
                SummonVariantMenuLayoutPolicy.Decide(
                    new SummonVariantMenuLayoutRequest(viewport.Anchor,
                        MenuWidth, desiredHeight,
                        new SummonVariantMenuRect(0f, 0f, viewport.Width,
                            viewport.Height),
                        viewport.Margin, viewport.Direction));

            return new Outcome
            {
                WithinSafeRect = decision.SafeRect.Contains(decision.FinalRect,
                    SummonVariantMenuLayoutPolicy.Epsilon),
                Overflows = desiredHeight > decision.ViewportHeight +
                    SummonVariantMenuLayoutPolicy.Epsilon,
                RequiresScrolling = decision.RequiresVerticalScrolling,
                ScrollExtent = decision.VerticalScrollExtent,
                ViewportHeight = decision.ViewportHeight,
                OffsetAtTop = decision.VerticalContentOffset(1f),
                OffsetAtBottom = decision.VerticalContentOffset(0f),
            };
        }
    }
}
